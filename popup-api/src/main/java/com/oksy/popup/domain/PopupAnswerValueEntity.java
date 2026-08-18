package com.oksy.popup.domain;

import java.time.LocalDateTime;

/**
 * POPUP_RESPONSE_VALUE 테이블의 선택값 한 건이다.
 * 한 답안에서 선택한 optionId와 당시 optionValue를 함께 보존한다.
 */
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
