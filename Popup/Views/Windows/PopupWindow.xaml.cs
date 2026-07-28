using Popup.Models;
using Popup.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace Popup.Views.Windows
{
    public partial class PopupWindow : Window
    {
        /*
         * PopupWindow 생성 시 사용할 설정 정보
         *
         * 생성자에서 전달받은 옵션을 저장해두고
         * 화면 초기화 시 사용한다.
         */
        private readonly PopupOptions _options;

        /*
         * PopupWindow 생성자
         *
         * options
         * 팝업 제목, 내용, 버튼 표시 여부 등
         * 팝업 생성에 필요한 설정 정보를 전달받는다.
         */
        public PopupWindow(PopupOptions options)
        {
            /*
             * PopupWindow.xaml을 읽어서
             * 화면 요소를 실제 객체로 만든다.
             */
            InitializeComponent();

            /*
             * 전달받은 옵션 객체가 null이면
             * 팝업을 생성할 수 없으므로 예외를 발생시킨다.
             */
            _options = options
                ?? throw new ArgumentNullException(nameof(options));

            /*
             * 전달받은 옵션 값을
             * 실제 화면에 적용한다.
             */
            ApplyOptions();
        }

        /*
         * PopupOptions에 설정된 값을
         * PopupWindow 화면에 적용한다.
         */
        private void ApplyOptions()
        {
            /*
             * 팝업 상단 제목을 설정한다.
             */
            PopupTitleText.Text = _options.Title;

            /*
             * 전달받은 UserControl을
             * ContentControl 안에 표시한다.
             */
            PopupContent.Content = _options.Content;

            /*
             * 상단 X 버튼 표시 여부를 설정한다.
             */
            CloseButton.Visibility =
                _options.ShowCloseButton
                    ? Visibility.Visible
                    : Visibility.Collapsed;


            /*
             * Header 전체 표시 여부를 적용한다.
             *
             * Visibility만 Collapsed로 변경하면
             * 제목과 닫기 버튼은 보이지 않지만,
             * Header 전용 Grid 행 높이 48은 그대로 남을 수 있다.
             *
             * 따라서 HeaderArea의 표시 여부와
             * HeaderRow의 높이를 함께 변경한다.
             */
            HeaderArea.Visibility = _options.ShowHeader
                ? Visibility.Visible
                : Visibility.Collapsed;

            /*
             * ShowHeader가 true면
             * Header가 사용할 높이 48을 유지한다.
             *
             * ShowHeader가 false면
             * Header 행 높이를 0으로 만들어
             * Header가 차지하던 빈 공간까지 제거한다.
             */
            HeaderRow.Height = _options.ShowHeader
                ? new GridLength(48)
                : new GridLength(0);

            /*
             * 상단 X 닫기 버튼 표시 여부를 적용한다.
             *
             * Header 전체가 숨겨져 있으면
             * ShowCloseButton이 true여도 버튼은 표시되지 않는다.
             */
            CloseButton.Visibility =
                _options.ShowHeader && _options.ShowCloseButton
                    ? Visibility.Visible
                    : Visibility.Collapsed;


            /*
             * Footer 표시 여부를 적용한다.
             *
             * Visibility만 Collapsed로 변경하면
             * Footer 안의 버튼과 체크박스는 보이지 않지만,
             * Grid의 Footer 행 높이 64는 그대로 남을 수 있다.
             *
             * 그래서 FooterArea의 표시 여부와
             * FooterRow의 높이를 함께 변경해야 한다.
             */
            FooterArea.Visibility = _options.ShowFooter
                ? Visibility.Visible
                : Visibility.Collapsed;

            /*
             * ShowFooter가 true면
             * Footer가 사용할 높이 64를 유지한다.
             *
             * ShowFooter가 false면
             * Footer 행 높이를 0으로 만들어
             * Footer가 차지하던 빈 공간까지 완전히 제거한다.
             */
            FooterRow.Height = _options.ShowFooter
            ? new GridLength(64)
            : new GridLength(0);

            /*
             * '30일간 보지 않기' 체크박스
             * 표시 여부를 설정한다.
             */
            DoNotShowAgainCheckBox.Visibility =
                _options.ShowDoNotShowAgain
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            /*
             * PopupOptions의 SizeMode에 따라
             * 팝업 크기를 계산해서 적용한다.
             */
            ApplyWindowSize();
        }

        /*
         * PopupOptions.SizeMode에 따라
         * PopupWindow의 실제 크기를 결정한다.
         */
        private void ApplyWindowSize()
        {
            switch (_options.SizeMode)
            {
                /*
                 * 기존 고정 크기 방식
                 *
                 * Width와 Height 값을 그대로 사용한다.
                 */
                case PopupSizeMode.Fixed:
                    {
                        SizeToContent =
                            SizeToContent.Manual;

                        Width =
                            _options.Width;

                        Height =
                            _options.Height;

                        break;
                    }

                /*
                 * 모니터 작업 영역의 비율로
                 * 팝업 크기를 계산한다.
                 */
                case PopupSizeMode.ViewportRatio:
                    {
                        SizeToContent =
                            SizeToContent.Manual;

                        Rect workArea =
                            SystemParameters.WorkArea;

                        /*
                         * 잘못된 비율이 들어와도
                         * 10%에서 100% 사이로 제한한다.
                         */
                        double widthRatio =
                            Math.Clamp(
                                _options.WidthRatio,
                                0.1,
                                1.0);

                        double heightRatio =
                            Math.Clamp(
                                _options.HeightRatio,
                                0.1,
                                1.0);

                        double calculatedWidth =
                            workArea.Width *
                            widthRatio;

                        double calculatedHeight =
                            workArea.Height *
                            heightRatio;

                        /*
                         * 최대 크기는 설정값뿐 아니라
                         * 실제 모니터 작업 영역도 넘지 않게 제한한다.
                         */
                        double maximumWidth =
                            Math.Min(
                                _options.MaximumWidth,
                                workArea.Width);

                        double maximumHeight =
                            Math.Min(
                                _options.MaximumHeight,
                                workArea.Height);

                        Width =
                            Math.Clamp(
                                calculatedWidth,
                                _options.MinimumWidth,
                                maximumWidth);

                        Height =
                            Math.Clamp(
                                calculatedHeight,
                                _options.MinimumHeight,
                                maximumHeight);

                        break;
                    }

                /*
                 * 콘텐츠 크기에 맞춰
                 * Window 크기를 자동으로 계산한다.
                 */
                case PopupSizeMode.Auto:
                    {
                        Rect workArea =
                            SystemParameters.WorkArea;

                        SizeToContent =
                            SizeToContent.WidthAndHeight;

                        MinWidth =
                            _options.MinimumWidth;

                        MinHeight =
                            _options.MinimumHeight;

                        MaxWidth =
                            Math.Min(
                                _options.MaximumWidth,
                                workArea.Width * 0.9);

                        MaxHeight =
                            Math.Min(
                                _options.MaximumHeight,
                                workArea.Height * 0.9);

                        break;
                    }

                /*
                 * 정의되지 않은 값이 들어온 경우
                 * 기존 고정 크기 방식으로 처리한다.
                 */
                default:
                    {
                        SizeToContent =
                            SizeToContent.Manual;

                        Width =
                            _options.Width;

                        Height =
                            _options.Height;

                        break;
                    }
            }
        }

        /*
         * 상단바 마우스 클릭 이벤트
         *
         * 상단바를 마우스로 누르고 움직이면
         * 팝업 창도 함께 이동한다.
         */
        private void Header_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /*
         * 공통 닫기 이벤트
         *
         * 상단 X 버튼과 하단 닫기 버튼이
         * 같은 메서드를 사용한다.
         */
        private void CloseButton_Click(
                object sender,
                RoutedEventArgs e)
        {
            SaveDoNotShowAgain();

            Close();
        }

        /*
         * "30일간 보지 않기" 체크 여부를 확인하여
         * 숨김 정보를 저장한다.
         */
        private void SaveDoNotShowAgain()
        {
            /*
             * 체크박스가 선택되지 않았다면
             * 숨김 정보를 저장하지 않는다.
             */
            if (!DoNotShowAgainCheckBox.IsChecked.GetValueOrDefault())
            {
                return;
            }

            /*
             * PopupOptions가 없다면
             * 팝업 정보를 확인할 수 없으므로 종료한다.
             *
             * 현재 _options는 생성자에서 반드시 들어오므로
             * 실제로 null이 될 가능성은 거의 없다.
             */
            if (_options == null)
            {
                return;
            }

            /*
             * PopupId가 없으면
             * 어떤 팝업을 숨겨야 하는지 알 수 없으므로
             * 저장하지 않는다.
             */
            if (string.IsNullOrWhiteSpace(
                    _options.PopupId))
            {
                return;
            }

            /*
             * 프로그램 전체가 함께 사용하는
             * PopupStorageService.Instance에 저장한다.
             *
             * 현재 시점으로부터 30일 뒤까지
             * 해당 PopupId를 숨김 상태로 저장한다.
             */
            PopupStorageService.Instance.HideUntil(
                _options.PopupId,
                DateTime.Now.AddDays(30));
        }
    }
}