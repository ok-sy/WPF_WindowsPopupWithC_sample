package com.oksy.popup.dto;

import java.time.OffsetDateTime;

/** 팝업 이벤트 저장 결과다. */
public record PopupEventResponseDto(
        String userId,
        String popupId,
        String eventType,
        OffsetDateTime recordedAt
) {
}
