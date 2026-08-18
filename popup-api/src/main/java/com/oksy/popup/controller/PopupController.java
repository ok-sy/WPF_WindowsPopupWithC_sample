package com.oksy.popup.controller;

import com.oksy.popup.dto.PopupResponseDto;
import com.oksy.popup.service.PopupService;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.bind.annotation.RequestParam;
import com.oksy.popup.dto.PopupHideRequestDto;
import com.oksy.popup.dto.PopupHideResponseDto;
import com.oksy.popup.dto.PopupSubmitRequestDto;
import com.oksy.popup.dto.PopupSubmitResponseDto;
import com.oksy.popup.dto.VideoProgressRequestDto;
import com.oksy.popup.dto.VideoProgressResponseDto;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import java.util.List;

/*
 * WPF 프로그램에서 요청하는
 * 팝업 관련 HTTP API를 제공하는 Controller다.
 *
 * Controller는 HTTP 요청과 응답을 담당하고,
 * 실제 팝업 조회 및 가공은 PopupService에 맡긴다.
 */
@RestController
@RequestMapping("/api/popups")
public class PopupController {

    /*
     * 팝업 조회 기능을 담당하는 Service다.
     */
    private final PopupService popupService;

    /*
     * 생성자 주입 방식으로
     * Spring이 관리하는 PopupService를 전달받는다.
     *
     * 필드 주입보다 필요한 객체가 명확하고
     * 테스트하기 편한 방식이다.
     */
    public PopupController(
            PopupService popupService) {

        this.popupService =
                popupService;
    }

    /*
     * GET /api/popups 요청을 처리한다.
     *
     * 현재:
     * → PopupService에서 만든 테스트 데이터를 반환
     *
     * PostgreSQL 연결 후:
     * → 사용자에게 표시할 DB 팝업 목록을 반환
     */
    /*
     * GET /api/popups?userId=TEST_USER 요청을 처리한다.
     *
     * 사용자 ID를 기준으로
     * 숨김 처리되지 않은 팝업 목록만 반환한다.
     */
    @GetMapping
    public List<PopupResponseDto> getPopups(

            /*
             * URL Query Parameter에서
             * 사용자 ID를 전달받는다.
             */
            @RequestParam
            String userId) {

        /*
         * 사용자 ID를 PopupService로 전달하여
         * 사용자별 노출 가능 팝업을 조회한다.
         */
        return popupService.getPopups(
                userId);
    }
    /*
     * POST /api/popups/{popupId}/hide 요청을 처리한다.
     *
     * 사용자가 WPF에서 '30일간 보지 않기'를 선택하면
     * 해당 사용자와 팝업의 숨김 만료 일시를 PostgreSQL에 저장한다.
     */
    @PostMapping("/{popupId}/hide")
    public PopupHideResponseDto hidePopup(

            /*
             * URL 경로에서 숨길 팝업 ID를 전달받는다.
             *
             * 예:
             * /api/popups/JAVA_TEXT_TEST_001/hide
             */
            @PathVariable
            String popupId,

            /*
             * HTTP 요청 본문의 JSON을
             * PopupHideRequestDto로 변환한다.
             *
             * @Valid를 통해 userId와 hideDays의
             * 유효성 검사를 실행한다.
             */
            @Valid
            @RequestBody
            PopupHideRequestDto requestDto) {

        /*
         * 실제 PostgreSQL 저장 처리는
         * PopupService에 위임한다.
         */
        return popupService.hidePopup(
                popupId,
                requestDto);
    }

    /**
     * POST /api/popups/{popupId}/responses
     * 설문 답안을 서버에서 검증·채점한 뒤 PostgreSQL에 저장한다.
     */
    @PostMapping("/{popupId}/responses")
    public PopupSubmitResponseDto submitResponse(
            @PathVariable String popupId,
            @Valid @RequestBody PopupSubmitRequestDto requestDto) {

        return popupService.submitResponse(popupId, requestDto);
    }

    /** POST /api/popups/{popupId}/video-progress */
    @PostMapping("/{popupId}/video-progress")
    public VideoProgressResponseDto saveVideoProgress(
            @PathVariable String popupId,
            @Valid @RequestBody VideoProgressRequestDto requestDto) {
        return popupService.saveVideoProgress(popupId, requestDto);
    }
}
