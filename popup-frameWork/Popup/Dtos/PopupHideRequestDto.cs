namespace Popup.Dtos
{
    /*
     * 사용자가 특정 팝업을
     * 일정 기간 보지 않도록 설정할 때
     * Java 서버로 전달하는 요청 DTO다.
     */
    public class PopupHideRequestDto
    {
        /*
         * 팝업을 숨긴 사용자 ID
         */
        public string UserId { get; set; } =
            string.Empty;

        /*
         * 팝업을 숨길 기간
         *
         * 예:
         * 30
         * → 현재 시점부터 30일 동안 숨김
         */
        public int HideDays { get; set; } =
            30;
    }
}