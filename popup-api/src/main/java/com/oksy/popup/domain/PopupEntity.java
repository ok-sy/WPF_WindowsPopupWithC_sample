package com.oksy.popup.domain;

import java.math.BigDecimal;
import java.time.OffsetDateTime;

/*
 * POPUP_NOTICE 테이블의 조회 결과 한 건을 담는 객체다.
 *
 * Oracle 컬럼의 값을 우선 원본 형태로 받아두고,
 * PopupService에서 WPF 응답용 PopupResponseDto로 변환한다.
 */
public record PopupEntity(

        /*
         * 팝업 고유 ID
         */
        String popupId,

        /*
         * TEXT, IMAGE, VIDEO, SURVEY, QUIZ 등의 팝업 종류
         */
        String popupType,

        /*
         * PopupWindow 상단 제목
         */
        String title,

        /*
         * 팝업 노출 시작 및 종료 일시
         */
        OffsetDateTime displayStartAt,
        OffsetDateTime displayEndAt,

        /*
         * SEQUENTIAL 또는 SIMULTANEOUS
         */
        String displayMode,

        /*
         * FIXED, VIEWPORT_RATIO 또는 AUTO
         */
        String sizeMode,

        /*
         * 팝업 크기 관련 값이다.
         *
         * Oracle NUMBER 컬럼을 정확하게 받기 위해
         * BigDecimal을 사용한다.
         */
        BigDecimal popupWidth,
        BigDecimal popupHeight,

        BigDecimal widthRatio,
        BigDecimal heightRatio,

        BigDecimal minimumWidth,
        BigDecimal minimumHeight,

        BigDecimal maximumWidth,
        BigDecimal maximumHeight,

        /*
         * Oracle에 저장된 Y/N 값이다.
         *
         * PopupService에서 boolean으로 변환한다.
         */
        String showHeaderYn,
        String showCloseButtonYn,
        String showFooterYn,
        String showDontShowYn,

        /*
         * CLOB에 저장된 팝업 종류별 content JSON 문자열이다.
         *
         * PopupService에서 Map<String, Object>로 변환한다.
         */
        String contentJson,

        /*
         * 팝업 표시 순서
         */
        int displayOrder,

        /*
         * 팝업 사용 여부
         */
        String useYn

) {
}