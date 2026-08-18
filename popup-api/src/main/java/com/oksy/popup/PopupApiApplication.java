package com.oksy.popup;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

/**
 * 팝업 REST API의 실행 시작점이다.
 *
 * <p>{@code SpringApplication.run}이 내장 Tomcat을 시작하고 이 클래스 아래의
 * controller, service, mapper, config 패키지를 탐색하여 Spring Bean으로 등록한다.
 * 비즈니스 로직은 두지 않고 애플리케이션 부팅만 담당한다.</p>
 */
@SpringBootApplication
public class PopupApiApplication {

    /** Java 프로세스로 API 서버를 시작한다. */
    public static void main(String[] args) {
		SpringApplication.run(PopupApiApplication.class, args);
	}

}
