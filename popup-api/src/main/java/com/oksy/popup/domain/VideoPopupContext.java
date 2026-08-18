package com.oksy.popup.domain;

import java.math.BigDecimal;

/**
 * 영상 시청 완료 판정에 필요한 설정만 조회한 경량 객체다.
 * Service가 popupType을 확인하고 completionRatio와 실제 시청률을 비교한다.
 */
public record VideoPopupContext(
        String popupId,
        String popupType,
        BigDecimal completionRatio
) {
}
