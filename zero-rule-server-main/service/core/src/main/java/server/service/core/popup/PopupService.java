package server.service.core.popup;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import server.domain.popup.PopupEntity;
import server.domain.popup.AdminPopupListItemDto;
import server.domain.popup.PopupEventResponseDto;
import server.domain.popup.PopupHideResponseDto;
import server.domain.popup.PopupOptionDto;
import server.domain.popup.PopupOptionEntity;
import server.domain.popup.PopupQuestionDto;
import server.domain.popup.PopupQuestionEntity;
import server.domain.popup.PopupResponseDto;
import server.domain.popup.PopupSubmissionContext;
import server.domain.popup.PopupSubmitAnswer;
import server.domain.popup.PopupSubmitResponseDto;
import server.domain.popup.VideoPopupContext;
import server.domain.popup.VideoProgressResponseDto;
import server.domain.popup.UserPopupStatusDto;
import server.repo.core.mapper.popup.PopupMapper;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.math.RoundingMode;
import java.util.LinkedHashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.Set;
import java.util.function.Function;
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

    /**
     * 관리자 화면에서 사용할 전체 팝업 목록을 조회한다.
     * 사용자 대상, 게시 기간, 숨김 여부를 적용하지 않는 것이 WPF 조회와의 차이다.
     */
    @Transactional(readOnly = true)
    public List<AdminPopupListItemDto> getAdminPopups() {
        return popupMapper.selectAdminPopups();
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

    /**
     * 설문 답안을 서버의 문항·정답과 대조해 채점하고 관련 응답 테이블에 저장한다.
     * 모든 저장은 한 트랜잭션이므로 중간 실패 시 일부 답안만 남지 않는다.
     */
    @Transactional
    public PopupSubmitResponseDto submitResponse(
            String popupId,
            String clientRequestId,
            String userId,
            OffsetDateTime responseStartedAt,
            List<PopupSubmitAnswer> answers) {
        if (popupId == null || popupId.isBlank()) {
            throw new IllegalArgumentException("팝업 ID는 필수입니다.");
        }
        if (clientRequestId == null || clientRequestId.isBlank()) {
            throw new IllegalArgumentException("요청 ID는 필수입니다.");
        }
        if (userId == null || userId.isBlank()) {
            throw new IllegalArgumentException("사용자 ID는 필수입니다.");
        }
        if (answers == null || answers.isEmpty()) {
            throw new IllegalArgumentException("하나 이상의 답안이 필요합니다.");
        }

        String normalizedPopupId = popupId.trim();
        String normalizedRequestId = clientRequestId.trim();
        String normalizedUserId = userId.trim();

        boolean eligible = popupMapper.selectAvailablePopups(normalizedUserId).stream()
                .anyMatch(popup -> normalizedPopupId.equals(popup.popupId()));
        if (!eligible) {
            throw new IllegalArgumentException(
                    "현재 사용자에게 제출 가능한 팝업이 아닙니다.");
        }

        PopupSubmissionContext context = popupMapper.selectSubmissionContext(
                normalizedUserId, normalizedPopupId);
        if (context == null
                || !("SURVEY".equalsIgnoreCase(context.popupType())
                || "QUIZ".equalsIgnoreCase(context.popupType()))
                || context.questionTemplateId() == null) {
            throw new IllegalArgumentException("설문형 팝업만 답안을 제출할 수 있습니다.");
        }

        List<PopupQuestionEntity> questions =
                popupMapper.selectQuestionsByTemplateIds(
                        List.of(context.questionTemplateId()));
        Map<Long, PopupQuestionEntity> questionById = questions.stream()
                .collect(Collectors.toMap(
                        PopupQuestionEntity::questionId, Function.identity()));

        Set<Long> submittedQuestionIds = new HashSet<>();
        for (PopupSubmitAnswer answer : answers) {
            if (answer == null || answer.questionId() == null) {
                throw new IllegalArgumentException("문항 ID는 필수입니다.");
            }
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
                    && !hasRequiredAnswer(question, answers)) {
                throw new IllegalArgumentException(
                        "필수 문항에 답해야 합니다. questionId="
                                + question.questionId());
            }
        }

        List<Long> questionIds = questions.stream()
                .map(PopupQuestionEntity::questionId)
                .toList();
        Map<Long, List<PopupOptionEntity>> optionsByQuestion = questionIds.isEmpty()
                ? Map.of()
                : popupMapper.selectOptionsByQuestionIds(questionIds).stream()
                .collect(Collectors.groupingBy(PopupOptionEntity::questionId));

        List<GradedAnswer> gradedAnswers = answers.stream()
                .map(answer -> gradeAnswer(
                        questionById.get(answer.questionId()),
                        answer,
                        optionsByQuestion.getOrDefault(
                                answer.questionId(), List.of())))
                .toList();
        BigDecimal totalScore = gradedAnswers.stream()
                .map(GradedAnswer::earnedScore)
                .filter(Objects::nonNull)
                .reduce(BigDecimal.ZERO, BigDecimal::add);
        BigDecimal passingScore = context.passingScore() == null
                ? BigDecimal.ZERO : context.passingScore();
        String passedYn = totalScore.compareTo(passingScore) >= 0 ? "Y" : "N";

        Long responseId = popupMapper.upsertPopupResponse(
                normalizedRequestId, normalizedUserId, normalizedPopupId,
                context.questionTemplateId(), responseStartedAt,
                totalScore, passedYn);
        if (responseId == null) {
            throw new IllegalStateException("설문 응답 저장에 실패했습니다.");
        }
        popupMapper.deleteResponseAnswers(responseId);

        for (GradedAnswer graded : gradedAnswers) {
            Long responseAnswerId = popupMapper.insertResponseAnswer(
                    responseId,
                    graded.question().questionId(),
                    normalizeText(graded.answer().textAnswer()),
                    graded.earnedScore(),
                    graded.correctYn(),
                    normalizedUserId);
            for (PopupOptionEntity selectedOption : graded.selectedOptions()) {
                popupMapper.insertResponseValue(
                        responseAnswerId,
                        selectedOption.optionId(),
                        selectedOption.optionValue(),
                        normalizedUserId);
            }
        }

        popupMapper.markPopupCompleted(
                normalizedUserId, normalizedPopupId, passedYn);
        return new PopupSubmitResponseDto(
                responseId, normalizedRequestId, normalizedUserId,
                normalizedPopupId, "SUBMITTED", totalScore.doubleValue(),
                "Y".equals(passedYn), OffsetDateTime.now());
    }

    private boolean hasRequiredAnswer(
            PopupQuestionEntity question,
            List<PopupSubmitAnswer> answers) {
        return answers.stream()
                .filter(answer -> question.questionId().equals(answer.questionId()))
                .anyMatch(answer -> "TEXT".equalsIgnoreCase(question.questionType())
                        ? normalizeText(answer.textAnswer()) != null
                        : !answer.optionIds().isEmpty());
    }

    private GradedAnswer gradeAnswer(
            PopupQuestionEntity question,
            PopupSubmitAnswer answer,
            List<PopupOptionEntity> availableOptions) {
        if ("TEXT".equalsIgnoreCase(question.questionType())) {
            if (!answer.optionIds().isEmpty()) {
                throw new IllegalArgumentException(
                        "서술형 문항에는 선택지를 제출할 수 없습니다. questionId="
                                + question.questionId());
            }
            return new GradedAnswer(question, answer, List.of(), null, null);
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

        Set<Long> uniqueOptionIds = new HashSet<>(answer.optionIds());
        if (uniqueOptionIds.size() != answer.optionIds().size()) {
            throw new IllegalArgumentException(
                    "같은 선택지를 중복 제출할 수 없습니다. questionId="
                            + question.questionId());
        }
        Map<Long, PopupOptionEntity> optionById = availableOptions.stream()
                .collect(Collectors.toMap(
                        PopupOptionEntity::optionId, Function.identity()));
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
        Set<Long> correctOptionIds = availableOptions.stream()
                .filter(option -> isYes(option.correctYn()))
                .map(PopupOptionEntity::optionId)
                .collect(Collectors.toSet());
        boolean correct = uniqueOptionIds.equals(correctOptionIds);
        BigDecimal earnedScore = correct
                ? question.questionScore() : BigDecimal.ZERO;
        return new GradedAnswer(
                question, answer, selectedOptions,
                earnedScore, correct ? "Y" : "N");
    }

    private String normalizeText(String value) {
        return value == null || value.isBlank() ? null : value.trim();
    }

    private record GradedAnswer(
            PopupQuestionEntity question,
            PopupSubmitAnswer answer,
            List<PopupOptionEntity> selectedOptions,
            BigDecimal earnedScore,
            String correctYn
    ) {
    }

    /** 영상 누적 시청시간을 기준으로 진행률과 완료 여부를 계산해 저장한다. */
    @Transactional
    public VideoProgressResponseDto saveVideoProgress(
            String popupId,
            String userId,
            BigDecimal durationSeconds,
            BigDecimal positionSeconds,
            BigDecimal maximumPositionSeconds,
            BigDecimal watchedSeconds) {
        if (popupId == null || popupId.isBlank()) {
            throw new IllegalArgumentException("팝업 ID는 필수입니다.");
        }
        if (userId == null || userId.isBlank()) {
            throw new IllegalArgumentException("사용자 ID는 필수입니다.");
        }
        if (durationSeconds == null || durationSeconds.signum() <= 0
                || positionSeconds == null || positionSeconds.signum() < 0
                || maximumPositionSeconds == null
                || maximumPositionSeconds.signum() < 0
                || watchedSeconds == null || watchedSeconds.signum() < 0) {
            throw new IllegalArgumentException("영상 재생시간 값이 올바르지 않습니다.");
        }
        if (positionSeconds.compareTo(durationSeconds) > 0
                || maximumPositionSeconds.compareTo(durationSeconds) > 0) {
            throw new IllegalArgumentException(
                    "영상 재생 위치는 전체 재생시간을 초과할 수 없습니다.");
        }

        String normalizedPopupId = popupId.trim();
        String normalizedUserId = userId.trim();
        VideoPopupContext context = popupMapper.selectVideoPopupContext(
                normalizedUserId, normalizedPopupId);
        if (context == null) {
            throw new IllegalArgumentException(
                    "현재 사용자에게 유효한 영상 팝업이 아닙니다.");
        }

        BigDecimal normalizedWatchedSeconds = watchedSeconds.min(durationSeconds);
        BigDecimal watchedRatio = normalizedWatchedSeconds.divide(
                durationSeconds, 4, RoundingMode.DOWN).min(BigDecimal.ONE);
        BigDecimal requiredRatio = context.completionRatio() == null
                ? BigDecimal.ONE : context.completionRatio();
        boolean completed = watchedRatio.compareTo(requiredRatio) >= 0;
        String completedYn = completed ? "Y" : "N";

        int affectedRows = popupMapper.upsertVideoProgress(
                normalizedUserId, normalizedPopupId,
                durationSeconds, positionSeconds, maximumPositionSeconds,
                normalizedWatchedSeconds, watchedRatio, completedYn);
        if (affectedRows <= 0) {
            throw new IllegalStateException("영상 진행률 저장에 실패했습니다.");
        }
        popupMapper.markPopupCompleted(
                normalizedUserId, normalizedPopupId, completedYn);
        OffsetDateTime completedAt = completed
                ? popupMapper.selectVideoCompletedAt(
                        normalizedUserId, normalizedPopupId)
                : null;

        return new VideoProgressResponseDto(
                normalizedUserId, normalizedPopupId,
                watchedRatio.doubleValue(), requiredRatio.doubleValue(),
                completed, completedAt);
    }

    /** 팝업이 실제 표시되거나 닫힌 이벤트를 사용자 상태에 누적한다. */
    @Transactional
    public PopupEventResponseDto recordPopupEvent(
            String popupId,
            String userId,
            String eventType) {
        if (popupId == null || popupId.isBlank()) {
            throw new IllegalArgumentException("팝업 ID는 필수입니다.");
        }
        if (userId == null || userId.isBlank()) {
            throw new IllegalArgumentException("사용자 ID는 필수입니다.");
        }
        if (eventType == null || eventType.isBlank()) {
            throw new IllegalArgumentException("이벤트 유형은 필수입니다.");
        }

        String normalizedPopupId = popupId.trim();
        String normalizedUserId = userId.trim();
        String normalizedEventType = eventType.trim().toUpperCase();
        if (!("DISPLAYED".equals(normalizedEventType)
                || "CLOSED".equals(normalizedEventType))) {
            throw new IllegalArgumentException(
                    "이벤트 유형은 DISPLAYED 또는 CLOSED여야 합니다.");
        }
        if (popupMapper.countActiveUserAndPopup(
                normalizedUserId, normalizedPopupId) == 0) {
            throw new IllegalArgumentException("유효한 사용자 또는 팝업이 아닙니다.");
        }
        if (popupMapper.upsertPopupEvent(
                normalizedUserId, normalizedPopupId, normalizedEventType) <= 0) {
            throw new IllegalStateException("팝업 이벤트 저장에 실패했습니다.");
        }
        return new PopupEventResponseDto(
                normalizedUserId, normalizedPopupId,
                normalizedEventType, OffsetDateTime.now());
    }

    /** 사용자의 팝업별 표시·숨김·완료 상태를 조회한다. */
    @Transactional(readOnly = true)
    public List<UserPopupStatusDto> getPopupStatuses(String userId) {
        if (userId == null || userId.isBlank()) {
            throw new IllegalArgumentException("사용자 ID는 필수입니다.");
        }
        return popupMapper.selectPopupStatuses(userId.trim());
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
