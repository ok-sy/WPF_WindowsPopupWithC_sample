namespace Popup.Dtos
{
    /*
     * TEXT 팝업의 content 영역을 담는 DTO
     *
     * 서버 JSON의 content 내부 값과
     * 동일한 구조로 작성한다.
     */
    public class TextPopupContentDto
    {
        /*
         * TEXT 팝업 본문 내부에 표시할 제목
         *
         * PopupWindow 공통 Header 제목과는 별개다.
         */
        public string ContentTitle { get; set; } =
            string.Empty;

        /*
         * 제목 아래에 표시할 간단한 설명
         */
        public string Description { get; set; } =
            string.Empty;

        /*
         * 왼쪽 카드 제목
         */
        public string LeftSectionTitle { get; set; } =
            string.Empty;

        /*
         * 왼쪽 카드 일반 본문
         */
        public string LeftSectionBody { get; set; } =
            string.Empty;

        /*
         * 왼쪽 카드 안에 표시할 강조 문구
         */
        public string HighlightText { get; set; } =
            string.Empty;

        /*
         * 오른쪽 카드 제목
         */
        public string RightSectionTitle { get; set; } =
            string.Empty;

        /*
         * 오른쪽 카드 일반 본문
         */
        public string RightSectionBody { get; set; } =
            string.Empty;

        /*
         * 오른쪽 카드 하단에 표시할 추가 설명
         */
        public string AdditionalDescription { get; set; } =
            string.Empty;
    }
}