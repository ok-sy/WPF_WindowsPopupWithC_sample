package com.oksy.popup.domain;

import java.time.LocalDate;
import java.time.LocalDateTime;

/**
 * 대상 그룹 안에서 AND로 판정할 조건 한 건이다.
 * 부서·직급·사번·입사일 중 conditionType에 해당하는 값 하나를 사용한다.
 */
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
