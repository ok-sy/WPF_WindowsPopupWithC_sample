/**
 * Java 메서드와 MyBatis XML의 PostgreSQL SQL을 연결하는 데이터 접근 계층이다.
 *
 * <p>인터페이스 메서드 이름은 XML의 select/insert/update/delete id와 같아야 한다.
 * {@code @Param} 이름은 XML의 {@code #{...}}와 일치해야 한다. Mapper는 SQL 실행만
 * 담당하고 대상 판정 이후의 JSON 조립, 채점 등의 업무 규칙은 Service가 담당한다.</p>
 */
package com.oksy.popup.mapper;
