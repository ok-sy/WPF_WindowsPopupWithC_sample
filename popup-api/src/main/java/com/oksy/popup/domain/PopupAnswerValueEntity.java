package com.oksy.popup.domain;

import java.time.LocalDateTime;

/** POPUP_RESPONSE_VALUE 테이블 한 건을 나타낸다. */
public record PopupAnswerValueEntity(
        Long responseValueId,
        Long responseAnswerId,
        Long optionId,
        String selectedValue,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt
) {
}
