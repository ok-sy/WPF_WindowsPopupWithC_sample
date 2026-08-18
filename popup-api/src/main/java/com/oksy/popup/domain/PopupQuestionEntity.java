package com.oksy.popup.domain;

import java.math.BigDecimal;
import java.time.LocalDateTime;

/**
 * POPUP_QUESTION 테이블의 설문·퀴즈 문항 한 건이다.
 * 필수 여부, 채점 여부, 배점과 표시 순서를 보존하여 검증과 채점에 사용한다.
 */
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
