package com.oksy.popup.mapper;

import com.oksy.popup.domain.PopupEntity;
import org.apache.ibatis.annotations.Mapper;

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
     * 1. USE_YN이 Y
     * 2. 노출 시작 일시가 지났음
     * 3. 노출 종료 일시가 지나지 않음
     */
    List<PopupEntity> selectAvailablePopups();
}