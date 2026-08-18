/**
 * WPF 클라이언트와 주고받는 JSON 형식을 정의하는 DTO 모음이다.
 *
 * <p>Request DTO는 {@code @NotBlank}, {@code @NotNull} 등의 Bean Validation으로
 * 잘못된 요청을 Controller 진입 전에 차단한다. Response DTO는 Service가 만든
 * 결과를 Jackson이 camelCase JSON으로 직렬화할 때 사용한다.</p>
 *
 * <p>DTO에는 DB 저장용 감사 컬럼이나 정답 여부를 넣지 않는다. 특히 문제 정답은
 * 클라이언트에 노출하지 않고 Service에서만 채점한다.</p>
 */
package com.oksy.popup.dto;
