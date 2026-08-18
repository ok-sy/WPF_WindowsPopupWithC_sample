package com.oksy.popup.domain;

import java.time.LocalDateTime;
import java.util.List;

/** POPUP_TARGET_GROUP 테이블과 그 조건 목록을 나타낸다. */
public record PopupTargetGroupEntity(
        Long targetGroupId,
        String popupId,
        String targetName,
        String targetDescription,
        Integer groupOrder,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt,
        List<PopupTargetConditionEntity> conditions
) {
    public PopupTargetGroupEntity {
        conditions = conditions == null ? List.of() : List.copyOf(conditions);
    }
}
