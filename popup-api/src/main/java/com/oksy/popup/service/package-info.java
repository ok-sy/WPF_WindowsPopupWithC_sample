/**
 * 팝업 시스템의 핵심 업무 규칙을 처리하는 Service 계층이다.
 *
 * <p>대상 사용자 검증, DB Entity의 WPF DTO 변환, 설문 필수값 검증과 채점,
 * 영상 완료율 계산, 표시·닫기 상태 저장을 담당한다. 여러 DB 작업이 하나의 결과로
 * 함께 성공해야 하는 메서드는 {@code @Transactional}로 묶어 중간 저장을 방지한다.</p>
 */
package com.oksy.popup.service;
