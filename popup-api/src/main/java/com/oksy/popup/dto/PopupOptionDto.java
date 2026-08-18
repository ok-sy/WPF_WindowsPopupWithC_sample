package com.oksy.popup.dto;

/** WPF에 전달할 문항 선택지다. */
public record PopupOptionDto(
        Long optionId,
        String value,
        String text,
        int sortOrder
) {
}
