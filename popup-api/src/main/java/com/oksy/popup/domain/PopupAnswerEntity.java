package com.oksy.popup.domain;

import java.math.BigDecimal;
import java.time.LocalDateTime;

/**
 * POPUP_RESPONSE_ANSWER 테이블의 문항별 답안 한 건이다.
 * 설문 전체 응답({@code responseId})과 질문을 연결하고 서술 답변, 획득점수,
 * 정답 여부를 보관한다. 선택지는 PopupAnswerValueEntity에 별도로 저장한다.
 */
public record PopupAnswerEntity(
        Long responseAnswerId,
        Long responseId,
        Long questionId,
        String textAnswer,
        BigDecimal earnedScore,
        String correctYn,
        String createdBy,
        LocalDateTime createdAt,
        String updatedBy,
        LocalDateTime updatedAt
) {
}
