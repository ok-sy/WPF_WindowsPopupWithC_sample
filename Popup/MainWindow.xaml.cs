using Popup.Models;
using Popup.Views.Contents;
using Popup.Views.Windows;
using System.Security.Policy;
using System.Windows;

namespace Popup
{
    public partial class MainWindow : Window
    {
        /*
         * MainWindow 생성자
         *
         * MainWindow.xaml을 읽어서
         * 화면 요소를 실제 객체로 만든다.
         */
        public MainWindow()
        {
            InitializeComponent();
        }

        /*
         * 팝업 열기 버튼 클릭 이벤트
         *
         * PopupOptions에 팝업 설정을 입력하고
         * PopupWindow를 생성하여 화면에 표시한다.
         */
        private void OpenPopupButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            /*
             * 팝업 가운데에 표시할
             * 실제 내용 화면을 생성한다.
             */
            TextPopupView popupContent =
                new TextPopupView();

            /*
             * 팝업 생성에 필요한 설정 정보를 만든다.
             *
             * 설정하지 않은 속성은
             * PopupOptions에 정의된 기본값을 사용한다.
             */
            PopupOptions popupOptions =
                new PopupOptions
                {
                    /*
                     * 팝업 상단바에 표시할 제목
                     */
                    Title = "",

                    /*
                     * PopupWindow의 ContentControl에
                     * 표시할 실제 내용 화면
                     */
                    Content = popupContent,

                    /*
                     * 상단 X 닫기 버튼 표시 여부
                     */
                    ShowCloseButton = false,

                    /*
                     * 하단 Footer 전체 표시 여부
                     */
                    ShowFooter = true,

                    /*
                     * 30일간 보지 않기
                     * 체크박스 표시 여부
                     */
                    ShowDoNotShowAgain = true,

                    /*
                     * 팝업 창 너비
                     */
                    Width = 760,

                    /*
                     * 팝업 창 높이
                     */
                    Height = 750
                };

            /*
             * 위에서 만든 옵션을 전달하여
             * 새로운 PopupWindow 객체를 생성한다.
             *
             * new를 호출할 때
             * PopupWindow 생성자가 실행된다.
             */
            PopupWindow popupWindow =
                new PopupWindow(popupOptions);

            /*
             * 현재 MainWindow를
             * 팝업의 부모 창으로 지정한다.
             *
             * PopupWindow의
             * WindowStartupLocation="CenterOwner" 설정에 따라
             * MainWindow 가운데에 팝업이 표시된다.
             */
            popupWindow.Owner = this;

            /*
             * 팝업을 일반 창 방식으로 표시한다.
             *
             * Show를 사용하면 팝업이 열려 있어도
             * MainWindow를 계속 사용할 수 있다.
             */
            popupWindow.Show();
        }

        /*
 * 이미지 팝업 열기 버튼 클릭 이벤트
 *
 * 외부 이미지를 표시하는 ImagePopupView를 생성하고,
 * 이미지 비율에 따라 PopupWindow 크기를 자동 변경한다.
 */
        string imageUrl = "https://images.unsplash.com/photo-1784037076368-fb4a699076e5?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D";
        private void OpenImagePopupButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            /*
             * 이미지 팝업에 표시할
             * 실제 콘텐츠 화면을 생성한다.
             */

            ImagePopupView imagePopupContent =
            new ImagePopupView(
                /*
                 * 이미지 콘텐츠 내부 제목
                 */
                "외부 이미지 안내",

                /*
                 * 표시할 외부 이미지 URL
                 */
                imageUrl,

                /*
                 * 이미지 설명
                 */
                "이미지 원본 표시 크기에 맞춰 팝업 크기가 자동으로 결정됩니다.",

                /*
                 * 이미지 설명 표시 여부
                 */
                false,

                /*
                 * 이미지 실제 크기를 기준으로
                 * 팝업 크기를 계산한다.
                 */
                ImagePopupSizeMode.FitToImage,
                imageWidth: 280,
                imageHeight: 400

                );

            /*
             * 이미지 팝업에 사용할
             * 공통 옵션을 생성한다.
             *
             * Width와 Height는
             * 이미지가 로드된 후 다시 자동 조절된다.
             */
            PopupOptions popupOptions =
                new PopupOptions
                {
                    /*
                     * PopupWindow 상단 제목
                     */
                    Title = "이미지 안내",

                    /*
                     * PopupWindow 가운데에 표시할
                     * 이미지 콘텐츠
                     */
                    Content = imagePopupContent,

                    /*
                     * 상단 X 닫기 버튼 표시 여부
                     */
                    ShowCloseButton = false,

                    /*
                     * 하단 Footer 표시 여부
                     */
                    ShowFooter = true,

                    /*
                     * 다시 보지 않기 체크박스 표시 여부
                     */
                    ShowDoNotShowAgain = true,

                    /*
                     * 이미지 로드 전 사용할
                     * 기본 팝업 너비
                     */
                    Width = 760,

                    /*
                     * 이미지 로드 전 사용할
                     * 기본 팝업 높이
                     */
                    Height = 760
                };

            /*
             * PopupOptions를 사용하여
             * PopupWindow를 생성한다.
             */
            PopupWindow popupWindow =
                new PopupWindow(popupOptions);

            /*
             * 이미지 비율 계산이 완료되어
             * 추천 팝업 크기가 전달되면 실행된다.
             */
            imagePopupContent.RecommendedSizeChanged +=
                (recommendedWidth, recommendedHeight) =>
                {
                    /*
                     * 현재 사용 가능한 모니터 영역 너비의
                     * 90%를 최대 팝업 너비로 설정한다.
                     */
                    double maximumWidth =
                        SystemParameters.WorkArea.Width * 0.9;

                    /*
                     * 현재 사용 가능한 모니터 영역 높이의
                     * 90%를 최대 팝업 높이로 설정한다.
                     */
                    double maximumHeight =
                        SystemParameters.WorkArea.Height * 0.9;

                    /*
                     * 추천 너비와 모니터 최대 너비 중
                     * 더 작은 값을 실제 팝업 너비로 사용한다.
                     */
                    popupWindow.Width =
                        Math.Min(
                            recommendedWidth,
                            maximumWidth);

                    /*
                     * 추천 높이와 모니터 최대 높이 중
                     * 더 작은 값을 실제 팝업 높이로 사용한다.
                     */
                    popupWindow.Height =
                        Math.Min(
                            recommendedHeight,
                            maximumHeight);
                };

            /*
             * 현재 MainWindow를
             * 팝업의 부모 창으로 지정한다.
             */
            popupWindow.Owner = this;

            /*
             * 이미지 팝업을 화면에 표시한다.
             */
            popupWindow.Show();
        }

        private void BtnVideoPopup_Click(
    object sender,
    RoutedEventArgs e)
        {
            /*
             * 영상 팝업에 표시할 VideoPopupView를 생성한다.
             *
             * 아래 경로는 테스트할 실제 영상 파일 경로로 변경해야 한다.
             */
            VideoPopupView videoPopupView =
                new VideoPopupView(
                    videoTitle: "교육 영상",
                    videoPath: "https://samplelib.com/preview/mp4/sample-5s.mp4",
                    videoDescription: "영상을 끝까지 시청해주세요.",
                    showDescription: true);

            /*
             * 생성한 VideoPopupView를
             * PopupOptions.Content에 전달한다.
             *
             * PopupWindow.Content에 직접 대입하면
             * Window 전체 XAML 구조가 교체되므로 사용하면 안 된다.
             */
            PopupOptions options =
                new PopupOptions
                {
                    Title = "교육 영상",
                    Content = videoPopupView,
                    Width = 850,
                    Height = 680,
                    ShowCloseButton = true,
                    ShowFooter = true,
                    ShowDoNotShowAgain = false
                };

            /*
             * PopupOptions를 전달하여 팝업 창을 생성하고 표시한다.
             */
            PopupWindow popupWindow =
                new PopupWindow(options);

            popupWindow.Show();
        }

    }
}