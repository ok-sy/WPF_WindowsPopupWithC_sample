using System.Windows;

namespace Popup
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /*
         * 테스트 버튼 클릭
         */
        private void OpenPopup_Click(
            object sender,
            RoutedEventArgs e)
        {
            /*
             * TextPopupWindow 객체 생성
             */
            TextPopupWindow popup = new TextPopupWindow();

            /*
             * 부모창 지정
             *
             * CenterOwner를 사용할 때 필요하다.
             */
            popup.Owner = this;

            /*
             * 팝업 표시
             *
             * Show()
             *  -> MainWindow도 클릭 가능
             *
             * ShowDialog()
             *  -> 팝업을 닫기 전까지 MainWindow 클릭 불가
             */
            popup.ShowDialog();
        }
    }
}