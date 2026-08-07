package com.oksy.popup.controller;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.time.OffsetDateTime;
import java.util.LinkedHashMap;
import java.util.Map;

/*
 * WPF와 Spring Boot 사이의
 * 기본 HTTP 통신 상태를 확인하는 Controller다.
 *
 * 아직 Oracle은 사용하지 않고
 * 서버가 정상 실행되는지만 확인한다.
 */
@RestController
@RequestMapping("/api/health")
public class HealthController {

    /*
     * GET /api/health 요청을 처리한다.
     *
     * 브라우저, Postman 또는 WPF가 요청하면
     * 서버 상태를 JSON으로 반환한다.
     */
    @GetMapping
    public Map<String, Object> getHealth() {

        /*
         * 응답 JSON의 항목 순서를 유지하기 위해
         * LinkedHashMap을 사용한다.
         */
        Map<String, Object> response =
                new LinkedHashMap<>();

        /*
         * 서버가 정상적으로 요청을 처리했다는 상태값
         */
        response.put(
                "status",
                "UP");

        /*
         * 어떤 프로그램의 응답인지 구분하기 위한 이름
         */
        response.put(
                "service",
                "popup-api");

        /*
         * 서버가 응답을 만든 현재 시각
         */
        response.put(
                "serverTime",
                OffsetDateTime.now());

        return response;
    }
}