package com.oksy.popup.dto;

/**
 * WPF가 선택형 문항을 그릴 때 사용하는 선택지다.
 * 서버 정답 여부는 의도적으로 포함하지 않아 네트워크 응답에서 정답이 노출되지 않는다.
 */
public record PopupOptionDto(
        Long optionId,
        String value,
        String text,
        int sortOrder
) {
}
