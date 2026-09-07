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
        private static readonly JsonSerializerOptions
            JsonOptions =
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

        public static PopupOptions Create(
            PopupResponseDto popupDto)
        {
            if (popupDto == null)
            {
                throw new ArgumentNullException(
                    nameof(popupDto));
            }

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

                /*
                 * 서버의 displayOrder를 그대로 전달한다.
                 * 숫자가 작을수록 먼저 표시되고,
                 * 같은 숫자는 PopupManager에서 하나의 그룹으로 처리한다.
                 */
                DisplayOrder =
                    popupDto.DisplayOrder,

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
                contentDto.BottomDescription,
                contentDto.ShowContentHeader,
                contentDto.ShowPlainText,
                contentDto.PlainText,
                contentDto.ShowLeftSection
                    ?? (!string.IsNullOrWhiteSpace(contentDto.LeftSectionTitle)
                        || !string.IsNullOrWhiteSpace(contentDto.LeftSectionBody)),
                contentDto.ShowBottomDescription
                    ?? !string.IsNullOrWhiteSpace(contentDto.BottomDescription),
                contentDto.MarkdownMode,
                contentDto.MarkdownContent);
        }

        private static FrameworkElement CreateImagePopupView(
            JsonElement contentJson)
        {
            ImagePopupContentDto contentDto =
                contentJson.Deserialize<ImagePopupContentDto>(
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    "IMAGE 팝업 content 변환에 실패했습니다.");

            if (string.Equals(
                contentDto.ImageSizeMode,
                "FILL",
                StringComparison.OrdinalIgnoreCase))
            {
                return new ImageFillPopupView(
                    imagePath: contentDto.ImageUrl,
                    linkUrl: contentDto.LinkUrl);
            }

            ImagePopupSizeMode imageSizeMode =
                ConvertImagePopupSizeMode(
                    contentDto.ImageSizeMode);

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

        private static VideoPopupView CreateVideoPopupView(
            JsonElement contentJson)
        {
            VideoPopupContentDto contentDto =
                contentJson.Deserialize<VideoPopupContentDto>(
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    "VIDEO 팝업 content 변환에 실패했습니다.");

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

        private static ImagePopupSizeMode ConvertImagePopupSizeMode(
            string imageSizeMode)
        {
            return imageSizeMode
                .Trim()
                .ToUpperInvariant() switch
            {
                "ADAPTIVE" =>
                    ImagePopupSizeMode.Adaptive,

                "FIT_TO_IMAGE" =>
                    ImagePopupSizeMode.FitToImage,

                "FIXED" =>
                    ImagePopupSizeMode.Adaptive,

                _ =>
                    throw new ArgumentException(
                        $"지원하지 않는 이미지 크기 방식입니다: " +
                        $"{imageSizeMode}")
            };
        }

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
