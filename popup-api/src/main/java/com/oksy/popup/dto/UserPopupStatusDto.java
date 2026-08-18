package com.oksy.popup.dto;

import java.time.OffsetDateTime;

/** 사용자별 팝업 표시·닫기·완료 상태다. */
public record UserPopupStatusDto(
        String userId,
        String popupId,
        String popupStatus,
        OffsetDateTime firstDisplayedAt,
        OffsetDateTime lastDisplayedAt,
        int displayCount,
        OffsetDateTime closedAt,
        OffsetDateTime hiddenUntilAt,
        boolean completed,
        OffsetDateTime completedAt
) {
}
