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
     * 여러 팝업을 순서대로 관리하는 클래스
     *
     * 팝업을 Queue에 저장한 뒤
     * 현재 팝업이 닫히면 다음 팝업을 표시한다.
     */
    public class PopupManager
    {
        /*
         * 아직 화면에 표시하지 않은
         * 팝업 설정을 저장하는 대기열
         */
        private readonly Queue<PopupOptions>
            _popupQueue = new();

        /*
         * 팝업의 부모가 되는 Window
         *
         * 일반적으로 MainWindow가 들어간다.
         */
        private readonly Window _owner;

        /*
         * 현재 화면에 표시 중인 팝업
         */
        private PopupWindow? _currentPopupWindow;

        /*
         * PopupManager 생성자
         */
        public PopupManager(
            Window owner)
        {
            _owner =
                owner;
        }

        /*
         * 팝업 한 개를 대기열에 추가한다.
         */
        public void Enqueue(
            PopupOptions popupOptions)
        {
            /*
             * 전달받은 팝업 설정을
             * Queue의 가장 뒤에 추가한다.
             */
            _popupQueue.Enqueue(
                popupOptions);

            /*
             * 현재 표시 중인 팝업이 없다면
             * 바로 다음 팝업을 표시한다.
             */
            ShowNext();
        }

        /*
         * PopupOptions의 DisplayMode를 확인하여
         * 순차 표시 또는 동시 표시를 자동으로 결정한다.
         */
        public void Show(
            PopupOptions popupOptions)
        {
            if (popupOptions.DisplayMode ==
                PopupDisplayMode.Simultaneous)
            {
                ShowImmediately(
                    popupOptions);

                return;
            }

            Enqueue(
                popupOptions);
        }

        /*
         * 여러 팝업의 DisplayMode를 각각 확인하여
         * 적절한 방식으로 표시한다.
         */
        public void ShowRange(
            IEnumerable<PopupOptions> popupOptionsList)
        {
            foreach (PopupOptions popupOptions
                     in popupOptionsList)
            {
                Show(
                    popupOptions);
            }
        }

        /*
         * 팝업 여러 개를 한 번에
         * 대기열에 추가한다.
         */
        public void EnqueueRange(
            IEnumerable<PopupOptions> popupOptionsList)
        {
            foreach (PopupOptions popupOptions
                     in popupOptionsList)
            {
                _popupQueue.Enqueue(
                    popupOptions);
            }

            /*
             * 전체 목록을 추가한 뒤
             * 첫 번째 팝업 표시를 시도한다.
             */
            ShowNext();
        }

        /*
         * Queue를 사용하지 않고
         * 팝업을 즉시 표시한다.
         */
        private void ShowImmediately(
            PopupOptions popupOptions)
        {
            /*
             * 전달받은 옵션으로
             * 즉시 표시할 PopupWindow를 생성한다.
             */
            PopupWindow popupWindow =
                new PopupWindow(
                    popupOptions);
            /*
             * 동시에 표시하는 설문 또는 퀴즈 팝업에서도
             * 제출 완료 시 해당 창만 닫을 수 있도록
             * 콘텐츠 이벤트를 팝업 창과 연결한다.
             */
            AttachContentEvents(
                popupWindow,
                popupOptions);

            /*
             * 실제 표시 및 닫힘 시점을 서버에 기록할 수 있도록
             * WPF Window 생명주기 이벤트를 연결한다.
             */
            AttachLifecycleEvents(
                popupWindow,
                popupOptions);
            /*
             * MainWindow를
             * 팝업의 부모 창으로 지정한다.
             */
            popupWindow.Owner =
                _owner;

            /*
             * 동시에 표시되는 팝업들이
             * 완전히 같은 위치에 겹치지 않도록 한다.
             */
            popupWindow.WindowStartupLocation =
                WindowStartupLocation.Manual;

            int openedPopupCount =
                Application.Current.Windows
                    .OfType<PopupWindow>()
                    .Count();

            popupWindow.Left =
                _owner.Left
                + 50
                + (openedPopupCount * 30);

            popupWindow.Top =
                _owner.Top
                + 50
                + (openedPopupCount * 30);

            popupWindow.Show();
        }

        /*
         * 대기열의 다음 팝업을 표시한다.
         */
        private void ShowNext()
        {
            /*
             * 이미 팝업이 열려 있다면
             * 현재 팝업이 닫힐 때까지 기다린다.
             */
            if (_currentPopupWindow != null)
            {
                return;
            }

            /*
             * 대기 중인 팝업이 없다면
             * 아무 작업도 하지 않는다.
             */
            if (_popupQueue.Count == 0)
            {
                return;
            }

            /*
             * Queue의 가장 앞에 있는
             * PopupOptions를 꺼낸다.
             */
            PopupOptions popupOptions =
                _popupQueue.Dequeue();

            /*
             * 꺼낸 옵션으로
             * 실제 PopupWindow를 생성한다.
             */
            _currentPopupWindow =
                new PopupWindow(
                    popupOptions);

            /*
             * 설문 또는 퀴즈 콘텐츠에서 발생하는
             * 제출 완료 이벤트를 현재 팝업 창과 연결한다.
             *
             * 일반 설문
             * → 제출 완료 후 팝업을 닫는다.
             *
             * 퀴즈
             * → 통과 점수 이상일 때만 제출 이벤트가 발생하며
             *   이벤트 발생 후 팝업을 닫는다.
             */
            AttachContentEvents(
                _currentPopupWindow,
                popupOptions);

            /*
             * 순차 팝업에도 실제 표시 및 닫힘 이벤트를 연결한다.
             */
            AttachLifecycleEvents(
                _currentPopupWindow,
                popupOptions);

            /*
             * MainWindow를
             * 팝업의 부모 창으로 지정한다.
             */
            _currentPopupWindow.Owner =
                _owner;

            /*
             * 현재 팝업이 닫히면 실행되는 이벤트
             */
            _currentPopupWindow.Closed +=
                CurrentPopupWindow_Closed;

            /*
             * 팝업을 화면에 표시한다.
             */
            _currentPopupWindow.Show();
        }

        /*
         * 현재 팝업이 닫혔을 때 실행된다.
         */
        private void CurrentPopupWindow_Closed(
            object? sender,
            System.EventArgs e)
        {
            /*
             * 닫힌 팝업의 이벤트 연결을 제거한다.
             */
            if (_currentPopupWindow != null)
            {
                _currentPopupWindow.Closed -=
                    CurrentPopupWindow_Closed;
            }

            /*
             * 현재 팝업이 더 이상 없음을 표시한다.
             */
            _currentPopupWindow =
                null;

            /*
             * Queue에 다음 팝업이 있다면
             * 이어서 표시한다.
             */
            ShowNext();
        }

        /*
         * 팝업 콘텐츠에서 발생하는 완료 이벤트를
         * 해당 PopupWindow와 연결한다.
         */
        private void AttachContentEvents(
            PopupWindow popupWindow,
            PopupOptions popupOptions)
        {
            /*
             * 일반 설문 또는 퀴즈가 제출되면
             * 해당 팝업 창을 닫는다.
             *
             * 퀴즈는 통과했을 때만
             * SurveySubmitted 이벤트가 발생하므로
             * 실패한 경우에는 창이 닫히지 않는다.
             */
            if (popupOptions.Content is
                SurveyPopupView surveyPopupView)
            {
                bool isSubmitting =
                    false;

                surveyPopupView.SurveySubmitted +=
                    async (sender, answers) =>
                    {
                        if (isSubmitting)
                        {
                            return;
                        }

                        isSubmitting =
                            true;

                        /*
                         * API 팝업에는 MainWindow가 설정한 콜백이 있다.
                         * 서버 저장이 성공해야만 팝업을 닫는다.
                         */
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
                            /*
                             * 저장 실패 시 창과 입력값을 그대로 유지하여
                             * 사용자가 다시 제출할 수 있게 한다.
                             */
                            MessageBox.Show(
                                "설문 응답을 서버에 저장하지 못했습니다.\n\n" +
                                exception.Message,
                                "설문 저장 오류",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                        finally
                        {
                            isSubmitting =
                                false;
                        }
                    };
            }

            if (popupOptions.Content is
                VideoPopupView videoPopupView)
            {
                bool isSavingVideoProgress =
                    false;

                videoPopupView.VideoProgressSaveRequested +=
                    async (sender, progress) =>
                    {
                        if (isSavingVideoProgress ||
                            popupOptions.SaveVideoProgressAsync == null)
                        {
                            return;
                        }

                        isSavingVideoProgress =
                            true;

                        try
                        {
                            bool completed =
                                await popupOptions.SaveVideoProgressAsync(
                                popupOptions.PopupId,
                                progress);

                            /*
                             * 서버가 완료 기준 충족을 확인한 뒤에만
                             * 필수 영상 팝업의 닫기를 허용한다.
                             */
                            if (completed)
                            {
                                popupOptions.IsCompleted =
                                    true;
                            }
                        }
                        catch (Exception exception)
                        {
                            /*
                             * 일시적인 저장 오류가 영상 재생을 중단하지 않게 하고,
                             * 다음 5초 저장 시 다시 시도한다.
                             */
                            Debug.WriteLine(
                                $"영상 진행률 저장 실패 " +
                                $"(PopupId: {popupOptions.PopupId}): " +
                                exception.Message);
                        }
                        finally
                        {
                            isSavingVideoProgress =
                                false;
                        }
                    };
            }
        }

        /*
         * PopupWindow의 실제 생명주기와
         * 서버 이벤트 저장 콜백을 연결한다.
         *
         * ContentRendered
         * → 팝업 내용이 실제 화면에 그려진 시점
         *
         * Closed
         * → 팝업 창이 완전히 닫힌 시점
         */
        private static void AttachLifecycleEvents(
            PopupWindow popupWindow,
            PopupOptions popupOptions)
        {
            /*
             * 예외적인 재렌더링이 발생해도 DISPLAYED가
             * 한 창당 한 번만 저장되도록 막는다.
             */
            bool displayedEventRecorded =
                false;

            popupWindow.ContentRendered +=
                async (sender, eventArgs) =>
                {
                    if (displayedEventRecorded)
                    {
                        return;
                    }

                    displayedEventRecorded =
                        true;

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

        /*
         * 서버 이벤트 저장 중 오류가 발생해도
         * 팝업 표시, 닫기, 다음 팝업 열기 동작은 계속 진행한다.
         */
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
