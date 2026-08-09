using System;

namespace Popup.Dtos
{
    /*
     * 팝업 숨김 처리 후
     * Java 서버가 반환하는 결과 DTO다.
     */
    public class PopupHideResponseDto
    {
        /*
         * 숨김 처리한 사용자 ID
         */
        public string UserId { get; set; } =
            string.Empty;

        /*
         * 숨김 처리한 팝업 ID
         */
        public string PopupId { get; set; } =
            string.Empty;

        /*
         * 서버에 저장된 숨김 유형
         *
         * 현재는 DAYS 값을 사용한다.
         */
        public string HideType { get; set; } =
            string.Empty;

        /*
         * 실제 숨김 종료 일시
         */
        public DateTimeOffset HiddenUntil { get; set; }
    }
}