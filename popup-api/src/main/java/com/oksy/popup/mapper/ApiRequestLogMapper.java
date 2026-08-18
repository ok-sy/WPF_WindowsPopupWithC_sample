package com.oksy.popup.mapper;

import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;

import java.time.OffsetDateTime;

/** API_REQUEST_LOG 저장 SQL을 호출한다. */
@Mapper
public interface ApiRequestLogMapper {

    int insertRequestLog(
            @Param("clientRequestId") String clientRequestId,
            @Param("userId") String userId,
            @Param("popupId") String popupId,
            @Param("apiPath") String apiPath,
            @Param("httpMethod") String httpMethod,
            @Param("clientIp") String clientIp,
            @Param("requestReceivedAt") OffsetDateTime requestReceivedAt,
            @Param("responseCompletedAt") OffsetDateTime responseCompletedAt,
            @Param("elapsedMilliseconds") long elapsedMilliseconds,
            @Param("httpStatusCode") int httpStatusCode,
            @Param("successYn") String successYn,
            @Param("errorCode") String errorCode,
            @Param("requestSummary") String requestSummary
    );
}
