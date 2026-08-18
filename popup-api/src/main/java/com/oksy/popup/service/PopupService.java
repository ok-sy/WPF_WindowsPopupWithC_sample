package com.oksy.popup.service;

import com.oksy.popup.domain.PopupEntity;
import com.oksy.popup.domain.PopupOptionEntity;
import com.oksy.popup.domain.PopupQuestionEntity;
import com.oksy.popup.dto.PopupHideRequestDto;
import com.oksy.popup.dto.PopupHideResponseDto;
import com.oksy.popup.dto.PopupOptionDto;
import com.oksy.popup.dto.PopupQuestionDto;
import com.oksy.popup.dto.PopupResponseDto;
import com.oksy.popup.mapper.PopupMapper;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import tools.jackson.core.type.TypeReference;
import tools.jackson.databind.ObjectMapper;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

/** PostgreSQL 조회 결과를 WPF 팝업 응답으로 변환한다. */
@Service
public class PopupService {

    private final PopupMapper popupMapper;
    private final ObjectMapper objectMapper;

    public PopupService(
            PopupMapper popupMapper,
            ObjectMapper objectMapper) {

        this.popupMapper = popupMapper;
        this.objectMapper = objectMapper;
    }

    public List<PopupResponseDto> getPopups(
            String userId) {

        if (userId == null || userId.isBlank()) {
            throw new IllegalArgumentException(
                    "사용자 ID는 필수입니다.");
        }

        List<PopupEntity> popupEntities =
                popupMapper.selectAvailablePopups(
                        userId.trim());

        List<Long> templateIds = popupEntities.stream()
                .map(PopupEntity::questionTemplateId)
                .filter(java.util.Objects::nonNull)
                .distinct()
                .toList();

        Map<Long, List<PopupQuestionDto>> questionsByTemplate =
                loadQuestions(templateIds);

        return popupEntities.stream()
                .map(popup -> toResponseDto(
                        popup,
                        questionsByTemplate.getOrDefault(
                                popup.questionTemplateId(),
                                List.of())))
                .toList();
    }

    private Map<Long, List<PopupQuestionDto>> loadQuestions(
            List<Long> templateIds) {

        if (templateIds.isEmpty()) {
            return Map.of();
        }

        List<PopupQuestionEntity> questions =
                popupMapper.selectQuestionsByTemplateIds(
                        templateIds);

        List<Long> questionIds = questions.stream()
                .map(PopupQuestionEntity::questionId)
                .toList();

        Map<Long, List<PopupOptionEntity>> optionsByQuestion =
                questionIds.isEmpty()
                        ? Map.of()
                        : popupMapper.selectOptionsByQuestionIds(
                                questionIds).stream()
                        .collect(Collectors.groupingBy(
                                PopupOptionEntity::questionId,
                                LinkedHashMap::new,
                                Collectors.toList()));

        return questions.stream()
                .collect(Collectors.groupingBy(
                        PopupQuestionEntity::questionTemplateId,
                        LinkedHashMap::new,
                        Collectors.mapping(
                                question -> toQuestionDto(
                                        question,
                                        optionsByQuestion.getOrDefault(
                                                question.questionId(),
                                                List.of())),
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

    @Transactional
    public PopupHideResponseDto hidePopup(
            String popupId,
            PopupHideRequestDto requestDto) {

        if (popupId == null || popupId.isBlank()) {
            throw new IllegalArgumentException(
                    "팝업 ID는 필수입니다.");
        }

        String normalizedUserId = requestDto.userId().trim();
        String normalizedPopupId = popupId.trim();

        int affectedRows = popupMapper.upsertPopupHide(
                normalizedUserId,
                normalizedPopupId,
                requestDto.hideDays());

        if (affectedRows <= 0) {
            throw new IllegalStateException(
                    "팝업 숨김 상태 저장에 실패했습니다. userId="
                            + normalizedUserId
                            + ", popupId="
                            + normalizedPopupId);
        }

        OffsetDateTime hiddenUntil =
                popupMapper.selectHiddenUntil(
                        normalizedUserId,
                        normalizedPopupId);

        if (hiddenUntil == null) {
            throw new IllegalStateException(
                    "팝업 숨김 만료 일시를 조회할 수 없습니다. userId="
                            + normalizedUserId
                            + ", popupId="
                            + normalizedPopupId);
        }

        return new PopupHideResponseDto(
                normalizedUserId,
                normalizedPopupId,
                "UNTIL",
                hiddenUntil);
    }

    private PopupResponseDto toResponseDto(
            PopupEntity popup,
            List<PopupQuestionDto> questions) {

        Map<String, Object> content =
                new LinkedHashMap<>(parseContentJson(
                        popup.popupId(),
                        popup.contentJson()));

        if ("SURVEY".equalsIgnoreCase(popup.popupType())
                || "QUIZ".equalsIgnoreCase(popup.popupType())) {
            content.put("questions", questions);
        }

        return new PopupResponseDto(
                popup.popupId(),
                popup.popupType(),
                popup.title(),
                popup.displayStartAt(),
                popup.displayEndAt(),
                popup.displayMode(),
                popup.sizeMode(),
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
                popup.questionTemplateId(),
                popup.periodMode(),
                popup.repeatInterval(),
                popup.repeatDayOfWeek(),
                popup.repeatDayOfMonth(),
                popup.hideDays(),
                toNullableDouble(popup.completionRatio()),
                toNullableDouble(popup.passingScore()),
                isYes(popup.allowCloseBeforeCompleteYn()),
                questions,
                content);
    }

    private double toDouble(
            BigDecimal value,
            double defaultValue) {

        return value == null
                ? defaultValue
                : value.doubleValue();
    }

    private Double toNullableDouble(
            BigDecimal value) {

        return value == null
                ? null
                : value.doubleValue();
    }

    private boolean isYes(
            String value) {

        return "Y".equalsIgnoreCase(value);
    }

    private Map<String, Object> parseContentJson(
            String popupId,
            String contentJson) {

        if (contentJson == null || contentJson.isBlank()) {
            return Map.of();
        }

        try {
            return objectMapper.readValue(
                    contentJson,
                    new TypeReference<Map<String, Object>>() {
                    });
        } catch (Exception exception) {
            throw new IllegalStateException(
                    "팝업 content JSON 변환에 실패했습니다. popupId="
                            + popupId,
                    exception);
        }
    }
}
