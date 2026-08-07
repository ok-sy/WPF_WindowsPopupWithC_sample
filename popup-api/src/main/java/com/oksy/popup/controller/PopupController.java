package com.oksy.popup.controller;

import com.oksy.popup.dto.PopupResponseDto;
import com.oksy.popup.service.PopupService;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

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
     * Oracle 연결 후:
     * → 사용자에게 표시할 DB 팝업 목록을 반환
     */
    @GetMapping
    public List<PopupResponseDto> getPopups() {

        /*
         * PopupService에서 조회한 팝업 목록을
         * 별도 변환 없이 JSON 응답으로 반환한다.
         */
        return popupService.getPopups();
    }
}