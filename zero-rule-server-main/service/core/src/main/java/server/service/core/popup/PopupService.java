package server.service.core.popup;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import server.domain.popup.PopupEntity;
import server.domain.popup.PopupHideResponseDto;
import server.domain.popup.PopupOptionDto;
import server.domain.popup.PopupOptionEntity;
import server.domain.popup.PopupQuestionDto;
import server.domain.popup.PopupQuestionEntity;
import server.domain.popup.PopupResponseDto;
import server.repo.core.mapper.popup.PopupMapper;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.stream.Collectors;

/** 팝업 조회 결과를 기존 WPF 응답 형식으로 조립한다. */
@Service
public class PopupService {

    private final PopupMapper popupMapper;
    private final ObjectMapper objectMapper;

    public PopupService(PopupMapper popupMapper, ObjectMapper objectMapper) {
        this.popupMapper = popupMapper;
        this.objectMapper = objectMapper;
    }

    /** 사용자별 기간·대상·숨김 조건을 통과한 팝업을 조회한다. */
    @Transactional(readOnly = true)
    public List<PopupResponseDto> getPopups(String userId) {
        if (userId == null || userId.isBlank()) {
            throw new IllegalArgumentException("사용자 ID는 필수입니다.");
        }

        List<PopupEntity> popups = popupMapper.selectAvailablePopups(userId.trim());
        List<Long> templateIds = popups.stream()
                .map(PopupEntity::questionTemplateId)
                .filter(Objects::nonNull)
                .distinct()
                .toList();
        Map<Long, List<PopupQuestionDto>> questionsByTemplate = loadQuestions(templateIds);

        return popups.stream()
                .map(popup -> toResponseDto(
                        popup,
                        questionsByTemplate.getOrDefault(
                                popup.questionTemplateId(), List.of())))
                .toList();
    }

    /**
     * 사용자가 선택한 기간 동안 팝업을 숨긴다.
     * 상태 저장과 저장 결과 재조회를 하나의 트랜잭션으로 묶는다.
     */
    @Transactional
    public PopupHideResponseDto hidePopup(
            String popupId,
            String userId,
            Integer hideDays) {
        if (popupId == null || popupId.isBlank()) {
            throw new IllegalArgumentException("팝업 ID는 필수입니다.");
        }
        if (userId == null || userId.isBlank()) {
            throw new IllegalArgumentException("사용자 ID는 필수입니다.");
        }
        if (hideDays == null || hideDays < 1 || hideDays > 3650) {
            throw new IllegalArgumentException(
                    "숨김 일수는 1일 이상 3650일 이하여야 합니다.");
        }

        String normalizedPopupId = popupId.trim();
        String normalizedUserId = userId.trim();
        int affectedRows = popupMapper.upsertPopupHide(
                normalizedUserId, normalizedPopupId, hideDays);
        if (affectedRows <= 0) {
            throw new IllegalStateException(
                    "팝업 숨김 상태 저장에 실패했습니다. userId="
                            + normalizedUserId + ", popupId=" + normalizedPopupId);
        }

        OffsetDateTime hiddenUntil = popupMapper.selectHiddenUntil(
                normalizedUserId, normalizedPopupId);
        if (hiddenUntil == null) {
            throw new IllegalStateException(
                    "팝업 숨김 만료 일시를 조회할 수 없습니다. userId="
                            + normalizedUserId + ", popupId=" + normalizedPopupId);
        }

        return new PopupHideResponseDto(
                normalizedUserId, normalizedPopupId, "UNTIL", hiddenUntil);
    }

    private Map<Long, List<PopupQuestionDto>> loadQuestions(List<Long> templateIds) {
        if (templateIds.isEmpty()) {
            return Map.of();
        }

        List<PopupQuestionEntity> questions =
                popupMapper.selectQuestionsByTemplateIds(templateIds);
        List<Long> questionIds = questions.stream()
                .map(PopupQuestionEntity::questionId)
                .toList();
        Map<Long, List<PopupOptionEntity>> optionsByQuestion = questionIds.isEmpty()
                ? Map.of()
                : popupMapper.selectOptionsByQuestionIds(questionIds).stream()
                .collect(Collectors.groupingBy(
                        PopupOptionEntity::questionId,
                        LinkedHashMap::new,
                        Collectors.toList()));

        return questions.stream().collect(Collectors.groupingBy(
                PopupQuestionEntity::questionTemplateId,
                LinkedHashMap::new,
                Collectors.mapping(
                        question -> toQuestionDto(
                                question,
                                optionsByQuestion.getOrDefault(
                                        question.questionId(), List.of())),
                        Collectors.toList())));
    }

    private PopupQuestionDto toQuestionDto(
            PopupQuestionEntity question,
            List<PopupOptionEntity> options) {
        List<PopupOptionDto> optionDtos = options.stream()
                .map(option -> new PopupOptionDto(
                        option.optionId(),
                        option.optionValue(),
                        option.optionText(),
                        option.sortOrder()))
                .toList();

        return new PopupQuestionDto(
                question.questionId(),
                question.questionTitle(),
                question.questionDescription(),
                question.questionType(),
                isYes(question.requiredYn()),
                isYes(question.scoredYn()),
                question.questionScore(),
                question.sortOrder(),
                optionDtos);
    }

    private PopupResponseDto toResponseDto(
            PopupEntity popup,
            List<PopupQuestionDto> questions) {
        Map<String, Object> content = new LinkedHashMap<>(
                parseContentJson(popup.popupId(), popup.contentJson()));
        if ("SURVEY".equalsIgnoreCase(popup.popupType())
                || "QUIZ".equalsIgnoreCase(popup.popupType())) {
            content.put("questions", questions);
        }

        return new PopupResponseDto(
                popup.popupId(), popup.popupType(), popup.title(),
                popup.displayStartAt(), popup.displayEndAt(),
                popup.displayMode(), popup.sizeMode(),
                toDouble(popup.popupWidth(), 900),
                toDouble(popup.popupHeight(), 620),
                toDouble(popup.widthRatio(), 0.7),
                toDouble(popup.heightRatio(), 0.75),
                toDouble(popup.minimumWidth(), 480),
                toDouble(popup.minimumHeight(), 320),
                toDouble(popup.maximumWidth(), 1200),
                toDouble(popup.maximumHeight(), 900),
                isYes(popup.showHeaderYn()),
                isYes(popup.showCloseButtonYn()),
                isYes(popup.showFooterYn()),
                isYes(popup.showDontShowYn()),
                popup.questionTemplateId(), popup.periodMode(),
                popup.repeatInterval(), popup.repeatDayOfWeek(),
                popup.repeatDayOfMonth(), popup.hideDays(),
                toNullableDouble(popup.completionRatio()),
                toNullableDouble(popup.passingScore()),
                isYes(popup.allowCloseBeforeCompleteYn()),
                questions, content);
    }

    private Map<String, Object> parseContentJson(String popupId, String contentJson) {
        if (contentJson == null || contentJson.isBlank()) {
            return Map.of();
        }
        try {
            return objectMapper.readValue(
                    contentJson,
                    new TypeReference<Map<String, Object>>() { });
        } catch (Exception exception) {
            throw new IllegalStateException(
                    "팝업 content JSON 변환에 실패했습니다. popupId=" + popupId,
                    exception);
        }
    }

    private boolean isYes(String value) {
        return "Y".equalsIgnoreCase(value);
    }

    private double toDouble(BigDecimal value, double defaultValue) {
        return value == null ? defaultValue : value.doubleValue();
    }

    private Double toNullableDouble(BigDecimal value) {
        return value == null ? null : value.doubleValue();
    }
}
