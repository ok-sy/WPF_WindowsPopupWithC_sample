package server.web.api.payload.popup;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;
import lombok.Builder;
import lombok.Data;
import server.domain.popup.AdminPopupListItemDto;
import server.domain.popup.PopupResponseDto;

import java.util.List;

/** 관리자 팝업 API의 요청·응답 형식을 모아 둔다. */
public final class PopupAdminPayloads {

    private PopupAdminPayloads() {
    }

    /** 팝업 목록 화면에 전달하는 응답 본문이다. */
    @Schema(description = "관리자 팝업 목록 응답")
    @Builder
    @Data
    public static class PopupListResponse {
        @Schema(description = "등록된 전체 팝업 목록")
        private List<AdminPopupListItemDto> popups;
    }

    /** 목록에서 선택한 팝업의 상세조회 요청이다. */
    @Schema(description = "관리자 팝업 상세조회 요청")
    @Data
    public static class PopupInfoRequest {
        @NotBlank
        @Size(max = 50)
        @Schema(description = "팝업 ID", example = "SAMPLE-TEXT-001")
        private String popupId;
    }

    /** 편집 폼과 CSS 미리보기가 함께 사용하는 상세 데이터다. */
    @Schema(description = "관리자 팝업 상세조회 응답")
    @Builder
    @Data
    public static class PopupInfoResponse {
        @Schema(description = "팝업 표시 설정과 유형별 콘텐츠")
        private PopupResponseDto popup;
    }
}
