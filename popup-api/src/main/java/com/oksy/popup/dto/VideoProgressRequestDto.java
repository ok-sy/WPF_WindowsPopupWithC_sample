package com.oksy.popup.dto;

import jakarta.validation.constraints.DecimalMin;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.math.BigDecimal;

/**
 * WPF 영상 플레이어가 주기적으로 보내는 시청 상태다.
 * 서버는 duration과 watchedSeconds로 비율을 다시 계산하므로 클라이언트 비율을 받지 않는다.
 */
public record VideoProgressRequestDto(
        @NotBlank String userId,
        @NotNull @DecimalMin("0.001") BigDecimal durationSeconds,
        @NotNull @DecimalMin("0.0") BigDecimal positionSeconds,
        @NotNull @DecimalMin("0.0") BigDecimal maximumPositionSeconds,
        @NotNull @DecimalMin("0.0") BigDecimal watchedSeconds
) {
}
