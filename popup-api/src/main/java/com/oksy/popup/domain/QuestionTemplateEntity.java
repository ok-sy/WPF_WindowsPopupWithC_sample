package com.oksy.popup.domain;

import java.time.LocalDateTime;

/**
 * 재사용 가능한 질문 템플릿의 특정 버전 한 건이다.
 * 같은 templateGroupId 안에서 버전을 관리하고 currentYn으로 현재 버전을 구분한다.
 */
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
