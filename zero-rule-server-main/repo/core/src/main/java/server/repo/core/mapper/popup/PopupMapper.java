package server.repo.core.mapper.popup;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;
import server.domain.popup.PopupEntity;
import server.domain.popup.PopupOptionEntity;
import server.domain.popup.PopupQuestionEntity;

import java.util.List;

/** popup 스키마의 팝업 표시 데이터를 조회한다. */
@Mapper
public interface PopupMapper {

    List<PopupEntity> selectAvailablePopups(@Param("userId") String userId);

    List<PopupQuestionEntity> selectQuestionsByTemplateIds(
            @Param("templateIds") List<Long> templateIds);

    List<PopupOptionEntity> selectOptionsByQuestionIds(
            @Param("questionIds") List<Long> questionIds);
}
