package server.web.api.payload.popup;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Builder;
import lombok.Data;
import server.domain.popup.AdminPopupListItemDto;

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
}
