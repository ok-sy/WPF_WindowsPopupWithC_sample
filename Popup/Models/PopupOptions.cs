using System.Windows;

namespace Popup.Models
{
    public class PopupOptions
    {
        /*
         * 팝업 상단에 표시할 제목
         */
        public string Title { get; set; } = string.Empty;

        /*
         * PopupWindow 안에 삽입할 실제 본문
         *
         * TextPopupView, ImagePopupView 등
         * 다양한 UserControl을 받을 수 있다.
         */
        public FrameworkElement? Content { get; set; }

        /*
         * 팝업 상단 Header 전체 표시 여부
         *
         * true
         * → 제목과 상단 X 닫기 버튼 영역을 표시한다.
         *
         * false
         * → Header 전체를 숨기고,
         *   Header가 사용하던 높이까지 제거한다.
         */
        public bool ShowHeader { get; set; } = true;

        /*
         * 최상단 X 닫기 버튼 표시 여부
         *
         * ShowHeader가 true일 때만 의미가 있다.
         */
        public bool ShowCloseButton { get; set; } = true;


        /*
         * 하단 Footer 표시 여부
         */
        public bool ShowFooter { get; set; } = true;

        /*
         * '30일간 보지 않기' 같은
         * 다시 보지 않기 영역 표시 여부
         */
        public bool ShowDoNotShowAgain { get; set; } = true;

        /*
         * 팝업 너비
         */
        public double Width { get; set; } = 900;

        /*
         * 팝업 높이
         */
        public double Height { get; set; } = 620;
    }
}