package com.oksy.popup.dto;

import java.time.OffsetDateTime;
import java.util.Map;

/*
 * WPF 프로그램에 전달할
 * 팝업 한 건의 응답 정보를 담는 DTO다.
 *
 * 각 필드 이름은 WPF의 PopupResponseDto와
 * 동일한 camelCase JSON으로 변환된다.
 */
public record PopupResponseDto(

        /*
         * 팝업을 구분하는 고유 ID다.
         *
         * 30일간 보지 않기,
         * 팝업 조회 및 로그 저장에 사용한다.
         */
        String popupId,

        /*
         * 팝업 종류다.
         *
         * TEXT
         * IMAGE
         * VIDEO
         * SURVEY
         * QUIZ
         */
        String popupType,

        /*
         * PopupWindow 상단에 표시할 제목이다.
         */
        String title,

        /*
         * 팝업 노출 시작 일시다.
         *
         * null이면 시작 일시 제한 없이 표시할 수 있다.
         */
        OffsetDateTime displayStartAt,

        /*
         * 팝업 노출 종료 일시다.
         *
         * null이면 종료 일시 제한 없이 표시할 수 있다.
         */
        OffsetDateTime displayEndAt,

        /*
         * 여러 팝업을 표시하는 방식이다.
         *
         * SEQUENTIAL
         * → 순차 표시
         *
         * SIMULTANEOUS
         * → 동시 표시
         */
        String displayMode,

        /*
         * 팝업 크기를 계산하는 방식이다.
         *
         * FIXED
         * VIEWPORT_RATIO
         * AUTO
         */
        String sizeMode,

        /*
         * FIXED 방식에서 사용하는 고정 너비다.
         */
        double width,

        /*
         * FIXED 방식에서 사용하는 고정 높이다.
         */
        double height,

        /*
         * 작업 영역 대비 팝업 너비 비율이다.
         */
        double widthRatio,

        /*
         * 작업 영역 대비 팝업 높이 비율이다.
         */
        double heightRatio,

        /*
         * 팝업이 작아질 수 있는 최소 너비다.
         */
        double minimumWidth,

        /*
         * 팝업이 작아질 수 있는 최소 높이다.
         */
        double minimumHeight,

        /*
         * 팝업이 커질 수 있는 최대 너비다.
         */
        double maximumWidth,

        /*
         * 팝업이 커질 수 있는 최대 높이다.
         */
        double maximumHeight,

        /*
         * PopupWindow 상단 Header 표시 여부다.
         */
        boolean showHeader,

        /*
         * 상단 닫기 버튼 표시 여부다.
         */
        boolean showCloseButton,

        /*
         * PopupWindow 하단 Footer 표시 여부다.
         */
        boolean showFooter,

        /*
         * 30일간 보지 않기 영역 표시 여부다.
         */
        boolean showDoNotShowAgain,

        /*
         * 팝업 종류별 상세 데이터다.
         *
         * 팝업 종류마다 내부 구조가 다르므로
         * Map으로 받아서 JSON 객체 형태로 전달한다.
         *
         * TEXT
         * → contentTitle, description 등
         *
         * IMAGE
         * → imageUrl, imageTitle 등
         *
         * VIDEO
         * → videoUrl, videoTitle 등
         */
        Map<String, Object> content
) {
}