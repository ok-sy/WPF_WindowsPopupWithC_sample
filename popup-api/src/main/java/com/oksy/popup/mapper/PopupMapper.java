package com.oksy.popup.mapper;

import com.oksy.popup.domain.PopupEntity;
import com.oksy.popup.domain.PopupOptionEntity;
import com.oksy.popup.domain.PopupQuestionEntity;
import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;
import java.time.OffsetDateTime;

import java.util.List;

/*
 * POPUP_NOTICE 테이블의 SQL을 호출하는 Mapper다.
 *
 * 실제 SQL은 같은 namespace를 사용하는
 * PopupMapper.xml에 작성한다.
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
}
