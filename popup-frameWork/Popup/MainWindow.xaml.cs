using Popup.Dtos;
using Popup.Models;
using Popup.Views.Contents;
using Popup.Views.Windows;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using Popup.Managers;
using Popup.Services;
using System.Collections.Generic;
using System.Net.Http;

namespace Popup
{
    public partial class MainWindow : Window
    {

        /*
         * 
         * 팝업을 순서대로 표시하는 관리자
         */
        private readonly PopupManager
            _popupManager;

        /*
         * DTO를 PopupOptions로 변환하는 서비스
         */
        private readonly PopupService
            _popupService;

        /*
         * Java Spring Boot 서버와 통신하여
         * 팝업 목록을 조회하는 API 서비스다.
         */
        private readonly PopupApiService
            _popupApiService;


        /*
         * MainWindow 생성자
         *
         * MainWindow.xaml을 읽어서
         * 화면 요소를 실제 객체로 만든다.
         */
        public MainWindow()
        {
            InitializeComponent();

            /*
             * MainWindow를 팝업의 부모 창으로 사용하는
             * PopupManager를 생성한다.
             */
            _popupManager =
                new PopupManager(
                    this);

            /*
             * DTO 변환용 PopupService를 생성한다.
             */
            _popupService =
                new PopupService();

            /*
             * Java Spring Boot 팝업 API와 통신하는
             * PopupApiService를 생성한다.
             */
            _popupApiService =
                new PopupApiService();
        }

    
        /*
         * Java 서버에서 현재 사용자에게 표시할 팝업을 조회하고
         * PopupManager를 통해 화면에 표시한다.
         */
        private async void OpenPopupButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            /*
             * 현재는 로그인 기능이 없으므로
             * 테스트용 사용자 ID를 직접 사용한다.
             *
             * 나중에는 로그인한 사용자 정보나
             * 사번을 이 위치에 연결한다.
             *
             * TEST_USER는 앞선 API 테스트에서
             * 30일 숨김 처리했을 수 있으므로
             * 우선 새로운 사용자 ID로 테스트한다.
             */
            const string currentUserId =
                "WPF_TEST_USER";

            try
            {
                /*
                 * Java 서버의 다음 API를 호출한다.
                 *
                 * GET /api/popups?userId=WPF_TEST_USER
                 *
                 * Java 서버는 Oracle에서 현재 사용자에게
                 * 표시할 수 있는 팝업만 조회하여 반환한다.
                 */
                List<PopupResponseDto> popupDtos =
                    await _popupApiService
                        .GetAvailablePopupsAsync(
                            currentUserId);

                /*
                 * 서버가 빈 배열을 반환했다면
                 * 현재 사용자에게 표시할 팝업이 없는 상태다.
                 *
                 * 예:
                 *
                 * 1. 노출 기간이 끝난 경우
                 * 2. 30일간 보지 않기가 적용된 경우
                 * 3. 사용 가능한 팝업 데이터가 없는 경우
                 */
                if (popupDtos.Count == 0)
                {
                    MessageBox.Show(
                        "현재 표시할 팝업이 없습니다.",
                        "팝업 조회",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                /*
                 * 서버에서 받은 PopupResponseDto 목록을
                 * 실제 화면 생성에 사용하는
                 * PopupOptions 목록으로 변환한다.
                 *
                 * PopupService 내부에서는 다음 작업을 수행한다.
                 *
                 * 1. 팝업 노출 기간 검사
                 * 2. 로컬 숨김 상태 검사
                 * 3. PopupType에 맞는 콘텐츠 생성
                 * 4. PopupOptions 생성
                 */
                List<PopupOptions> popupOptionsList =
                    _popupService.CreatePopupOptions(
                        popupDtos);

                /*
                 * 정책 검사를 통과한 팝업이 없다면
                 * PopupManager에 전달할 필요가 없다.
                 */
                if (popupOptionsList.Count == 0)
                {
                    MessageBox.Show(
                        "팝업 정책 검사를 통과한 항목이 없습니다.",
                        "팝업 조회",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }
                /*
                 * 서버에서 생성된 모든 PopupOptions에
                 * 30일 숨김 저장 함수를 연결한다.
                 *
                 * PopupWindow에서 체크박스를 선택하고 닫으면
                 * 이 함수가 호출된다.
                 */
                foreach (PopupOptions popupOptions
                         in popupOptionsList)
                {
                    /*
                     * PopupWindow는 PopupId와 숨김 일수만 전달한다.
                     *
                     * 현재 사용자 ID와 PopupApiService는
                     * MainWindow가 알고 있으므로 여기서 연결한다.
                     */
                    popupOptions.HidePopupAsync =
                        async (popupId, hideDays) =>
                        {
                            /*
                             * Java 서버에 숨김 요청을 보내고
                             * Oracle 저장이 완료될 때까지 기다린다.
                             */
                            await _popupApiService.HidePopupAsync(
                                popupId,
                                currentUserId,
                                hideDays);
                        };
                }
                /*
                 * 생성된 PopupOptions 목록을
                 * PopupManager에 전달한다.
                 *
                 * PopupManager는 DisplayMode 값에 따라
                 * 팝업을 순차 또는 동시 방식으로 표시한다.
                 */
                _popupManager.ShowRange(
                    popupOptionsList);
            }
            catch (HttpRequestException exception)
            {
                /*
                 * Java 서버가 실행되지 않았거나
                 * HTTP 오류 상태가 반환되면 실행된다.
                 */
                MessageBox.Show(
                    "팝업 서버에 연결하지 못했습니다.\n\n" +
                    exception.Message,
                    "서버 연결 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                /*
                 * Java 서버가 설정된 제한 시간 안에
                 * 응답하지 않으면 실행된다.
                 */
                MessageBox.Show(
                    "팝업 서버의 응답 시간이 초과되었습니다.",
                    "서버 응답 지연",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception exception)
            {
                /*
                 * JSON 변환 실패나 PopupFactory 오류처럼
                 * 예상하지 못한 문제가 발생하면 실행된다.
                 */
                MessageBox.Show(
                    "팝업을 불러오는 중 오류가 발생했습니다.\n\n" +
                    exception.Message,
                    "팝업 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

                /*
                 * 모니터 작업 영역을 기준으로
                 * 팝업 크기를 비율 계산한다.
                 */
                SizeMode =
                    PopupSizeMode.ViewportRatio,

                /*
                 * 작업 영역 너비의 70%
                 */
                WidthRatio =
                    0.7,

                /*
                 * 작업 영역 높이의 75%
                 */
                HeightRatio =
                    0.75,

                /*
                 * 계산 결과가 지나치게 작아지지 않도록
                 * 최소 크기를 제한한다.
                 */
                MinimumWidth =
                    600,

                MinimumHeight =
                    450,

                /*
                 * 고해상도 모니터에서 팝업이
                 * 지나치게 커지지 않도록 제한한다.
                 */
                MaximumWidth =
                    1200,

                MaximumHeight =
                    900,

                ShowCloseButton =
                    true,

                ShowFooter =
                    true,

                ShowDoNotShowAgain =
                    false
            };

            /*
             * PopupOptions를 전달하여 팝업 창을 생성하고 표시한다.
             */
            PopupWindow popupWindow =
                new PopupWindow(options);

            popupWindow.Show();
        }


        private void OpenSurveyPopupButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            List<SurveyQuestion> questions = new List<SurveyQuestion>
    {
        new SurveyQuestion
        {
            QuestionId = 1,
            Title = "교육 내용에 얼마나 만족하셨나요?",
            Description = "전체 교육 내용을 기준으로 평가해주세요.",
            QuestionType = SurveyQuestionType.Rating5,
            IsRequired = true
        },

        new SurveyQuestion
        {
            QuestionId = 2,
            Title = "교육을 알게 된 경로를 선택해주세요.",
            QuestionType = SurveyQuestionType.SingleChoice,
            IsRequired = true,

            Options =
            {
                new SurveyOption
                {
                    Value = "EMAIL",
                    Text = "이메일"
                },
                new SurveyOption
                {
                    Value = "NOTICE",
                    Text = "사내 공지"
                },
                new SurveyOption
                {
                    Value = "RECOMMEND",
                    Text = "동료 추천"
                },
                new SurveyOption
                {
                    Value = "ETC",
                    Text = "기타"
                }
            }
        },

        new SurveyQuestion
        {
            QuestionId = 3,
            Title = "도움이 되었던 내용을 선택해주세요.",
            Description = "여러 항목을 선택할 수 있습니다.",
            QuestionType = SurveyQuestionType.MultipleChoice,
            IsRequired = false,

            Options =
            {
                new SurveyOption
                {
                    Value = "THEORY",
                    Text = "이론 설명"
                },
                new SurveyOption
                {
                    Value = "EXAMPLE",
                    Text = "실습 예제"
                },
                new SurveyOption
                {
                    Value = "DOCUMENT",
                    Text = "교육 자료"
                },
                new SurveyOption
                {
                    Value = "QNA",
                    Text = "질의응답"
                }
            }
        },

        new SurveyQuestion
        {
            QuestionId = 4,
            Title = "추가 의견을 작성해주세요.",
            Description = "개선이 필요한 점이나 좋았던 점을 자유롭게 작성해주세요.",
            QuestionType = SurveyQuestionType.Text,
            IsRequired = false
        }
    };

            SurveyPopupView surveyView = new SurveyPopupView(
            "교육 만족도 설문",
            "더 나은 교육을 위해 아래 문항에 응답해주세요.",
            questions);

            PopupOptions options =
            new PopupOptions
            {
                Title =
                    "교육 만족도 설문",

                Content =
                    surveyView,

                ShowHeader =
                    false,

                ShowFooter =
                    false,

                /*
                 * 화면 작업 영역을 기준으로
                 * 설문 팝업 크기를 계산한다.
                 */
                SizeMode =
                    PopupSizeMode.ViewportRatio,

                WidthRatio =
                    0.55,

                HeightRatio =
                    0.75,

                MinimumWidth =
                    600,

                MinimumHeight =
                    500,

                MaximumWidth =
                    900,

                MaximumHeight =
                    900
            };

            PopupWindow popupWindow =
                new PopupWindow(options);

            /*
             * SurveyPopupView에서 제출 이벤트가 발생하면
             * 전달받은 응답 목록을 처리한다.
             */
            surveyView.SurveySubmitted +=
                (surveySender, answers) =>
                {
                    /*
                     * 현재는 API가 연결되지 않았으므로
                     * 응답 개수만 확인한다.
                     *
                     * 나중에는 이 위치에서
                     * API 호출 또는 DB 저장 처리를 한다.
                     */
                    int answeredCount = answers.Count(answer =>
                        answer.SelectedValues.Count > 0 ||
                        !string.IsNullOrWhiteSpace(
                            answer.TextAnswer));

                    MessageBox.Show(
                        $"전체 {answers.Count}문항 중 " +
                        $"{answeredCount}문항이 제출되었습니다.",
                        "설문 제출 완료",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    /*
                     * 제출 처리가 성공한 뒤
                     * 설문 팝업을 닫는다.
                     */
                    popupWindow.Close();
                };

            popupWindow.ShowDialog();
        }
        /*
 * 퀴즈 팝업 열기 버튼 클릭 이벤트
 *
 * SurveyPopupView를 QuizMode로 실행하여
 * 객관식 문항을 채점하고 통과 점수를 확인한다.
 */
        private void OpenQuizPopupButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            /*
             * 퀴즈에서 사용할 문항 목록을 만든다.
             */
            List<SurveyQuestion> questions =
                new List<SurveyQuestion>
                {
            new SurveyQuestion
            {
                QuestionId = 1,
                Title = "다음 중 개인정보에 해당하는 것은?",
                Description = "정답을 하나 선택해주세요.",
                QuestionType =
                    SurveyQuestionType.SingleChoice,
                IsRequired = true,

                /*
                 * 이 문항은 채점 대상이다.
                 */
                IsScored = true,

                Options =
                {
                    new SurveyOption
                    {
                        Value = "PHONE",
                        Text = "휴대전화 번호"
                    },
                    new SurveyOption
                    {
                        Value = "WEATHER",
                        Text = "오늘의 날씨"
                    },
                    new SurveyOption
                    {
                        Value = "BUILDING",
                        Text = "회사 건물 층수"
                    }
                },

                /*
                 * 정답은 SurveyOption.Text가 아니라
                 * SurveyOption.Value 값을 사용한다.
                 */
                CorrectAnswers =
                {
                    "PHONE"
                }
            },

            new SurveyQuestion
            {
                QuestionId = 2,
                Title = "올바른 비밀번호 관리 방법을 모두 선택하세요.",
                Description = "복수 선택 문항입니다.",
                QuestionType =
                    SurveyQuestionType.MultipleChoice,
                IsRequired = true,
                IsScored = true,

                Options =
                {
                    new SurveyOption
                    {
                        Value = "LONG",
                        Text = "충분히 긴 비밀번호를 사용한다."
                    },
                    new SurveyOption
                    {
                        Value = "REUSE",
                        Text = "모든 사이트에서 같은 비밀번호를 사용한다."
                    },
                    new SurveyOption
                    {
                        Value = "MFA",
                        Text = "다중 인증을 사용한다."
                    },
                    new SurveyOption
                    {
                        Value = "SHARE",
                        Text = "동료와 비밀번호를 공유한다."
                    }
                },

                CorrectAnswers =
                {
                    "LONG",
                    "MFA"
                }
            },

            new SurveyQuestion
            {
                QuestionId = 3,
                Title = "의심스러운 이메일을 받았을 때 가장 적절한 행동은?",
                QuestionType =
                    SurveyQuestionType.SingleChoice,
                IsRequired = true,
                IsScored = true,

                Options =
                {
                    new SurveyOption
                    {
                        Value = "CLICK",
                        Text = "링크를 눌러 내용을 확인한다."
                    },
                    new SurveyOption
                    {
                        Value = "REPORT",
                        Text = "링크를 누르지 않고 보안 담당자에게 신고한다."
                    },
                    new SurveyOption
                    {
                        Value = "FORWARD",
                        Text = "동료들에게 그대로 전달한다."
                    }
                },

                CorrectAnswers =
                {
                    "REPORT"
                }
            },

            new SurveyQuestion
            {
                QuestionId = 4,
                Title = "교육에 대한 의견을 작성해주세요.",
                Description = "이 문항은 채점하지 않습니다.",
                QuestionType =
                    SurveyQuestionType.Text,
                IsRequired = false,
                IsScored = false
            }
                };

            /*
             * SurveyPopupView를 QuizMode로 생성한다.
             *
             * isQuizMode = true
             * → 제출 버튼이 채점 버튼으로 변경된다.
             *
             * passingScore = 80
             * → 80점 이상이어야 제출 완료 이벤트가 발생한다.
             */
            SurveyPopupView quizView =
                new SurveyPopupView(
                    "정보보안 교육 평가",
                    "채점 문항에서 80점 이상이면 통과입니다.",
                    questions,
                    isQuizMode: true,
                    passingScore: 80);

            PopupOptions options =
            new PopupOptions
            {
                Title =
                    "정보보안 교육 평가",

                Content =
                    quizView,

                ShowHeader =
                    false,

                ShowFooter =
                    false,

                /*
                 * 모니터 작업 영역을 기준으로
                 * 퀴즈 팝업 크기를 계산한다.
                 */
                SizeMode =
                    PopupSizeMode.ViewportRatio,

                WidthRatio =
                    0.55,

                HeightRatio =
                    0.75,

                MinimumWidth =
                    600,

                MinimumHeight =
                    500,

                MaximumWidth =
                    900,

                MaximumHeight =
                    900
            };

            PopupWindow popupWindow =
                new PopupWindow(options);

            /*
             * 사용자가 통과 점수 이상을 받아
             * SurveySubmitted 이벤트가 발생하면 실행된다.
             */
            quizView.SurveySubmitted +=
                (quizSender, answers) =>
                {
                    /*
                     * 현재는 테스트 단계이므로
                     * 제출된 문항 개수만 안내한다.
                     *
                     * 나중에는 이 위치에서
                     * API 저장 처리를 한다.
                     */
                    int answeredCount =
                        answers.Count(answer =>
                            answer.SelectedValues.Count > 0 ||
                            !string.IsNullOrWhiteSpace(
                                answer.TextAnswer));

                    MessageBox.Show(
                        $"전체 {answers.Count}문항 중 " +
                        $"{answeredCount}문항이 제출되었습니다.",
                        "평가 제출 완료",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    /*
                     * 통과 후 저장 처리가 완료되면
                     * 퀴즈 팝업을 닫는다.
                     */
                    popupWindow.Close();
                };

            popupWindow.ShowDialog();
        }
    }
}