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
