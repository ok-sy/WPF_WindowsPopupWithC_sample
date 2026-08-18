package com.oksy.popup.mapper;

import com.oksy.popup.domain.PopupEntity;
import com.oksy.popup.domain.PopupOptionEntity;
import com.oksy.popup.domain.PopupQuestionEntity;
import com.oksy.popup.domain.PopupSubmissionContext;
import com.oksy.popup.domain.VideoPopupContext;
import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;
import java.time.OffsetDateTime;

import java.util.List;
import java.math.BigDecimal;
import com.oksy.popup.dto.UserPopupStatusDto;

/**
 * 팝업 조회와 사용자 행동 저장 SQL을 호출하는 MyBatis Mapper다.
 *
 * <p>메서드는 SQL을 직접 포함하지 않고, 같은 namespace의
 * {@code resources/mapper/PopupMapper.xml}에 있는 동일한 id의 SQL과 연결된다.
 * {@code @Param} 이름을 변경하면 XML 바인딩 이름도 함께 변경해야 한다.</p>
 */
@Mapper
public interface PopupMapper {

    /*
     * 현재 시각을 기준으로
     * 사용자에게 노출 가능한 팝업 목록을 조회한다.
     *
     * 조회 조건:
     * 1. ACTIVE_YN이 Y
     * 2. 노출 시작 일시가 지났음
     * 3. 노출 종료 일시가 지나지 않음
     */
    /*
     * 사용자 ID를 기준으로
     * 숨김 처리되지 않은 팝업 목록을 조회한다.
     */
    List<PopupEntity> selectAvailablePopups(

            /*
             * XML에서 #{userId} 이름으로 사용할 값이다.
             */
            @Param("userId")
            String userId
    );

    List<PopupQuestionEntity> selectQuestionsByTemplateIds(
            @Param("templateIds")
            List<Long> templateIds
    );

    List<PopupOptionEntity> selectOptionsByQuestionIds(
            @Param("questionIds")
            List<Long> questionIds
    );
    /*
     * 사용자 팝업 숨김 상태를 저장한다.
     *
     * 동일한 USER_ID + POPUP_ID가 없으면:
     * → INSERT
     *
     * 이미 존재하면:
     * → HIDDEN_UNTIL과 UPDATED_AT을 UPDATE
     */
    int upsertPopupHide(

            /*
             * 팝업을 숨길 사용자 ID
             */
            @Param("userId")
            String userId,

            /*
             * 숨길 팝업 ID
             */
            @Param("popupId")
            String popupId,

            /*
             * 현재 시각부터 숨길 일수
             */
            @Param("hideDays")
            Integer hideDays
    );

    /*
     * 저장된 사용자 팝업 숨김 만료 일시를 조회한다.
     *
     * UPSERT 처리 후 실제 PostgreSQL 서버 시각으로 계산된
     * HIDDEN_UNTIL 값을 응답하기 위해 사용한다.
     */
    OffsetDateTime selectHiddenUntil(

            @Param("userId")
            String userId,

            @Param("popupId")
            String popupId
    );

    PopupSubmissionContext selectSubmissionContext(
            @Param("userId") String userId,
            @Param("popupId") String popupId
    );

    Long selectResponseIdByClientRequestId(
            @Param("clientRequestId") String clientRequestId,
            @Param("userId") String userId,
            @Param("popupId") String popupId
    );

    Long upsertPopupResponse(
            @Param("clientRequestId") String clientRequestId,
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("questionTemplateId") Long questionTemplateId,
            @Param("responseStartedAt") OffsetDateTime responseStartedAt,
            @Param("totalScore") BigDecimal totalScore,
            @Param("passedYn") String passedYn
    );

    int deleteResponseAnswers(@Param("responseId") Long responseId);

    Long insertResponseAnswer(
            @Param("responseId") Long responseId,
            @Param("questionId") Long questionId,
            @Param("textAnswer") String textAnswer,
            @Param("earnedScore") BigDecimal earnedScore,
            @Param("correctYn") String correctYn,
            @Param("userId") String userId
    );

    int insertResponseValue(
            @Param("responseAnswerId") Long responseAnswerId,
            @Param("optionId") Long optionId,
            @Param("selectedValue") String selectedValue,
            @Param("userId") String userId
    );

    int markPopupCompleted(
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("passedYn") String passedYn
    );

    VideoPopupContext selectVideoPopupContext(
            @Param("userId") String userId,
            @Param("popupId") String popupId
    );

    int upsertVideoProgress(
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("durationSeconds") BigDecimal durationSeconds,
            @Param("positionSeconds") BigDecimal positionSeconds,
            @Param("maximumPositionSeconds") BigDecimal maximumPositionSeconds,
            @Param("watchedSeconds") BigDecimal watchedSeconds,
            @Param("watchedRatio") BigDecimal watchedRatio,
            @Param("completedYn") String completedYn
    );

    OffsetDateTime selectVideoCompletedAt(
            @Param("userId") String userId,
            @Param("popupId") String popupId
    );

    int countActiveUserAndPopup(
            @Param("userId") String userId,
            @Param("popupId") String popupId
    );

    int upsertPopupEvent(
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("eventType") String eventType
    );

    List<UserPopupStatusDto> selectPopupStatuses(
            @Param("userId") String userId
    );
}
