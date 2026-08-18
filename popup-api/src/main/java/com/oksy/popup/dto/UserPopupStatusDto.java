package com.oksy.popup.dto;

import java.time.OffsetDateTime;

/**
 * user_popup_status 조회 결과를 WPF에 전달한다.
 * 최초·최근 표시, 표시 횟수, 닫기, 숨김 만료, 완료 상태를 한 번에 확인할 수 있다.
 */
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
