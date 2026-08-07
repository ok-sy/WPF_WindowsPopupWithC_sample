package com.oksy.popup.dto;

import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

/*
 * 사용자가 팝업을 일정 기간 숨길 때
 * WPF에서 Java 서버로 보내는 요청 DTO다.
 *
 * 현재는 로그인 기능이 없으므로
 * 테스트용 userId를 요청에서 직접 받는다.
 *
 * 운영 환경에서는 인증 정보에서 사용자 ID를 구하고
 * 요청 DTO에서는 userId를 제거할 예정이다.
 */
public record PopupHideRequestDto(

        /*
         * 팝업을 숨길 사용자 ID다.
         *
         * 빈 문자열이나 null은 허용하지 않는다.
         */
        @NotBlank(
                message = "사용자 ID는 필수입니다.")
        String userId,

        /*
         * 팝업을 숨길 일수다.
         *
         * 30일간 보지 않기:
         * → 30
         *
         * 최소 1일부터 최대 3650일까지 허용한다.
         */
        @NotNull(
                message = "숨김 일수는 필수입니다.")
        @Min(
                value = 1,
                message = "숨김 일수는 1일 이상이어야 합니다.")
        @Max(
                value = 3650,
                message = "숨김 일수는 3650일 이하여야 합니다.")
        Integer hideDays

) {
}