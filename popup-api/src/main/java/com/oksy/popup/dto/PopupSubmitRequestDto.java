package com.oksy.popup.dto;

import jakarta.validation.Valid;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;

import java.time.OffsetDateTime;
import java.util.List;

/**
 * WPF가 설문 제출 시 보내는 요청 JSON이다.
 * clientRequestId는 재전송 식별, userId는 응답 사용자, answers는 문항별 답안이다.
 */
public record PopupSubmitRequestDto(
        @NotBlank String clientRequestId,
        @NotBlank String userId,
        OffsetDateTime responseStartedAt,
        @NotNull @Size(min = 1) List<@Valid PopupSubmitAnswerDto> answers
) {
    public PopupSubmitRequestDto {
        answers = answers == null ? null : List.copyOf(answers);
    }
}
