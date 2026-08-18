package com.oksy.popup.domain;

import java.math.BigDecimal;
import java.time.LocalDateTime;

/** POPUP_QUESTION 테이블 한 건을 나타낸다. */
public record PopupQuestionEntity(
        Long questionId,
        Long questionTemplateId,
        String questionType,
        String questionTitle,
        String questionDescription,
        String requiredYn,
        String scoredYn,
        BigDecimal questionScore,
        Integer sortOrder,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt
) {
}
