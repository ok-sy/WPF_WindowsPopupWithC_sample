package com.oksy.popup.controller;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import java.time.OffsetDateTime;
import java.util.LinkedHashMap;
import java.util.Map;

/** 잘못된 API 요청을 WPF가 읽기 쉬운 400 JSON으로 변환한다. */
@RestControllerAdvice
public class ApiExceptionHandler {

    @ExceptionHandler(IllegalArgumentException.class)
    public ResponseEntity<Map<String, Object>> handleIllegalArgument(
            IllegalArgumentException exception) {
        return badRequest(exception.getMessage());
    }

    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<Map<String, Object>> handleValidation(
            MethodArgumentNotValidException exception) {
        String message = exception.getBindingResult().getFieldErrors().stream()
                .findFirst()
                .map(error -> error.getField() + ": " + error.getDefaultMessage())
                .orElse("요청값이 올바르지 않습니다.");
        return badRequest(message);
    }

    private ResponseEntity<Map<String, Object>> badRequest(String message) {
        Map<String, Object> body = new LinkedHashMap<>();
        body.put("timestamp", OffsetDateTime.now());
        body.put("status", HttpStatus.BAD_REQUEST.value());
        body.put("error", "BAD_REQUEST");
        body.put("message", message);
        return ResponseEntity.badRequest().body(body);
    }
}
