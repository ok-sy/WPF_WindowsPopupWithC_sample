using Popup.Dtos;
using System.Text.Json;

namespace Popup.Services
{
    /// <summary>
    /// Java API와 DB 없이 화면 시연에 사용할 WPF 전용 샘플 데이터를 만든다.
    /// 서버 응답과 같은 DTO 구조를 사용하므로 실제 PopupFactory와 화면을 그대로 검증한다.
    /// </summary>
    public static class DemoPopupDataService
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public static List<PopupResponseDto> CreatePopups()
        {
            const string demoJson =
                """
                [
                  {
                    "popupId": "DEMO-TEXT-001",
                    "popupType": "TEXT",
                    "title": "서비스 이용 안내",
                    "displayMode": "SEQUENTIAL",
                    "sizeMode": "VIEWPORT_RATIO",
                    "widthRatio": 0.58,
                    "heightRatio": 0.68,
                    "minimumWidth": 620,
                    "minimumHeight": 470,
                    "maximumWidth": 980,
                    "maximumHeight": 820,
                    "showHeader": true,
                    "showCloseButton": true,
                    "showFooter": true,
                    "showDoNotShowAgain": false,
                    "content": {
                      "contentTitle": "팝업 시스템 화면 시연",
                      "description": "현재 EXE는 WPF 전용 Demo Mode로 실행 중입니다.",
                      "leftSectionTitle": "시연 내용",
                      "leftSectionBody": "TEXT, IMAGE, VIDEO, SURVEY, QUIZ 팝업을 순서대로 확인할 수 있습니다.",
                      "highlightText": "Java API와 PostgreSQL은 실행하지 않아도 됩니다.",
                      "rightSectionTitle": "운영 모드 전환",
                      "rightSectionBody": "appsettings.json의 DemoMode를 false로 변경하면 실제 API 조회 모드로 실행됩니다.",
                      "additionalDescription": "팝업을 닫으면 다음 샘플 화면이 자동으로 표시됩니다."
                    }
                  },
                  {
                    "popupId": "DEMO-IMAGE-001",
                    "popupType": "IMAGE",
                    "title": "이미지 팝업 시연",
                    "displayMode": "SEQUENTIAL",
                    "sizeMode": "FIXED",
                    "width": 720,
                    "height": 760,
                    "showHeader": true,
                    "showCloseButton": true,
                    "showFooter": true,
                    "showDoNotShowAgain": false,
                    "content": {
                      "imageTitle": "이미지 콘텐츠",
                      "imageUrl": "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=900&q=80",
                      "description": "외부 이미지 URL을 표시하는 팝업입니다.",
                      "showDescription": true,
                      "imageSizeMode": "ADAPTIVE",
                      "imageWidth": 620,
                      "imageHeight": 520
                    }
                  },
                  {
                    "popupId": "DEMO-VIDEO-001",
                    "popupType": "VIDEO",
                    "title": "교육 영상 시연",
                    "displayMode": "SEQUENTIAL",
                    "sizeMode": "VIEWPORT_RATIO",
                    "widthRatio": 0.70,
                    "heightRatio": 0.75,
                    "minimumWidth": 680,
                    "minimumHeight": 500,
                    "maximumWidth": 1200,
                    "maximumHeight": 900,
                    "showHeader": true,
                    "showCloseButton": true,
                    "showFooter": true,
                    "showDoNotShowAgain": false,
                    "allowCloseBeforeComplete": true,
                    "content": {
                      "videoTitle": "Demo Mode 교육 영상",
                      "videoUrl": "https://interactive-examples.mdn.mozilla.net/media/cc0-videos/flower.mp4",
                      "description": "재생, 일시정지, 전체화면 컨트롤을 확인하세요.",
                      "showDescription": true
                    }
                  },
                  {
                    "popupId": "DEMO-SURVEY-001",
                    "popupType": "SURVEY",
                    "title": "교육 만족도 설문",
                    "displayMode": "SEQUENTIAL",
                    "sizeMode": "VIEWPORT_RATIO",
                    "widthRatio": 0.55,
                    "heightRatio": 0.75,
                    "minimumWidth": 620,
                    "minimumHeight": 520,
                    "maximumWidth": 900,
                    "maximumHeight": 900,
                    "showHeader": false,
                    "showCloseButton": true,
                    "showFooter": false,
                    "content": {
                      "surveyTitle": "교육 만족도 설문",
                      "description": "시연 화면 확인을 위해 아래 문항에 응답해주세요.",
                      "questions": [
                        {
                          "questionId": 1001,
                          "title": "화면 구성이 이해하기 쉬웠나요?",
                          "questionType": "RATING5",
                          "isRequired": true
                        },
                        {
                          "questionId": 1002,
                          "title": "가장 유용한 팝업 유형을 선택해주세요.",
                          "questionType": "SINGLE_CHOICE",
                          "isRequired": true,
                          "options": [
                            { "optionId": 1101, "value": "TEXT", "text": "텍스트" },
                            { "optionId": 1102, "value": "IMAGE", "text": "이미지" },
                            { "optionId": 1103, "value": "VIDEO", "text": "영상" },
                            { "optionId": 1104, "value": "SURVEY", "text": "설문" }
                          ]
                        },
                        {
                          "questionId": 1003,
                          "title": "추가 의견을 작성해주세요.",
                          "questionType": "TEXT",
                          "isRequired": false
                        }
                      ]
                    }
                  },
                  {
                    "popupId": "DEMO-QUIZ-001",
                    "popupType": "QUIZ",
                    "title": "정보보안 교육 평가",
                    "displayMode": "SEQUENTIAL",
                    "sizeMode": "VIEWPORT_RATIO",
                    "widthRatio": 0.55,
                    "heightRatio": 0.75,
                    "minimumWidth": 620,
                    "minimumHeight": 520,
                    "maximumWidth": 900,
                    "maximumHeight": 900,
                    "showHeader": false,
                    "showCloseButton": true,
                    "showFooter": false,
                    "content": {
                      "surveyTitle": "정보보안 교육 평가",
                      "description": "두 문항을 모두 맞히면 통과합니다.",
                      "passingScore": 100,
                      "questions": [
                        {
                          "questionId": 2001,
                          "title": "개인정보에 해당하는 것은?",
                          "questionType": "SINGLE_CHOICE",
                          "isRequired": true,
                          "isScored": true,
                          "correctAnswers": ["PHONE"],
                          "options": [
                            { "optionId": 2101, "value": "PHONE", "text": "휴대전화 번호" },
                            { "optionId": 2102, "value": "WEATHER", "text": "오늘의 날씨" }
                          ]
                        },
                        {
                          "questionId": 2002,
                          "title": "안전한 비밀번호 관리 방법을 모두 선택하세요.",
                          "questionType": "MULTIPLE_CHOICE",
                          "isRequired": true,
                          "isScored": true,
                          "correctAnswers": ["LONG", "MFA"],
                          "options": [
                            { "optionId": 2201, "value": "LONG", "text": "충분히 긴 비밀번호 사용" },
                            { "optionId": 2202, "value": "REUSE", "text": "모든 사이트에서 재사용" },
                            { "optionId": 2203, "value": "MFA", "text": "다중 인증 사용" }
                          ]
                        }
                      ]
                    }
                  }
                ]
                """;

            return JsonSerializer.Deserialize<List<PopupResponseDto>>(
                       demoJson,
                       JsonOptions)
                   ?? throw new InvalidOperationException(
                       "Demo Mode 샘플 데이터를 읽지 못했습니다.");
        }
    }
}
