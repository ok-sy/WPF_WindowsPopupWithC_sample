using System.Windows;
using System;
using System.Threading.Tasks;


namespace Popup.Models
{
    /*
     * 팝업 창의 크기를 결정하는 방식이다.
     */
    public enum PopupSizeMode
    {
        Fixed,
        ViewportRatio,
        Fullscreen,
        Auto
    }

    /*
     * 여러 팝업이 조회됐을 때
     * 팝업을 표시하는 방식이다.
     */
    public enum PopupDisplayMode
    {
        /*
         * 같은 우선순위 그룹 안에서도
         * 한 개씩 차례대로 표시한다.
         */
        Sequential,

        /*
         * 같은 DisplayOrder 그룹의 팝업들과
         * 동시에 화면에 표시한다.
         */
        Simultaneous
    }

    public class PopupOptions
    {
        public string PopupId { get; set; } = string.Empty;

        public Func<string, int, Task>?
            HidePopupAsync
        { get; set; }

        public Func<string, Task>?
            PopupDisplayedAsync
        { get; set; }

        public Func<string, Task>?
            PopupClosedAsync
        { get; set; }

        public Func<string, List<SurveyAnswer>, Task>?
            SubmitSurveyAsync
        { get; set; }

        public Func<string, VideoProgressSnapshot, Task<bool>>?
            SaveVideoProgressAsync
        { get; set; }

        public double CompletionRatio { get; set; } =
            1.0;

        public bool AllowCloseBeforeComplete { get; set; } =
            true;

        public bool IsCompleted { get; set; }

        public string Title { get; set; } =
            string.Empty;

        public FrameworkElement? Content { get; set; }

        /*
         * 같은 DisplayOrder 그룹 안에서
         * 팝업을 순차 또는 동시로 표시할지 결정한다.
         */
        public PopupDisplayMode DisplayMode { get; set; } =
            PopupDisplayMode.Sequential;

        /*
         * 팝업 표시 우선순위다.
         *
         * 1 → 가장 먼저 표시
         * 2 → 1번 그룹이 모두 닫힌 뒤 표시
         *
         * 같은 숫자의 팝업은 하나의 그룹으로 처리한다.
         */
        public int DisplayOrder { get; set; } =
            100;

        public bool ShowHeader { get; set; } =
            true;

        public bool ShowCloseButton { get; set; } =
            true;

        public bool ShowFooter { get; set; } =
            true;

        public bool ShowDoNotShowAgain { get; set; } =
            true;

        public PopupSizeMode SizeMode { get; set; } =
            PopupSizeMode.Fixed;

        public double Width { get; set; } =
            900;

        public double Height { get; set; } =
            620;

        public double WidthRatio { get; set; } =
            0.7;

        public double HeightRatio { get; set; } =
            0.75;

        public double MinimumWidth { get; set; } =
            480;

        public double MinimumHeight { get; set; } =
            320;

        public double MaximumWidth { get; set; } =
            1200;

        public double MaximumHeight { get; set; } =
            900;
    }
}
