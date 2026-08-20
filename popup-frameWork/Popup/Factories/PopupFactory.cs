using Popup.Dtos;
using Popup.Models;
using Popup.Views.Contents;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows;

namespace Popup.Factories
{
    public static class PopupFactory
    {
        /*
         * content 내부 JSON을 종류별 DTO로 변환할 때
         * camelCase와 PascalCase를 모두 허용한다.
         */
        private static readonly JsonSerializerOptions
            JsonOptions =
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
      

        /*
         * 공통 DTO를 받아
         * 팝업 종류에 맞는 View와 PopupOptions를 생성한다.
         */
        public static PopupOptions Create(
            PopupResponseDto popupDto)
        {
            if (popupDto == null)
            {
                throw new ArgumentNullException(
                    nameof(popupDto));
            }

            /*
             * popupType에 따라
             * 실제 본문 View를 생성한다.
             */
            FrameworkElement content =
            popupDto.PopupType
                .Trim()
                .ToUpperInvariant() switch
            {
                "TEXT" =>
                    CreateTextPopupView(
                        popupDto.Content),

                "IMAGE" =>
                    CreateImagePopupView(
                        popupDto.Content),

                "VIDEO" =>
                    CreateVideoPopupView(
                        popupDto.Content),

                "SURVEY" =>
                    CreateSurveyPopupView(
                        popupDto.Content,
                        isQuizMode: false),

                "QUIZ" =>
                    CreateSurveyPopupView(
                        popupDto.Content,
                        isQuizMode: true),

                _ =>
                    throw new NotSupportedException(
                        $"지원하지 않는 팝업 종류입니다: " +
                        $"{popupDto.PopupType}")
            };

            /*
             * 서버 DTO의 공통 설정을
             * PopupOptions로 변환한다.
             */
            return new PopupOptions
            {
                PopupId = popupDto.PopupId,

                Title =
                    popupDto.Title,

                Content =
                    content,

                DisplayMode =
                ConvertPopupDisplayMode(
                    popupDto.DisplayMode),

                ShowHeader =
                    popupDto.ShowHeader,

                ShowCloseButton =
                    popupDto.ShowCloseButton,

                ShowFooter =
                    popupDto.ShowFooter,

                ShowDoNotShowAgain =
                    popupDto.ShowDoNotShowAgain,

                CompletionRatio =
                    popupDto.CompletionRatio
                    ?? 1.0,

                AllowCloseBeforeComplete =
                    popupDto.AllowCloseBeforeComplete,

                SizeMode =
                    ConvertPopupSizeMode(
                        popupDto.SizeMode),

                Width =
                    popupDto.Width,

                Height =
                    popupDto.Height,

                WidthRatio =
                    popupDto.WidthRatio,

                HeightRatio =
                    popupDto.HeightRatio,

                MinimumWidth =
                    popupDto.MinimumWidth,

                MinimumHeight =
                    popupDto.MinimumHeight,

                MaximumWidth =
                    popupDto.MaximumWidth,

                MaximumHeight =
                    popupDto.MaximumHeight
            };
        }

        /*
         * TEXT content JSON을
         * TextPopupContentDto로 변환하고
         * TextPopupView를 생성한다.
         */
        private static TextPopupView CreateTextPopupView(
            JsonElement contentJson)
        {
        TextPopupContentDto contentDto =
         contentJson.Deserialize<TextPopupContentDto>(
             JsonOptions)
         ?? throw new InvalidOperationException(
             "TEXT 팝업 content 변환에 실패했습니다.");

        return new TextPopupView(
                contentDto.ContentTitle,
                contentDto.Description,
                contentDto.LeftSectionTitle,
                contentDto.LeftSectionBody,
                contentDto.HighlightText,
                contentDto.RightSectionTitle,
                contentDto.RightSectionBody,
                contentDto.AdditionalDescription,
                contentDto.ShowHighlight
                    ?? !string.IsNullOrWhiteSpace(contentDto.HighlightText),
                contentDto.ShowRightSection
                    ?? (!string.IsNullOrWhiteSpace(contentDto.RightSectionTitle)
                        || !string.IsNullOrWhiteSpace(contentDto.RightSectionBody)
                        || !string.IsNullOrWhiteSpace(contentDto.AdditionalDescription)),
                contentDto.BottomDescription);
        }

        /*
         * IMAGE content JSON을
         * ImagePopupContentDto로 변환하고
         * ImagePopupView를 생성한다.
         */
        private static ImagePopupView CreateImagePopupView(
            JsonElement contentJson)
        {
        ImagePopupContentDto contentDto =
            contentJson.Deserialize<ImagePopupContentDto>(
                JsonOptions)
            ?? throw new InvalidOperationException(
                "IMAGE 팝업 content 변환에 실패했습니다.");

        /*
         * DTO에서는 이미지 크기 모드를 문자열로 받으므로
         * 내부 enum으로 변환한다.
         */
        ImagePopupSizeMode imageSizeMode =
                ConvertImagePopupSizeMode(
                    contentDto.ImageSizeMode);

            /*
             * JSON에서 0이 전달되면
             * 크기를 직접 지정하지 않은 것으로 처리한다.
             */
            double? imageWidth =
                contentDto.ImageWidth > 0
                    ? contentDto.ImageWidth
                    : null;

            double? imageHeight =
                contentDto.ImageHeight > 0
                    ? contentDto.ImageHeight
                    : null;

            return new ImagePopupView(
                imageTitle:
                    contentDto.ImageTitle,

                imagePath:
                    contentDto.ImageUrl,

                imageDescription:
                    contentDto.Description,

                showDescription:
                    contentDto.ShowDescription,

                sizeMode:
                    imageSizeMode,

                imageWidth:
                    imageWidth,

                imageHeight:
                    imageHeight);
        }

        /*
         * VIDEO content JSON을
         * VideoPopupContentDto로 변환하고
         * VideoPopupView를 생성한다.
         */
        private static VideoPopupView CreateVideoPopupView(
            JsonElement contentJson)
        {
            VideoPopupContentDto contentDto =
                contentJson.Deserialize<VideoPopupContentDto>(
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    "VIDEO 팝업 content 변환에 실패했습니다.");

            /*
             * 현재 VideoPopupView 생성자가 지원하는 값부터 연결한다.
             *
             * 나머지 옵션은 이후 VideoPopupView에
             * 속성이나 생성자 인자를 추가한 뒤 연결한다.
             */
            return new VideoPopupView(
                videoTitle:
                    contentDto.VideoTitle,

                videoPath:
                    contentDto.VideoUrl,

                videoDescription:
                    contentDto.Description,

                showDescription:
                    contentDto.ShowDescription);
        }

         /*
         * SURVEY 또는 QUIZ content JSON을
         * SurveyPopupContentDto로 변환하고
         * SurveyPopupView를 생성한다.
         */
        private static SurveyPopupView CreateSurveyPopupView(
            JsonElement contentJson,
            bool isQuizMode)
        {
            SurveyPopupContentDto contentDto =
                contentJson.Deserialize<SurveyPopupContentDto>(
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    "SURVEY 또는 QUIZ content 변환에 실패했습니다.");

            List<SurveyQuestion> questions =
                new List<SurveyQuestion>();

            foreach (SurveyQuestionDto questionDto
                     in contentDto.Questions)
            {
                SurveyQuestion question =
                    new SurveyQuestion
                    {
                        QuestionId =
                            questionDto.QuestionId,

                        Title =
                            questionDto.Title,

                        Description =
                            questionDto.Description,

                        QuestionType =
                            ConvertSurveyQuestionType(
                                questionDto.QuestionType),

                        IsRequired =
                            questionDto.IsRequired,

                        IsScored =
                            questionDto.IsScored,

                        CorrectAnswers =
                            new List<string>(
                                questionDto.CorrectAnswers)
                    };

                foreach (SurveyOptionDto optionDto
                         in questionDto.Options)
                {
                    question.Options.Add(
                        new SurveyOption
                        {
                            OptionId =
                                optionDto.OptionId,

                            Value =
                                optionDto.Value,

                            Text =
                                optionDto.Text
                        });
                }

                questions.Add(question);
            }

            return new SurveyPopupView(
                title:
                    contentDto.SurveyTitle,

                description:
                    contentDto.Description,

                questions:
                    questions,

                isQuizMode:
                    isQuizMode,

                passingScore:
                    contentDto.PassingScore);
        }

        /*
         * 서버에서 받은 질문 유형 문자열을
         * SurveyQuestionType enum으로 변환한다.
         */
        private static SurveyQuestionType ConvertSurveyQuestionType(
            string questionType)
        {
            return questionType
                .Trim()
                .ToUpperInvariant() switch
            {
                "RATING5" =>
                    SurveyQuestionType.Rating5,

                "SINGLE_CHOICE" =>
                    SurveyQuestionType.SingleChoice,

                "MULTIPLE_CHOICE" =>
                    SurveyQuestionType.MultipleChoice,

                "TEXT" =>
                    SurveyQuestionType.Text,

                _ =>
                    throw new ArgumentException(
                        $"지원하지 않는 설문 질문 유형입니다: " +
                        $"{questionType}")
            };
        }

        /*
         * 서버에서 받은 이미지 크기 모드 문자열을
         * ImagePopupSizeMode enum으로 변환한다.
         */
        private static ImagePopupSizeMode ConvertImagePopupSizeMode(
            string imageSizeMode)
        {
            return imageSizeMode
                .Trim()
                .ToUpperInvariant() switch
            {
                /*
                 * 이미지 영역이 현재 팝업 크기에 맞춰
                 * 유동적으로 배치되는 방식
                 */
                "ADAPTIVE" =>
                    ImagePopupSizeMode.Adaptive,

                /*
                 * 이미지 원본 크기 또는 요청 크기에 맞춰
                 * 팝업 권장 크기를 계산하는 방식
                 */
                "FIT_TO_IMAGE" =>
                    ImagePopupSizeMode.FitToImage,

                /*
                 * DTO 기본값이 FIXED로 되어 있었다면
                 * 현재 내부 enum에는 Fixed가 없을 가능성이 있으므로
                 * Adaptive로 연결한다.
                 */
                "FIXED" =>
                    ImagePopupSizeMode.Adaptive,

                _ =>
                    throw new ArgumentException(
                        $"지원하지 않는 이미지 크기 방식입니다: " +
                        $"{imageSizeMode}")
            };
        }

        /*
         * 서버에서 받은 팝업 표시 방식 문자열을
         * 프로그램 내부 enum으로 변환한다.
         */
        private static PopupDisplayMode ConvertPopupDisplayMode(
            string displayMode)
        {
            return displayMode
                .Trim()
                .ToUpperInvariant() switch
            {
                "SEQUENTIAL" =>
                    PopupDisplayMode.Sequential,

                "SIMULTANEOUS" =>
                    PopupDisplayMode.Simultaneous,

                _ =>
                    throw new ArgumentException(
                        $"지원하지 않는 팝업 표시 방식입니다: " +
                        $"{displayMode}")
            };
        }

        /*
         * 서버에서 받은 문자열 크기 모드를
         * 프로그램 내부 enum으로 변환한다.
         */
        private static PopupSizeMode ConvertPopupSizeMode(
            string sizeMode)
        {
            return sizeMode
                .Trim()
                .ToUpperInvariant() switch
            {
                "FIXED" =>
                    PopupSizeMode.Fixed,

                "VIEWPORT_RATIO" =>
                    PopupSizeMode.ViewportRatio,

                "FULLSCREEN" =>
                    PopupSizeMode.Fullscreen,

                "AUTO" =>
                    PopupSizeMode.Auto,

                _ =>
                    throw new ArgumentException(
                        $"지원하지 않는 팝업 크기 방식입니다: " +
                        $"{sizeMode}")
            };
        }
    }

}
