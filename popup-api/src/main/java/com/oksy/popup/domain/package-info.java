/**
 * PostgreSQL 조회 결과와 서버 내부 계산에 사용하는 도메인 객체 모음이다.
 *
 * <p>이 패키지의 {@code *Entity} record는 주로 DB 테이블 한 행과 대응한다.
 * Controller가 직접 반환하지 않고 Mapper가 값을 채운 뒤 Service가 DTO로 변환한다.
 * 따라서 DB 컬럼명과 타입을 보존하는 것이 목적이며, WPF JSON 규격과는 분리한다.</p>
 *
 * <p>{@code PopupSubmissionContext}, {@code VideoPopupContext}는 전체 테이블을
 * 읽지 않고 검증에 필요한 일부 컬럼만 조회하는 경량 객체다.</p>
 */
package com.oksy.popup.domain;
