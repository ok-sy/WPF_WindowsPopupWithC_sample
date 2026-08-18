package com.oksy.popup.domain;

import java.math.BigDecimal;

/** 영상 진행률 검증에 필요한 팝업 설정이다. */
public record VideoPopupContext(
        String popupId,
        String popupType,
        BigDecimal completionRatio
) {
}
