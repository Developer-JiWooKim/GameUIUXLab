# UI/UX 설계 문서 — 주문받아라!!

| 항목 | 값 |
| :--- | :--- |
| 과목 | 게임 UI/UX 프로그래밍 (NCS `0803020529_24v5`) |
| 엔진 | Unity 6 / UGUI / TextMeshPro / Input System |
| 씬 | `Assets/MyAssets/PORTFOLIO_Assets/Scene/PortfolioAssignment.unity` (단일 씬) |
| 작성 기준 | 2026-08-19, UI 배치 완료 시점의 실제 하이라키 |

이 문서는 과제 요건 **1번(UI/UX 설계 문서)** 과 **2번(GUI 디자인 가이드·구현 가능성 검토)** 의
제출 산출물입니다. 스크립트 단위의 구현 명세는 [script-design.md](script-design.md) 에 있습니다.

- 이 문서: **무엇을 왜 그렇게 배치했는가**
- `script-design.md`: **어떤 스크립트가 무엇을 소유하고 어디에 붙는가**
- `checklist.md`: 검증 기록 (요건 8, 테스트하며 작성)

---

## 1. UI/UX 콘셉트

> 이 게임의 UI는 **"지금 무엇을 눌러야 하는가"를 한 화면 안에서 끝내는 것**을 목표로 한다.
> 플레이어는 위에서 주문을 읽고, 아래에서 케이크를 누른다. 그 사이에 쟁반이 놓여 진행 상황을
> 보여준다. 주문·쟁반·진열대가 세로로 한 줄에 놓이기 때문에 시선은 위아래로만 움직이며,
> 메뉴를 열거나 모드를 바꾸는 동작이 전혀 없다. 누른 즉시 결과가 나오고(성공·실패), 결과는
> 색과 문구로 동시에 전달된다. 화려한 연출 대신 **읽히는 배치와 즉각적인 반응**에 집중한다.

---

## 2. 플레이 루프

```text
손님 1명 등장
 └ 주문 카드 표시 (품목 아이콘 + 수량)
    └ 플레이어가 진열대 버튼을 누름 ─── 누른 즉시 판정
         ├ 주문에 있는 품목 → 쟁반에 담김, 계속 진행
         │                    └ 주문을 전부 채움 → 성공, 점수 +100, 처리 +1
         └ 주문에 없는 품목 → 그 자리에서 실패, 실패 +1
              ↓
            판정 연출 0.8초 (진열대 잠금) → 다음 손님
영업시간 120초(인스펙터 조정 가능) 종료 → 결과 화면

※ 플레이 중 Pause 버튼 또는 Esc 로 일시정지
```

### 2.1 왜 제출 버튼을 없앴는가 (원안 변경)

**원안**: 진열대에서 케이크를 담고 → 제출 버튼을 눌러 판정.
**변경**: 누른 즉시 판정.

| 근거 | 내용 |
| :--- | :--- |
| 조작 횟수 | 주문 1건당 최소 2탭(담기+제출)에서 1탭으로 줄어듦. 세로형 캐주얼 게임에서 확인 버튼은 리듬을 끊는다 |
| 화면의 정직성 | 즉시 판정에서는 쟁반에 담긴 것이 **항상 정답의 일부**다. 틀린 것이 쟁반에 남아 있다가 나중에 실패로 밝혀지는 상황이 사라진다 |
| 상태 수 감소 | "담는 중 / 제출 대기 / 판정 중" 3단계가 "진행 중 / 판정 중" 2단계로 줄어듦 |

**대가로 생긴 것**: 판정 직후 화면이 즉시 바뀌면 왜 틀렸는지 볼 수 없고, 연타하면 주문이
줄줄이 실패한다. → **판정 후 0.8초 지연 + 진열대 입력 잠금**으로 해결한다 (11장, 12.5).

---

## 3. 화면 목록

단일 씬에서 `Canvas > UIRoot` 아래 4개 패널을 켜고 끄는 방식으로 전환한다.
`SceneManager.LoadScene` 을 쓰지 않는 이유는 로딩 공백 없이 상태를 유지하기 위해서다.

### 3.1 Screen_Title

| 요소 | 오브젝트 | 역할 |
| :--- | :--- | :--- |
| 타이틀 | `TitleLabel` (TitleLabelText "주문 받아라!!", Logo, Image ×2) | 게임 정체성 |
| 시작 | `MenuButton_Start` (500×200) | → Screen_Play |
| 종료 | `MenuButton_Quit` (500×200) | 애플리케이션 종료 |

- 첫 키보드 포커스: **`MenuButton_Start`**
- 버튼 2개만 두어 "무엇을 눌러야 하는지" 고민할 여지를 없앤다.

### 3.2 Screen_Play

세로 화면을 3개 영역으로 나눈다. 실제 높이 배분은 6장 참고.

| 영역 | 오브젝트 | 내용 |
| :--- | :--- | :--- |
| 상단 HUD | `TopNav` (h 400) | `Score`(점수), `Timer`(게이지+숫자), `CurrentSlot`(성공/실패), `PauseButton` |
| 중단 | `MidNav` (h 1200) | `OrderPanel`(손님+주문 카드), `ChoiceListPanel`(쟁반) |
| 하단 | `BotNav` (h 280) | `DisplayStandList`(진열대 텍스트 + 케이크 버튼 5개) |

- 첫 키보드 포커스: **`CakeButton_1`**
- `OrderGridLayout` / `ChoiceGridLayout` 에 Horizontal Layout Group 적용, 자식은 Prefab 인스턴스
- 추가 예정: `ToastMessage` (11장)

### 3.3 Screen_Pause

`Screen_Play` **위에 겹쳐** 띄운다. Play 를 끄면 뒤가 비어 "일시정지"로 읽히지 않는다.
단, 겹쳐 있어도 `GameState` 의 시간은 멈춘다.

| 요소 | 오브젝트 |
| :--- | :--- |
| 제목 | `PauseText` "일시정지" |
| 계속하기 | `ResumeButton` (500×150) → Screen_Play |
| 타이틀로 | `GoTitleButton` (500×150) → ConfirmPopup 열기 |
| 확인 팝업 | `ConfirmPopup` (기본 비활성) — ConfirmText, ConfirmDescriptionText, YesButton, NoButton |

- 첫 키보드 포커스: **`ResumeButton`**
- 팝업의 첫 포커스: **`NoButton`** — 되돌릴 수 없는 선택을 기본값으로 두지 않는다
- 진행 중인 게임을 버리는 동작이므로 확인 팝업을 한 단계 둔다

### 3.4 Screen_Result

| 요소 | 오브젝트 | 연결 데이터 |
| :--- | :--- | :--- |
| 제목 | `ResultText` "결과" | — |
| 성공 | `SuccessText` | `GameState.successCount` |
| 실패 | `FailText` | `GameState.failCount` |
| 점수 | `ScoreText` | `GameState.score` |
| 랭크 | `RankText` | `RankTable.Evaluate(successCount, failCount)` → A~F + 등급 색 |
| 다시하기 | `ReplayButton` | → Screen_Play (상태 초기화) |
| 타이틀로 | `GoTitleButton` | → Screen_Title |

- 첫 키보드 포커스: **`ReplayButton`**

---

## 4. 화면 전환 흐름

```text
Screen_Title
   │ [시작] / [Submit]
   ▼
Screen_Play ──[Pause 버튼] / [Cancel]──▶ Screen_Pause ──[타이틀로]──▶ ConfirmPopup
   │   ▲                                    │    ▲                        │
   │   └──────[계속하기] / [Cancel]──────────┘    └───[아니오] / [Cancel]──┘
   │                                                                      │
   │ [영업시간 0초]                                        [예]───────────┐│
   ▼                                                                     ▼▼
Screen_Result ──[다시하기]──▶ Screen_Play (초기화)              Screen_Title
              └─[타이틀로]──▶ Screen_Title
```

### 4.1 Cancel(Esc) 의 상황별 동작

Esc 는 화면마다 다르게 동작한다. **가장 위 레이어 하나만** 닫는 것이 원칙이다.

| 현재 상태 | Esc |
| :--- | :--- |
| Title | 무시 (실수로 게임을 종료시키지 않는다) |
| Play | Pause 열기 |
| Pause, 팝업 닫힘 | Pause 닫기 |
| Pause, 팝업 열림 | **팝업만** 닫기 |
| Result | 무시 |

### 4.2 전환 시 반드시 하는 일

화면을 켤 때마다 `EventSystem.current.SetSelectedGameObject(firstButton)` 을 호출한다.
호출하지 않으면 전환 직후 키보드 포커스가 사라져 방향키가 먹지 않는다.
`null` 로 한 번 비우고 다시 지정한다 — 같은 오브젝트를 재지정할 때 `OnSelect` 가 발생하지 않아
하이라이트가 안 그려지는 경우가 있다.

---

## 5. 기준 해상도와 Canvas Scaler

| 항목 | 값 | 근거 |
| :--- | :--- | :--- |
| 기준 해상도 | **1080 × 1920** (세로) | 국내 보급률이 가장 높은 FHD 세로 비율 |
| UI Scale Mode | Scale With Screen Size | 해상도가 달라도 논리 좌표를 유지 |
| Screen Match Mode | Match Width Or Height | — |
| Match | **0 (Width 기준)** | 세로 게임은 가로폭이 조작 폭을 결정한다. 폭을 고정하면 진열대 버튼 5개의 크기·간격이 모든 기기에서 동일하게 유지되고, 세로로 길어진 만큼은 중앙 영역이 늘어난다 |

### 5.1 세로 비율 변화 대응

상단(`TopNav` 400)과 하단(`BotNav` 280)은 각각 Top·Bottom 앵커로 **높이를 고정**하고,
중앙(`MidNav` 1200)만 신축시킨다. 화면이 길어지면 주문 카드와 쟁반 사이 여백이 늘어날 뿐,
HUD 와 진열대는 항상 같은 자리에 있다.

검증 대상 해상도 (요건 8):

| 해상도 | 비율 | 확인 사항 |
| :--- | :--- | :--- |
| 1080 × 1920 | 9:16 | 기준 |
| 1080 × 2340 | 9:19.5 | 중앙 여백 증가 |
| 1080 × 2400 | 9:20 | 중앙 여백 증가 |
| 1536 × 2048 | 3:4 (태블릿) | **가장 위험** — 세로가 짧아 중앙 영역이 압축된다 |

---

## 6. Screen_Play 배치 설계

### 6.1 영역 배분 (실측)

| 영역 | 앵커 | 높이 | 배치 근거 |
| :--- | :--- | ---: | :--- |
| `TopNav` | Top stretch | 400 | 상시 정보이며 조작 대상이 아니다. 손가락에 가려지지 않는 위쪽에 둔다 |
| `MidNav` | Center stretch | 1200 | 주문(`OrderPanel` 600) + 쟁반(`ChoiceListPanel` 520). 확인 빈도 1순위라 시선이 먼저 닿는 화면 중앙 |
| `BotNav` | Bottom stretch | 280 | 조작 빈도가 가장 높다. 세로 그립에서 엄지가 자연스럽게 닿는 하단 |

합계 1880 + 여백 ≈ 1920.

### 6.2 시선 이동 경로

```
주문 카드(무엇을 담을지 읽음)
   ↓ 아래로
쟁반(얼마나 담았는지 확인)
   ↓ 아래로
진열대(누름)
   ↑ 결과가 쟁반에 즉시 반영
```

주문 → 쟁반 → 진열대를 **세로 한 줄**로 놓아 시선이 좌우로 흩어지지 않게 했다.
쟁반을 주문과 진열대 **사이**에 둔 것이 핵심이다. 진열대에서 누른 결과가 바로 위 칸에
나타나므로 확인을 위해 눈이 멀리 이동하지 않는다.

---

## 7. HUD와 데이터 연결 (요건 3)

**UI 는 데이터를 표시할 뿐 소유하지 않는다.** 점수 변수는 `GameState` 에 있고
`HudController` 는 그 값을 읽어 TextMeshPro 에 반영한다.

| 표시 요소 | 오브젝트 | 연결 데이터 | 갱신 시점 |
| :--- | :--- | :--- | :--- |
| 시간 게이지 | `Progress Slider_Yellow` | `GameState.remainingTime / playTimeSeconds` | 매 프레임 |
| 시간 숫자 | `TimerText` | `remainingTime` (정수 변환) | **초 단위가 바뀔 때만** |
| 점수 | `ScoreValueText` | `GameState.score` | 판정 시 |
| 성공 / 실패 | `CurrentSlotValueText` | `successCount`, `failCount` | 판정 시 |
| 주문 카드 | `OrderGridLayout` | `GameState.currentOrder` | 손님 교체 시 |
| 손님 번호 | `CustomerNameText` | `GameState.customerNumber` | 손님 교체 시 |
| 쟁반 슬롯 | `ChoiceGridLayout` | `GameState.tray` | 담길 때 |

요건은 "3개 이상"이며 위에서 **시간·점수·성공/실패·주문 안내 4종**이 실제 상태와 연결된다.

매 프레임 문자열을 만들지 않기 위해 `GameState` 는 값이 바뀔 때 이벤트를 발행하고
View 가 구독한다. 시간 숫자는 정수 초가 바뀌는 순간에만 `text` 를 갱신한다.

---

## 8. 입력 인터페이스 (요건 5)

**포인터 계열(마우스·터치)과 선택 계열(키보드·게임패드)** 두 갈래를 모두 지원한다.
두 갈래는 조작 모델이 다르다 — 포인터는 "누르고 싶은 것을 직접 가리키고", 선택은 "현재 선택을
이동시킨 뒤 확정한다". 이 차이가 8.3~8.4 의 설계를 결정한다.

### 8.1 입력 방식

```text
디저트 담기   : 마우스 좌클릭 / 터치 / Submit(선택 상태에서)
메뉴 이동     : 방향키·WASD / 게임패드 좌스틱·우스틱·D-pad   (Navigate)
확인          : Enter / 게임패드 A(buttonSouth)              (Submit)
일시정지·취소 : Esc / 게임패드 B(buttonEast)                 (Cancel)
```

| 입력 | 구현 방식 | 비고 |
| :--- | :--- | :--- |
| 마우스 | UGUI Button `onClick` | 기본 경로 |
| 터치 | UGUI Button `onClick` | **마우스와 동일 경로. 추가 구현 없음** |
| 키보드 Navigate / Submit | `InputSystemUIInputModule` | 코드 불필요 |
| **게임패드 Navigate / Submit** | `InputSystemUIInputModule` | **코드 불필요. 기존 바인딩 그대로** |
| 키보드·게임패드 Cancel | 액션 직접 구독 | 선택된 오브젝트와 무관하게 받아야 하므로 |

### 8.2 Input Actions 매핑

기존 `Assets/InputSystem_Actions.inputactions` 의 **`UI` 맵을 그대로 사용**한다.
UI 입력밖에 없는 게임이므로 새 맵을 만들지 않는다. `Player` 맵(Move/Jump/…)은 미사용.

| 액션 | 바인딩 | 게임패드 | 처리 |
| :--- | :--- | :--- | :--- |
| `Point` / `Click` | `<Mouse>/position`, 마우스 좌클릭, 터치 | 없음 (포인터 없음) | 모듈이 처리 → Button `onClick` |
| `Navigate` | 방향키 / WASD 2D 컴포짓 | `leftStick`, `rightStick`, `dpad` | 모듈이 처리 |
| `Submit` | `*/{Submit}` | `buttonSouth` (A) | 모듈이 처리 |
| `Cancel` | `*/{Cancel}` | `buttonEast` (B) | **`UiInputRouter` 가 직접 구독** |

**게임패드 지원에 새 바인딩 작업이 필요 없다.** `Submit` / `Cancel` 의 바인딩 경로
`*/{Submit}` · `*/{Cancel}` 는 컨트롤 스킴 그룹에 `Gamepad` 가 포함되어 있고, 게임패드 레이아웃의
`buttonSouth` · `buttonEast` 가 각각 Submit · Cancel usage 를 갖는다. `Navigate` 에도 좌스틱·우스틱·
D-pad 컴포짓이 이미 들어 있다. 즉 **Esc 처리 코드 한 곳이 B 버튼까지 같이 처리한다.**

`InputSystemUIInputModule` 은 Cancel 을 **현재 선택된 오브젝트에게만** 전달한다.
"선택이 무엇이든 Esc/B 를 누르면 일시정지"를 구현하려면 `InputActionReference` 로
`UI/Cancel` 을 직접 구독해야 한다.

### 8.3 포인터 조작 시 선택 해제

마우스로 버튼을 누르면 그 버튼이 선택 상태로 남아 테두리가 계속 보인다. 방금 키보드로 고른
것처럼 보여 혼란스럽다. `UI/Click` 이 마우스·터치에서 발생하면
`SetSelectedGameObject(null)` 로 하이라이트를 해제한다.

단, 화면 전환 버튼을 마우스로 누르면 **해제 → 새 화면의 첫 버튼 선택** 순서가 되어야 한다.
순서가 뒤집히면 새 화면의 포커스가 지워진다.

### 8.4 선택 복구 — 게임패드 지원의 실제 작업

8.3 의 해제 로직과 게임패드가 정면으로 충돌한다.

```
마우스로 버튼 클릭  →  선택 = null
그 상태에서 게임패드 스틱을 기울임
   → Navigate 액션은 발생하지만 이동시킬 선택 대상이 없다
   → 아무 일도 일어나지 않는다 = 게임패드가 죽은 것처럼 보인다
```

게임패드에는 포인터가 없으므로 **선택이 유일한 조작 수단**이다. 마우스는 선택이 없어도
직접 누를 수 있어서 이 문제가 드러나지 않지만, 게임패드는 여기서 완전히 막힌다.

**대응**: `Navigate` 와 `Submit` 이 발생했을 때 선택이 비어 있으면 현재 화면의 첫 버튼을
다시 선택한다.

```
Navigate 또는 Submit 발생
  └ EventSystem.currentSelectedGameObject == null 이면
       → ScreenManager 가 알려주는 현재 화면의 첫 버튼을 선택
```

첫 입력은 이동이 아니라 **포커스 복구**로 소비된다. 콘솔 UI 의 일반적인 동작이며,
"마우스를 쓰다가 패드를 집어 들면 어딘가에 커서가 되살아난다"는 기대와 일치한다.
키보드에도 동일하게 적용되므로 키보드 조작의 안정성도 함께 올라간다.

### 8.5 선택 계열 입력을 위한 Navigation 배선

포인터는 아무 버튼이나 직접 누를 수 있지만, 게임패드·키보드는 **Navigation 으로 이어진
경로가 없으면 도달할 수 없다.** 배선이 곧 접근성이다.

| 화면 | 배선 |
| :--- | :--- |
| Title | `MenuButton_Start` ↕ `MenuButton_Quit` (Vertical) |
| Play | `CakeButton_1~5` 가로 1열 (Horizontal 또는 Explicit). 5번 버튼의 Up → `PauseButton` 으로 연결해 상단 HUD 도달 경로를 확보 |
| Pause | `ResumeButton` ↕ `GoTitleButton` |
| ConfirmPopup | `NoButton` ↔ `YesButton` |
| Result | `ReplayButton` ↕ `GoTitleButton` |

진열대는 `Automatic` 을 쓰지 않는다. 좌표 기준 탐색이라 밀집 배치에서 순서가 어긋난다.

`PauseButton` 도달 경로를 배선하지 않아도 Cancel(Esc / B) 로 일시정지를 열 수 있으므로
기능적으로는 막히지 않는다. 다만 **눈에 보이는 버튼에 도달할 수 없는 상태**는 그 자체로
UX 결함이므로 배선한다.

---

## 9. UI Prefab (요건 6)

| Prefab | 구조 | 재사용 지점 | 상태 |
| :--- | :--- | :--- | :--- |
| `OrderPrefab` | CakeIcon + CountText | 주문 카드 슬롯 | 제작 완료 |
| `ChoicePrefab` | CakeIcon + CountText | 쟁반 슬롯 | 제작 완료 |
| `Score` | ScoreText + ScoreValueText | 상단 HUD 점수 | 제작 완료 |
| `Progress Slider_Yellow` | Background + Fill Area/Fill | 영업시간 게이지 | 제작 완료 |
| `ToastMessage` | 배경 Image + 메시지 Text | 성공/실패 알림 | **미제작 (11장)** |

- 요건은 "2개 이상"이며 현재 4종이 이미 사용 중이다.
- `OrderPrefab` 과 `ChoicePrefab` 은 구조가 동일하므로 **`DessertIconView` 컴포넌트를 공유**한다.
  스프라이트 교체·개수 표시 로직이 한 곳에만 존재한다.
- 반복 배치되는 진열대 버튼(`CakeButton_1~5`)은 씬에 직접 배치되어 있다. 각 버튼은
  `ShelfButton` 컴포넌트로 자신의 `DessertType` 만 들고 있으며, 입력 처리는 `ShelfView` 가 한다.

---

## 10. UI 리소스 규칙 (요건 1)

### 10.1 에셋 출처와 역할 분리

두 개의 에셋 팩을 쓰되 **역할을 겹치지 않게** 나눠 스타일 혼선을 막는다.

| 역할 | 출처 | 경로 |
| :--- | :--- | :--- |
| 디저트 품목 아이콘 5종 | Sweet Cakes Icon Pack (ricimi) | `MyAssets/PORTFOLIO_Assets/Sprites/CakeIcon/` |
| 버튼·패널·게이지 배경, 프레임 | Free Casual GUI (Sky Den Games) | `Skyden_Games/Free_Casual_GUI/` |

에셋 스토어 폴더는 **읽기 전용**으로 취급한다. 수정이 필요하면 `MyAssets` 로 복사해서 쓴다.

### 10.2 디저트 아이콘

형태가 서로 다르도록 5종을 선별했다. **색만 다르고 실루엣이 같으면** 빠르게 누를 때 구분이
안 되어 즉시 판정 방식에서 치명적이다.

| DessertType | 파일 | 실루엣 |
| :--- | :--- | :--- |
| `ChocoCake` | `Choco_Cake` | 둥근 홀케이크 |
| `KiwiBigCake` | `Kiwi_BigCake` | 큰 원형 |
| `LemonRectangularCake` | `Lemon_RectangularCake` | 사각형 |
| `RainbowCupCake` | `Rainbow_CupCake` | 컵케이크 |
| `SkullCakePiece` | `Skull_CakePiece` | 삼각 조각 |

enum 순서를 스프라이트 배열 인덱스로 사용하므로 **순서를 바꾸지 않는다.**

### 10.3 글꼴

| 항목 | 규칙 |
| :--- | :--- |
| 한국어 | `KERISKEDU_R SDF` (`MyAssets/PORTFOLIO_Assets/Font/`) |
| 숫자·영문 | `Baloo-Regular SDF` (Free Casual GUI 동봉) |
| Weight | Regular / Bold 2종만 |
| 굽기 방식 | TMP Font Asset Creator, Custom Characters (사용 문자만 포함해 아틀라스 절약) |
| 레거시 `Text` | **사용 금지** — 전부 `TextMeshProUGUI` |

### 10.4 색 규칙

| 역할 | 용도 |
| :--- | :--- |
| Normal | 버튼 기본, 게이지 평상시 |
| Success | 성공 토스트 배경 |
| Danger | 실패 토스트 배경, 남은 시간 10초 이하 게이지 점멸의 한쪽 끝 |
| Disabled | 판정 중 잠긴 진열대 버튼 |
| 등급 색 A~F | 결과 화면 `RankText` (11.4) |

Disabled 는 **기본 회색을 그대로 쓰지 않는다.** 기본값은 배경과 구분이 잘 안 되어 잠긴 것인지
알아보기 어렵다 (12.5 참고).

### 10.5 버튼 상태

| 상태 | 표현 | 발생 조건 |
| :--- | :--- | :--- |
| Normal | 기본 색상 | 평상시 |
| Highlighted | 밝기 상승 | 마우스 hover / 키보드 선택 |
| Pressed | 축소 + 밝기 하강 | 누르는 중 |
| Disabled | Disabled 색 | 판정 연출 중 (진열대) |

### 10.6 패널

Free Casual GUI 의 패널 스프라이트는 **9-Slice Border 를 설정하고 사용한다.**
설정하지 않으면 크기를 늘렸을 때 모서리 장식이 늘어나 깨진다.

---

## 11. 피드백 설계 (요건 7)

| 상황 | 피드백 | 담당 |
| :--- | :--- | :--- |
| 주문 성공 | Success 색 토스트 "주문 성공!" | `ToastController` |
| 주문에 없는 품목 선택 | Danger 색 토스트 "주문에 없는 품목이에요" | `ToastController` |
| 판정 연출 중 | 진열대 버튼 5개 `interactable = false` + Disabled 색 | `ShelfView` |
| **남은 시간 10초 이하** | **게이지가 평상시 색 ↔ Danger 색으로 점멸** (색 변화 ①) | `HudController` |
| **최종 등급 표시** | **등급별로 RankText 색이 달라짐** (색 변화 ②) | `ResultView` + `RankTable` |
| 버튼 hover / 선택 / 눌림 | Normal / Highlighted / Pressed | UGUI Button |
| 쟁반 담김 / 빈칸 | 실선 슬롯 / 점선 슬롯 | `TrayView` |

요건은 "2개 이상"이며 위에서 **알림·비활성화·색 변화(2종)·버튼 상태**를 모두 구현한다.

### 11.3 시간 경고 점멸

남은 시간 10초부터 게이지 색이 평상시 색과 Danger 색 사이를 오간다. 정적인 색 전환보다
"시간이 흐르고 있다"는 것이 강하게 전달된다.

**점멸의 위상은 `Time.time` 이 아니라 `remainingTime` 이 결정한다.**
`Time.time` 을 쓰면 일시정지 중에도 게이지가 계속 깜빡여, 게임은 멈췄는데 HUD 만 살아
움직이는 상태가 된다. 남은 시간을 위상으로 쓰면 `Tick()` 이 멈출 때 색도 함께 얼어붙는다.
일시정지 대응이 별도 코드 없이 따라온다.

**숫자는 점멸시키지 않는다.** 마지막 10초에 플레이어가 하는 일은 숫자를 읽는 것이다.
글자 색이 계속 변하면 대비가 흔들려 읽기 어려워진다. 게이지는 점멸, 숫자는 Danger 색 고정.

**속도는 초당 1회.** 초당 3회를 넘는 점멸은 광과민성 발작 유발 기준(WCAG 2.3.1)에 걸린다.
시간이 줄수록 빨라지는 가속 점멸은 넣지 않는다 — 얻는 것 없이 이 기준만 위험해진다.

### 11.4 등급 색

최종 등급 A~F 를 문자와 색으로 동시에 전달한다. 색만으로 등급을 구분하게 두지 않고
**문자를 주 신호로, 색을 보조 신호로** 쓴다 (색각 이상 대응).

| 등급 | 색 |
| :-- | :--- |
| A | `#FFD24A` 금색 |
| B | `#6FD1FF` 하늘 |
| C | `#7ED37E` 초록 |
| D | `#FFB067` 주황 |
| E | `#FF8A6B` 연한 빨강 |
| F | `#9AA0A6` 회색 |

`RankText` 는 400px 로 매우 크므로 대비 기준이 완화된다(큰 글자 3:1). 회색인 F 만
배경과의 대비를 확인한다.

### 11.1 토스트 설계

- 매번 `Instantiate` 하지 않고 `Screen_Play` 아래 `ToastMessage` 오브젝트 **1개를 켜고 끈다.**
- 표시 시간은 `GameState.judgeDelaySeconds`(0.8초) **이하**로 둔다. 토스트가 다음 손님의 주문
  위로 넘어와 떠 있으면 어느 판정에 대한 알림인지 구분되지 않는다.
- 연속으로 뜰 수 있으므로 이전 코루틴을 반드시 중단하고 새로 시작한다.
- `Raycast Target` 을 꺼서 버튼 클릭을 가로채지 않게 한다.

### 11.2 비활성화 피드백을 진열대가 담당하는 이유

원안에서는 "쟁반이 비어 있으면 제출 버튼 비활성화"가 요건 7의 비활성화 항목이었다.
제출 버튼을 없앴으므로 **판정 연출 중 진열대 잠금**이 그 역할을 대신한다.
연타 방지라는 기능적 필요와 비활성화 피드백이라는 요건을 **하나로 해결**한다.

---

## 12. GUI 디자인 가이드 · 구현 가능성 검토 (요건 2)

### 12.1 최소 버튼 크기

기준: 1080px 폭 기기(xxhdpi, 3배 밀도)에서 권장 최소 터치 타겟 48dp = **144px**.

| 버튼 | 실제 크기 | 판정 |
| :--- | :--- | :--- |
| `MenuButton_Start` / `_Quit` | 500 × 200 | 충분 |
| `ResumeButton`, `GoTitleButton`, `ReplayButton`, `YesButton`, `NoButton` | 500 × 150 | 충분 |
| `CakeButton_1~5` (진열대) | 180 × 180 | 충분 |
| `PauseButton` | **100 × 100** | **미달 (144 미만)** |

> **검토 결과 — 변경 필요**: `PauseButton` 이 권장 최소치보다 작다. 조작 빈도가 낮은
> 보조 버튼이지만 실패 시 되돌릴 수 없는 위치(상단 구석)에 있어 오조작 위험이 있다.
> **144 × 144 이상으로 키운다.** 아이콘 자체는 키우지 않고 버튼의 히트 영역만 넓히면
> 시각적 비중을 유지하면서 터치 안정성을 확보할 수 있다.

### 12.2 글자 크기와 대비

기준: 1080px 폭에서 본문 최소 14dp = **42px**.

| 용도 | 요소 | 크기 | 판정 |
| :--- | :--- | ---: | :--- |
| 랭크 | `RankText` | 400 | — |
| 팝업 제목 | `ConfirmText` / `PauseText` | 180 / 160 | 충분 |
| 화면 제목 | `ResultText`, `TitleLabelText` | 150 / 100 | 충분 |
| HUD 수치 | `CurrentSlotValueText` | 100 | 충분 |
| 버튼 라벨 | 각 버튼 | 70 ~ 80 | 충분 |
| 본문·HUD 라벨 | `TimerText`, `ScoreText`, `ChoiceText` 등 | 60 | 충분 |
| 보조 라벨 | `CurrentText` "성공 / 실패" | 50 | 경계 |
| 최소 | `CustomerNameText`, `ConfirmDescriptionText` | **40** | **미달 (42 미만)** |

> **검토 결과 — 변경 필요**: 40px 텍스트 2종이 권장 최소치를 근소하게 밑돈다.
> `ConfirmDescriptionText` 는 되돌릴 수 없는 선택을 설명하는 문장이므로 **48px 이상**으로
> 올린다. `CustomerNameText`("손님 9")는 장식성 라벨이라 그대로 두어도 무방하나, 손님 번호가
> 진행 상황을 알려주는 정보이므로 함께 올리는 편이 낫다.

대비는 배경 패널 위 텍스트 기준 **4.5:1 이상**을 유지한다. Free Casual GUI 의 패널은 중간 채도
배경이 많아 흰 텍스트 + 어두운 아웃라인 조합으로 확보한다.

### 12.3 정보 우선순위

| 순위 | 정보 | 배치 | 근거 |
| :--- | :--- | :--- | :--- |
| 1 | 현재 주문 | 화면 중앙 상단, 가장 큰 아이콘 | 이걸 못 읽으면 게임이 성립하지 않는다 |
| 2 | 남은 시간 | 상단, 게이지 + 숫자 이중 표시 | 긴박감의 원천. 색 변화로 10초 이하를 알린다 |
| 3 | 쟁반 상태 | 주문 바로 아래 | 진행 상황. 주문과 붙여 놓아 대조가 쉽다 |
| 4 | 점수 | 상단 좌측 | 결과 지표지만 플레이 중 판단에 영향을 주지 않는다 |
| 5 | 성공 / 실패 | 상단 | 누적 기록. 결과 화면에서 다시 보여준다 |
| 6 | 일시정지 | 상단 구석 | 사용 빈도가 가장 낮다 |

시간을 게이지와 숫자로 **이중 표시**한 이유: 게이지는 남은 양을 한눈에, 숫자는 정확한 초를
알려준다. 둘 중 하나만 있으면 "곧 끝나는가"와 "몇 초 남았는가" 중 하나를 놓친다.

### 12.4 지원 입력 방식

| 입력 | 지원 | 검증 방법 |
| :--- | :--- | :--- |
| 마우스 | 지원 | 에디터에서 직접 클릭 |
| 터치 | 지원 (마우스와 동일 경로) | Device Simulator |
| 키보드 Navigate | 지원 (방향키 / WASD) | 화면별 이동 순서 확인 |
| 키보드 Submit | 지원 (Enter) | 선택 상태에서 Enter |
| 키보드 Cancel | 지원 (Esc) | 4.1 의 4가지 상황 |
| 게임패드 Navigate | 지원 (좌스틱 / 우스틱 / D-pad) | 8.5 의 배선대로 이동하는지 |
| 게임패드 Submit | 지원 (A / buttonSouth) | 선택 상태에서 A |
| 게임패드 Cancel | 지원 (B / buttonEast) | 4.1 의 4가지 상황 |

**게임패드 검증 시 반드시 확인할 것**: 마우스로 버튼을 한 번 클릭한 **직후** 게임패드 스틱을
기울여 본다. 8.4 의 선택 복구가 없으면 여기서 아무 반응이 없다. 실기 패드가 없으면
Input Debugger 의 가상 장치나 Device Simulator 로 대체한다.

### 12.5 구현 가능성 검토에서 원안을 바꾼 항목

| # | 원안 | 문제 | 변경 |
| :-- | :--- | :--- | :--- |
| 1 | 담기 → 제출 버튼으로 판정 | 주문 1건에 최소 2탭 필요, 조작 리듬이 끊김 | **즉시 판정**으로 변경 (2.1) |
| 2 | 판정 즉시 다음 손님 | 왜 틀렸는지 볼 시간이 없고, 연타 시 주문이 연쇄 실패 | **0.8초 지연 + 진열대 잠금** 추가 |
| 3 | 쟁반 슬롯 클릭으로 담기 취소 | 즉시 판정에서는 담긴 것이 항상 정답이라 뺄 이유가 없음 | **쟁반을 표시 전용으로** 변경 |
| 4 | 제출 버튼 비활성화 = 요건 7 근거 | 제출 버튼이 사라짐 | **진열대 잠금**이 비활성화 피드백을 대신 (11.2) |
| 5 | `PauseButton` 100 × 100 | 권장 최소 터치 타겟 144px 미달 | 144 × 144 이상으로 확대 |
| 6 | `ConfirmDescriptionText` 40px | 권장 본문 최소 42px 미달 | 48px 이상으로 확대 |
| 7 | 진열대 Navigation `Automatic` | 가로 1열 밀집 배치에서 이동 순서가 어긋남 | `Horizontal` 또는 `Explicit` 로 지정 |
| 8 | Submit = Enter / Space | 기본 바인딩 `*/{Submit}` 은 Enter 만 잡음 | **Enter 만 지원**으로 문서 정정 (또는 `<Keyboard>/space` 추가) |
| 9 | Navigate = 방향키 / Tab | Input System UI 모듈에 Tab 순회 기능이 없음 | **방향키만 지원**으로 문서 정정 |
| 10 | 게임패드 범위 제외 | 기존 `UI` 맵에 게임패드 바인딩이 이미 들어 있어 추가 비용이 사실상 없음 | **게임패드 지원으로 변경** (8장) |
| 11 | 포인터 클릭 시 선택 해제 | 게임패드에는 포인터가 없어, 선택이 비면 조작 수단이 완전히 사라짐 | `Navigate`·`Submit` 시 **선택 복구** 추가 (8.4) |
| 12 | 상단 `PauseButton` 도달 경로 없음 | 선택 계열 입력은 배선된 경로로만 이동 가능 | `CakeButton_5` 의 Up → `PauseButton` 배선 (8.5) |
| 13 | `RankText` 를 범위 밖으로 두고 비활성화 | 결과 화면에 이미 배치되어 있고, 색 변화 피드백을 하나 더 확보할 수 있음 | **등급 A~F 시스템 도입** (11.4) |
| 14 | 랭크 산식에 점수·성공·실패를 모두 사용 | `score ≡ successCount × 100` 이라 점수는 독립 정보가 없음. 같은 값을 두 번 세게 됨 | **처리량(success)·정확도(fail) 두 축만** 사용 |
| 15 | 남은 시간 10초 이하 → Danger 색 정적 전환 | 정적 전환은 한 번 바뀌고 나면 시간이 흐른다는 느낌을 주지 못함 | **점멸로 변경.** 단 위상은 `Time.time` 이 아니라 `remainingTime` (11.3) |

1~4번은 설계 판단, 5~9번은 실제 값 확인 과정에서 드러난 항목,
10~12번은 게임패드 지원, 13~15번은 색 변화 피드백을 결정하면서 따라온 항목이다.
구현·테스트 중 추가로 발견하는 항목은 `checklist.md` 에 **기대 / 실제 / 원인 / 수정 / 재확인**
형식으로 기록한다.

---

## 13. 범위에서 제외한 항목

| 제외 항목 | 사유 |
| :--- | :--- |
| Safe Area(노치) 대응 | 실기기 배포가 제출 범위가 아니다. 상단 HUD 여백으로 갈음 |
| 드래그 앤 드롭 | 포인터 전용 조작이 되어 키보드 대응과 충돌한다 |
| 난이도 상승, 콤보·보너스 | UI/UX 검증이 목적이므로 게임성 확장은 범위 밖 |
| 최고 점수 저장, 사운드 | 요건에 없다 |
| 씬 분리 | 단일 씬 + 패널 전환으로 충분하며 로딩 공백이 없다 |
| 조이스틱 · XR | `UI` 맵에 바인딩은 남아 있으나 검증하지 않는다. 게임패드와 달리 조작 모델이 달라 별도 설계가 필요하다 |
| 입력 장치별 버튼 아이콘 표시 | 키보드/패드에 따라 안내 아이콘을 바꾸는 것은 별도 리소스가 필요하다. 조작 자체는 양쪽 모두 동작한다 |
| 오브젝트 풀링, 외부 트윈 라이브러리 | 슬롯 수가 최대 6개로 최적화 필요가 없다 |

---

## 14. 과제 요건 대응표

| 요건 | 대응 | 위치 |
| :--- | :--- | :--- |
| 1. UI/UX 설계 문서 | 화면 목록·입력·전환·Prefab·해상도·리소스 규칙·콘셉트 | 이 문서 1~11장 |
| 2. GUI 가이드·구현 가능성 검토 | 최소 버튼 크기, 글자 크기, 정보 우선순위, 입력 방식, 변경 9건 | 12장 |
| 3. HUD 3개 이상 + 데이터 연결 | 시간·점수·성공/실패·주문 안내 **4종** | 7장 |
| 4. 화면 3개 이상 + 버튼 전환 | Title / Play / Pause / Result **4개** | 3~4장 |
| 5. 마우스 + 키보드/게임패드 2개 이상 | 마우스·터치 + **키보드·게임패드 양쪽**에서 Navigate·Submit·Cancel **3종** | 8장 |
| 6. 재사용 Prefab 2개 이상 | OrderPrefab, ChoicePrefab, Score, Progress Slider **4종** (+ToastMessage) | 9장 |
| 7. 피드백 2개 이상 | 알림·비활성화·**색 변화 2종**(시간 점멸·등급 색)·버튼 상태 | 11장 |
| 8. 체크리스트 검증 | 해상도 4종, 입력, 전환, 데이터 연결 | `checklist.md` |

---

## 15. 구현 순서

세부 명세는 [script-design.md](script-design.md) 7장을 따른다. 요약하면,

```
1. 화면 전환 골격 (ScreenManager) + 포커스 지정
2. Cancel(Esc / 패드 B) 처리 + 선택 복구 (UiInputRouter)
3. 시간 + HUD 바인딩
4. 주문 생성 + 주문 카드 표시
5. 진열대 입력 → 즉시 판정 → 쟁반 표시
6. 판정 지연·잠금 + 토스트
7. 결과 화면 + 다시하기
8. 12.5 의 5~7·12번 반영 (버튼 크기, 글자 크기, Navigation 배선)
9. 해상도 4종 테스트 + 입력 3계열(마우스·키보드·게임패드) 테스트 → checklist.md 기록
```
