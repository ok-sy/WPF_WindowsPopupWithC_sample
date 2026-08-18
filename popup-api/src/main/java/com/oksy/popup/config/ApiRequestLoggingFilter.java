package com.oksy.popup.config;

import com.oksy.popup.mapper.ApiRequestLogMapper;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

import java.io.IOException;
import java.time.OffsetDateTime;
import java.time.temporal.ChronoUnit;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * 모든 HTTP 요청의 시작과 종료를 감싸 API_REQUEST_LOG에 감사 로그를 남긴다.
 *
 * <p>OncePerRequestFilter이므로 한 요청에 한 번만 실행된다. Controller 실행 전
 * 시작 시각을 저장하고 실행 후 상태코드와 경과시간을 기록한다. 로그 INSERT가
 * 실패해도 원래 API 응답은 정상 반환되어야 하므로 저장 예외는 내부에서 차단한다.</p>
 */
@Component
public class ApiRequestLoggingFilter extends OncePerRequestFilter {

    private static final Pattern POPUP_PATH = Pattern.compile(
            "^/api/popups/([^/]+)/(hide|responses|video-progress|events)$");

    private final ApiRequestLogMapper logMapper;

    public ApiRequestLoggingFilter(ApiRequestLogMapper logMapper) {
        this.logMapper = logMapper;
    }

    @Override
    protected void doFilterInternal(
            HttpServletRequest request,
            HttpServletResponse response,
            FilterChain filterChain) throws ServletException, IOException {

        OffsetDateTime startedAt = OffsetDateTime.now();
        try {
            filterChain.doFilter(request, response);
        } finally {
            OffsetDateTime completedAt = OffsetDateTime.now();
            long elapsed = Math.max(0,
                    ChronoUnit.MILLIS.between(startedAt, completedAt));
            saveLogSafely(request, response, startedAt, completedAt, elapsed);
        }
    }

    private void saveLogSafely(
            HttpServletRequest request,
            HttpServletResponse response,
            OffsetDateTime startedAt,
            OffsetDateTime completedAt,
            long elapsed) {
        try {
            int status = response.getStatus();
            String userId = firstNonBlank(
                    request.getParameter("userId"),
                    request.getHeader("X-User-Id"));
            String query = request.getQueryString();

            logMapper.insertRequestLog(
                    blankToNull(request.getHeader("X-Client-Request-Id")),
                    blankToNull(userId),
                    extractPopupId(request.getRequestURI()),
                    limit(request.getRequestURI(), 1000),
                    limit(request.getMethod(), 10),
                    limit(resolveClientIp(request), 100),
                    startedAt, completedAt, elapsed, status,
                    status < 400 ? "Y" : "N",
                    status < 400 ? null : "HTTP_" + status,
                    limit(query, 2000));
        } catch (Exception ignored) {
            // 로그 저장 장애가 실제 API 응답까지 실패시키면 안 된다.
        }
    }

    private String extractPopupId(String path) {
        Matcher matcher = POPUP_PATH.matcher(path);
        return matcher.matches() ? matcher.group(1) : null;
    }

    private String resolveClientIp(HttpServletRequest request) {
        String forwarded = request.getHeader("X-Forwarded-For");
        if (forwarded != null && !forwarded.isBlank()) {
            return forwarded.split(",")[0].trim();
        }
        return request.getRemoteAddr();
    }

    private String firstNonBlank(String first, String second) {
        return first != null && !first.isBlank() ? first : second;
    }

    private String blankToNull(String value) {
        return value == null || value.isBlank() ? null : value.trim();
    }

    private String limit(String value, int maxLength) {
        if (value == null) {
            return null;
        }
        return value.length() <= maxLength
                ? value : value.substring(0, maxLength);
    }
}
