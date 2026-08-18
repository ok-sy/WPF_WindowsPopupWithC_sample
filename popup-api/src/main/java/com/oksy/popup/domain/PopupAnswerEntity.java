package com.oksy.popup.domain;

import java.math.BigDecimal;
import java.time.LocalDateTime;

/** POPUP_RESPONSE_ANSWER 테이블 한 건을 나타낸다. */
public record PopupAnswerEntity(
        Long responseAnswerId,
        Long responseId,
        Long questionId,
        String textAnswer,
        BigDecimal earnedScore,
        String correctYn,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt
) {
}
