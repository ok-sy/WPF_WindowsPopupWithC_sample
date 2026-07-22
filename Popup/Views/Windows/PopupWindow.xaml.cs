using Popup.Models;
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
             * 하단 Footer 표시 여부를 설정한다.
             */
            FooterArea.Visibility =
                _options.ShowFooter
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            /*
             * '30일간 보지 않기' 체크박스
             * 표시 여부를 설정한다.
             */
            DoNotShowAgainCheckBox.Visibility =
                _options.ShowDoNotShowAgain
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            /*
             * 팝업 크기를 설정한다.
             */
            Width = _options.Width;
            Height = _options.Height;
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
            /*
             * 아직 실제 저장 기능은 만들지 않았다.
             *
             * 지금은 선택 여부만 확인한다.
             */
            if (DoNotShowAgainCheckBox.IsChecked == true)
            {
                MessageBox.Show(
                    "30일간 보지 않기가 선택되었습니다.",
                    "선택 확인");
            }

            /*
             * 팝업 창을 닫는다.
             */
            Close();
        }
    }
}