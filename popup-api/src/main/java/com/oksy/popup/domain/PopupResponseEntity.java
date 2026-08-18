package com.oksy.popup.domain;

import java.math.BigDecimal;
import java.time.LocalDateTime;

/**
 * POPUP_RESPONSE 테이블의 사용자별 설문 제출 결과 한 건이다.
 * 중복 요청 식별자, 제출 상태, 총점과 통과 여부를 저장하는 응답 헤더 역할이다.
 */
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
