package com.oksy.popup.domain;

import java.time.LocalDateTime;

/** POPUP_OPTION 테이블 한 건을 나타낸다. */
public record PopupOptionEntity(
        Long optionId,
        Long questionId,
        String optionValue,
        String optionText,
        String correctYn,
        Integer sortOrder,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt
) {
}
