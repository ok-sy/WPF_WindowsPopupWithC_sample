using Popup.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Popup.Views.Contents
{
    /// <summary>
    /// 설문 질문을 화면에 표시하는 사용자 컨트롤
    /// </summary>
    public partial class SurveyPopupView : UserControl
    {
        // 현재 화면에 표시된 질문 목록
        private readonly List<SurveyQuestion> _questions = new();

        /// <summary>
        /// Visual Studio 미리보기와 기본 생성을 위한 생성자
        /// </summary>
        public SurveyPopupView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 실제 설문 데이터를 받아 화면을 만드는 생성자
        /// </summary>
        public SurveyPopupView(
            string title,
            string description,
            List<SurveyQuestion> questions)
        {
            InitializeComponent();

            SurveyTitleText.Text = title;
            SurveyDescriptionText.Text = description;

            _questions = questions ?? new List<SurveyQuestion>();

            BuildQuestions();
        }

        /// <summary>
        /// 질문 목록을 화면에 순서대로 추가한다.
        /// </summary>
        private void BuildQuestions()
        {
            QuestionListPanel.Children.Clear();

            for (int index = 0; index < _questions.Count; index++)
            {
                SurveyQuestion question = _questions[index];

                Border questionCard = CreateQuestionCard(
                    question,
                    index + 1);

                QuestionListPanel.Children.Add(questionCard);
            }
        }

        /// <summary>
        /// 질문 하나의 카드 영역을 만든다.
        /// </summary>
        private Border CreateQuestionCard(
            SurveyQuestion question,
            int questionNumber)
        {
            Border cardBorder = new Border
            {
                Margin = new Thickness(0, 0, 0, 16),
                Padding = new Thickness(18),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(
                    Color.FromRgb(229, 231, 235)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };

            StackPanel cardPanel = new StackPanel();

            TextBlock titleText = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(17, 24, 39)),
                TextWrapping = TextWrapping.Wrap
            };

            string requiredMark = question.IsRequired
                ? " *"
                : string.Empty;

            titleText.Text =
                $"{questionNumber}. {question.Title}{requiredMark}";

            cardPanel.Children.Add(titleText);

            if (!string.IsNullOrWhiteSpace(question.Description))
            {
                TextBlock descriptionText = new TextBlock
                {
                    Margin = new Thickness(0, 6, 0, 0),
                    FontSize = 13,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(107, 114, 128)),
                    Text = question.Description,
                    TextWrapping = TextWrapping.Wrap
                };

                cardPanel.Children.Add(descriptionText);
            }

            FrameworkElement answerControl =
                CreateAnswerControl(question);

            cardPanel.Children.Add(answerControl);

            cardBorder.Child = cardPanel;

            return cardBorder;
        }

        /// <summary>
        /// 질문 종류에 맞는 입력 화면을 만든다.
        /// </summary>
        private FrameworkElement CreateAnswerControl(
            SurveyQuestion question)
        {
            switch (question.QuestionType)
            {
                case SurveyQuestionType.Rating5:
                    return CreateRating5Control(question);

                case SurveyQuestionType.SingleChoice:
                    return CreateSingleChoiceControl(question);

                case SurveyQuestionType.MultipleChoice:
                    return CreateMultipleChoiceControl(question);

                case SurveyQuestionType.Text:
                    return CreateTextControl(question);

                default:
                    return new TextBlock
                    {
                        Margin = new Thickness(0, 14, 0, 0),
                        Text = "지원하지 않는 질문 형식입니다.",
                        Foreground = Brushes.Red
                    };
            }
        }

        /// <summary>
        /// 5점 평가 문항을 만든다.
        /// </summary>
        private FrameworkElement CreateRating5Control(
            SurveyQuestion question)
        {
            StackPanel ratingPanel = new StackPanel
            {
                Margin = new Thickness(0, 16, 0, 0),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            List<SurveyOption> ratingOptions =
                GetRating5Options(question);

            foreach (SurveyOption option in ratingOptions)
            {
                RadioButton radioButton = new RadioButton
                {
                    Margin = new Thickness(10, 0, 10, 0),
                    VerticalContentAlignment =
                        VerticalAlignment.Center,

                    Content = option.Text,

                    // 나중에 응답을 수집할 때 사용할 실제 값
                    Tag = option.Value,

                    // 질문마다 다른 그룹 이름을 사용한다.
                    GroupName = $"Question_{question.QuestionId}"
                };

                ratingPanel.Children.Add(radioButton);
            }

            return ratingPanel;
        }

        /// <summary>
        /// 일반 단일 선택 문항을 만든다.
        /// </summary>
        private FrameworkElement CreateSingleChoiceControl(
            SurveyQuestion question)
        {
            StackPanel optionPanel = new StackPanel
            {
                Margin = new Thickness(0, 14, 0, 0)
            };

            foreach (SurveyOption option in question.Options)
            {
                RadioButton radioButton = new RadioButton
                {
                    Margin = new Thickness(0, 0, 0, 10),
                    Content = option.Text,
                    Tag = option.Value,
                    GroupName = $"Question_{question.QuestionId}"
                };

                optionPanel.Children.Add(radioButton);
            }

            return optionPanel;
        }

        /// <summary>
        /// 복수 선택 문항을 만든다.
        /// </summary>
        private FrameworkElement CreateMultipleChoiceControl(
            SurveyQuestion question)
        {
            StackPanel optionPanel = new StackPanel
            {
                Margin = new Thickness(0, 14, 0, 0)
            };

            foreach (SurveyOption option in question.Options)
            {
                CheckBox checkBox = new CheckBox
                {
                    Margin = new Thickness(0, 0, 0, 10),
                    Content = option.Text,
                    Tag = option.Value
                };

                optionPanel.Children.Add(checkBox);
            }

            return optionPanel;
        }

        /// <summary>
        /// 주관식 입력 문항을 만든다.
        /// </summary>
        private FrameworkElement CreateTextControl(
            SurveyQuestion question)
        {
            TextBox textBox = new TextBox
            {
                Margin = new Thickness(0, 14, 0, 0),
                MinHeight = 90,
                Padding = new Thickness(10),
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility =
                    ScrollBarVisibility.Auto,

                // 어떤 질문의 TextBox인지 구분하기 위한 값
                Tag = question.QuestionId
            };

            return textBox;
        }

        /// <summary>
        /// Rating5의 기본 보기 다섯 개를 반환한다.
        ///
        /// 질문에 직접 Options가 들어 있다면
        /// 직접 전달받은 Options를 우선 사용한다.
        /// </summary>
        private List<SurveyOption> GetRating5Options(
            SurveyQuestion question)
        {
            if (question.Options != null &&
                question.Options.Count > 0)
            {
                return question.Options;
            }

            return new List<SurveyOption>
            {
                new SurveyOption
                {
                    Value = "1",
                    Text = "매우 좋지 않음"
                },
                new SurveyOption
                {
                    Value = "2",
                    Text = "좋지 않음"
                },
                new SurveyOption
                {
                    Value = "3",
                    Text = "보통"
                },
                new SurveyOption
                {
                    Value = "4",
                    Text = "좋음"
                },
                new SurveyOption
                {
                    Value = "5",
                    Text = "매우 좋음"
                }
            };
        }

        private void SubmitButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            // 다음 단계에서 필수 문항 검사와 응답 수집을 구현한다.
        }
    }
}