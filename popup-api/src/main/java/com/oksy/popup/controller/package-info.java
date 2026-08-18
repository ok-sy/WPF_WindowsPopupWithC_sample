/**
 * HTTP 주소와 Java 메서드를 연결하는 REST Controller 계층이다.
 *
 * <p>Controller는 요청을 DTO로 변환하고 {@code @Valid} 검사를 실행한 뒤
 * 실제 업무 처리를 Service에 위임한다. SQL이나 채점 규칙은 이 계층에 두지 않는다.</p>
 *
 * <p>{@code ApiExceptionHandler}는 예외를 공통 JSON 오류 응답으로 변환하여
 * WPF가 실패 원인을 일정한 형식으로 처리할 수 있게 한다.</p>
 */
package com.oksy.popup.controller;
