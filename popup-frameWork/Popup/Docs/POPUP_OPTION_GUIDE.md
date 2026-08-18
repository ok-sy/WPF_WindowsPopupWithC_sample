# WPF 팝업 옵션 가이드

## 1. 문서 목적

이 문서는 Oracle `POPUP_NOTICE` 테이블과 Java API를 통해 WPF 팝업을 생성할 때 사용하는 옵션을 설명한다.

지원하는 팝업 종류는 다음과 같다.

| `popupType` | 용도 |
|---|---|
| `TEXT` | 제목, 본문, 강조 문구로 구성된 안내 팝업 |
| `IMAGE` | 이미지와 설명을 표시하는 팝업 |
| `VIDEO` | 동영상과 재생 컨트롤을 표시하는 팝업 |
| `SURVEY` | 평가, 객관식, 주관식 설문 팝업 |
| `QUIZ` | 정답과 통과 점수가 있는 퀴즈 팝업 |

전체 데이터 흐름은 다음과 같다.

```text
Oracle POPUP_NOTICE
→ Java MyBatis
→ Java REST API
→ WPF PopupResponseDto
→ PopupFactory
→ PopupOptions + 타입별 View
→ PopupWindow
```

---

## 2. 공통 응답 구조

모든 팝업은 다음과 같은 공통 구조를 사용한다.

```json
{
  "popupId": "POPUP_001",
  "popupType": "TEXT",
  "title": "서비스 안내",
  "displayStartAt": "2026-08-01T00:00:00+09:00",
  "displayEndAt": "2026-09-01T23:59:59+09:00",
  "displayMode": "SEQUENTIAL",
  "sizeMode": "VIEWPORT_RATIO",
  "width": 900,
  "height": 620,
  "widthRatio": 0.7,
  "heightRatio": 0.75,
  "minimumWidth": 480,
  "minimumHeight": 320,
  "maximumWidth": 1200,
  "maximumHeight": 900,
  "showHeader": true,
  "showCloseButton": true,
  "showFooter": true,
  "showDoNotShowAgain": false,
  "content": {}
}
```

### 2.1 식별 및 노출 기간

| JSON 옵션 | Oracle 컬럼 | 형식 | 필수 | 설명 |
|---|---|---:|---:|---|
| `popupId` | `POPUP_ID` | 문자열 | O | 팝업 고유 ID. 숨김 저장과 로그의 기준값 |
| `popupType` | `POPUP_TYPE` | 문자열 | O | `TEXT`, `IMAGE`, `VIDEO`, `SURVEY`, `QUIZ` |
| `title` | `TITLE` | 문자열 | O | 공통 Header에 표시할 제목 |
| `displayStartAt` | `DISPLAY_START_AT` | 일시 | X | 노출 시작 일시. `null`이면 시작 제한 없음 |
| `displayEndAt` | `DISPLAY_END_AT` | 일시 | X | 노출 종료 일시. `null`이면 종료 제한 없음 |

노출 가능 조건은 다음과 같다.

```text
USE_YN = Y
AND 현재 시각 >= DISPLAY_START_AT
AND 현재 시각 <= DISPLAY_END_AT
AND 사용자 숨김 기간이 만료되었거나 숨김 기록이 없음
```

### 2.2 표시 방식

| `displayMode` | 동작 |
|---|---|
| `SEQUENTIAL` | 한 팝업을 닫으면 다음 팝업을 표시 |
| `SIMULTANEOUS` | 해당 팝업들을 각각의 창으로 동시에 표시 |

동시에 띄울 팝업은 모두 다음 값을 사용한다.

```json
"displayMode": "SIMULTANEOUS"
```

`SHOW_ALL`은 현재 허용값이 아니다.

### 2.3 창 크기 방식

| `sizeMode` | 동작 | 사용하는 옵션 |
|---|---|---|
| `FIXED` | 지정한 고정 크기 사용 | `width`, `height` |
| `VIEWPORT_RATIO` | 모니터 작업 영역 비율로 계산 | 비율 및 최소·최대 옵션 |
| `FULLSCREEN` | 팝업이 열리는 모니터 전체를 사용 | 다른 크기 옵션을 사용하지 않음 |
| `AUTO` | 콘텐츠 크기에 맞춰 자동 계산 | 최소·최대 옵션 |

#### FIXED

```json
{
  "sizeMode": "FIXED",
  "width": 900,
  "height": 620
}
```

#### VIEWPORT_RATIO

```json
{
  "sizeMode": "VIEWPORT_RATIO",
  "widthRatio": 0.7,
  "heightRatio": 0.75,
  "minimumWidth": 600,
  "minimumHeight": 450,
  "maximumWidth": 1200,
  "maximumHeight": 900
}
```

계산 방식은 다음과 같다.

```text
모니터 작업 영역 × 비율
→ minimum 값보다 작으면 minimum 사용
→ maximum 값보다 크면 maximum 사용
```

비율은 `0`보다 크고 `1` 이하여야 한다.

#### FULLSCREEN

```json
{
  "sizeMode": "FULLSCREEN"
}
```

`FULLSCREEN`은 TEXT, IMAGE, VIDEO, SURVEY 팝업에 공통으로 적용된다.
팝업이 열리는 모니터의 전체 영역을 사용하므로 작업표시줄도 덮는다.
`width`, `height`, 비율 및 최소·최대 크기 옵션은 무시한다.

### 2.4 공통 화면 옵션

| JSON 옵션 | 기본값 | 설명 |
|---|---:|---|
| `showHeader` | `true` | 공통 상단 제목 영역 표시 여부 |
| `showCloseButton` | `true` | Header 안의 X 버튼 표시 여부 |
| `showFooter` | `true` | 하단 체크박스와 닫기 버튼 영역 표시 여부 |
| `showDoNotShowAgain` | `false` | `30일간 보지 않기` 체크박스 표시 여부 |

주의사항:

- `showHeader`가 `false`면 `showCloseButton`이 `true`여도 상단 X 버튼은 표시되지 않는다.
- `showFooter`가 `false`면 하단 닫기 버튼과 `30일간 보지 않기` 영역이 모두 숨겨진다.
- `showDoNotShowAgain`은 일반적으로 `showFooter`와 함께 `true`로 설정한다.
- Header가 없는 팝업은 콘텐츠 상단의 투명한 이동 영역으로 창을 드래그할 수 있다.

---

## 3. TEXT 팝업

### 3.1 content 옵션

| 옵션 | 형식 | 설명 |
|---|---|---|
| `contentTitle` | 문자열 | 콘텐츠 내부의 큰 제목 |
| `description` | 문자열 | 제목 아래의 설명 |
| `leftSectionTitle` | 문자열 | 왼쪽 카드 제목 |
| `leftSectionBody` | 문자열 | 왼쪽 카드 본문 |
| `highlightText` | 문자열 | 강조 영역 문구 |
| `rightSectionTitle` | 문자열 | 오른쪽 카드 제목 |
| `rightSectionBody` | 문자열 | 오른쪽 카드 본문 |
| `additionalDescription` | 문자열 | 오른쪽 카드의 추가 설명 |

### 3.2 JSON 예시

```json
{
  "popupId": "TEXT_001",
  "popupType": "TEXT",
  "title": "서비스 안내",
  "displayMode": "SEQUENTIAL",
  "sizeMode": "VIEWPORT_RATIO",
  "widthRatio": 0.65,
  "heightRatio": 0.7,
  "minimumWidth": 600,
  "minimumHeight": 450,
  "maximumWidth": 1000,
  "maximumHeight": 850,
  "showHeader": true,
  "showCloseButton": true,
  "showFooter": true,
  "showDoNotShowAgain": true,
  "content": {
    "contentTitle": "서비스 이용 안내",
    "description": "아래 내용을 확인해주세요.",
    "leftSectionTitle": "1. 주요 안내",
    "leftSectionBody": "서비스 이용 시 확인할 내용입니다.",
    "highlightText": "중요한 안내 문구입니다.",
    "rightSectionTitle": "2. 상세 내용",
    "rightSectionBody": "상세 설명이 표시됩니다.",
    "additionalDescription": "추가 설명이 표시됩니다."
  }
}
```

---

## 4. IMAGE 팝업

### 4.1 content 옵션

| 옵션 | 형식 | 기본값 | 설명 |
|---|---|---:|---|
| `imageTitle` | 문자열 | 빈 문자열 | 이미지 콘텐츠 제목 |
| `imageUrl` | 문자열 | 빈 문자열 | 이미지 URL 또는 로컬 파일 경로 |
| `description` | 문자열 | 빈 문자열 | 이미지 설명 |
| `showDescription` | 논리값 | `true` | 설명 표시 여부 |
| `imageSizeMode` | 문자열 | `FIXED` | 이미지 표시 크기 방식 |
| `imageWidth` | 숫자 | `0` | 요청 이미지 너비 |
| `imageHeight` | 숫자 | `0` | 요청 이미지 높이 |
| `linkUrl` | 문자열 | 빈 문자열 | 이미지 클릭 시 이동할 외부 URL |

`imageSizeMode`는 PopupWindow 전체 크기를 결정하는 공통 `sizeMode`와 다른 옵션이다.

| `imageSizeMode` | 설명 |
|---|---|
| `FIXED` | 지정된 이미지 영역 또는 기본 크기 사용 |
| `FIT_TO_IMAGE` | 이미지 원본 비율을 기준으로 표시 영역 계산 |

### 4.2 JSON 예시

```json
{
  "popupId": "IMAGE_001",
  "popupType": "IMAGE",
  "title": "이미지 안내",
  "displayMode": "SEQUENTIAL",
  "sizeMode": "VIEWPORT_RATIO",
  "widthRatio": 0.6,
  "heightRatio": 0.75,
  "minimumWidth": 600,
  "minimumHeight": 450,
  "maximumWidth": 1100,
  "maximumHeight": 900,
  "showHeader": true,
  "showCloseButton": true,
  "showFooter": true,
  "showDoNotShowAgain": true,
  "content": {
    "imageTitle": "외부 이미지 안내",
    "imageUrl": "https://example.com/notice.jpg",
    "description": "이미지 내용을 확인해주세요.",
    "showDescription": true,
    "imageSizeMode": "FIT_TO_IMAGE",
    "imageWidth": 900,
    "imageHeight": 600,
    "linkUrl": "https://example.com/detail"
  }
}
```

---

## 5. VIDEO 팝업

### 5.1 content 옵션

| 옵션 | 형식 | 기본값 | 설명 |
|---|---|---:|---|
| `videoTitle` | 문자열 | 빈 문자열 | 영상 콘텐츠 제목 |
| `videoUrl` | 문자열 | 빈 문자열 | MP4 URL 또는 로컬 영상 경로 |
| `description` | 문자열 | 빈 문자열 | 영상 설명 |
| `showDescription` | 논리값 | `true` | 설명 표시 여부 |
| `showControls` | 논리값 | `true` | 영상 컨트롤 표시 여부 |
| `allowFullScreen` | 논리값 | `true` | 전체화면 허용 여부 |
| `allowPlaybackRateChange` | 논리값 | `true` | 배속 변경 허용 여부 |
| `autoPlay` | 논리값 | `false` | 팝업 표시 후 자동 재생 여부 |
| `isLoop` | 논리값 | `false` | 반복 재생 여부 |
| `defaultVolume` | 숫자 | `0.7` | 기본 음량. `0.0`부터 `1.0` |
| `completionRatio` | 숫자 | `0.9` | 영상 완료로 인정할 재생 비율 |
| `allowCloseBeforeCompletion` | 논리값 | `true` | 시청 완료 전 닫기 허용 여부 |

일부 확장 옵션은 DTO에 존재하지만 현재 View에서 아직 모두 강제되지 않을 수 있다. 서버에서 값을 추가하기 전 WPF View 지원 여부를 함께 확인한다.

### 5.2 JSON 예시

```json
{
  "popupId": "VIDEO_001",
  "popupType": "VIDEO",
  "title": "교육 영상",
  "displayMode": "SEQUENTIAL",
  "sizeMode": "VIEWPORT_RATIO",
  "widthRatio": 0.7,
  "heightRatio": 0.75,
  "minimumWidth": 700,
  "minimumHeight": 500,
  "maximumWidth": 1200,
  "maximumHeight": 900,
  "showHeader": true,
  "showCloseButton": true,
  "showFooter": true,
  "showDoNotShowAgain": false,
  "content": {
    "videoTitle": "정보보안 교육 영상",
    "videoUrl": "https://example.com/security-training.mp4",
    "description": "영상을 끝까지 시청해주세요.",
    "showDescription": true,
    "showControls": true,
    "allowFullScreen": true,
    "allowPlaybackRateChange": false,
    "autoPlay": false,
    "isLoop": false,
    "defaultVolume": 0.7,
    "completionRatio": 0.9,
    "allowCloseBeforeCompletion": false
  }
}
```

---

## 6. SURVEY 팝업

### 6.1 content 공통 옵션

| 옵션 | 형식 | 기본값 | 설명 |
|---|---|---:|---|
| `surveyTitle` | 문자열 | 빈 문자열 | 설문 내부 제목 |
| `description` | 문자열 | 빈 문자열 | 설문 설명 |
| `questions` | 배열 | 빈 배열 | 설문 문항 목록 |
| `passingScore` | 숫자 | `0` | 일반 설문에서는 사용하지 않음 |
| `validateRequiredQuestions` | 논리값 | `true` | 필수 문항 검증 여부 |

### 6.2 질문 옵션

| 옵션 | 형식 | 설명 |
|---|---|---|
| `questionId` | 정수 | 질문 고유 번호 |
| `title` | 문자열 | 질문 제목 |
| `description` | 문자열 | 질문 부가 설명 |
| `questionType` | 문자열 | 질문 유형 |
| `isRequired` | 논리값 | 필수 응답 여부 |
| `options` | 배열 | 객관식 선택지 목록 |
| `isScored` | 논리값 | 채점 대상 여부. 일반 설문은 보통 `false` |
| `correctAnswers` | 문자열 배열 | 정답 목록. 일반 설문은 빈 배열 |

### 6.3 질문 유형

| `questionType` | UI | `options` 사용 |
|---|---|---:|
| `RATING5` | 1점부터 5점까지 평가 | X |
| `SINGLE_CHOICE` | 하나만 선택하는 라디오 버튼 | O |
| `MULTIPLE_CHOICE` | 여러 개 선택하는 체크박스 | O |
| `TEXT` | 주관식 입력란 | X |

선택지 구조:

```json
{
  "value": "PHONE",
  "text": "휴대전화 번호"
}
```

`value`는 서버에 저장하거나 정답을 비교할 실제 값이고, `text`는 사용자 화면에 표시할 문구다.

### 6.4 JSON 예시

```json
{
  "popupId": "SURVEY_001",
  "popupType": "SURVEY",
  "title": "만족도 설문",
  "displayMode": "SEQUENTIAL",
  "sizeMode": "VIEWPORT_RATIO",
  "widthRatio": 0.55,
  "heightRatio": 0.75,
  "minimumWidth": 600,
  "minimumHeight": 500,
  "maximumWidth": 900,
  "maximumHeight": 900,
  "showHeader": false,
  "showCloseButton": false,
  "showFooter": false,
  "showDoNotShowAgain": false,
  "content": {
    "surveyTitle": "교육 만족도 설문",
    "description": "아래 문항에 응답해주세요.",
    "passingScore": 0,
    "validateRequiredQuestions": true,
    "questions": [
      {
        "questionId": 1,
        "title": "교육 내용에 얼마나 만족하셨나요?",
        "description": "1점부터 5점까지 선택해주세요.",
        "questionType": "RATING5",
        "isRequired": true,
        "isScored": false,
        "options": [],
        "correctAnswers": []
      },
      {
        "questionId": 2,
        "title": "추가 의견을 작성해주세요.",
        "description": "선택 입력 항목입니다.",
        "questionType": "TEXT",
        "isRequired": false,
        "isScored": false,
        "options": [],
        "correctAnswers": []
      }
    ]
  }
}
```

---

## 7. QUIZ 팝업

QUIZ는 SURVEY와 같은 질문 구조와 `SurveyPopupView`를 사용하지만 채점 기능이 활성화된다.

| 설정 | SURVEY | QUIZ |
|---|---:|---:|
| `popupType` | `SURVEY` | `QUIZ` |
| `isScored` | 보통 `false` | 채점 문항은 `true` |
| `correctAnswers` | 빈 배열 | 실제 정답 값 배열 |
| `passingScore` | 보통 `0` | 통과 점수 설정 |

### 7.1 정답 작성 규칙

단일 선택 정답:

```json
"correctAnswers": ["PHONE"]
```

복수 선택 정답:

```json
"correctAnswers": ["LOCK", "REPORT"]
```

`correctAnswers`에는 선택지의 표시 문구인 `text`가 아니라 선택지의 `value`를 넣는다.

운영 환경에서는 클라이언트로 정답을 전달하면 사용자가 응답 JSON을 분석하여 정답을 확인할 수 있다. 실제 보안이 필요한 시험은 서버 채점 방식으로 변경하는 것을 권장한다.

### 7.2 JSON 예시

```json
{
  "popupId": "QUIZ_001",
  "popupType": "QUIZ",
  "title": "정보보안 평가",
  "displayMode": "SEQUENTIAL",
  "sizeMode": "VIEWPORT_RATIO",
  "widthRatio": 0.55,
  "heightRatio": 0.75,
  "minimumWidth": 600,
  "minimumHeight": 500,
  "maximumWidth": 900,
  "maximumHeight": 900,
  "showHeader": false,
  "showCloseButton": false,
  "showFooter": false,
  "showDoNotShowAgain": false,
  "content": {
    "surveyTitle": "정보보안 교육 평가",
    "description": "80점 이상이면 통과입니다.",
    "passingScore": 80,
    "validateRequiredQuestions": true,
    "questions": [
      {
        "questionId": 1,
        "title": "다음 중 개인정보에 해당하는 것은?",
        "description": "정답을 하나 선택해주세요.",
        "questionType": "SINGLE_CHOICE",
        "isRequired": true,
        "isScored": true,
        "options": [
          { "value": "PHONE", "text": "휴대전화 번호" },
          { "value": "WEATHER", "text": "오늘의 날씨" }
        ],
        "correctAnswers": ["PHONE"]
      },
      {
        "questionId": 2,
        "title": "안전한 행동을 모두 선택해주세요.",
        "description": "정답을 복수로 선택해주세요.",
        "questionType": "MULTIPLE_CHOICE",
        "isRequired": true,
        "isScored": true,
        "options": [
          { "value": "LOCK", "text": "자리를 비울 때 화면 잠금" },
          { "value": "SHARE", "text": "동료와 비밀번호 공유" },
          { "value": "REPORT", "text": "의심스러운 메일 신고" }
        ],
        "correctAnswers": ["LOCK", "REPORT"]
      }
    ]
  }
}
```

---

## 8. 사용자 숨김 옵션

`showDoNotShowAgain`이 `true`이고 사용자가 체크 후 팝업을 닫으면 WPF가 다음 API를 호출한다.

```http
POST /api/popups/{popupId}/hide
Content-Type: application/json
```

```json
{
  "userId": "WPF_TEST_USER",
  "hideDays": 30
}
```

Oracle `USER_POPUP_STATE`에는 다음과 같이 저장된다.

| 컬럼 | 예시 | 설명 |
|---|---|---|
| `USER_ID` | `WPF_TEST_USER` | 숨김을 선택한 사용자 |
| `POPUP_ID` | `TEXT_001` | 숨길 팝업 |
| `HIDE_TYPE` | `UNTIL` | 기간 숨김 |
| `HIDDEN_UNTIL` | 현재 시각 + 30일 | 숨김 종료 시각 |

숨김 기간이 남아 있으면 `GET /api/popups?userId=...` 결과에서 해당 팝업이 제외된다.

---

## 9. Oracle 저장 시 주의사항

`CONTENT_JSON`은 Oracle `CLOB`이며 유효한 JSON이어야 한다.

SQL에서 여러 줄 JSON을 입력할 때 Oracle 대체 인용문을 사용하면 작은따옴표 처리 오류를 줄일 수 있다.

```sql
q'~
{
    "contentTitle": "안내 제목",
    "description": "안내 내용"
}
~'
```

검증 쿼리:

```sql
SELECT
    POPUP_ID,
    POPUP_TYPE,
    DISPLAY_MODE,
    SIZE_MODE,
    DISPLAY_ORDER,
    USE_YN
FROM POPUP_NOTICE
ORDER BY DISPLAY_ORDER, POPUP_ID;
```

---

## 10. 권장 설정 조합

| 팝업 종류 | Header | Footer | 다시 보지 않기 | 권장 크기 |
|---|---:|---:|---:|---|
| TEXT | O | O | 필요에 따라 O | `VIEWPORT_RATIO` 0.60 × 0.70 |
| IMAGE | O | O | 필요에 따라 O | `VIEWPORT_RATIO` 0.60 × 0.75 |
| VIDEO | O | 정책에 따라 | 교육 정책에 따라 | `VIEWPORT_RATIO` 0.70 × 0.75 |
| SURVEY | X | X | X | `VIEWPORT_RATIO` 0.55 × 0.75 |
| QUIZ | X | X | X | `VIEWPORT_RATIO` 0.55 × 0.75 |

SURVEY와 QUIZ는 View 내부에 제출 또는 채점 버튼이 있으므로 공통 Footer를 숨기는 구성을 권장한다.

---

## 11. 신규 팝업 등록 확인 목록

- `POPUP_ID`가 중복되지 않는가?
- `POPUP_TYPE`이 지원되는 값인가?
- 현재 시각이 노출 시작과 종료 범위 안에 있는가?
- `DISPLAY_MODE`가 `SEQUENTIAL` 또는 `SIMULTANEOUS`인가?
- 최소 크기가 최대 크기보다 크지 않은가?
- `CONTENT_JSON`이 유효한 JSON인가?
- `content` 구조가 `popupType`과 일치하는가?
- 외부 이미지 또는 영상 URL에 WPF PC가 접근할 수 있는가?
- SURVEY 필수 문항에 `isRequired`가 설정되었는가?
- QUIZ 정답이 선택지의 `value`와 정확히 일치하는가?
- `30일간 보지 않기`가 필요하면 Header/Footer 닫기 동작과 함께 테스트했는가?
- 순차 및 동시 표시에서 창 종료 흐름이 정상인가?

---

## 12. 현재 구현 범위와 향후 확장

현재 구현 범위:

- Oracle 팝업 목록 조회
- Java API를 통한 WPF 전달
- 5가지 팝업 View 생성
- 순차 및 동시 표시
- 사용자별 30일 숨김 저장
- 표시 기간 및 숨김 상태 조회 제외

향후 확장 예정:

- 사용자·부서·직급·역할별 노출 대상 판정
- 설문 응답 서버 저장
- 퀴즈 서버 채점
- 영상 시청시간·완료 상태 로깅
- 팝업 관리 화면 및 옵션 유효성 검증
