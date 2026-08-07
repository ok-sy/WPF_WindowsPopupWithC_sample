using System.Windows.Controls;

namespace Popup.Views.Contents
{
    public partial class TextPopupView : UserControl
    {
        /*
         * Visual Studio 미리보기 또는
         * 기존 코드에서 사용하는 기본 생성자
         */
        public TextPopupView()
        {
            /*
             * TextPopupView.xaml을 읽어서
             * 화면 요소를 생성한다.
             */
            InitializeComponent();
        }

        /*
         * 서버 DTO에서 전달받은 값으로
         * TEXT 팝업 화면을 구성하는 생성자
         */
        public TextPopupView(
            string contentTitle,
            string description,
            string leftSectionTitle,
            string leftSectionBody,
            string highlightText,
            string rightSectionTitle,
            string rightSectionBody,
            string additionalDescription)
            {
                /*
                 * TextPopupView.xaml을 읽어서
                 * x:Name이 지정된 화면 요소를 생성한다.
                 *
                 * 이 코드보다 먼저 TextBlock에 접근하면
                 * 아직 객체가 만들어지지 않아 오류가 발생한다.
                 */
                InitializeComponent();

                /*
                 * DTO에서 전달받은 값을
                 * 화면의 각 TextBlock에 표시한다.
                 */
                ContentTitleText.Text =
                    contentTitle;

                ContentDescriptionText.Text =
                    description;

                LeftSectionTitleText.Text =
                    leftSectionTitle;

                LeftSectionBodyText.Text =
                    leftSectionBody;

                HighlightTextBlock.Text =
                    highlightText;

                RightSectionTitleText.Text =
                    rightSectionTitle;

                RightSectionBodyText.Text =
                    rightSectionBody;

                AdditionalDescriptionText.Text =
                    additionalDescription;
            }
        }
}