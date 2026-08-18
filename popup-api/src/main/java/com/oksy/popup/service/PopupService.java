package com.oksy.popup.service;

import com.oksy.popup.domain.PopupEntity;
import com.oksy.popup.dto.PopupResponseDto;
import com.oksy.popup.mapper.PopupMapper;
import org.springframework.stereotype.Service;
import tools.jackson.core.type.TypeReference;
import tools.jackson.databind.ObjectMapper;
import com.oksy.popup.dto.PopupHideRequestDto;
import com.oksy.popup.dto.PopupHideResponseDto;
import org.springframework.transaction.annotation.Transactional;

import java.time.OffsetDateTime;
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
     * 사용자의 숨김 상태를 반영하여
     * 현재 노출 가능한 팝업 목록을 조회한다.
     */
    public List<PopupResponseDto> getPopups(
            String userId) {

        /*
         * 빈 사용자 ID로 조회되는 것을 방지한다.
         */
        if (userId == null ||
                userId.isBlank()) {

            throw new IllegalArgumentException(
                    "사용자 ID는 필수입니다.");
        }

        /*
         * 앞뒤 공백을 제거한 사용자 ID로
         * Oracle 팝업 목록을 조회한다.
         */
        String normalizedUserId =
                userId.trim();

        /*
         * 사용자 숨김 상태와 노출 기간을 반영하여
         * 표시 가능한 팝업만 조회한다.
         */
        List<PopupEntity> popupEntities =
                popupMapper.selectAvailablePopups(
                        normalizedUserId);

        /*
         * DB 조회 결과를 WPF 응답 DTO로 변환한다.
         */
        return popupEntities.stream()
                .map(this::toResponseDto)
                .toList();
    }


    /*
     * 사용자가 선택한 팝업을
     * 지정한 기간 동안 숨김 처리한다.
     *
     * 하나의 트랜잭션 안에서:
     *
     * 1. 사용자 숨김 상태 MERGE
     * 2. 실제 숨김 만료 일시 조회
     *
     * 를 순서대로 실행한다.
     */
    @Transactional
    public PopupHideResponseDto hidePopup(
            String popupId,
            PopupHideRequestDto requestDto) {

        /*
         * URL에서 받은 PopupId가
         * null 또는 빈 문자열인지 검사한다.
         */
        if (popupId == null ||
                popupId.isBlank()) {

            throw new IllegalArgumentException(
                    "팝업 ID는 필수입니다.");
        }

        /*
         * 사용자 ID 앞뒤의 불필요한 공백을 제거한다.
         *
         * DTO의 @NotBlank 검사는 Controller에서
         * @Valid를 적용했을 때 실행된다.
         */
        String normalizedUserId =
                requestDto.userId()
                        .trim();

        /*
         * 팝업 ID 앞뒤의 불필요한 공백을 제거한다.
         */
        String normalizedPopupId =
                popupId.trim();

        /*
         * USER_POPUP_STATE 테이블에
         * 사용자 팝업 숨김 상태를 저장한다.
         *
         * 기존 데이터가 없으면 INSERT,
         * 기존 데이터가 있으면 UPDATE한다.
         */
        int affectedRows =
                popupMapper.upsertPopupHide(
                        normalizedUserId,
                        normalizedPopupId,
                        requestDto.hideDays());

        /*
         * MERGE 결과로 변경된 행이 없다면
         * 정상적으로 저장되지 않은 상태이므로 예외를 발생시킨다.
         */
        if (affectedRows <= 0) {

            throw new IllegalStateException(
                    "팝업 숨김 상태 저장에 실패했습니다. " +
                            "userId=" +
                            normalizedUserId +
                            ", popupId=" +
                            normalizedPopupId);
        }

        /*
         * Oracle의 SYSTIMESTAMP를 기준으로 계산된
         * 실제 숨김 만료 일시를 다시 조회한다.
         *
         * Java 시각을 따로 계산하지 않기 때문에
         * DB 저장값과 API 응답값이 정확히 일치한다.
         */
        OffsetDateTime hiddenUntil =
                popupMapper.selectHiddenUntil(
                        normalizedUserId,
                        normalizedPopupId);

        /*
         * MERGE는 성공했지만 숨김 만료 일시가 없다면
         * 저장 결과가 비정상인 상태다.
         */
        if (hiddenUntil == null) {

            throw new IllegalStateException(
                    "팝업 숨김 만료 일시를 조회할 수 없습니다. " +
                            "userId=" +
                            normalizedUserId +
                            ", popupId=" +
                            normalizedPopupId);
        }

        /*
         * WPF에 반환할 숨김 처리 결과를 생성한다.
         */
        return new PopupHideResponseDto(
                normalizedUserId,
                normalizedPopupId,
                "UNTIL",
                hiddenUntil
        );
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

                popupEntity.questionTemplateId(),
                popupEntity.periodMode(),
                popupEntity.repeatInterval(),
                popupEntity.repeatDayOfWeek(),
                popupEntity.repeatDayOfMonth(),
                popupEntity.hideDays(),
                toNullableDouble(
                        popupEntity.completionRatio()),
                toNullableDouble(
                        popupEntity.passingScore()),
                isYes(
                        popupEntity.allowCloseBeforeCompleteYn()),
                List.of(),

                /*
                 * 종류별 content JSON 객체
                 */
                content
        );
    }

    private Double toNullableDouble(
            java.math.BigDecimal value) {

        return value == null
                ? null
                : value.doubleValue();
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
