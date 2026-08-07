package com.oksy.popup.service;

import com.oksy.popup.domain.PopupEntity;
import com.oksy.popup.dto.PopupResponseDto;
import com.oksy.popup.mapper.PopupMapper;
import org.springframework.stereotype.Service;
import tools.jackson.core.type.TypeReference;
import tools.jackson.databind.ObjectMapper;

import java.util.List;
import java.util.Map;

/*
 * Oracle에서 팝업 데이터를 조회하고
 * WPF에 전달할 PopupResponseDto로 변환하는 Service다.
 */
@Service
public class PopupService {

    /*
     * POPUP_NOTICE 테이블 조회를 담당하는 Mapper다.
     */
    private final PopupMapper popupMapper;

    /*
     * CONTENT_JSON 문자열을
     * 실제 JSON 객체로 변환하는 도구다.
     */
    private final ObjectMapper objectMapper;

    /*
     * Spring이 관리하는 PopupMapper와 ObjectMapper를
     * 생성자 주입 방식으로 전달받는다.
     */
    public PopupService(
            PopupMapper popupMapper,
            ObjectMapper objectMapper) {

        this.popupMapper =
                popupMapper;

        this.objectMapper =
                objectMapper;
    }

    /*
     * 현재 노출 가능한 팝업 목록을 조회한다.
     *
     * Oracle 조회 결과인 PopupEntity를
     * WPF 응답용 PopupResponseDto로 변환하여 반환한다.
     */
    public List<PopupResponseDto> getPopups() {

        /*
         * POPUP_NOTICE 테이블에서
         * 현재 노출 가능한 팝업을 조회한다.
         */
        List<PopupEntity> popupEntities =
                popupMapper.selectAvailablePopups();

        /*
         * DB 조회 객체를 WPF 응답 DTO로 변환한다.
         */
        return popupEntities.stream()
                .map(this::toResponseDto)
                .toList();
    }

    /*
     * Oracle 조회 결과 한 건을
     * WPF 응답 DTO 한 건으로 변환한다.
     */
    private PopupResponseDto toResponseDto(
            PopupEntity popupEntity) {

        /*
         * CLOB에서 조회한 JSON 문자열을
         * Map<String, Object> 형태로 변환한다.
         *
         * 이렇게 변환해야 최종 응답에서 content가
         * 문자열이 아니라 JSON 객체로 내려간다.
         */
        Map<String, Object> content =
                parseContentJson(
                        popupEntity.popupId(),
                        popupEntity.contentJson());

        return new PopupResponseDto(
                popupEntity.popupId(),
                popupEntity.popupType(),
                popupEntity.title(),

                popupEntity.displayStartAt(),
                popupEntity.displayEndAt(),

                popupEntity.displayMode(),
                popupEntity.sizeMode(),

                /*
                 * Oracle NUMBER 값을
                 * WPF DTO의 double 값으로 변환한다.
                 */
                popupEntity.popupWidth()
                        .doubleValue(),

                popupEntity.popupHeight()
                        .doubleValue(),

                popupEntity.widthRatio()
                        .doubleValue(),

                popupEntity.heightRatio()
                        .doubleValue(),

                popupEntity.minimumWidth()
                        .doubleValue(),

                popupEntity.minimumHeight()
                        .doubleValue(),

                popupEntity.maximumWidth()
                        .doubleValue(),

                popupEntity.maximumHeight()
                        .doubleValue(),

                /*
                 * Oracle Y/N 값을
                 * JSON boolean 값으로 변환한다.
                 */
                isYes(
                        popupEntity.showHeaderYn()),

                isYes(
                        popupEntity.showCloseButtonYn()),

                isYes(
                        popupEntity.showFooterYn()),

                isYes(
                        popupEntity.showDontShowYn()),

                /*
                 * 종류별 content JSON 객체
                 */
                content
        );
    }

    /*
     * Oracle의 Y/N 값을 boolean으로 변환한다.
     *
     * Y
     * → true
     *
     * N 또는 null
     * → false
     */
    private boolean isYes(
            String value) {

        return "Y".equalsIgnoreCase(
                value);
    }

    /*
     * CONTENT_JSON 문자열을
     * Map<String, Object>로 변환한다.
     */
    private Map<String, Object> parseContentJson(
            String popupId,
            String contentJson) {

        /*
         * CONTENT_JSON이 비어 있으면
         * 잘못 저장된 팝업 데이터이므로 예외를 발생시킨다.
         */
        if (contentJson == null ||
                contentJson.isBlank()) {

            throw new IllegalStateException(
                    "팝업 content JSON이 비어 있습니다. " +
                            "popupId=" +
                            popupId);
        }

        try {
            /*
             * JSON 문자열을 Map으로 변환한다.
             *
             * 예:
             * {"contentTitle":"안내"}
             *
             * ↓
             *
             * Map<String, Object>
             */
            return objectMapper.readValue(
                    contentJson,
                    new TypeReference<Map<String, Object>>() {
                    });

        } catch (Exception exception) {

            /*
             * 잘못된 JSON이 DB에 저장된 경우
             * 어떤 팝업에서 오류가 났는지 확인할 수 있도록
             * PopupId를 포함하여 예외를 발생시킨다.
             */
            throw new IllegalStateException(
                    "팝업 content JSON 변환에 실패했습니다. " +
                            "popupId=" +
                            popupId,
                    exception);
        }
    }
}