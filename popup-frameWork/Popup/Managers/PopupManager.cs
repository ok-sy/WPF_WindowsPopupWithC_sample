using Popup.Models;
using Popup.Views.Windows;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using Popup.Views.Contents;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Popup.Managers
{
    /*
     * 여러 팝업을 표시 우선순위(DisplayOrder) 단위로 관리한다.
     *
     * 예:
     * Order 1 / SEQUENTIAL
     * → 한 개 표시 후 닫힐 때까지 대기
     *
     * Order 2 / SIMULTANEOUS 3개
     * → 세 개를 동시에 표시
     * → 세 개가 모두 닫혀야 다음 그룹으로 진행
     *
     * Order 3
     * → 그 다음 표시
     */
    public class PopupManager
    {
        private readonly Queue<List<PopupOptions>>
            _popupGroupQueue = new();

        private readonly Queue<PopupOptions>
            _sequentialGroupQueue = new();

        private readonly HashSet<PopupWindow>
            _activeGroupWindows = new();

        private readonly Window _owner;

        /*
         * 모든 모니터의 배경 클릭을 차단하는 Overlay 관리자다.
         * PopupWindow별로 Overlay를 만들지 않고 PopupManager가 한 세트만 관리한다.
         */
        private readonly BackgroundOverlayManager
            _backgroundOverlayManager = new();

        private readonly bool _useBackgroundOverlay;

        private bool _isGroupActive;

        /*
         * backgroundOverlayOpacity
         * 0.0 = 투명, 1.0 = 완전 불투명이다.
         * 기존 호출부와 호환되도록 기본값을 제공한다.
         */
        public PopupManager(
            Window owner,
            bool useBackgroundOverlay = true,
            double backgroundOverlayOpacity = 0.45)
        {
            _owner = owner;
            _useBackgroundOverlay = useBackgroundOverlay;
            _backgroundOverlayManager.Opacity =
                Math.Clamp(backgroundOverlayOpacity, 0.0, 1.0);
        }

        public void Enqueue(
            PopupOptions popupOptions)
        {
            ShowRange(new[] { popupOptions });
        }

        public void Show(
            PopupOptions popupOptions)
        {
            ShowRange(new[] { popupOptions });
        }

        public void ShowRange(
            IEnumerable<PopupOptions> popupOptionsList)
        {
            if (popupOptionsList == null)
            {
                return;
            }

            List<List<PopupOptions>> groups =
                popupOptionsList
                    .Where(option => option != null)
                    .OrderBy(option => option.DisplayOrder)
                    .ThenBy(option => option.PopupId)
                    .GroupBy(option => option.DisplayOrder)
                    .Select(group => group.ToList())
                    .ToList();

            if (groups.Count == 0)
            {
                return;
            }

            foreach (List<PopupOptions> group in groups)
            {
                _popupGroupQueue.Enqueue(group);
            }

            /*
             * 첫 팝업 그룹이 들어오는 시점에 Overlay를 먼저 띄운다.
             * 이미 표시 중이면 BackgroundOverlayManager가 중복 생성하지 않는다.
             */
            if (_useBackgroundOverlay)
            {
                _backgroundOverlayManager.Show();
            }

            ShowNextGroup();
        }

        public void EnqueueRange(
            IEnumerable<PopupOptions> popupOptionsList)
        {
            ShowRange(popupOptionsList);
        }

        private void ShowNextGroup()
        {
            if (_isGroupActive ||
                _popupGroupQueue.Count == 0)
            {
                return;
            }

            List<PopupOptions> group =
                _popupGroupQueue.Dequeue();

            if (group.Count == 0)
            {
                ShowNextGroup();
                return;
            }

            _isGroupActive = true;

            bool showSimultaneously =
                group.All(option =>
                    option.DisplayMode ==
                    PopupDisplayMode.Simultaneous);

            if (showSimultaneously)
            {
                ShowSimultaneousGroup(group);
                return;
            }

            foreach (PopupOptions popupOptions in group)
            {
                _sequentialGroupQueue.Enqueue(popupOptions);
            }

            ShowNextSequentialPopup();
        }

        private void ShowSimultaneousGroup(
            IReadOnlyList<PopupOptions> group)
        {
            foreach (PopupOptions popupOptions in group)
            {
                PopupWindow popupWindow =
                    CreatePopupWindow(popupOptions);

                _activeGroupWindows.Add(popupWindow);

                popupWindow.Closed +=
                    SimultaneousPopupWindow_Closed;

                PositionSimultaneousPopup(
                    popupWindow,
                    _activeGroupWindows.Count - 1);

                popupWindow.Show();
            }
        }

        private void SimultaneousPopupWindow_Closed(
            object? sender,
            EventArgs e)
        {
            if (sender is not PopupWindow popupWindow)
            {
                return;
            }

            popupWindow.Closed -=
                SimultaneousPopupWindow_Closed;

            _activeGroupWindows.Remove(
                popupWindow);

            if (_activeGroupWindows.Count > 0)
            {
                return;
            }

            CompleteCurrentGroup();
        }

        private void ShowNextSequentialPopup()
        {
            if (_sequentialGroupQueue.Count == 0)
            {
                CompleteCurrentGroup();
                return;
            }

            PopupOptions popupOptions =
                _sequentialGroupQueue.Dequeue();

            PopupWindow popupWindow =
                CreatePopupWindow(popupOptions);

            popupWindow.Closed +=
                SequentialPopupWindow_Closed;

            PositionSequentialPopup(
                popupWindow,
                popupOptions);

            popupWindow.Show();
        }

        private void SequentialPopupWindow_Closed(
            object? sender,
            EventArgs e)
        {
            if (sender is PopupWindow popupWindow)
            {
                popupWindow.Closed -=
                    SequentialPopupWindow_Closed;
            }

            ShowNextSequentialPopup();
        }

        private void CompleteCurrentGroup()
        {
            _isGroupActive = false;

            /*
             * 다음 DisplayOrder 그룹이 있으면 Overlay는 유지한다.
             * 모든 그룹이 끝난 시점에만 닫아서 순차 팝업 사이의 깜빡임과
             * SIMULTANEOUS Overlay 중첩을 방지한다.
             */
            if (_popupGroupQueue.Count == 0)
            {
                _backgroundOverlayManager.Close();
                return;
            }

            ShowNextGroup();
        }

        private PopupWindow CreatePopupWindow(
            PopupOptions popupOptions)
        {
            PopupWindow popupWindow =
                new PopupWindow(
                    popupOptions);

            /*
             * Overlay가 Topmost이므로 실제 팝업도 그 위에 유지한다.
             * Overlay가 비활성화된 경우에는 기존 PopupWindow 동작을 유지한다.
             */
            if (_backgroundOverlayManager.IsVisible)
            {
                popupWindow.Topmost = true;
            }

            AttachContentEvents(
                popupWindow,
                popupOptions);

            AttachLifecycleEvents(
                popupWindow,
                popupOptions);

            return popupWindow;
        }

        private void PositionSequentialPopup(
            PopupWindow popupWindow,
            PopupOptions popupOptions)
        {
            if (_owner.IsVisible)
            {
                popupWindow.Owner = _owner;
                return;
            }

            if (popupOptions.SizeMode !=
                PopupSizeMode.Fullscreen)
            {
                popupWindow.WindowStartupLocation =
                    WindowStartupLocation.CenterScreen;
            }
        }

        private void PositionSimultaneousPopup(
            PopupWindow popupWindow,
            int openedPopupIndex)
        {
            popupWindow.WindowStartupLocation =
                WindowStartupLocation.Manual;

            if (_owner.IsVisible)
            {
                popupWindow.Owner = _owner;

                popupWindow.Left =
                    _owner.Left
                    + 50
                    + (openedPopupIndex * 30);

                popupWindow.Top =
                    _owner.Top
                    + 50
                    + (openedPopupIndex * 30);

                return;
            }

            popupWindow.Left =
                SystemParameters.WorkArea.Left
                + ((SystemParameters.WorkArea.Width
                    - popupWindow.Width) / 2)
                + (openedPopupIndex * 30);

            popupWindow.Top =
                SystemParameters.WorkArea.Top
                + ((SystemParameters.WorkArea.Height
                    - popupWindow.Height) / 2)
                + (openedPopupIndex * 30);
        }

        private void AttachContentEvents(
            PopupWindow popupWindow,
            PopupOptions popupOptions)
        {
            if (popupOptions.Content is
                SurveyPopupView surveyPopupView)
            {
                bool isSubmitting = false;

                surveyPopupView.SurveySubmitted +=
                    async (sender, answers) =>
                    {
                        if (isSubmitting)
                        {
                            return;
                        }

                        isSubmitting = true;

                        try
                        {
                            if (popupOptions.SubmitSurveyAsync != null)
                            {
                                await popupOptions.SubmitSurveyAsync(
                                    popupOptions.PopupId,
                                    answers);
                            }

                            popupWindow.Close();
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(
                                "설문 응답을 서버에 저장하지 못했습니다.\n\n" +
                                exception.Message,
                                "설문 저장 오류",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                        finally
                        {
                            isSubmitting = false;
                        }
                    };
            }

            if (popupOptions.Content is
                VideoPopupView videoPopupView)
            {
                bool isSavingVideoProgress = false;

                videoPopupView.VideoProgressSaveRequested +=
                    async (sender, progress) =>
                    {
                        if (isSavingVideoProgress ||
                            popupOptions.SaveVideoProgressAsync == null)
                        {
                            return;
                        }

                        isSavingVideoProgress = true;

                        try
                        {
                            bool completed =
                                await popupOptions.SaveVideoProgressAsync(
                                    popupOptions.PopupId,
                                    progress);

                            if (completed)
                            {
                                popupOptions.IsCompleted = true;
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(
                                $"영상 진행률 저장 실패 " +
                                $"(PopupId: {popupOptions.PopupId}): " +
                                exception.Message);
                        }
                        finally
                        {
                            isSavingVideoProgress = false;
                        }
                    };
            }
        }

        private static void AttachLifecycleEvents(
            PopupWindow popupWindow,
            PopupOptions popupOptions)
        {
            bool displayedEventRecorded = false;

            popupWindow.ContentRendered +=
                async (sender, eventArgs) =>
                {
                    if (displayedEventRecorded)
                    {
                        return;
                    }

                    displayedEventRecorded = true;

                    await InvokeLifecycleCallbackSafelyAsync(
                        popupOptions.PopupDisplayedAsync,
                        popupOptions.PopupId,
                        "DISPLAYED");
                };

            popupWindow.Closed +=
                async (sender, eventArgs) =>
                {
                    await InvokeLifecycleCallbackSafelyAsync(
                        popupOptions.PopupClosedAsync,
                        popupOptions.PopupId,
                        "CLOSED");
                };
        }

        private static async Task InvokeLifecycleCallbackSafelyAsync(
            Func<string, Task>? lifecycleCallback,
            string popupId,
            string eventType)
        {
            if (lifecycleCallback == null ||
                string.IsNullOrWhiteSpace(
                    popupId))
            {
                return;
            }

            try
            {
                await lifecycleCallback(
                    popupId);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"팝업 {eventType} 이벤트 저장 실패 " +
                    $"(PopupId: {popupId}): {exception.Message}");
            }
        }
    }
}
