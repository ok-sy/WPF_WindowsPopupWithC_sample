using System.Windows;
using System;
using System.Threading.Tasks;
using System.Windows;


namespace Popup.Models
{
    /*
     * 팝업 창의 크기를 결정하는 방식이다.
     */
    public enum PopupSizeMode
    {
        /*
         * 기존 방식이다.
         *
         * Width와 Height 값을 그대로 사용한다.
         */
        Fixed,

        /*
         * 모니터의 사용 가능한 작업 영역을 기준으로
         * WidthRatio와 HeightRatio를 곱해 계산한다.
         */
        ViewportRatio,

        /*
         * 팝업 안의 콘텐츠 크기에 맞춰
         * Window 크기를 자동으로 결정한다.
         */
        Auto
    }

    /*
     * 여러 팝업이 조회됐을 때
     * 팝업을 표시하는 방식이다.
     */
    public enum PopupDisplayMode
    {
        /*
         * 앞 팝업이 닫힌 뒤
         * 다음 팝업을 표시한다.
         */
        Sequential,

        /*
         * 기존 팝업의 종료 여부와 관계없이
         * 즉시 화면에 표시한다.
         */
        Simultaneous
    }

    public class PopupOptions
    {

        /*
         * 팝업 고유 ID
         *
         * PopupResponseDto에서 전달받아
         * "30일간 보지 않기" 등의
         * 사용자 설정 저장에 사용한다.
         */
        public string PopupId { get; set; } = string.Empty;

        /*
         * "30일간 보지 않기"를 저장할 때
         * 호출할 비동기 함수다.
         *
         * 첫 번째 값
         * → 숨길 PopupId
         *
         * 두 번째 값
         * → 숨길 일수
         *
         * PopupOptions는 서버 주소나 사용자 ID를
         * 직접 알지 않고 저장 함수만 전달받는다.
         */
        public Func<string, int, Task>?
            HidePopupAsync
        { get; set; }

        /*
         * 팝업 상단에 표시할 제목
         */
        public string Title { get; set; } =
            string.Empty;

        /*
         * PopupWindow 안에 삽입할 실제 본문
         *
         * TextPopupView, ImagePopupView 등
         * 다양한 FrameworkElement를 받을 수 있다.
         */
        public FrameworkElement? Content { get; set; }

        /*
         * 여러 팝업을 표시하는 방식
         *
         * 기본값은 팝업이 겹치지 않도록
         * 순차 표시로 설정한다.
         */
        public PopupDisplayMode DisplayMode { get; set; } =
            PopupDisplayMode.Sequential;

        /*
         * 팝업 상단 Header 전체 표시 여부
         *
         * true
         * → 제목과 상단 X 닫기 버튼 영역을 표시한다.
         *
         * false
         * → Header 전체를 숨기고
         *   Header가 사용하던 높이도 제거한다.
         */
        public bool ShowHeader { get; set; } =
            true;

        /*
         * 최상단 X 닫기 버튼 표시 여부
         *
         * ShowHeader가 true일 때만 의미가 있다.
         */
        public bool ShowCloseButton { get; set; } =
            true;

        /*
         * 하단 Footer 표시 여부
         */
        public bool ShowFooter { get; set; } =
            true;

        /*
         * '30일간 보지 않기' 같은
         * 다시 보지 않기 영역 표시 여부
         */
        public bool ShowDoNotShowAgain { get; set; } =
            true;

        /*
         * 팝업 크기 계산 방식
         *
         * 기본값은 기존 동작을 유지하기 위해
         * Fixed로 설정한다.
         */
        public PopupSizeMode SizeMode { get; set; } =
            PopupSizeMode.Fixed;

        /*
         * Fixed 모드에서 사용하는 고정 너비
         */
        public double Width { get; set; } =
            900;

        /*
         * Fixed 모드에서 사용하는 고정 높이
         */
        public double Height { get; set; } =
            620;

        /*
         * ViewportRatio 모드에서 사용하는 화면 너비 비율
         *
         * 0.7
         * → 작업 영역 너비의 70%
         */
        public double WidthRatio { get; set; } =
            0.7;

        /*
         * ViewportRatio 모드에서 사용하는 화면 높이 비율
         *
         * 0.75
         * → 작업 영역 높이의 75%
         */
        public double HeightRatio { get; set; } =
            0.75;

        /*
         * 동적으로 계산된 팝업의 최소 너비
         */
        public double MinimumWidth { get; set; } =
            480;

        /*
         * 동적으로 계산된 팝업의 최소 높이
         */
        public double MinimumHeight { get; set; } =
            320;

        /*
         * 동적으로 계산된 팝업의 최대 너비
         */
        public double MaximumWidth { get; set; } =
            1200;

        /*
         * 동적으로 계산된 팝업의 최대 높이
         */
        public double MaximumHeight { get; set; } =
            900;
    }
}