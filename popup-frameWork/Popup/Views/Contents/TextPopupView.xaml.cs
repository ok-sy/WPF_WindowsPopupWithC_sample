using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
            string additionalDescription,
            bool showHighlight,
            bool showRightSection,
            string bottomDescription,
            bool showContentHeader,
            bool showPlainText,
            string plainText,
            bool showLeftSection,
            bool showBottomDescription,
            bool markdownMode,
            string markdownContent)
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

                ContentHeaderPanel.Visibility = showContentHeader
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                PlainTextBlock.Text = plainText;
                PlainTextBlock.Visibility = !markdownMode && showPlainText
                    ? Visibility.Visible
                    : Visibility.Collapsed;

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

                HighlightContainer.Visibility = showHighlight
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                RightSectionCard.Visibility = showRightSection
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                RightSectionSpacerColumn.Width = showRightSection
                    ? new GridLength(18)
                    : new GridLength(0);

                RightSectionColumn.Width = showRightSection
                    ? new GridLength(1, GridUnitType.Star)
                    : new GridLength(0);
                RightSectionColumn.MinWidth = showRightSection
                    ? 260
                    : 0;

                LeftSectionCard.Visibility = showLeftSection
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                LeftSectionColumn.Width = showLeftSection
                    ? new GridLength(1, GridUnitType.Star)
                    : new GridLength(0);
                LeftSectionColumn.MinWidth = showLeftSection ? 260 : 0;

                RightSectionSpacerColumn.Width = showLeftSection && showRightSection
                    ? new GridLength(18)
                    : new GridLength(0);
                ContentColumnsGrid.Visibility = !markdownMode && (showLeftSection || showRightSection)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                HighlightContainer.Visibility = !markdownMode && showHighlight
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                BottomDescriptionText.Text = bottomDescription;
                BottomDescriptionText.Visibility =
                    !showBottomDescription || string.IsNullOrWhiteSpace(bottomDescription)
                        ? Visibility.Collapsed
                        : Visibility.Visible;

                MarkdownPanel.Visibility = markdownMode
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                if (markdownMode)
                {
                    RenderMarkdown(markdownContent);
                }
            }

        private void RenderMarkdown(string markdown)
        {
            MarkdownPanel.Children.Clear();
            foreach (string sourceLine in (markdown ?? string.Empty).Replace("\r", string.Empty).Split('\n'))
            {
                string line = sourceLine.TrimEnd();
                TextBlock block = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 8),
                    FontSize = 15
                };

                string content = line;
                if (line.StartsWith("### "))
                {
                    block.FontSize = 17;
                    block.FontWeight = FontWeights.SemiBold;
                    content = line.Substring(4);
                }
                else if (line.StartsWith("## "))
                {
                    block.FontSize = 20;
                    block.FontWeight = FontWeights.Bold;
                    content = line.Substring(3);
                }
                else if (line.StartsWith("# "))
                {
                    block.FontSize = 24;
                    block.FontWeight = FontWeights.Bold;
                    content = line.Substring(2);
                }
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    content = "• " + line.Substring(2);
                    block.Margin = new Thickness(14, 0, 0, 6);
                }

                AddInlineMarkdown(block, content);
                MarkdownPanel.Children.Add(block);
            }
        }

        private static void AddInlineMarkdown(TextBlock block, string content)
        {
            int position = 0;
            while (position < content.Length)
            {
                int boldStart = content.IndexOf("**", position, StringComparison.Ordinal);
                if (boldStart < 0)
                {
                    block.Inlines.Add(new Run(content.Substring(position)));
                    break;
                }
                if (boldStart > position)
                {
                    block.Inlines.Add(new Run(content.Substring(position, boldStart - position)));
                }
                int boldEnd = content.IndexOf("**", boldStart + 2, StringComparison.Ordinal);
                if (boldEnd < 0)
                {
                    block.Inlines.Add(new Run(content.Substring(boldStart)));
                    break;
                }
                block.Inlines.Add(new Run(content.Substring(boldStart + 2, boldEnd - boldStart - 2))
                {
                    FontWeight = FontWeights.Bold
                });
                position = boldEnd + 2;
            }
        }
        }
}
