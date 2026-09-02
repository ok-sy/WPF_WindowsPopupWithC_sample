# Popup

## WPF Demo Mode

`Popup/appsettings.json`에서 `PopupApi.DemoMode`를 `true`로 설정하면
Java API와 PostgreSQL 없이 WPF 화면만 시연할 수 있다.

```json
{
  "PopupApi": {
    "DemoMode": true
  }
}
```

Demo Mode에서는 프로그램 시작 시 팝업 선택 화면이 표시된다. TEXT, IMAGE,
VIDEO, SURVEY, QUIZ 버튼으로 원하는 화면을 하나씩 열거나, `전체 팝업 순차 실행`
버튼으로 5종을 순서대로 확인할 수 있다. 팝업 조회, 이벤트 기록, 숨김, 설문 저장,
영상 진행률 저장을 포함한 Java API 통신과 주기 조회는 실행하지 않는다.

실제 API 연동 모드로 돌아가려면 `DemoMode`를 `false`로 변경한다.

### 폐쇄망 Demo 미디어

다음 파일명으로 미디어를 추가하면 빌드와 publish 결과의 `Media` 폴더로
자동 복사된다.

```text
Popup/Media/demo-image.jpg
Popup/Media/demo-video.mp4
```

실행할 때는 특정 PC의 경로를 사용하지 않고 `Popup.exe`가 있는 폴더를 기준으로
`Media/demo-image.jpg`, `Media/demo-video.mp4`를 찾는다.
