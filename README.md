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

### 바로 플레이하기 (Windows)

[**Releases**](https://github.com/Developer-JiWooKim/GameUIUXLab/releases/latest) 에서 zip 을 받아 압축을 풀고 exe 를 실행합니다. (Windows 10/11 64bit)

### 에디터에서 열기

> 외부 에셋은 라이선스 때문에 저장소에 포함하지 않았습니다. 먼저 아래 [외부 에셋 / 라이선스](#외부-에셋--라이선스) 의 두 에셋을 임포트해야 UI 가 정상적으로 보입니다.

1. Unity Hub 에서 이 저장소 폴더를 프로젝트로 추가하고 **Unity 6000.5.8f1** 로 엽니다.
2. 외부 에셋 두 개를 표의 경로 그대로 임포트합니다.
3. Project 창에서 씬을 더블클릭해 엽니다.
   `Assets/MyAssets/PORTFOLIO_Assets/Scene/PortfolioAssignment.unity`
4. **Game 뷰 해상도를 1080 × 1920 (세로) 로 맞춥니다.**
5. Play 를 누르면 `Screen_Title` 에서 시작합니다.

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

---

## 외부 에셋 / 라이선스

### 저장소에 포함하지 않은 에셋

아래 에셋은 원본 재배포가 허용되지 않아 저장소에서 제외했습니다 (`.gitignore` 등록).
씬과 Prefab 은 원본 에셋의 GUID 로 참조하므로, **수정 없이 아래 경로에 임포트하기만 하면** 참조가 그대로 복구됩니다.

| 에셋 | 제작 | 임포트 경로 | 용도 | 라이선스 |
| :--- | :--- | :--- | :--- | :--- |
| [Sweet Cakes Icon Pack](https://assetstore.unity.com/packages/2d/gui/icons/sweet-cakes-icon-pack-88182) | Unity Asset Store | `Assets/SweetCakesIconPack/` | 디저트 아이콘 5종 (`Sprites/Cakes/`) | Asset Store Standard EULA (유료) |
| [Free Casual GUI](https://uncogames.itch.io/free-casual-gui) | Unco Games Studio | `Assets/Skyden_Games/Free_Casual_GUI/` | 버튼 · 패널 · HUD · 게이지 · 배경 스프라이트, `Baloo-Regular SDF` 폰트 | 무료 사용 가능, 원본 그대로 재배포 금지 |

### 저장소에 포함된 서드파티 리소스

| 리소스 | 경로 | 라이선스 |
| :--- | :--- | :--- |
| 케리스 케듀체 (KERISKEDU_R) — © KERIS, 제작 투게더그룹 | `Assets/MyAssets/PORTFOLIO_Assets/Font/` | [SIL Open Font License 1.1](Assets/MyAssets/PORTFOLIO_Assets/Font/OFL.txt) · [배포처](https://copyright.keris.or.kr/wft/fntDwnldView?fntGrpId=GFT202512150000000000002) |
| TextMesh Pro 기본 리소스 | `Assets/TextMesh Pro/` | Unity Companion License |

### 크레딧

- UI 그래픽: **Free Casual GUI** © Unco Games Studio
- 디저트 아이콘: **Sweet Cakes Icon Pack** (Unity Asset Store)
- 폰트: **케리스 케듀체** © KERIS (OFL 1.1), **Baloo** © Ek Type (OFL 1.1, Free Casual GUI 동봉)

스크린샷과 플레이 영상에 보이는 UI 그래픽 · 아이콘의 저작권은 위 제작자에게 있습니다.
