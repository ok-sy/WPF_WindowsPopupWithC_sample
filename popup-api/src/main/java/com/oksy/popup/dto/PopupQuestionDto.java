package com.oksy.popup.dto;

import java.math.BigDecimal;
import java.util.List;

/** WPF에 전달할 설문 또는 퀴즈 문항이다. */
public record PopupQuestionDto(
        Long questionId,
        String title,
        String description,
        String questionType,
        boolean isRequired,
        boolean isScored,
        BigDecimal questionScore,
        int sortOrder,
        List<PopupOptionDto> options
) {
    public PopupQuestionDto {
        options = options == null ? List.of() : List.copyOf(options);
    }
}
