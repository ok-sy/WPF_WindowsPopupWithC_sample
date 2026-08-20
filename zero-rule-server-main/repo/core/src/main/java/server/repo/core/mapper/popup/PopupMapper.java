package server.repo.core.mapper.popup;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;
import server.domain.popup.PopupEntity;
import server.domain.popup.PopupOptionEntity;
import server.domain.popup.PopupQuestionEntity;
import server.domain.popup.PopupSubmissionContext;
import server.domain.popup.VideoPopupContext;

import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.List;

/** popup 스키마의 팝업 표시 데이터를 조회한다. */
@Mapper
public interface PopupMapper {

    List<PopupEntity> selectAvailablePopups(@Param("userId") String userId);

    List<PopupQuestionEntity> selectQuestionsByTemplateIds(
            @Param("templateIds") List<Long> templateIds);

    List<PopupOptionEntity> selectOptionsByQuestionIds(
            @Param("questionIds") List<Long> questionIds);

    int upsertPopupHide(
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("hideDays") int hideDays);

    OffsetDateTime selectHiddenUntil(
            @Param("userId") String userId,
            @Param("popupId") String popupId);

    PopupSubmissionContext selectSubmissionContext(
            @Param("userId") String userId,
            @Param("popupId") String popupId);

    Long upsertPopupResponse(
            @Param("clientRequestId") String clientRequestId,
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("questionTemplateId") Long questionTemplateId,
            @Param("responseStartedAt") OffsetDateTime responseStartedAt,
            @Param("totalScore") BigDecimal totalScore,
            @Param("passedYn") String passedYn);

    int deleteResponseAnswers(@Param("responseId") Long responseId);

    Long insertResponseAnswer(
            @Param("responseId") Long responseId,
            @Param("questionId") Long questionId,
            @Param("textAnswer") String textAnswer,
            @Param("earnedScore") BigDecimal earnedScore,
            @Param("correctYn") String correctYn,
            @Param("userId") String userId);

    int insertResponseValue(
            @Param("responseAnswerId") Long responseAnswerId,
            @Param("optionId") Long optionId,
            @Param("selectedValue") String selectedValue,
            @Param("userId") String userId);

    int markPopupCompleted(
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("passedYn") String passedYn);

    VideoPopupContext selectVideoPopupContext(
            @Param("userId") String userId,
            @Param("popupId") String popupId);

    int upsertVideoProgress(
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("durationSeconds") BigDecimal durationSeconds,
            @Param("positionSeconds") BigDecimal positionSeconds,
            @Param("maximumPositionSeconds") BigDecimal maximumPositionSeconds,
            @Param("watchedSeconds") BigDecimal watchedSeconds,
            @Param("watchedRatio") BigDecimal watchedRatio,
            @Param("completedYn") String completedYn);

    OffsetDateTime selectVideoCompletedAt(
            @Param("userId") String userId,
            @Param("popupId") String popupId);

    int countActiveUserAndPopup(
            @Param("userId") String userId,
            @Param("popupId") String popupId);

    int upsertPopupEvent(
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("eventType") String eventType);
}
