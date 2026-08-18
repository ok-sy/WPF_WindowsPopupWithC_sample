package com.oksy.popup.domain;

import java.math.BigDecimal;

/** 제출 대상 팝업과 사용자의 서버 검증에 필요한 값이다. */
public record PopupSubmissionContext(
        String popupId,
        String popupType,
        Long questionTemplateId,
        BigDecimal passingScore
) {
}
