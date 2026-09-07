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
        /*
         * DisplayOrder별 팝업 그룹을 저장하는 대기열이다.
         */
        private readonly Queue<List<PopupOptions>>
            _popupGroupQueue = new();

        /*
         * 순차 표시 그룹 안에서 아직 표시하지 않은 팝업 대기열이다.
         */
        private readonly Queue<PopupOptions>
            _sequentialGroupQueue = new();

        /*
         * 현재 동시 표시 그룹에서 열려 있는 창들이다.
         * 모두 닫혀야 다음 DisplayOrder 그룹으로 이동한다.
         */
        private readonly HashSet<PopupWindow>
            _activeGroupWindows = new();

        private readonly Window _owner;

        /*
         * 현재 하나의 DisplayOrder 그룹을 처리 중인지 나타낸다.
         */
        private bool _isGroupActive;

        public PopupManager(
            Window owner)
        {
            _owner = owner;
        }

        /*
         * 팝업 한 개도 그룹 처리 규칙을 동일하게 사용한다.
         */
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

        /*
         * 여러 팝업을 DisplayOrder 오름차순으로 정렬한 뒤
         * 같은 순위를 하나의 그룹으로 묶는다.
         */
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

            foreach (List<PopupOptions> group in groups)
            {
                _popupGroupQueue.Enqueue(group);
            }

            ShowNextGroup();
        }

        public void EnqueueRange(
            IEnumerable<PopupOptions> popupOptionsList)
        {
            ShowRange(popupOptionsList);
        }

        /*
         * 다음 DisplayOrder 그룹을 시작한다.
         */
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

            /*
             * 같은 우선순위의 모든 팝업이 SIMULTANEOUS일 때만
             * 해당 그룹 전체를 동시에 표시한다.
             *
             * 같은 우선순위에 SEQUENTIAL과 SIMULTANEOUS가 섞여 있으면
             * 동작이 모호해지므로 안전하게 순차 처리한다.
             */
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

        /*
         * 같은 DisplayOrder의 동시 팝업을 한꺼번에 표시한다.
         */
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

        /*
         * 동시 그룹의 창 하나가 닫힐 때마다 남은 창 수를 확인한다.
         * 마지막 창이 닫혀야 다음 우선순위 그룹으로 이동한다.
         */
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

        /*
         * 순차 그룹에서 다음 팝업 한 개를 표시한다.
         */
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

        /*
         * 현재 DisplayOrder 그룹의 처리를 종료하고
         * 다음 우선순위 그룹을 시작한다.
         */
        private void CompleteCurrentGroup()
        {
            _isGroupActive = false;
            ShowNextGroup();
        }

        /*
         * PopupWindow 생성과 공통 이벤트 연결을 한 곳에서 처리한다.
         */
        private PopupWindow CreatePopupWindow(
            PopupOptions popupOptions)
        {
            PopupWindow popupWindow =
                new PopupWindow(
                    popupOptions);

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

        /*
         * 팝업 콘텐츠에서 발생하는 완료 이벤트를
         * 해당 PopupWindow와 연결한다.
         */
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

        /*
         * Window가 실제 표시/닫힌 시점의 서버 이벤트를 연결한다.
         */
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
