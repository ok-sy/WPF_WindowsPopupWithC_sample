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
using System.Text.Json;
using System.IO;


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
 * appsettings.json에서
 * Java 팝업 API 기본 주소를 읽는다.
 *
 * 반환 예:
 * http://localhost:8080
 */
        private static string LoadPopupApiBaseUrl()
        {
            /*
             * 실행 중인 Popup.exe가 위치한 폴더를 기준으로
             * appsettings.json의 전체 경로를 만든다.
             *
             * AppContext.BaseDirectory를 사용해야
             * 바로가기나 다른 작업 폴더에서 실행해도
             * EXE 옆의 설정 파일을 정확하게 찾을 수 있다.
             */
            string configurationFilePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "appsettings.json");

            /*
             * 설정 파일이 없으면
             * 어떤 API 서버를 호출해야 하는지 알 수 없으므로
             * 자세한 경로를 포함해 예외를 발생시킨다.
             */
            if (!File.Exists(
                    configurationFilePath))
            {
                throw new FileNotFoundException(
                    "팝업 API 설정 파일을 찾을 수 없습니다.\n" +
                    configurationFilePath,
                    configurationFilePath);
            }

            /*
             * appsettings.json의 전체 내용을
             * 문자열로 읽는다.
             */
            string configurationJson =
                File.ReadAllText(
                    configurationFilePath);

            /*
             * JSON 문자열을 탐색 가능한
             * JsonDocument 객체로 변환한다.
             *
             * using을 사용하므로 메서드 종료 시
             * JsonDocument가 자동으로 정리된다.
             */
            using JsonDocument configurationDocument =
                JsonDocument.Parse(
                    configurationJson);

            JsonElement rootElement =
                configurationDocument.RootElement;

            /*
             * 다음 구조에서 PopupApi 영역을 찾는다.
             *
             * {
             *   "PopupApi": {
             *     "BaseUrl": "..."
             *   }
             * }
             */
            if (!rootElement.TryGetProperty(
                    "PopupApi",
                    out JsonElement popupApiElement))
            {
                throw new InvalidOperationException(
                    "appsettings.json에 PopupApi 설정이 없습니다.");
            }

            /*
             * PopupApi 내부에서 BaseUrl 값을 찾는다.
             */
            if (!popupApiElement.TryGetProperty(
                    "BaseUrl",
                    out JsonElement baseUrlElement))
            {
                throw new InvalidOperationException(
                    "appsettings.json에 PopupApi.BaseUrl 설정이 없습니다.");
            }

            /*
             * JSON의 BaseUrl 값을
             * C# 문자열로 변환한다.
             */
            string? baseUrl =
                baseUrlElement.GetString();

            /*
             * 속성은 존재하지만 값이 비어 있는 경우에도
             * 잘못된 설정으로 처리한다.
             */
            if (string.IsNullOrWhiteSpace(
                    baseUrl))
            {
                throw new InvalidOperationException(
                    "PopupApi.BaseUrl 값이 비어 있습니다.");
            }

            return baseUrl;
        }

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
             * Java Spring Boot 서버의 주소를 전달하여
             * PopupApiService를 생성한다.
             */
            /*
             * appsettings.json에서
             * Java 팝업 API 주소를 읽는다.
             */
            string popupApiBaseUrl =
                LoadPopupApiBaseUrl();

            /*
             * 설정 파일에서 읽은 API 주소를 전달하여
             * PopupApiService를 생성한다.
             */
            _popupApiService =
                new PopupApiService(
                    popupApiBaseUrl);
        }

   
        /*
         * Java API에서 현재 사용자에게 노출할 팝업을 조회하고
         * PopupManager를 통해 화면에 표시한다.
         */
        private async void OpenPopupButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            /*
             * 로그인 기능이 아직 없으므로
             * 테스트용 사용자 ID를 고정해서 사용한다.
             *
             * 이후 로그인 기능이 연결되면
             * 현재 로그인한 사용자의 ID로 교체한다.
             */
            const string currentUserId =
                "E1002";

            try
            {
                /*
                 * Java API와 PostgreSQL DB를 통해
                 * 현재 사용자에게 노출 가능한 팝업만 조회한다.
                 */
                List<PopupResponseDto> popupDtos =
                    await _popupApiService
                        .GetAvailablePopupsAsync(
                            currentUserId);

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
                 * 서버 DTO를 실제 PopupWindow에서 사용하는
                 * PopupOptions 목록으로 변환한다.
                 */
                List<PopupOptions> popupOptionsList =
                    _popupService.CreatePopupOptions(
                        popupDtos);

                foreach (PopupOptions popupOptions
                         in popupOptionsList)
                {
                    /*
                     * PopupWindow에서 "다시 보지 않기"를 선택하면
                     * 실행할 서버 저장 콜백을 설정한다.
                     *
                     * PopupWindow는 API 주소나 사용자 ID를 직접 알지 않고,
                     * 이 콜백만 호출한다.
                     */
                    popupOptions.HidePopupAsync =
                        async (
                            popupId,
                            hideDays) =>
                        {
                            await _popupApiService
                                .HidePopupAsync(
                                    popupId,
                                    currentUserId,
                                    hideDays);
                        };

                    /*
                     * 팝업 내용이 실제 화면에 표시되면
                     * 서버에 DISPLAYED 이벤트를 저장한다.
                     */
                    popupOptions.PopupDisplayedAsync =
                        async popupId =>
                        {
                            await _popupApiService
                                .RecordPopupEventAsync(
                                    popupId,
                                    currentUserId,
                                    "DISPLAYED");
                        };

                    /*
                     * 팝업 창이 실제로 닫히면
                     * 서버에 CLOSED 이벤트를 저장한다.
                     */
                    popupOptions.PopupClosedAsync =
                        async popupId =>
                        {
                            await _popupApiService
                                .RecordPopupEventAsync(
                                    popupId,
                                    currentUserId,
                                    "CLOSED");
                        };

                    /*
                     * 설문/퀴즈 팝업의 제출 이벤트를
                     * 서버 응답 저장 API와 연결한다.
                     */
                    popupOptions.SubmitSurveyAsync =
                        async (popupId, surveyAnswers) =>
                        {
                            List<PopupSubmitAnswerRequestDto> requestAnswers =
                                surveyAnswers
                                    .Select(answer =>
                                        new PopupSubmitAnswerRequestDto
                                        {
                                            QuestionId =
                                                answer.QuestionId,

                                            TextAnswer =
                                                string.IsNullOrWhiteSpace(
                                                    answer.TextAnswer)
                                                    ? null
                                                    : answer.TextAnswer,

                                            OptionIds =
                                                new List<long>(
                                                    answer.SelectedOptionIds)
                                        })
                                    .ToList();

                            await _popupApiService
                                .SubmitResponseAsync(
                                    popupId,
                                    currentUserId,
                                    requestAnswers);
                        };

                    /*
                     * VideoPopupView가 측정한 재생 위치와 실제 시청시간을
                     * 서버의 영상 진행률 API에 저장한다.
                     */
                    popupOptions.SaveVideoProgressAsync =
                        async (popupId, progress) =>
                        {
                            VideoProgressResponseDto response =
                                await _popupApiService
                                .SaveVideoProgressAsync(
                                    popupId,
                                    new VideoProgressRequestDto
                                    {
                                        UserId =
                                            currentUserId,
                                        DurationSeconds =
                                            progress.DurationSeconds,
                                        PositionSeconds =
                                            progress.PositionSeconds,
                                        MaximumPositionSeconds =
                                            progress.MaximumPositionSeconds,
                                        WatchedSeconds =
                                            progress.WatchedSeconds
                                    });

                            return response.Completed;
                        };
                }

                /*
                 * 조회한 팝업을 PopupManager에 전달한다.
                 *
                 * 서버의 displayMode 값에 따라:
                 *
                 * SEQUENTIAL
                 * → 한 개씩 순차적으로 표시
                 * SIMULTANEOUS
                 * → 여러 팝업을 한 번에 표시
                 */
                _popupManager.ShowRange(
                    popupOptionsList);
            }
            catch (HttpRequestException exception)
            {
                MessageBox.Show(
                    "Java 팝업 서버에 연결할 수 없습니다.\n\n" +
                    "Spring Boot 서버가 실행 중인지 확인해주세요.\n\n" +
                    exception.Message,
                    "서버 연결 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "팝업 서버의 응답 시간이 초과되었습니다.",
                    "서버 응답 시간 초과",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "팝업을 불러오는 중 오류가 발생했습니다.\n\n" +
                    exception.Message,
                    "팝업 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /*
 * C# 코드 안에 작성한 평문 JSON으로
 * TEXT 팝업을 생성하는 테스트 이벤트다.
 *
 * Java API나 Oracle DB를 호출하지 않고:
 *
 * 평문 JSON
 * → PopupResponseDto
 * → PopupOptions
 * → PopupManager
 * → PopupWindow
 *
 * 순서로 팝업을 표시한다.
 */
        private void OpenTextPopupButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                /*
                 * 서버 응답과 같은 구조로 작성한
                 * 로컬 테스트용 평문 JSON이다.
                 *
                 * 대괄호로 감싼 이유는
                 * PopupService.CreatePopupOptions()에
                 * 팝업 목록을 전달하기 위해서다.
                 */
                string popupJson =
                    """
            [
              {
                "popupId": "LOCAL_TEXT_TEST_001",
                "popupType": "TEXT",
                "title": "로컬 TEXT 팝업 테스트",
                "displayStartAt": "2026-01-01T00:00:00+09:00",
                "displayEndAt": "2027-12-31T23:59:59+09:00",
                "displayMode": "SEQUENTIAL",
                "sizeMode": "VIEWPORT_RATIO",
                "width": 900,
                "height": 620,
                "widthRatio": 0.60,
                "heightRatio": 0.70,
                "minimumWidth": 600,
                "minimumHeight": 450,
                "maximumWidth": 1000,
                "maximumHeight": 850,
                "showHeader": true,
                "showCloseButton": true,
                "showFooter": true,
                "showDoNotShowAgain": false,
                "content": {
                  "contentTitle": "로컬 JSON 팝업 안내",
                  "description": "Java API를 호출하지 않고 C# 평문 JSON으로 생성한 팝업입니다.",
                  "leftSectionTitle": "1. 테스트 목적",
                  "leftSectionBody": "TEXT 팝업의 JSON 변환과 화면 생성을 독립적으로 확인합니다.",
                  "highlightText": "현재 팝업은 Oracle DB와 관계없이 표시됩니다.",
                  "rightSectionTitle": "2. 처리 흐름",
                  "rightSectionBody": "JSON을 PopupResponseDto로 변환한 뒤 PopupService로 전달합니다.",
                  "additionalDescription": "API 서버가 꺼져 있어도 이 팝업은 정상적으로 열려야 합니다."
                }
              }
            ]
            """;

                /*
                 * Java 서버의 camelCase JSON 속성과
                 * C# DTO의 PascalCase 속성을 연결하기 위한 설정이다.
                 *
                 * JSON:
                 * popupId
                 *
                 * C#:
                 * PopupId
                 */
                JsonSerializerOptions jsonOptions =
                    new JsonSerializerOptions
                    {
                        /*
                         * JSON 속성 이름의
                         * 대소문자를 구분하지 않는다.
                         */
                        PropertyNameCaseInsensitive =
                            true
                    };

                /*
                 * 평문 JSON 배열을
                 * PopupResponseDto 목록으로 변환한다.
                 */
                List<PopupResponseDto>? popupDtos =
                    JsonSerializer.Deserialize<
                        List<PopupResponseDto>>(
                            popupJson,
                            jsonOptions);

                /*
                 * 역직렬화 결과가 없거나
                 * JSON 배열이 비어 있으면 팝업을 만들 수 없다.
                 */
                if (popupDtos == null ||
                    popupDtos.Count == 0)
                {
                    MessageBox.Show(
                        "로컬 JSON에서 팝업 정보를 읽지 못했습니다.",
                        "TEXT 팝업 오류",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                /*
                 * PopupResponseDto 목록을
                 * 실제 화면 생성에 사용하는
                 * PopupOptions 목록으로 변환한다.
                 *
                 * PopupService 내부에서 PopupType이 TEXT인지 확인하고
                 * TextPopupView를 생성한다.
                 */
                List<PopupOptions> popupOptionsList =
                    _popupService.CreatePopupOptions(
                        popupDtos);

                /*
                 * 생성된 TEXT 팝업을
                 * PopupManager를 통해 표시한다.
                 *
                 * 현재 JSON에는 한 건만 있으므로
                 * TEXT 팝업 하나가 열린다.
                 */
                _popupManager.ShowRange(
                    popupOptionsList);
            }
            catch (JsonException exception)
            {
                /*
                 * JSON 문법이 잘못되었거나
                 * DTO 형식과 맞지 않을 때 실행된다.
                 */
                MessageBox.Show(
                    "TEXT 팝업 JSON 형식이 올바르지 않습니다.\n\n" +
                    exception.Message,
                    "JSON 변환 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception exception)
            {
                /*
                 * PopupFactory 또는 View 생성 과정에서
                 * 예상하지 못한 오류가 발생했을 때 실행된다.
                 */
                MessageBox.Show(
                    "TEXT 팝업을 생성하는 중 오류가 발생했습니다.\n\n" +
                    exception.Message,
                    "TEXT 팝업 오류",
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
