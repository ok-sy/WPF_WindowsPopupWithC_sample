package com.oksy.popup.dto;

import java.math.BigDecimal;
import java.util.List;

/**
 * WPF가 설문·퀴즈 화면을 구성할 때 사용하는 문항이다.
 * 문항 유형과 필수 여부, 배점, 선택지 목록을 포함하며 목록은 불변 복사한다.
 */
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
