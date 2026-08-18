package com.oksy.popup.domain;

import java.time.LocalDateTime;

/** QUESTION_TEMPLATE 테이블 한 건을 나타낸다. */
public record QuestionTemplateEntity(
        Long questionTemplateId,
        String templateGroupId,
        String templateName,
        String templateType,
        Integer templateVersion,
        String currentYn,
        String activeYn,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt
) {
}
