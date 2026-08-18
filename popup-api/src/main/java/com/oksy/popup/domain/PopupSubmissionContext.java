package com.oksy.popup.domain;

import java.math.BigDecimal;

/**
 * 설문 제출 검증에 필요한 팝업 설정만 조회한 경량 객체다.
 * 전체 PopupEntity를 다시 읽지 않고 유형, 템플릿, 통과점수만 Service에 전달한다.
 */
public record PopupSubmissionContext(
        String popupId,
        String popupType,
        Long questionTemplateId,
        BigDecimal passingScore
) {
}
