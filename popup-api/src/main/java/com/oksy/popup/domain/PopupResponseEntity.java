package com.oksy.popup.domain;

import java.math.BigDecimal;
import java.time.LocalDateTime;

/** POPUP_RESPONSE 테이블 한 건을 나타낸다. */
public record PopupResponseEntity(
        Long responseId,
        String clientRequestId,
        String popupId,
        String employeeNo,
        Long questionTemplateId,
        String responseStatus,
        LocalDateTime responseStartedAt,
        LocalDateTime submittedAt,
        LocalDateTime receivedAt,
        BigDecimal totalScore,
        String passedYn,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt
) {
}
