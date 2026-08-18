package com.oksy.popup.domain;

import java.time.LocalDate;
import java.time.LocalDateTime;

/** POPUP_TARGET_CONDITION 테이블 한 건을 나타낸다. */
public record PopupTargetConditionEntity(
        Long targetConditionId,
        Long targetGroupId,
        String conditionType,
        String conditionOperator,
        String departmentId,
        String positionId,
        String employeeNo,
        LocalDate conditionDateValue,
        String includeChildYn,
        Integer conditionOrder,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt
) {
}
