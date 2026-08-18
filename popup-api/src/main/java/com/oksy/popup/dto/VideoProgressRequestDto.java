package com.oksy.popup.dto;

import jakarta.validation.constraints.DecimalMin;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.math.BigDecimal;

/** WPF 영상 플레이어가 서버에 저장할 현재 시청 상태다. */
public record VideoProgressRequestDto(
        @NotBlank String userId,
        @NotNull @DecimalMin("0.001") BigDecimal durationSeconds,
        @NotNull @DecimalMin("0.0") BigDecimal positionSeconds,
        @NotNull @DecimalMin("0.0") BigDecimal maximumPositionSeconds,
        @NotNull @DecimalMin("0.0") BigDecimal watchedSeconds
) {
}
