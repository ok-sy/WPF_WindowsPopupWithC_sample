package server.domain.popup;

import java.math.BigDecimal;
import java.util.List;

/** WPF가 설문·퀴즈 화면을 구성할 때 사용하는 문항이다. */
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
