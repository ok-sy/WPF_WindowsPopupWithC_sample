# Popup API 소스 가이드

이 문서는 처음 프로젝트를 보는 개발자가 파일을 열기 전에 전체 흐름과 각 소스의 목적을 찾을 수 있도록 정리한 안내서다. 소스 내부의 JavaDoc 및 SQL 주석과 함께 본다.

## 1. 전체 호출 흐름

```text
WPF HTTP 요청
→ ApiRequestLoggingFilter
→ PopupController
→ PopupService
→ PopupMapper / ApiRequestLogMapper
→ MyBatis XML
→ PostgreSQL
→ DTO JSON 응답
```

- Controller: URL, 요청 JSON, 응답 JSON 담당
- Service: 검증, 채점, 완료 판정 등 업무 규칙 담당
- Mapper: Java와 SQL 연결
- Domain: DB 조회 결과와 서버 내부 데이터
- DTO: WPF와 주고받는 데이터
- Config: 모든 요청에 공통 적용되는 처리

## 2. 실행과 공통 처리

| 파일 | 목적 | 주요 함수 |
|---|---|---|
| `PopupApiApplication.java` | Spring Boot와 내장 Tomcat 시작 | `main`: API 프로세스 실행 |
| `application.yaml` | 포트, PostgreSQL, MyBatis XML 위치 설정 | 환경변수가 있으면 환경변수를 우선 사용 |
| `ApiRequestLoggingFilter.java` | 모든 요청의 처리시간과 결과를 감사 로그로 저장 | `doFilterInternal`: 요청 전후 측정, `saveLogSafely`: DB 로그 저장 |
| `ApiExceptionHandler.java` | Java 예외를 공통 오류 JSON으로 변환 | `handleIllegalArgument`, `handleValidation`, `handleUnexpected` |
| `HealthController.java` | 서버가 실행 중인지 확인 | `health`: 상태·서비스명·서버시각 반환 |

## 3. 팝업 API 진입점

`PopupController.java`는 다음 URL을 제공한다.

| 함수 | HTTP API | 역할 |
|---|---|---|
| `getPopups` | `GET /api/popups?userId=...` | 사용자에게 표시할 팝업 조회 |
| `hidePopup` | `POST /api/popups/{id}/hide` | 일정 기간 다시 보지 않기 저장 |
| `submitResponse` | `POST /api/popups/{id}/responses` | 설문 답안 제출 및 서버 채점 |
| `saveVideoProgress` | `POST /api/popups/{id}/video-progress` | 영상 시청률과 완료 여부 저장 |
| `recordPopupEvent` | `POST /api/popups/{id}/events` | 팝업 표시·닫기 기록 |
| `getPopupStatuses` | `GET /api/popups/statuses?userId=...` | 사용자별 표시·숨김·완료 상태 조회 |

Controller는 직접 SQL을 실행하지 않는다. `@Valid`로 요청 형식을 확인하고 `PopupService`에 처리를 맡긴다.

## 4. 핵심 업무 처리

`PopupService.java`의 공개 함수 역할은 다음과 같다.

| 함수 | 처리 순서 |
|---|---|
| `getPopups` | 대상·기간·숨김 조건 조회 → 템플릿 문항·선택지 조회 → WPF DTO 조립 |
| `hidePopup` | 사용자·팝업 키로 `user_popup_status` UPSERT → 실제 숨김 만료시각 반환 |
| `submitResponse` | 제출 대상 검증 → 필수/중복/소속 검증 → 서버 정답 채점 → 세 응답 테이블 저장 → 완료 상태 저장 |
| `saveVideoProgress` | 영상 위치 검증 → 누적 시청률 계산 → DB 기준 완료비율 비교 → 진행률 및 완료 상태 저장 |
| `recordPopupEvent` | `DISPLAYED` 또는 `CLOSED` 검증 → 표시 횟수/시각 또는 닫은 시각 저장 |
| `getPopupStatuses` | 사용자별 `user_popup_status` 목록 반환 |

`@Transactional`이 붙은 함수는 중간 SQL에서 오류가 발생하면 그 함수에서 변경한 DB 내용을 전부 되돌린다.

## 5. Mapper와 SQL

| 파일 | 역할 |
|---|---|
| `PopupMapper.java` | 팝업·설문·영상·상태 SQL의 Java 메서드 선언 |
| `PopupMapper.xml` | 대상 조건, 질문 조회, 숨김, 응답 저장, 영상 진행률, 생명주기 SQL |
| `ApiRequestLogMapper.java` | 요청 로그 INSERT 메서드 선언 |
| `ApiRequestLogMapper.xml` | `api_request_log` INSERT SQL |

`PopupMapper.xml`의 핵심 규칙:

- 대상 조건 하나의 그룹 안에서는 `AND`
- 여러 대상 그룹 사이에서는 `OR`
- 하위 부서 포함은 재귀 CTE로 계산
- 정답 컬럼은 서버 채점에서만 사용하고 조회 응답에는 포함하지 않음
- UPSERT는 PostgreSQL `ON CONFLICT` 사용
- 이미 완료된 상태는 이후 낮은 진행률이나 닫기 이벤트가 취소하지 않음

## 6. Domain 파일

| 파일 | 대응 데이터 |
|---|---|
| `PopupEntity` | 팝업 조회 결과 한 건 |
| `QuestionTemplateEntity` | 질문 템플릿 버전 |
| `PopupQuestionEntity` | 질문 한 건과 점수 설정 |
| `PopupOptionEntity` | 선택지와 서버 정답 여부 |
| `PopupTargetEntity` | 팝업의 전체 대상 그룹 묶음 |
| `PopupTargetGroupEntity` | OR로 연결되는 대상 그룹 한 개 |
| `PopupTargetConditionEntity` | 그룹 안에서 AND로 판정할 조건 한 개 |
| `PopupResponseEntity` | 설문 제출 결과 |
| `PopupAnswerEntity` | 제출 결과의 문항별 답과 획득점수 |
| `PopupAnswerValueEntity` | 선택형 문항에서 고른 선택지 |
| `PopupSubmissionContext` | 설문 제출 검증에 필요한 일부 팝업 설정 |
| `VideoPopupContext` | 영상 완료 판정에 필요한 일부 팝업 설정 |

Entity는 DB 구조를 표현한다. WPF로 직접 반환하지 않는다.

## 7. DTO 파일

| 구분 | 파일 |
|---|---|
| 팝업 조회 | `PopupResponseDto`, `PopupQuestionDto`, `PopupOptionDto` |
| 숨김 | `PopupHideRequestDto`, `PopupHideResponseDto` |
| 설문 제출 | `PopupSubmitRequestDto`, `PopupSubmitAnswerDto`, `PopupSubmitResponseDto` |
| 영상 진행률 | `VideoProgressRequestDto`, `VideoProgressResponseDto` |
| 표시·닫기 이벤트 | `PopupEventRequestDto`, `PopupEventResponseDto` |
| 사용자 상태 | `UserPopupStatusDto` |

Request DTO는 WPF가 보내는 JSON이고 Response DTO는 서버가 반환하는 JSON이다. 목록 필드는 record 생성 시 복사하여 외부에서 내용을 임의 변경하지 못하게 한다.

## 8. 수정할 때 지켜야 할 흐름

새 API를 추가할 때는 일반적으로 다음 순서로 작업한다.

1. 요청·응답 DTO 작성
2. Controller에 URL 함수 추가
3. Service에 검증 및 업무 로직 추가
4. Mapper 인터페이스에 DB 메서드 추가
5. 같은 이름의 MyBatis XML SQL 추가
6. 트랜잭션 필요 여부 확인
7. WPF JSON 필드명과 Java DTO 필드명 확인

DB 컬럼이 바뀌면 DDL, Entity, Mapper XML resultMap을 함께 확인해야 한다. API JSON이 바뀌면 Java DTO와 WPF DTO를 함께 수정해야 한다.
