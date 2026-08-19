package server.web.api.popup;

import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import server.domain.popup.PopupResponseDto;
import server.service.core.popup.PopupService;

import java.util.List;

/** WPF 팝업 클라이언트가 사용하는 공개 조회 API다. */
@Tag(name = "Popup")
@RestController
@RequestMapping("/api/popups")
public class PopupController {

    private final PopupService popupService;

    public PopupController(PopupService popupService) {
        this.popupService = popupService;
    }

    /** 기존 popup-api와 같은 JSON 배열 계약을 유지한다. */
    @Operation(summary = "사용자에게 표시할 팝업 목록 조회")
    @GetMapping
    public List<PopupResponseDto> getPopups(@RequestParam String userId) {
        return popupService.getPopups(userId);
    }
}
