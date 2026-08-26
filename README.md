# 주문받아라!! — UI/UX 포트폴리오

---

## 프로젝트 정보

| 항목 | 값 |
| :--- | :--- |
| 엔진 | Unity 6 (6000.5.8f1) |
| 씬 | `Assets/MyAssets/PORTFOLIO_Assets/Scene/PortfolioAssignment.unity` |
| 씬 구성 | **단일 씬** — 화면 전환은 패널 활성/비활성 |
| UI | UGUI + TextMeshPro |
| 입력 | Input System (`Assets/InputSystem_Actions.inputactions` 의 `UI` 맵) |
| 기준 해상도 | 1080 × 1920 (세로), Canvas Scaler Match **0** (Width 기준) |
| 스크립트 | `Assets/MyAssets/PORTFOLIO_Assets/Scripts/` (`Core` / `Views` / `Screens`) |

---

## 실행 방법

1. Unity Hub 에서 이 저장소 폴더를 프로젝트로 추가하고 **Unity 6000.5.8f1** 로 엽니다.
2. Project 창에서 씬을 더블클릭해 엽니다.
   `Assets/MyAssets/PORTFOLIO_Assets/Scene/PortfolioAssignment.unity`
3. **Game 뷰 해상도를 1080 × 1920 (세로) 로 맞춥니다.**
4. Play 를 누르면 `Screen_Title` 에서 시작합니다.

### 조작

| 입력 | 동작 |
| :--- | :--- |
| 마우스 좌클릭 / 터치 | 버튼 누르기, 진열대에서 디저트 담기 |
| 방향키 · WASD / 게임패드 좌스틱 · D-pad | 선택 이동 (Navigate) |
| Enter / 게임패드 A | 확인 (Submit) |
| `1` ~ `5` | 진열대 버튼 1~5번 즉시 누르기 |
| Esc / 게임패드 B | 취소 — Play 에서는 일시정지 열기 (Cancel) |

---

## 실행 화면

- 시작 화면
<img src="Assets/MyAssets/PORTFOLIO_Assets/ScreenShot/Title.jpg">

<br>

- 플레이 화면
<img src="Assets/MyAssets/PORTFOLIO_Assets/ScreenShot/Play.jpg">

<br>

- 결과 화면
<img src="Assets/MyAssets/PORTFOLIO_Assets/ScreenShot/Result.jpg">

---

## 관련 문서 설명

| 문서 | 내용 | 위치 |
| :--- | :--- | :--- |
| **UI/UX 설계 문서** | 화면 목록, 화면 전환 흐름, 기준 해상도와 Canvas Scaler, HUD-데이터 연결, 입력 인터페이스, UI Prefab, 리소스 규칙, 피드백 설계 | [UIUX_ARCHITECTURE.md](UIUX_ARCHITECTURE.md) |
| **GUI 디자인 가이드 · 구현 가능성 검토** | 최소 버튼 크기, 글자 크기와 대비, 정보 우선순위, 지원 입력 방식, 원안에서 바꾼 항목 기록 | [GUI_GUIDE.md](GUI_GUIDE.md) |
| UI 점검 체크리스트 | 기대 / 실제 / 원인 / 수정 / 재확인 형식의 검증 기록 | [CHECKLIST.md](CHECKLIST.md) |
