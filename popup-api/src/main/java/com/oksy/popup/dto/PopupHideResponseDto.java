package com.oksy.popup.dto;

import java.time.OffsetDateTime;

/*
 * 사용자 팝업 숨김 처리가 완료된 후
 * Java 서버가 WPF에 반환하는 응답 DTO다.
 */
public record PopupHideResponseDto(

        /*
         * 숨김 처리한 사용자 ID다.
         */
        String userId,

        /*
         * 숨김 처리한 팝업 ID다.
         */
        String popupId,

        /*
         * 숨김 방식이다.
         *
         * 현재 API에서는 UNTIL을 반환한다.
         */
        String hideType,

        /*
         * 팝업 숨김이 끝나는 일시다.
         */
        OffsetDateTime hiddenUntil

) {
}