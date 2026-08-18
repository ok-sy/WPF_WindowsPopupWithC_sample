package com.oksy.popup.domain;

import java.time.LocalDateTime;

/**
 * POPUP_OPTION 테이블의 선택지 한 건이다.
 * correctYn은 서버 채점에만 사용하며 PopupOptionDto로 변환할 때 제외한다.
 */
public record PopupOptionEntity(
        Long optionId,
        Long questionId,
        String optionValue,
        String optionText,
        String correctYn,
        Integer sortOrder,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt
) {
}
