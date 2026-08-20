package server.web.api.popup;

import cl.cloverframework.api.CLNewApiResponse;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import server.service.core.popup.PopupService;
import server.web.api.payload.popup.PopupAdminPayloads;
import server.web.support.ApiBaseController;

/**
 * 로그인한 zero-rule-web 관리자가 사용하는 팝업 관리 API다.
 * WPF 공개 API와 달리 /apis/** 경로를 사용해 기존 인증 필터를 거친다.
 */
@Tag(name = "Popup Admin")
@RestController
@RequestMapping("/apis/popup")
public class PopupAdminController extends ApiBaseController {

    private final PopupService popupService;

    public PopupAdminController(PopupService popupService) {
        this.popupService = popupService;
    }

    /** 비활성·기간 만료 팝업을 포함한 관리자용 전체 목록을 조회한다. */
    @Operation(summary = "관리자 팝업 전체 목록 조회")
    @PostMapping("/list")
    public CLNewApiResponse<PopupAdminPayloads.PopupListResponse> getPopups() {
        return resultMsg(
                "BE00000001",
                PopupAdminPayloads.PopupListResponse.builder()
                        .popups(popupService.getAdminPopups())
                        .build());
    }
}
