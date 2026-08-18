package com.oksy.popup.service;

import com.oksy.popup.domain.PopupEntity;
import com.oksy.popup.domain.PopupOptionEntity;
import com.oksy.popup.domain.PopupQuestionEntity;
import com.oksy.popup.domain.PopupSubmissionContext;
import com.oksy.popup.dto.PopupHideRequestDto;
import com.oksy.popup.dto.PopupHideResponseDto;
import com.oksy.popup.dto.PopupOptionDto;
import com.oksy.popup.dto.PopupQuestionDto;
import com.oksy.popup.dto.PopupResponseDto;
import com.oksy.popup.dto.PopupSubmitAnswerDto;
import com.oksy.popup.dto.PopupSubmitRequestDto;
import com.oksy.popup.dto.PopupSubmitResponseDto;
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
import java.util.HashSet;
import java.util.Set;
import java.util.function.Function;
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

    /** 설문 답안을 검증하고 서버 기준으로 채점한 뒤 한 트랜잭션으로 저장한다. */
    @Transactional
    public PopupSubmitResponseDto submitResponse(
            String popupId,
            PopupSubmitRequestDto requestDto) {

        if (popupId == null || popupId.isBlank()) {
            throw new IllegalArgumentException("팝업 ID는 필수입니다.");
        }

        String normalizedPopupId = popupId.trim();
        String userId = requestDto.userId().trim();
        String clientRequestId = requestDto.clientRequestId().trim();

        boolean eligible = popupMapper.selectAvailablePopups(userId).stream()
                .anyMatch(popup -> normalizedPopupId.equals(popup.popupId()));
        if (!eligible) {
            throw new IllegalArgumentException(
                    "현재 사용자에게 제출 가능한 팝업이 아닙니다.");
        }

        PopupSubmissionContext context = popupMapper.selectSubmissionContext(
                userId, normalizedPopupId);
        if (context == null) {
            throw new IllegalArgumentException(
                    "제출 가능한 사용자 또는 팝업이 아닙니다.");
        }
        if (!("SURVEY".equalsIgnoreCase(context.popupType())
                || "QUIZ".equalsIgnoreCase(context.popupType()))
                || context.questionTemplateId() == null) {
            throw new IllegalArgumentException("설문형 팝업만 답안을 제출할 수 있습니다.");
        }

        List<PopupQuestionEntity> questions =
                popupMapper.selectQuestionsByTemplateIds(
                        List.of(context.questionTemplateId()));
        Map<Long, PopupQuestionEntity> questionById = questions.stream()
                .collect(Collectors.toMap(
                        PopupQuestionEntity::questionId,
                        Function.identity()));

        Set<Long> submittedQuestionIds = new HashSet<>();
        for (PopupSubmitAnswerDto answer : requestDto.answers()) {
            if (!submittedQuestionIds.add(answer.questionId())) {
                throw new IllegalArgumentException(
                        "같은 문항을 중복 제출할 수 없습니다. questionId="
                                + answer.questionId());
            }
            if (!questionById.containsKey(answer.questionId())) {
                throw new IllegalArgumentException(
                        "현재 설문에 포함되지 않은 문항입니다. questionId="
                                + answer.questionId());
            }
        }

        for (PopupQuestionEntity question : questions) {
            if (isYes(question.requiredYn())
                    && !hasRequiredAnswer(question, requestDto.answers())) {
                throw new IllegalArgumentException(
                        "필수 문항에 답해야 합니다. questionId="
                                + question.questionId());
            }
        }

        List<Long> questionIds = questions.stream()
                .map(PopupQuestionEntity::questionId)
                .toList();
        List<PopupOptionEntity> options = questionIds.isEmpty()
                ? List.of()
                : popupMapper.selectOptionsByQuestionIds(questionIds);
        Map<Long, List<PopupOptionEntity>> optionsByQuestion = options.stream()
                .collect(Collectors.groupingBy(PopupOptionEntity::questionId));

        List<GradedAnswer> gradedAnswers = requestDto.answers().stream()
                .map(answer -> gradeAnswer(
                        questionById.get(answer.questionId()),
                        answer,
                        optionsByQuestion.getOrDefault(
                                answer.questionId(), List.of())))
                .toList();

        BigDecimal totalScore = gradedAnswers.stream()
                .map(GradedAnswer::earnedScore)
                .filter(java.util.Objects::nonNull)
                .reduce(BigDecimal.ZERO, BigDecimal::add);
        BigDecimal passingScore = context.passingScore() == null
                ? BigDecimal.ZERO : context.passingScore();
        String passedYn = totalScore.compareTo(passingScore) >= 0 ? "Y" : "N";

        Long responseId = popupMapper.upsertPopupResponse(
                clientRequestId, userId, normalizedPopupId,
                context.questionTemplateId(), requestDto.responseStartedAt(),
                totalScore, passedYn);
        popupMapper.deleteResponseAnswers(responseId);

        for (GradedAnswer graded : gradedAnswers) {
            Long responseAnswerId = popupMapper.insertResponseAnswer(
                    responseId, graded.question().questionId(),
                    normalizeText(graded.answer().textAnswer()),
                    graded.earnedScore(), graded.correctYn(), userId);
            for (PopupOptionEntity selectedOption : graded.selectedOptions()) {
                popupMapper.insertResponseValue(
                        responseAnswerId, selectedOption.optionId(),
                        selectedOption.optionValue(), userId);
            }
        }

        popupMapper.markPopupCompleted(
                userId, normalizedPopupId, passedYn);
        return buildSubmitResponse(responseId, clientRequestId, userId,
                normalizedPopupId, totalScore, "Y".equals(passedYn));
    }

    private boolean hasRequiredAnswer(
            PopupQuestionEntity question,
            List<PopupSubmitAnswerDto> answers) {
        return answers.stream()
                .filter(answer -> question.questionId().equals(answer.questionId()))
                .anyMatch(answer -> "TEXT".equalsIgnoreCase(question.questionType())
                        ? normalizeText(answer.textAnswer()) != null
                        : !answer.optionIds().isEmpty());
    }

    private GradedAnswer gradeAnswer(
            PopupQuestionEntity question,
            PopupSubmitAnswerDto answer,
            List<PopupOptionEntity> availableOptions) {

        if ("TEXT".equalsIgnoreCase(question.questionType())) {
            if (!answer.optionIds().isEmpty()) {
                throw new IllegalArgumentException(
                        "서술형 문항에는 선택지를 제출할 수 없습니다. questionId="
                                + question.questionId());
            }
            return new GradedAnswer(
                    question, answer, List.of(), null, null);
        }

        if (normalizeText(answer.textAnswer()) != null) {
            throw new IllegalArgumentException(
                    "선택형 문항에는 textAnswer를 제출할 수 없습니다. questionId="
                            + question.questionId());
        }
        if ("SINGLE_CHOICE".equalsIgnoreCase(question.questionType())
                && answer.optionIds().size() > 1) {
            throw new IllegalArgumentException(
                    "단일 선택 문항은 하나만 선택할 수 있습니다. questionId="
                            + question.questionId());
        }

        Map<Long, PopupOptionEntity> optionById = availableOptions.stream()
                .collect(Collectors.toMap(
                        PopupOptionEntity::optionId, Function.identity()));
        Set<Long> uniqueOptionIds = new HashSet<>(answer.optionIds());
        if (uniqueOptionIds.size() != answer.optionIds().size()) {
            throw new IllegalArgumentException(
                    "같은 선택지를 중복 제출할 수 없습니다. questionId="
                            + question.questionId());
        }
        List<PopupOptionEntity> selectedOptions = answer.optionIds().stream()
                .map(optionId -> {
                    PopupOptionEntity option = optionById.get(optionId);
                    if (option == null) {
                        throw new IllegalArgumentException(
                                "현재 문항에 포함되지 않은 선택지입니다. optionId="
                                        + optionId);
                    }
                    return option;
                })
                .toList();

        if (!isYes(question.scoredYn())) {
            return new GradedAnswer(
                    question, answer, selectedOptions, null, null);
        }

        Set<Long> correctIds = availableOptions.stream()
                .filter(option -> isYes(option.correctYn()))
                .map(PopupOptionEntity::optionId)
                .collect(Collectors.toSet());
        boolean correct = uniqueOptionIds.equals(correctIds);
        BigDecimal earnedScore = correct
                ? question.questionScore() : BigDecimal.ZERO;
        return new GradedAnswer(
                question, answer, selectedOptions,
                earnedScore, correct ? "Y" : "N");
    }

    private String normalizeText(String value) {
        if (value == null || value.isBlank()) {
            return null;
        }
        return value.trim();
    }

    private PopupSubmitResponseDto buildSubmitResponse(
            Long responseId,
            String clientRequestId,
            String userId,
            String popupId,
            BigDecimal totalScore,
            boolean passed) {
        return new PopupSubmitResponseDto(
                responseId, clientRequestId, userId, popupId,
                "SUBMITTED", totalScore.doubleValue(), passed,
                OffsetDateTime.now());
    }

    private record GradedAnswer(
            PopupQuestionEntity question,
            PopupSubmitAnswerDto answer,
            List<PopupOptionEntity> selectedOptions,
            BigDecimal earnedScore,
            String correctYn
    ) {
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
