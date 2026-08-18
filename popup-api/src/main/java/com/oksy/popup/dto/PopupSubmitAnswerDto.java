package com.oksy.popup.dto;

import jakarta.validation.constraints.NotNull;

import java.util.List;

/** 설문 문항 하나에 사용자가 입력한 답이다. */
public record PopupSubmitAnswerDto(
        @NotNull Long questionId,
        String textAnswer,
        List<Long> optionIds
) {
    public PopupSubmitAnswerDto {
        optionIds = optionIds == null ? List.of() : List.copyOf(optionIds);
    }
}
