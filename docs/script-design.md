# 스크립트 · Input Actions 설계 문서

작성 기준: `Assets/MyAssets/PORTFOLIO_Assets/Scene/PortfolioAssignment.unity` 의 현재 하이라키
(2026-08-19, UI 배치 완료 시점)

이 문서는 **구현 전에 읽는 설계도**입니다. 각 스크립트가 무엇을 소유하고 무엇을 소유하지 않는지,
어떤 씬 오브젝트에 붙는지, 인스펙터에 무엇을 꽂는지를 정합니다.
코드를 그대로 받아쓰는 문서가 아니라, 직접 작성할 때 헷갈리는 지점을 미리 없애는 것이 목적입니다.

---

## 0. 판정 방식 — 즉시 판정 (제출 버튼 없음)

제출 버튼을 두지 않기로 했습니다. **진열대 버튼을 누른 그 순간 판정합니다.**

```
주문에 있는 품목을 누름   → 쟁반에 담김, 계속 진행
주문에 없는 품목을 누름   → 그 자리에서 실패, 다음 손님
주문을 전부 채움          → 그 자리에서 성공, 다음 손님
```

이 방식이 더 낫습니다. "케이크 누르고 → 제출 누르고" 라는 2단계가 사라져서 터치 횟수가 줄고,
쟁반에 담긴 것은 항상 정답의 일부라서 화면이 거짓말을 하지 않습니다.

**대신 따라오는 것 3가지** — 이걸 안 하면 즉시 판정은 망가집니다.

| # | 항목 | 이유 |
| :-- | :--- | :--- |
| 1 | **판정 후 지연 + 입력 잠금** | 실패한 순간 주문이 바로 바뀌면 **왜 틀렸는지 볼 시간이 없습니다.** 그리고 잠그지 않으면 연타 한 번에 주문 서너 개가 순식간에 실패 처리됩니다. 4-4 참고 |
| 2 | **실패 토스트** | 제출 버튼이 없어졌으므로, 실패를 알려줄 수단이 토스트밖에 없습니다. 씬·프리팹 모두 아직 없으니 `Screen_Play` 아래 `ToastMessage` 오브젝트를 1개 두고 켜고/끄기 (매번 Instantiate 하지 않음) |
| 3 | **요건 7 "비활성화" 대체** | 제출 버튼의 `interactable = false` 가 사라졌습니다. 1번의 입력 잠금이 그 역할을 대신합니다 — 판정 중 진열대 5개 버튼이 Disabled 색으로 바뀝니다. **기능과 피드백을 동시에 해결하므로 반드시 넣으세요.** |

이와 별개로 결정할 것 하나: `Screen_Result` 의 **RankText** 는 요건 목록에 없는 항목입니다.
점수 구간으로 A/B/C 를 찍는 3줄 규칙으로 끝내거나, 오브젝트를 끄세요.

> 이 변경으로 CLAUDE.md 와 어긋나는 부분이 3군데 생겼습니다. 9장 참고.

---

## 1. 현재 씬 하이라키와 담당 스크립트

```
Canvas (Scale With Screen Size / 1080x1920 / Match 0)   ← 설정 확인 완료
└ UIRoot                                   ← ScreenManager, GameState, OrderGenerator, DessertTable
  ├ Screen_Title
  │ ├ TitleLabel (TitleLabelText, Logo, Image, Image (1))
  │ ├ MenuButton_Start                     ← 첫 선택 버튼
  │ └ MenuButton_Quit
  ├ Screen_Play
  │ ├ TopNav                               ← HudController
  │ │ ├ Score (프리팹 인스턴스)
  │ │ │ ├ ScoreText          "점수 :"
  │ │ │ └ ScoreValueText     "1000"        ← 점수 바인딩
  │ │ ├ Timer
  │ │ │ ├ Progress Slider_Yellow (프리팹)  ← 영업시간 게이지
  │ │ │ ├ TimerIcon
  │ │ │ └ TimerText          "120"         ← 남은 시간 바인딩
  │ │ ├ CurrentSlot
  │ │ │ ├ CurrentText        "성공 / 실패"
  │ │ │ └ CurrentSlotValueText  "0 / 0"    ← 처리/실패 바인딩  ※ 이름 끝에 공백 있음
  │ │ └ PauseButton (PauseIcon)
  │ ├ MidNav
  │ │ ├ OrderPanel                         ← OrderCardView
  │ │ │ ├ Customer (CustomerIcon, CustomerNameText "손님 9")
  │ │ │ ├ CurrentOrderText   "현재 주문"
  │ │ │ └ OrderGridLayout                  ← OrderPrefab 을 여기에 생성
  │ │ └ ChoiceListPanel                    ← TrayView (표시 전용)
  │ │   ├ ChoiceText         "쟁반"
  │ │   └ ChoiceGridLayout                 ← ChoicePrefab 을 여기에 생성
  │ ├ BotNav
  │ │ └ DisplayStandList                   ← ShelfView
  │ │   ├ DisplayStandText   "진열대"
  │ │   └ CakeButton_1 ~ CakeButton_5      ← 각각 ShelfButton
  │ └ (ToastMessage)                       ← 0-2 에서 추가할 것 (필수)
  ├ Screen_Pause
  │ └ PausePopup
  │   ├ PauseText, ResumeButton ← 첫 선택 버튼, GoTitleButton
  │   └ ConfirmPopup                       ← 별도 레이어로 취급
  │     └ ConfirmText, ConfirmDescriptionText, YesButton, NoButton ← 첫 선택은 NoButton
  └ Screen_Result                          ← ResultView
    ├ ResultText "결과"
    ├ ResultTextPanel (SuccessText, FailText, ScoreText, RankText)
    ├ ReplayButton ← 첫 선택 버튼
    └ GoTitleButton
```

> `CurrentSlotValueText ` 는 이름 끝에 **공백**이 들어가 있습니다. `[SerializeField]` 로 꽂을 거라
> 동작에는 문제없지만, 지금 고쳐 두는 편이 낫습니다.

---

## 2. 원칙 — 데이터와 표시의 분리

과제 요건 3번("HUD 표시 값이 실제 게임 상태와 연결")의 채점 근거가 이 구조입니다.

```
GameState        점수·시간·주문·쟁반·성공/실패를 "소유"한다
   │  이벤트로 알림
   ▼
Controller/View  GameState를 읽어서 TMP·Image에 "반영만" 한다
```

지켜야 할 선:

- `HudController` 안에 `private int score;` 가 있으면 안 됩니다. 점수를 더하는 주체는 `GameState` 하나입니다.
- View 는 `GameState` 를 **읽기만** 합니다. 버튼을 눌렀을 때는 `gameState.Pick(type)` 처럼
  GameState 의 메서드를 호출하고, 화면 갱신은 GameState 가 던진 이벤트를 받아서 합니다.
- View 가 스스로 판단해서 화면을 바꾸면 안 됩니다. (예: TrayView 가 직접 "성공!" 을 띄우지 않음)

---

## 3. 스크립트 목록

`Assets/MyAssets/PORTFOLIO_Assets/Scripts/` 아래에 둡니다.

```
Scripts/
├ Core/
│ ├ DessertType.cs        enum
│ ├ DessertTable.cs       DessertType → 스프라이트 조회
│ ├ OrderGenerator.cs     무작위 주문 생성
│ ├ GameState.cs          게임 데이터 + 진행 (이 프로젝트의 심장)
│ └ RankTable.cs          성공·실패 → 등급 A~F 판정
├ UI/
│ ├ ScreenManager.cs      화면 전환 + 첫 선택 버튼 지정
│ ├ UiInputRouter.cs      Cancel(Esc·패드B), 포인터 시 선택 해제, 선택 복구
│ ├ HudController.cs      TopNav 바인딩
│ ├ OrderCardView.cs      주문 카드 표시
│ ├ TrayView.cs           쟁반 표시 (표시 전용)
│ ├ ShelfView.cs          진열대 5버튼 입력 수신 + 판정 중 잠금
│ ├ ShelfButton.cs        버튼 1개가 자기 DessertType 을 들고 있음
│ ├ DessertIconView.cs    OrderPrefab / ChoicePrefab 공용 컴포넌트
│ ├ ToastController.cs    알림 표시·소멸
│ └ ResultView.cs         결과 화면 값 채우기
└ States/
  ├ IState.cs             화면 상태 계약 (FirstSelected / Enter / Exit / OnCancel)
  ├ UiStateBase.cs        공통 부모. 각 Screen_ 패널에 붙는다
  ├ TitleState.cs
  ├ PlayState.cs
  ├ PauseState.cs
  ├ ConfirmState.cs
  └ ResultState.cs
```

각 파일 100줄을 넘지 않아야 정상입니다. `GameState` 만 150줄 근처가 됩니다.

기존 파일 처리:

| 파일 | 처리 |
| :--- | :--- |
| `UIFlowController.cs` | **삭제 완료.** `Scripts/UI/ScreenManager.cs` 가 대체합니다. |
| `States/IState.cs` | **채택.** 스택 FSM 으로 확정했습니다. 부록 A 참고. |

네임스페이스는 기존 파일의 `Assets.MyAssets.PORTFOLIO_Assets.Scripts...` 를 그대로 이어가든,
전부 `DessertShop` 으로 통일하든 **둘 중 하나로** 가세요. 섞이면 `using` 이 지저분해집니다.

---

## 4. 스크립트 상세

### 4-1. `DessertType.cs`

```csharp
public enum DessertType
{
    ChocoCake,
    KiwiBigCake,
    LemonRectangularCake,
    RainbowCupCake,
    SkullCakePiece
}
```

MonoBehaviour 아님. enum 하나만 있는 파일입니다.
**순서를 바꾸지 마세요.** `(int)DessertType` 을 배열 인덱스로 쓸 예정이라, 순서가 바뀌면
인스펙터에 꽂아둔 스프라이트가 전부 어긋납니다.

---

### 4-2. `DessertTable.cs` — 붙일 곳: `UIRoot`

`DessertType` 하나로 아이콘을 얻는 조회 테이블입니다.
이게 없으면 OrderCardView·TrayView·ShelfView 가 각자 스프라이트 배열을 들게 되고,
스프라이트를 바꿀 때 세 군데를 고쳐야 합니다.

| 인스펙터 | 타입 | 꽂을 것 |
| :--- | :--- | :--- |
| `sprites` | `Sprite[5]` | `Sprites/CakeIcon/` 의 5종을 **enum 순서대로** |
| `displayNames` | `string[5]` | "초코 케이크" 등 (필요 없으면 생략) |

공개 API

- `Sprite GetSprite(DessertType type)`
- `int Count` → 5. OrderGenerator 가 `Random.Range(0, table.Count)` 에 씁니다. 5를 하드코딩하지 마세요.

---

### 4-3. `OrderGenerator.cs` — 붙일 곳: `UIRoot`

무작위 주문을 만들기만 합니다. 게임 상태를 건드리지 않습니다.

| 인스펙터 | 타입 | 기본값 | 의미 |
| :--- | :--- | :--- | :--- |
| `dessertTable` | `DessertTable` | UIRoot | 품목 개수 조회 |
| `minKinds` | `int` | 1 | 주문에 들어갈 품목 **종류** 수 최소 |
| `maxKinds` | `int` | 3 | 최대 |
| `minCountPerKind` | `int` | 1 | 종류별 **개수** 최소 |
| `maxCountPerKind` | `int` | 2 | 최대 |
| `maxTotalCount` | `int` | 6 | 총 개수 상한 (쟁반 슬롯 수와 같아야 함) |

공개 API

- `List<DessertType> Generate()` → 예: `[ChocoCake, ChocoCake, KiwiBigCake]`

구현 주의

- 종류를 뽑을 때 **중복되지 않게** 뽑아야 합니다. 같은 종류를 두 번 뽑으면 개수가 의도보다 늘어납니다.
  후보 리스트를 만들어 뽑을 때마다 제거하는 방식이 가장 단순합니다.
- 총 개수가 `maxTotalCount` 를 넘지 않는지 마지막에 확인하세요.
  기본값은 `3종 × 2개 = 6` 이라 안전하지만, 값을 바꾸면 깨집니다.
- 이 값과 `OrderGridLayout` / `ChoiceGridLayout` 의 칸 수가 어긋나면 UI 가 넘칩니다.

---

### 4-4. `GameState.cs` — 붙일 곳: `UIRoot`

**이 프로젝트에서 데이터를 소유하는 유일한 스크립트입니다.**

보유 데이터

| 필드 | 타입 | 설명 |
| :--- | :--- | :--- |
| `score` | `int` | 점수 |
| `remainingTime` | `float` | 남은 영업시간 |
| `currentOrder` | `List<DessertType>` | 현재 손님의 주문 (표시용 원본) |
| `remaining` | `Dictionary<DessertType,int>` | 주문에서 **아직 안 담은 개수**. 즉시 판정의 핵심 |
| `tray` | `List<DessertType>` | 쟁반에 담긴 것 |
| `isJudging` | `bool` | 판정 연출 중 입력 잠금 |
| `successCount` | `int` | 처리 수 |
| `failCount` | `int` | 실패 수 |
| `customerNumber` | `int` | 손님 번호 (CustomerNameText "손님 9") |

전부 `private` + 읽기 전용 프로퍼티로 노출합니다.
`public List<DessertType> Tray` 로 열어 두면 바깥에서 `Add()` 를 해버릴 수 있으므로
`IReadOnlyList<DessertType>` 로 내보내세요.

| 인스펙터 | 타입 | 기본값 |
| :--- | :--- | :--- |
| `orderGenerator` | `OrderGenerator` | UIRoot |
| `playTimeSeconds` | `float` | 120 |
| `scorePerSuccess` | `int` | 100 |
| `trayCapacity` | `int` | 6 |
| `judgeDelaySeconds` | `float` | 0.8 — 판정 후 다음 손님까지의 간격 |

공개 메서드

| 메서드 | 하는 일 |
| :--- | :--- |
| `StartGame()` | 모든 값 초기화 → 첫 손님 생성 → 관련 이벤트 **전부** 발행 |
| `Pick(DessertType type)` | 진열대 버튼 입력. **여기서 즉시 판정합니다.** |
| `Tick(float deltaTime)` | 시간 감소, 0 이면 `OnGameOver` |
| `SetRunning(bool)` | 일시정지 제어 |

`Submit()` 도 `RemoveFromTray()` 도 없습니다. 게임 진행 중 바깥에서 부르는 메서드는 `Pick()` 하나뿐입니다.

이벤트 (View 가 구독)

```csharp
event Action<int>          OnScoreChanged;   // 점수
event Action<float, float> OnTimeChanged;    // (남은 시간, 전체 시간)
event Action<int, int>     OnCountChanged;   // (성공, 실패)
event Action<IReadOnlyList<DessertType>, int> OnOrderChanged;  // (주문, 손님번호)
event Action<IReadOnlyList<DessertType>>      OnTrayChanged;
event Action<bool>         OnJudged;         // true=성공 → 토스트
event Action<bool>         OnJudgingChanged; // true=입력 잠금 → ShelfView 가 버튼을 끔
event Action               OnGameOver;       // → ScreenManager 가 Result 로 전환
```

이벤트를 쓰는 이유는 요건이 아니라 **"Update() 에서 매 프레임 문자열을 만들지 말 것"** 을
지키기 위해서입니다. 값이 바뀔 때만 이벤트가 나가므로 TMP 갱신도 그때만 일어납니다.

시간 처리

- `Tick()` 은 `GameState.Update()` 안에서 스스로 호출해도 되고 ScreenManager 가 불러 줘도 됩니다.
  **단 Play 화면일 때만** 흘러야 합니다. Pause 중에 시간이 줄면 안 됩니다.
- `Time.timeScale = 0` 을 쓰지 말고 `isRunning` bool 로 막으세요.
  timeScale 은 나중에 토스트 코루틴까지 같이 멈춰 버립니다.
- `OnTimeChanged` 는 매 프레임 나갑니다. 게이지는 매 프레임 갱신해도 되지만
  **TimerText 는 정수 초가 바뀔 때만** 갱신하세요. 그 판단은 `HudController` 쪽에서 합니다.

### 즉시 판정 로직

주문을 `List` 로만 들고 있으면 매번 개수를 다시 세야 합니다.
**"남은 개수" 딕셔너리**를 같이 들고 있으면 판정이 세 줄로 끝납니다.

주문을 만들 때 `remaining` 을 채웁니다. `[초코, 초코, 키위]` → `{ 초코:2, 키위:1 }`

```
Pick(type):
    if (isJudging) return;                       // 판정 연출 중이면 무시

    if (remaining 에 type 이 없거나 remaining[type] == 0)
        → 실패 확정

    remaining[type]--;  tray.Add(type);  OnTrayChanged 발행

    if (remaining 의 값이 전부 0)
        → 성공 확정
```

- **순서는 여전히 무관합니다.** 주문에 있는 것을 아무 순서로나 누르면 됩니다.
- 쟁반에 담긴 것은 **항상 정답의 일부**입니다. 틀린 것은 애초에 담기지 않습니다.
- **개수 초과도 자동으로 잡힙니다.** 초코 2개짜리 주문에서 세 번째 초코를 누르면
  `remaining[초코] == 0` 이므로 실패입니다. 따로 검사할 필요가 없습니다.
- 이전 설계의 "품목별 개수 전체 비교" 는 이제 필요 없습니다. 지우세요.

### 판정 후 처리 — 바로 다음 손님으로 넘기지 마세요

```
성공/실패 확정
  → successCount++ (성공이면 score += scorePerSuccess) 또는 failCount++
  → OnJudged(bool)          // 토스트 표시
  → OnScoreChanged / OnCountChanged
  → isJudging = true  →  OnJudgingChanged(true)     // 진열대 버튼 잠금
  → judgeDelaySeconds 만큼 대기 (코루틴)
  → 쟁반 비움, customerNumber++, 새 주문 + remaining 재구성
  → OnTrayChanged / OnOrderChanged
  → isJudging = false →  OnJudgingChanged(false)
```

지연을 넣는 이유 두 가지. 둘 다 실제로 겪게 됩니다.

1. **왜 틀렸는지 볼 시간이 없습니다.** 실패한 순간 주문 카드가 다음 손님 것으로 바뀌면,
   방금 무엇을 잘못 눌렀는지 확인할 수 없습니다. 토스트가 떠 있는 동안 **틀린 주문이 그대로
   보여야** 피드백이 의미를 갖습니다.
2. **연타로 주문이 줄줄이 실패합니다.** 즉시 판정에서 가장 크게 티나는 결함입니다.
   버튼을 빠르게 두세 번 누르면 실패 → 새 주문 → 또 실패가 0.1초 안에 일어납니다.

`isJudging` 은 **두 겹으로** 막으세요.

| 층 | 위치 | 없으면 |
| :--- | :--- | :--- |
| 데이터 | `Pick()` 첫 줄의 `if (isJudging) return;` | 키보드 Submit 이나 빠른 클릭이 뚫습니다 |
| 표시 | `ShelfView` 가 버튼 `interactable = false` | 버튼이 눌리는 것처럼 보여서 사용자가 고장으로 오해합니다 |

일시정지 중에도 `Pick()` 이 동작하면 안 됩니다. `isRunning` 검사도 같은 자리에 넣으세요.

---

### 4-5. `ScreenManager.cs` — 붙일 곳: `UIRoot`

화면 전환과 **키보드 포커스 지정**을 담당합니다. 포커스 지정이 요건 5 의 핵심입니다.

스택 FSM 의 소유자입니다. 설계 근거는 부록 A.

| 인스펙터 | 타입 | 꽂을 것 |
| :--- | :--- | :--- |
| `titleState` | `TitleState` | Screen_Title |
| `playState` | `PlayState` | Screen_Play |
| `pauseState` | `PauseState` | Screen_Pause |
| `confirmState` | `ConfirmState` | ConfirmPopup |
| `resultState` | `ResultState` | Screen_Result |

첫 선택 버튼은 각 상태의 `firstSelected` 로 옮겨 갔습니다. 화면과 그 화면의 첫 버튼이
같은 오브젝트에 붙어 있어야 나중에 헷갈리지 않습니다.

공개 멤버

| 멤버 | 호출 지점 |
| :--- | :--- |
| `ShowTitle()` | Pause·Result 의 GoTitleButton, YesButton → `Set` |
| `ShowPlay()` | MenuButton_Start, ReplayButton → `Set` |
| `ShowResult()` | `OnGameOver` 구독 → `Set` |
| `ShowPause()` / `HidePause()` | PauseButton, ResumeButton, Cancel → `Push` / `Pop` |
| `OpenConfirm()` / `CloseConfirm()` | Pause 의 GoTitleButton, NoButton, Cancel → `Push` / `Pop` |
| `Quit()` | MenuButton_Quit |
| `HandleCancel()` | `UiInputRouter` → `Top.OnCancel()` |
| `CurrentFirstSelected` | `UiInputRouter` 의 선택 복구 (5-7) |

**반드시 지킬 것**

```csharp
private void ApplyFocus()
{
    // 화면을 켤 때마다 포커스를 다시 잡지 않으면 전환 후 키보드 입력이 먹통이 됩니다 (요건 5)
    EventSystem.current.SetSelectedGameObject(null);
    EventSystem.current.SetSelectedGameObject(CurrentFirstSelected);
}
```

`null` 로 한 번 비우고 다시 넣는 이유: 이전 선택이 남아 있으면 같은 오브젝트를 다시 지정할 때
`OnSelect` 가 발생하지 않아 하이라이트가 안 그려지는 경우가 있습니다.

`Push` 는 **패널을 켠 뒤에** 포커스를 줘야 합니다. 순서가 뒤바뀌면 비활성 오브젝트를
선택하려 해서 아무 일도 일어나지 않습니다.

`Pop` 은 **닫은 뒤 반드시 `ApplyFocus()` 를 다시** 불러야 합니다. 빠뜨리면 확인 팝업을
닫았을 때 포커스가 사라진 채 Pause 화면에 남습니다.

`Screen_Pause` 는 `Screen_Play` **위에 겹쳐** 띄웁니다 (`Push`). Pause 를 켤 때 Play 를
끄지 마세요 — 끄면 뒤에 게임 화면이 안 보여서 "일시정지" 로 읽히지 않습니다.
`Push` 가 아래층의 `Exit()` 를 부르지 않으므로 이건 자동으로 지켜집니다.
단 Play 가 켜져 있어도 시간은 멈춰야 합니다 → `PauseState.Enter()` 에서 `SetRunning(false)`.

`Quit()` 은 에디터에서 아무 일도 하지 않습니다. 다음처럼 쓰세요.

```csharp
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
```

---

### 4-6. `UiInputRouter.cs` — 붙일 곳: `UIRoot`

Cancel(Esc / 게임패드 B), 포인터 입력 시 선택 해제, **선택 복구** 를 담당합니다.
자세한 내용은 5장.

| 인스펙터 | 타입 | 꽂을 것 |
| :--- | :--- | :--- |
| `screenManager` | `ScreenManager` | UIRoot |
| `cancelAction` | `InputActionReference` | `InputSystem_Actions` → `UI/Cancel` |
| `clickAction` | `InputActionReference` | `InputSystem_Actions` → `UI/Click` |
| `navigateAction` | `InputActionReference` | `InputSystem_Actions` → `UI/Navigate` |
| `submitAction` | `InputActionReference` | `InputSystem_Actions` → `UI/Submit` |

Cancel 분기 — 상황별로 다르게 동작해야 합니다.

| 현재 상태 | Esc |
| :--- | :--- |
| Title | 무시 |
| Play | Pause 열기 |
| Pause (ConfirmPopup 닫힘) | Pause 닫기 |
| Pause (ConfirmPopup 열림) | **팝업만** 닫기 |
| Result | 무시 |

ConfirmPopup 이 열려 있을 때 Pause 까지 같이 닫히면 사용자가 놀랍니다.
위에서 아래로 순서대로 검사하고 **가장 위 레이어 하나만** 닫으세요.

이 분기 하나가 **Esc 와 게임패드 B 를 동시에 처리합니다.** `UI/Cancel` 의 바인딩
`*/{Cancel}` 에 Gamepad 그룹이 포함되어 있어 장치를 구분할 필요가 없습니다.

`ScreenManager` 에 현재 화면의 첫 버튼을 돌려주는 프로퍼티가 하나 필요합니다
(선택 복구에 사용). 예: `GameObject CurrentFirstSelected { get; }`

---

### 4-7. `DessertIconView.cs` — 붙일 곳: `OrderPrefab`, `ChoicePrefab` 루트

두 프리팹은 구조가 같습니다 (`CakeIcon` + `CountText`).
**컴포넌트를 하나로 공유**하면 "프리팹 재사용" 근거가 더 명확해집니다.

| 인스펙터 | 타입 | 꽂을 것 |
| :--- | :--- | :--- |
| `iconImage` | `Image` | CakeIcon |
| `countText` | `TextMeshProUGUI` | CountText |
| `button` | `Button` | (쟁반 슬롯에서 취소용) 자기 자신 |

공개 API

- `void SetIcon(Sprite sprite, int count)` — `count <= 1` 이면 `countText` 오브젝트를 끕니다
  ("×1" 은 표시하지 않는 편이 깔끔합니다)
- `void SetClickHandler(Action onClick)` — 쟁반 슬롯에서만 사용

> 지금 `OrderPrefab` 과 `ChoicePrefab` 은 **별도 프리팹 2개**입니다. 요건의 "재사용 Prefab 2개 이상"은
> 이미 충족합니다. CLAUDE.md 의 "DessertIcon 하나를 3곳에서 재사용" 형태로 맞추려고
> **지금 합치는 것은 권장하지 않습니다** — 배치가 이미 끝났고 되돌리는 비용이 이득보다 큽니다.
> 체크리스트에 "두 프리팹은 동일 구조·동일 컴포넌트를 공유한다" 라고 적는 것으로 충분합니다.

---

### 4-8. `OrderCardView.cs` — 붙일 곳: `OrderPanel`

| 인스펙터 | 타입 | 꽂을 것 |
| :--- | :--- | :--- |
| `gameState` | `GameState` | UIRoot |
| `dessertTable` | `DessertTable` | UIRoot |
| `gridRoot` | `Transform` | OrderGridLayout |
| `iconPrefab` | `DessertIconView` | OrderPrefab |
| `customerNameText` | `TextMeshProUGUI` | CustomerNameText |

동작

- `OnEnable` 에서 `gameState.OnOrderChanged += Refresh`, `OnDisable` 에서 해제.
  **해제를 빼먹으면** 화면을 껐다 켤 때마다 구독이 쌓여 아이콘이 중복 생성됩니다. 가장 흔한 버그입니다.
- `Refresh(order, customerNumber)`
  1. `gridRoot` 의 기존 자식을 전부 `Destroy`
  2. 주문을 **품목별로 묶어서** (초코 ×2) 아이콘 1개 + 개수 표시로 생성
  3. `customerNameText.text = "손님 " + customerNumber`
- 묶어서 표시하는 이유: 슬롯을 적게 써서 그리드가 안정적이고, 개별 나열인 쟁반과 시각적으로
  대비되어 "주문 vs 담은 것" 이 구분됩니다.

---

### 4-9. `TrayView.cs` — 붙일 곳: `ChoiceListPanel`

즉시 판정으로 바뀌면서 **표시 전용**이 되었습니다. 클릭을 받지 않습니다.

| 인스펙터 | 타입 | 꽂을 것 |
| :--- | :--- | :--- |
| `gameState` | `GameState` | UIRoot |
| `dessertTable` | `DessertTable` | UIRoot |
| `gridRoot` | `Transform` | ChoiceGridLayout |
| `iconPrefab` | `DessertIconView` | ChoicePrefab |

동작

- `gameState.OnTrayChanged` 구독 → 슬롯 다시 그림. 이게 전부입니다.
- **쟁반은 개별 나열**입니다 (묶지 않음). 담은 순서대로 한 칸씩 채워야 "3개 담았다" 가 즉시 읽힙니다.
- 빈 슬롯 표현(요건 7의 "실선 슬롯 / 점선 슬롯")은, 미리 `trayCapacity` 개의 슬롯을 만들어 두고
  빈 칸은 점선 스프라이트 + 아이콘 숨김으로 처리하는 것이 가장 단순합니다.

**슬롯 클릭으로 담기 취소는 넣지 마세요.** 즉시 판정에서는 쟁반에 담긴 것이 항상 정답이므로,
빼는 행동은 순수한 손해일 뿐 아무 의미가 없습니다. `ChoicePrefab` 에 Button 이 붙어 있다면
`interactable` 을 꺼두거나 컴포넌트를 제거하세요. 눌리는데 아무 일도 안 일어나는 것이
가장 나쁩니다.

---

### 4-10. `ShelfButton.cs` — 붙일 곳: `CakeButton_1` ~ `CakeButton_5` (5개 전부)

| 인스펙터 | 타입 |
| :--- | :--- |
| `type` | `DessertType` (버튼마다 다르게 지정) |

공개 API

- `DessertType Type => type;`
- `Button Button` (`GetComponent<Button>()` 캐시)

버튼 이미지의 스프라이트는 이미 배치되어 있으므로 `Start()` 에서 `DessertTable` 로 덮어쓰지 마세요.
**인스펙터에 꽂은 `type` 과 실제 보이는 아이콘이 일치하는지**만 눈으로 확인하면 됩니다.
여기가 어긋나면 "눌렀는데 다른 게 담기는" 버그가 되고, 원인을 찾기가 매우 어렵습니다.
체크리스트 항목으로 넣어 두세요.

---

### 4-11. `ShelfView.cs` — 붙일 곳: `DisplayStandList`

| 인스펙터 | 타입 | 꽂을 것 |
| :--- | :--- | :--- |
| `gameState` | `GameState` | UIRoot |
| `shelfButtons` | `ShelfButton[]` | CakeButton_1 ~ 5 |

동작

- `Start()` 에서 각 버튼의 `onClick` 에 `gameState.Pick(btn.Type)` 를 연결합니다.
  인스펙터에서 손으로 5개를 꽂는 것보다 코드가 낫습니다 — 하나 빠뜨리면 조용히 동작하지 않습니다.
- 람다에서 `foreach` 변수를 캡처할 때는 지역 변수로 복사해 두는 습관이 좋습니다.
- **`OnJudgingChanged` 구독 → 5개 버튼 `interactable` 일괄 제어** (필수)

  ```csharp
  private void SetJudging(bool judging)
  {
      // 판정 연출이 끝날 때까지 진열대를 잠근다. 안 막으면 연타로 주문이 줄줄이 실패한다
      foreach (var b in shelfButtons) b.Button.interactable = !judging;
  }
  ```

  이 잠금이 요건 7 의 "비활성화" 피드백 항목을 담당합니다. Button 의 **Disabled Color 가
  기본값이면 눈에 잘 안 띕니다.** 인스펙터에서 명확한 색으로 바꾸고, 잠긴 순간을 캡처해 두세요.
  채점 증거가 됩니다.
- 일시정지에 들어갈 때도 같은 방식으로 잠글지 결정하세요. `Screen_Pause` 가 위를 덮으므로
  클릭은 어차피 막히지만, 키보드 Navigate 가 뒤쪽 진열대 버튼으로 새어 나갈 수 있습니다.

**Navigation 설정** (요건 5. 스크립트가 아니라 인스펙터 작업)

현재 `CakeButton_1` 의 Navigation 이 **Automatic** 입니다. 진열대는 가로 1열이므로
`Horizontal` 또는 `Explicit` 로 바꾸세요. Automatic 은 좌표로 다음 버튼을 찾기 때문에
버튼이 밀집되면 이동 순서가 어긋납니다. **5개 전부** 바꿔야 합니다.

게임패드·키보드는 **배선된 경로로만 이동할 수 있습니다.** 배선이 곧 접근성입니다.
`CakeButton_5` 의 Up 을 `PauseButton` 으로 연결해 상단 HUD 도달 경로를 확보하세요
(`Explicit` 로 지정해야 가능합니다). Cancel(Esc / 패드 B)로도 일시정지를 열 수 있으므로
기능이 막히지는 않지만, **눈에 보이는데 도달할 수 없는 버튼**은 그 자체로 UX 결함입니다.

판정 중 `interactable = false` 로 잠기면 그 버튼은 Navigation 대상에서도 빠집니다.
5개가 동시에 잠기므로 이동할 곳이 없어지는데, 0.8초 뒤 풀리고 선택은 유지되므로
문제되지 않습니다.

---

### 4-12. `HudController.cs` — 붙일 곳: `TopNav`

| 인스펙터 | 타입 | 꽂을 것 |
| :--- | :--- | :--- |
| `gameState` | `GameState` | UIRoot |
| `scoreValueText` | `TextMeshProUGUI` | ScoreValueText |
| `timerText` | `TextMeshProUGUI` | TimerText |
| `timerGauge` | `Slider` | Progress Slider_Yellow |
| `gaugeFillImage` | `Image` | Progress Slider_Yellow / Fill Area / Fill |
| `countValueText` | `TextMeshProUGUI` | CurrentSlotValueText |
| `dangerTimeThreshold` | `float` | 10 |
| `normalColor` / `dangerColor` | `Color` | 게이지 평상시 / 위험 색 |
| `pulseSpeed` | `float` | 2 — 점멸 속도 (초당 깜빡임 = `pulseSpeed / 2`) |

동작

- `OnScoreChanged` → `scoreValueText.text = score.ToString()`
- `OnCountChanged` → `countValueText.text = success + " / " + fail`
- `OnTimeChanged(remain, total)`

  ```
  timerGauge.value = remain / total;                       // 매 프레임 OK
  int sec = Mathf.CeilToInt(remain);
  if (sec != lastSec) { timerText.text = sec.ToString(); lastSec = sec; }   // 초가 바뀔 때만

  if (remain > dangerTimeThreshold)
  {
      gaugeFillImage.color = normalColor;                  // 다시하기 시 색 복구가 여기서 일어난다
      timerText.color = normalColor;
  }
  else
  {
      // 남은 시간 자체를 위상으로 쓴다. Time.time 을 쓰면 안 되는 이유는 아래 참고.
      float t = Mathf.PingPong(remain * pulseSpeed, 1f);
      gaugeFillImage.color = Color.Lerp(normalColor, dangerColor, t);
      timerText.color = dangerColor;                       // 숫자는 고정. 점멸시키지 않는다
  }
  ```

- `CeilToInt` 를 쓰는 이유: `FloorToInt` 면 시작하자마자 "119" 가 뜹니다.

**점멸을 `Time.time` 이 아니라 `remain` 으로 구동하는 이유** (중요)

`Time.time` 을 쓰면 **일시정지 중에도 게이지가 계속 깜빡입니다.** 게임은 멈췄는데 HUD만
살아 움직여서 고장으로 보입니다. `remain` 을 위상으로 쓰면 `Tick()` 이 멈추는 순간
`OnTimeChanged` 도 멈추므로 색이 그 자리에 얼어붙습니다. **일시정지 대응이 공짜로 따라옵니다.**

**숫자를 같이 점멸시키지 않는 이유**

마지막 10초에 하는 일은 숫자를 읽는 것입니다. 글자 색이 계속 변하면 대비가 흔들려
읽기 어려워집니다. 게이지는 점멸, 숫자는 Danger 색 고정 — 경고는 전달하면서 가독성은 지킵니다.

**속도 상한**

`pulseSpeed / 2` 가 초당 깜빡임 횟수입니다. 기본값 2 → 초당 1회.
**`pulseSpeed` 를 6보다 크게 두지 마세요.** 초당 3회를 넘는 점멸은 광과민성 발작 유발
기준(WCAG 2.3.1)에 걸립니다. 시간이 줄수록 빨라지는 가속 점멸은 넣지 않습니다 — 값이
없는데 이 기준만 위험해집니다.

`else` 분기에서 `normalColor` 를 **반드시 다시 대입**해야 합니다. 빠뜨리면 다시하기를 눌러도
게이지가 빨간 채로 남습니다.

---

### 4-13. `ToastController.cs` — 붙일 곳: `ToastMessage` (0-2 에서 추가)

| 인스펙터 | 타입 |
| :--- | :--- |
| `gameState` | `GameState` |
| `root` | `GameObject` |
| `messageText` | `TextMeshProUGUI` |
| `backgroundImage` | `Image` |
| `successColor` / `dangerColor` | `Color` |
| `successMessage` / `failMessage` | `string` ("주문 성공!" / "주문에 없는 품목이에요") |
| `showSeconds` | `float` (기본 1.2) |

`showSeconds` 는 `GameState.judgeDelaySeconds` **보다 짧거나 같게** 두세요.
토스트가 다음 손님 주문 위로 넘어와 떠 있으면, 방금 판정에 대한 알림인지 새 주문에 대한
알림인지 구분이 안 됩니다.

동작

- `gameState.OnJudged(bool success)` 구독 → 색·문구 설정 → `root.SetActive(true)` →
  `showSeconds` 후 `SetActive(false)`
- 소멸은 코루틴으로. 연달아 뜰 수 있으므로 **이전 코루틴을 반드시 중단**하고 새로 시작하세요.
  안 하면 두 번째 토스트가 첫 번째의 타이머 때문에 일찍 사라집니다.

  ```csharp
  if (routine != null) StopCoroutine(routine);
  routine = StartCoroutine(ShowRoutine());
  ```

- `Invoke` / `InvokeRepeating` 은 쓰지 마세요. 중단 관리가 코루틴보다 불편합니다.
- 토스트가 버튼 클릭을 먹지 않도록 Raycast Target 을 꺼 두세요.

---

### 4-14. `ResultView.cs` — 붙일 곳: `Screen_Result`

| 인스펙터 | 타입 | 꽂을 것 |
| :--- | :--- | :--- |
| `gameState` | `GameState` | UIRoot |
| `rankTable` | `RankTable` | UIRoot |
| `successText` | `TextMeshProUGUI` | SuccessText |
| `failText` | `TextMeshProUGUI` | FailText |
| `scoreText` | `TextMeshProUGUI` | ScoreText |
| `rankText` | `TextMeshProUGUI` | RankText |

동작

- `OnEnable()` 에서 `gameState` 값을 읽어 채웁니다. 이벤트 구독이 필요 없습니다 —
  결과 화면은 켜지는 순간 한 번만 그리면 되기 때문입니다.
- 랭크:

  ```
  var rule = rankTable.Evaluate(gameState.SuccessCount, gameState.FailCount);
  rankText.text  = rule.label;
  rankText.color = rule.color;      // 요건 7 색 변화 피드백
  ```

- 등급 판정 규칙을 여기에 두지 마세요. `if (score > 500) "A"` 같은 하드코딩은
  값을 바꿀 때마다 코드를 고쳐야 하고, View 가 판단을 하게 됩니다. 4-15 참고.

---

### 4-15. `RankTable.cs` — 붙일 곳: `UIRoot`

성공·실패 건수로 최종 등급(A~F)을 판정합니다. `DessertTable` 과 같은 성격의 조회 컴포넌트입니다.

**점수를 입력으로 받지 않습니다.** 현재 설계에서 `score` 는 `successCount × 100` 과
완전히 같은 값이라, 점수와 성공 건수를 둘 다 넣으면 같은 정보를 두 번 세는 것입니다.
실질 축은 **처리량(success)** 과 **정확도(fail)** 두 개입니다.

| 인스펙터 | 타입 | 기본값 |
| :--- | :--- | :--- |
| `successWeight` | `int` | 100 |
| `failPenalty` | `int` | 50 |
| `rules` | `RankRule[6]` | 아래 표 |

```
rankScore = successCount × successWeight − failCount × failPenalty
```

실패 가중치를 성공의 절반으로 둔 이유: **실패는 1탭이면 끝나 시간을 거의 쓰지 않습니다**
(판정 지연 0.8초뿐). 성공은 평균 3~4탭이 듭니다. 감점이 없으면 "일단 아무거나 눌러보기" 가
무비용이 됩니다.

`RankRule` 구조

| 필드 | 용도 |
| :--- | :--- |
| `label` | RankText 에 표시할 문자 |
| `minRankScore` | 이 값 **이상**이면 이 등급 |
| `color` | 등급별 RankText 색 (요건 7 색 변화 피드백) |

등급 테이블 (**잠정치. 반드시 튜닝할 것**)

| 등급 | `minRankScore` | 무실패 환산 | 색 제안 |
| :-- | --: | :-- | :--- |
| A | 2800 | 28건 | `#FFD24A` 금색 |
| B | 2200 | 22건 | `#6FD1FF` 하늘 |
| C | 1600 | 16건 | `#7ED37E` 초록 |
| D | 1000 | 10건 | `#FFB067` 주황 |
| E | 400 | 4건 | `#FF8A6B` 연한 빨강 |
| F | `int.MinValue` | — | `#9AA0A6` 회색 |

**튜닝 방법**: 120초에 몇 건을 처리할 수 있는지는 실제로 눌러봐야 압니다.
정직하게 한 판 플레이해서 나온 성공 건수를 `S` 라 하면,
A = `S×1.15`, B = `S×0.95`, C = `S×0.75`, D = `S×0.5`, E = `S×0.25` (각각 ×100).
**C 가 "평범하게 하면 나오는 등급"** 이 되도록 잡는 것이 기준입니다.

구현 주의

- 마지막 항목(F)의 `minRankScore` 를 `int.MinValue` 로 두어 **모든 입력이 반드시 하나에
  걸리게** 합니다. 빈 문자열이 표시될 여지를 없앱니다.
- `rules` 가 비어 있으면 `"-"` + 흰색을 돌려줍니다. 인스펙터를 안 채운 채 실행해도
  예외가 나지 않아야 합니다.
- **`color.a <= 0` 이면 흰색으로 보정하세요.** `Serializable struct` 의 `Color` 기본값은
  알파 0입니다. 배열에 항목을 추가하고 색을 안 만지면 글자가 투명해져 아무것도 안 보입니다.
  원인을 찾기 어려운 조용한 버그가 되는 자리입니다.
- `rankScore` 는 음수가 될 수 있습니다(0성공 20실패 = −1000). 정상이며 F 입니다.

**게임 중 `score` 에는 실패 감점을 넣지 마세요.** 이 산식은 결과 화면 계산 전용입니다.
HUD 점수가 실패마다 줄어들면 그 감소를 알리는 피드백을 새로 설계해야 하고,
CLAUDE.md 가 금지한 "보너스·배수 시스템" 쪽으로 넘어갑니다.

---

## 5. Input Actions 설계

### 5-1. 현재 상태 (에셋 확인 결과)

| 항목 | 값 |
| :--- | :--- |
| 에셋 | `Assets/InputSystem_Actions.inputactions` (Unity 기본 템플릿 그대로) |
| 액션 맵 | `Player`, `UI` |
| 컨트롤 스킴 | Keyboard&Mouse, Gamepad, Touch, Joystick, XR |
| EventSystem | `InputSystemUIInputModule` + 위 에셋 연결됨 (확인 완료) |
| C# 클래스 생성 | **꺼져 있음** (`generateWrapperCode: 0`) |

### 5-2. 방침 — 새로 만들지 말고 기존 UI 맵을 씁니다

이 게임은 **UI 입력밖에 없습니다.** 캐릭터를 움직이지 않으므로 새 액션 맵을 만들 이유가 없습니다.

| 맵 | 처리 |
| :--- | :--- |
| `Player` (Move / Look / Jump / Crouch / Sprint …) | **사용 안 함.** 지워도 되고 둬도 됩니다. 문서에 "미사용" 이라고만 적으면 충분합니다. |
| `UI` | 그대로 사용 |

컨트롤 스킴 중 **Keyboard&Mouse / Gamepad / Touch 3종이 검증 대상**입니다.
Joystick / XR 은 바인딩만 남겨 두고 검증하지 않습니다.
**지우다가 UI 맵을 깨뜨리는 위험이 더 큽니다.** 그냥 두세요.

### 5-2-1. 게임패드는 바인딩 작업이 필요 없습니다

에셋을 확인한 결과, `UI` 맵에 게임패드 바인딩이 **이미 전부 들어 있습니다.**

| 액션 | 게임패드 바인딩 | 확인 |
| :--- | :--- | :--- |
| `Navigate` | `<Gamepad>/leftStick`, `rightStick`, `dpad` 2D 컴포짓 | 있음 |
| `Submit` | `*/{Submit}` 의 groups 에 `Gamepad` 포함 → `buttonSouth` (A) | 있음 |
| `Cancel` | `*/{Cancel}` 의 groups 에 `Gamepad` 포함 → `buttonEast` (B) | 있음 |

따라서 게임패드 지원을 위해 **새로 만들 액션도, 추가할 바인딩도 없습니다.**
5-5 의 Cancel 구독 코드 한 곳이 Esc 와 B 버튼을 동시에 처리합니다.

실제로 해야 할 일은 **5-7 의 선택 복구**와 **Navigation 배선**(4-11) 두 가지뿐입니다.

### 5-3. UI 맵의 액션 — 무엇이 코드가 필요하고 무엇이 자동인가

| 액션 | 현재 바인딩 | 코드 필요? |
| :--- | :--- | :--- |
| `Point` | 마우스 위치, 터치 | 불필요 — 모듈이 처리 |
| `Click` | 마우스 왼쪽, 터치 탭 | **필요 — 선택 해제 (5-6)** |
| `Navigate` | 방향키 / WASD / 스틱 / D-pad | **필요 — 선택 복구 (5-7)**. 이동 자체는 모듈이 처리 |
| `Submit` | `*/{Submit}` (Enter / 패드 A) | **필요 — 선택 복구 (5-7)**. 확정 자체는 모듈이 처리 |
| `Cancel` | `*/{Cancel}` (Escape / 패드 B) | **필요 — 직접 구독 (5-5)** |
| RightClick / MiddleClick / ScrollWheel / TrackedDevice* | — | 미사용 |

**마우스와 터치가 같은 경로인 이유**: `Click` / `Point` 액션에 마우스와 Touchscreen 바인딩이
둘 다 들어 있고, 모듈이 양쪽을 동일한 PointerEventData 로 변환합니다.
그래서 요건의 "터치 입력" 은 추가 구현이 필요 없습니다.
**에디터의 Device Simulator 로 확인만 하면 됩니다.**

### 5-4. 손봐야 할 바인딩 2가지

**① Space 를 Submit 에 추가해야 합니다**

CLAUDE.md 에는 "Submit = Enter / Space" 라고 적혀 있지만, 현재 바인딩 `*/{Submit}` 은
**Enter 만** 잡습니다. Space 키에는 Submit usage 가 없습니다. Space 도 쓰려면 바인딩을 추가하세요.

```
UI > Submit > + Add Binding > Path: <Keyboard>/space
```

요건은 "Navigate / Submit / Cancel 중 2개 이상" 이므로 Enter 만으로도 통과합니다.
Space 를 안 쓸 거면 CLAUDE.md 쪽 문구를 고쳐 문서와 실제를 일치시키세요.

**② Tab 네비게이션은 기본 지원되지 않습니다**

`Navigate` 는 2D Vector 컴포짓이라 Tab(단일 버튼)을 자연스럽게 넣을 수 없습니다.
Input System UI 모듈에는 레거시 EventSystem 의 Tab 순회 기능이 없습니다.
**방향키로 요건을 충족하고, Tab 은 지원하지 않는다고 문서에 적으세요.**
억지로 넣으면 "Tab = 오른쪽 이동" 이라는 이상한 동작이 됩니다.

게임패드에는 해당 사항이 없습니다. 스틱·D-pad 가 방향 입력을 그대로 제공합니다.

### 5-5. Cancel(Esc) 을 코드에서 받는 방법

`InputSystemUIInputModule` 은 Cancel 입력을 **현재 선택된 GameObject** 에게만 보냅니다
(`ICancelHandler`). 우리가 원하는 건 "선택이 뭐든 상관없이 Esc 를 누르면 일시정지" 이므로
액션을 **직접 구독**해야 합니다.

`UiInputRouter.cs`

```csharp
[SerializeField] private InputActionReference cancelAction;  // UI/Cancel

private void OnEnable()  => cancelAction.action.performed += HandleCancel;
private void OnDisable() => cancelAction.action.performed -= HandleCancel;

private void HandleCancel(InputAction.CallbackContext ctx) { /* 4-6 의 분기표 */ }
```

주의점

- **`action.Enable()` 을 호출하지 마세요.** `InputSystemUIInputModule` 이 같은 에셋의 UI 맵을
  이미 활성화합니다. 중복 Enable 이 바로 문제를 일으키지는 않지만, 모듈이 Disable 할 때
  참조가 꼬여 원인 파악이 어려워집니다.
- `InputActionReference` 는 인스펙터 드롭다운에서 `InputSystem_Actions/UI/Cancel` 을 고릅니다.
  에셋을 통째로 꽂는 게 아닙니다.
- 구독 해제(`OnDisable`)를 빼먹으면 씬을 다시 로드할 때 죽은 핸들러가 호출됩니다.

### 5-6. 포인터 입력 시 선택 하이라이트 해제

마우스로 버튼을 누르면 그 버튼이 `selected` 상태로 남아 테두리가 계속 보입니다.
"방금 키보드로 고른 것" 처럼 보여서 혼란스럽습니다. (체크리스트에 기록할 항목)

```csharp
[SerializeField] private InputActionReference clickAction;  // UI/Click

private void HandleClick(InputAction.CallbackContext ctx)
{
    // 마우스·터치로 조작한 순간 키보드 선택 표시를 지운다
    var device = ctx.control.device;
    if (device is Mouse || device is Touchscreen)
        EventSystem.current.SetSelectedGameObject(null);
}
```

- `Click` 은 PassThrough 라 누를 때와 뗄 때 모두 콜백이 옵니다. `performed` 만 구독하면 됩니다.
- 눌린 순간 선택을 해제해도 버튼 클릭은 정상 동작합니다.
  포인터 클릭은 선택 상태와 무관한 별도 경로(`IPointerClickHandler`)로 전달되기 때문입니다.
- **주의**: 화면 전환 버튼을 마우스로 누르면 → 여기서 `null` 로 해제 → 곧바로 `ScreenManager` 가
  새 화면의 첫 버튼을 선택합니다. 순서가 반대가 되면 새 화면의 포커스가 지워집니다.
  전환 후에도 포커스가 잡히는지 반드시 눈으로 확인하세요.

### 5-7. 선택 복구 — 게임패드 지원에서 실제로 해야 하는 일

5-6 이 선택을 `null` 로 만드는 순간, **게임패드는 조작 수단을 완전히 잃습니다.**

```
마우스로 버튼 클릭 → 선택 = null
  → 게임패드 스틱을 기울임
  → Navigate 액션은 발생하지만 이동시킬 대상이 없다
  → 아무 일도 일어나지 않는다
```

`InputSystemUIInputModule` 은 `currentSelectedGameObject` 가 `null` 이면 Navigate 를
어디에도 적용하지 않습니다. 마우스는 선택이 없어도 직접 누를 수 있어 문제가 드러나지 않지만,
**게임패드에는 포인터가 없어서 여기서 완전히 막힙니다.**

```csharp
[SerializeField] private InputActionReference navigateAction;  // UI/Navigate
[SerializeField] private InputActionReference submitAction;    // UI/Submit

private void RestoreSelection(InputAction.CallbackContext ctx)
{
    // 포인터로 조작한 뒤 선택이 비어 있으면, 방향 입력을 이동이 아니라 포커스 복구에 쓴다
    if (EventSystem.current.currentSelectedGameObject != null) return;
    EventSystem.current.SetSelectedGameObject(screenManager.CurrentFirstSelected);
}
```

- `Navigate` 와 `Submit` 양쪽에 연결합니다. 스틱을 기울이든 A 를 누르든 되살아나야 합니다.
- 첫 입력은 **이동이 아니라 복구로 소비**됩니다. 콘솔 UI 의 일반적인 동작이며,
  "패드를 집어 들면 커서가 어딘가에 되살아난다"는 기대와 일치합니다.
- 키보드에도 그대로 적용되므로 키보드 조작의 안정성도 같이 올라갑니다.
- `Navigate` 는 PassThrough Vector2 라 스틱을 놓을 때도 콜백이 옵니다. `performed` 만
  구독하고, 필요하면 `ctx.ReadValue<Vector2>()` 가 0 에 가까울 때 무시하세요.
- `screenManager.CurrentFirstSelected` 가 파괴되었거나 비활성이면 선택이 먹지 않습니다.
  화면 전환 직후에는 `ScreenManager` 가 이미 선택을 지정하므로 실제로 겹칠 일은 드뭅니다.

### 5-8. 게임패드 테스트 방법

실기 패드가 없어도 확인할 수 있습니다.

| 방법 | 경로 |
| :--- | :--- |
| Input Debugger | `Window > Analysis > Input Debugger` — 연결된 장치와 실제 입력 값을 확인 |
| 가상 장치 | Input Debugger 의 `Options > Add Devices...` 로 Gamepad 추가 |
| Device Simulator | 터치 검증용 (게임패드는 지원하지 않음) |

**반드시 확인할 시나리오**: 마우스로 버튼을 클릭한 **직후** 스틱을 기울인다.
5-7 이 없으면 여기서 아무 반응이 없습니다.

---

## 6. 한 판의 실행 순서

```
[Start]
  ScreenManager.ShowTitle()
    → Screen_Title 만 활성, EventSystem 선택 = MenuButton_Start

[시작 버튼]
  ScreenManager.ShowPlay()
    → gameState.StartGame()
        score=0, time=60, success=0, fail=0, tray 비움
        customerNumber=1, currentOrder = orderGenerator.Generate()
        OnScoreChanged / OnCountChanged / OnTimeChanged / OnOrderChanged / OnTrayChanged 발행
    → 각 View 가 초기 상태를 그림
    → 선택 = CakeButton_1

[매 프레임, isRunning 일 때만]
  gameState.Tick(Time.deltaTime) → OnTimeChanged
    → HudController: 게이지 갱신, 초가 바뀌면 텍스트 갱신, 10초 이하면 Danger 색

[진열대 버튼 클릭]  ── 여기서 즉시 판정
  ShelfView → gameState.Pick(type)

  ├ isJudging 또는 !isRunning        → 무시
  │
  ├ 주문에 남아 있는 품목            → remaining[type]--, tray.Add
  │                                    → OnTrayChanged → TrayView 슬롯 갱신
  │                                    → remaining 이 전부 0 이면 ↓ 성공
  │
  ├ [성공] score += 100, successCount++
  └ [실패] failCount++
       ↓ 공통
     OnJudged(bool)         → ToastController 표시
     OnScoreChanged / OnCountChanged → HudController
     OnJudgingChanged(true) → ShelfView 가 버튼 5개 잠금
       ↓ judgeDelaySeconds (0.8초) 대기 — 이 동안 틀린 주문이 그대로 보인다
     쟁반 비움 + 다음 손님 → OnTrayChanged / OnOrderChanged
     OnJudgingChanged(false) → 버튼 잠금 해제

[시간 0]
  gameState → isRunning = false, OnGameOver
    → ScreenManager.ShowResult()
    → ResultView.OnEnable() 에서 최종 값 표시, 선택 = ReplayButton

[Esc]
  UiInputRouter → 현재 상태에 따라 Pause 열기/닫기, Popup 닫기
```

---

## 7. 구현 순서

한 번에 다 만들지 말고 **매 단계마다 Play 를 눌러 확인**하세요.

| 단계 | 작업 | 확인 방법 |
| :--- | :--- | :--- |
| 1 | `DessertType`, `DessertTable` | 인스펙터에 스프라이트 5개가 순서대로 꽂혔는가 |
| 2 | `ScreenManager` + 화면 버튼 연결 | 마우스로 Title ↔ Play ↔ Pause ↔ Result 왕복 |
| 3 | 화면별 첫 버튼 포커스 지정 | 전환 후 방향키가 먹는가 |
| 4 | `UiInputRouter` (Cancel + 선택 복구) | Esc·패드 B 로 Pause 열고 닫기 / 마우스 클릭 직후 스틱을 기울여 포커스가 되살아나는가 |
| 5 | `GameState` (시간만) + `HudController` | 120초가 줄고, 10초부터 게이지가 깜빡이는가. **일시정지 중에는 깜빡임이 멈추는가** |
| 6 | `OrderGenerator` + `OrderCardView` | 주문 아이콘이 그려지는가 |
| 7 | `ShelfButton` / `ShelfView` / `TrayView` | 주문에 있는 걸 누르면 쟁반에 담기는가 |
| 8 | `Pick()` 즉시 판정 + 점수/카운트 | 아래 4가지 경우를 전부 손으로 확인 |
| 9 | 판정 지연 + 입력 잠금 + `ToastController` | **버튼을 마구 연타해서** 주문이 줄줄이 실패하지 않는가 |
| 10 | `ResultView` + `RankTable` + 다시하기 | 등급 경계값 바로 위/아래에서 등급과 색이 갈리는가, 재시작 시 값이 완전히 초기화되는가 |
| 11 | Navigation 배선 (진열대 Horizontal/Explicit, CakeButton_5 Up → PauseButton) | 방향키·스틱으로 5개를 순서대로 이동하고 상단 Pause 까지 올라가는가 |
| 12 | 해상도 4종 + 입력 3계열 테스트 | 1080×1920 / 1080×2340 / 1080×2400 / 1536×2048, 마우스·키보드·게임패드 |

**8단계에서 확인할 4가지** — 즉시 판정은 경계가 좁아서 여기서 대부분의 버그가 나옵니다.

| 입력 | 기대 |
| :--- | :--- |
| 주문에 있는 품목을 순서 섞어서 전부 누름 | 성공 |
| 주문에 없는 품목을 누름 | 그 자리에서 실패 |
| 초코 2개 주문에서 초코를 **세 번** 누름 | 세 번째에 실패 |
| 마지막 한 개를 누름 | 그 순간 성공 (더 누를 것이 남지 않음) |

**9단계와 10단계도 직접 손으로 여러 번 눌러 보세요.** 연타로 주문이 연쇄 실패하는 버그,
다시하기 후 점수가 이어지는 버그가 가장 흔합니다.

---

## 8. 자주 나오는 실수

| 증상 | 원인 | 대응 |
| :--- | :--- | :--- |
| 화면 전환 후 방향키가 안 먹음 | 활성화 시 `SetSelectedGameObject` 미호출 | 4-5 의 `Activate()` 패턴 |
| 마우스로 누른 버튼에 테두리가 남음 | 선택 상태 유지 | 5-6 |
| 마우스 클릭 후 게임패드·키보드가 죽음 | 선택이 `null` 인데 복구 안 함 | 5-7 |
| 게임패드 스틱을 살짝만 기울여도 선택이 튐 | Navigate 는 PassThrough | `performed` 만 구독, 데드존 확인 |
| 진열대에서 위로 올라가지지 않음 | Navigation 배선 없음 | `CakeButton_5` Up → `PauseButton` |
| 화면을 껐다 켤 때마다 아이콘이 중복 생성 | `OnDisable` 에서 구독 해제 안 함 | `+=` 와 `-=` 를 항상 쌍으로 |
| Pause 중에도 시간이 줄어듦 | `isRunning` 검사 누락 | `Time.timeScale` 대신 bool |
| 진열대 방향키 순서가 뒤죽박죽 | Navigation = Automatic | Horizontal / Explicit |
| 연타 한 번에 주문 서너 개가 실패 | `isJudging` 잠금 없음 | 4-4 의 두 겹 잠금 |
| 왜 실패했는지 모르겠음 | 판정 즉시 다음 주문으로 넘어감 | `judgeDelaySeconds` 동안 화면 유지 |
| 다시하기 후 점수가 이어짐 | `StartGame()` 에서 초기화 누락 | 초기화 항목 7개를 체크리스트로 (`remaining`, `isJudging` 포함) |
| 눌렀는데 다른 디저트가 담김 | `ShelfButton.type` 과 아이콘 불일치 | 5개를 눈으로 대조 |
| 긴 한글이 넘침 | TMP Overflow 설정 | Auto Size 또는 Overflow 조정 |
| 토스트가 버튼 클릭을 먹음 | Raycast Target 켜짐 | 꺼 두기 |

실제로 겪은 것은 `docs/checklist.md` 에 **기대 / 실제 / 원인 / 수정 / 재확인** 형식으로 그때그때
기록하세요. 요건 8번은 사후에 지어내면 티가 납니다.

---

## 9. CLAUDE.md 갱신 필요

즉시 판정으로 바꾸면서 CLAUDE.md 와 어긋난 곳이 3군데 있습니다. 지금 고쳐 두세요.
설계 문서와 실제 구현이 다르면 그 자체가 감점 요인입니다.

| 위치 | 현재 내용 | 고칠 내용 |
| :--- | :--- | :--- |
| 게임 흐름도 | "진열대에서 디저트를 눌러 쟁반에 담기 → 제출" | "진열대에서 디저트를 누름 → 그 즉시 판정" |
| 스크립트 표 `TrayView` | "쟁반 슬롯 표시, 담기·취소 처리" | "쟁반 슬롯 표시" (표시 전용) |
| 피드백 표 첫 줄 | "쟁반이 비어 있음 → 제출 버튼 `interactable = false`" | "판정 연출 중 → 진열대 버튼 `interactable = false`" |

"절대 하지 말 것" 목록과는 충돌하지 않습니다. 즉시 판정은 기능을 **줄이는** 변경이라
난이도 곡선·콤보·드래그앤드롭 어느 것도 새로 들어오지 않습니다.

---

## 부록 A. 화면 제어 — 스택 FSM (채택)

**결정: 상태 패턴을 쓰되 평면 FSM이 아니라 스택(pushdown)으로 간다.**

### A-1. 평면 FSM이 깨지는 지점

상태 4개(Title/Play/Pause/Result)를 놓고 "Enter 에서 내 패널 켜고, Exit 에서 끈다" 로 만들면:

```
Play ──[Pause]──▶ PlayState.Exit()  → Screen_Play 꺼짐 (일시정지인데 뒤가 빔)
Pause ─[계속하기]▶ PlayState.Enter() → StartGame() 재호출 → 게임이 처음부터 다시 시작
```

두 번째가 치명적입니다. 피하려면 `Enter()` 와 `Resume()` 을 나눠야 하는데,
**그 순간 이미 스택입니다.** 어설프게 피하면 "Pause 일 땐 Play 를 끄지 마라",
"Play 로 돌아올 땐 Enter 말고…" 같은 특례가 쌓이고 FSM 을 쓴 이점이 사라집니다.

### A-2. 스택이 맞는 이유

`Play → Pause → Confirm` 은 전환이 아니라 **겹침**입니다. 씬 하이라키가 이미 그렇습니다 —
`ConfirmPopup` 은 `PausePopup` 의 자식입니다. 그리고 Cancel 규칙 "가장 위 레이어 하나만
닫는다"(4-6)가 **Pop 그 자체**입니다.

| 전환 | 연산 |
| :--- | :--- |
| Title → Play, Play → Result, Result → Play, Confirm[예] → Title | `Set` (스택을 비우고 새로 쌓기) |
| Play → Pause, Pause → Confirm | `Push` (아래층 그대로) |
| Confirm → Pause, Pause → Play | `Pop` |
| Cancel | `Top.OnCancel()` — Title·Result 는 무시, 나머지는 Pop |

`Push` 는 아래층의 `Exit()` 를 부르지 않으므로 재시작 버그가 **구조적으로** 생기지 않습니다.

### A-3. 인터페이스

```csharp
public interface IState
{
    GameObject FirstSelected { get; }   // Pop 후 포커스 복구에 필요
    void Enter();
    void Exit();
    void OnCancel();
}
```

- `Tick()` 은 넣지 마세요. 시간 갱신은 `GameState` 의 책임입니다. 넣는 순간 데이터가
  상태 클래스로 새어 나가고, 이 프로젝트가 지키기로 한 분리 원칙이 무너집니다.
- 패널 활성화와 포커스 지정은 **`ScreenManager` 가 전담**합니다. 상태가 자기 패널을 직접
  켜고 끄면 Push 로 겹칠 때 아래층이 사라집니다.

### A-4. 상태는 MonoBehaviour 로, 각 패널에 붙인다

| 컴포넌트 | 붙일 곳 | `firstSelected` |
| :--- | :--- | :--- |
| `TitleState` | `Screen_Title` | `MenuButton_Start` |
| `PlayState` | `Screen_Play` | `CakeButton_1` |
| `PauseState` | `Screen_Pause` | `ResumeButton` |
| `ConfirmState` | `ConfirmPopup` | `NoButton` |
| `ResultState` | `Screen_Result` | `ReplayButton` |

패널에 붙여 두면 첫 버튼 참조가 인스펙터로 해결되어 배선 코드가 사라집니다.

> **`[SerializeField]` 에 인터페이스 타입을 쓰지 마세요.** Unity 직렬화기는 인터페이스를
> 직렬화하지 못해 인스펙터에 칸 자체가 나오지 않습니다. `ScreenManager` 의 참조 필드는
> `TitleState`, `PlayState` … 같은 **구체 타입**으로 선언하고, 공통 동작은
> `UiStateBase` 추상 클래스로 묶습니다.

---

## 부록 B. 인스펙터 연결 최종 점검표

구현이 끝나면 위에서 아래로 확인하세요. 빈칸 하나가 `NullReferenceException` 입니다.

| 오브젝트 | 붙는 스크립트 | 꽂아야 할 참조 |
| :--- | :--- | :--- |
| UIRoot | DessertTable | 스프라이트 5 |
| UIRoot | RankTable | 등급 규칙 6줄 (+ 숫자 2) |
| UIRoot | OrderGenerator | 1 (+ 숫자 5) |
| UIRoot | GameState | 1 (+ 숫자 4) |
| UIRoot | ScreenManager | 5 (상태 컴포넌트) |
| UIRoot | UiInputRouter | 5 |
| TopNav | HudController | 6 (+ 색 2, 숫자 2) |
| OrderPanel | OrderCardView | 5 |
| ChoiceListPanel | TrayView | 4 |
| DisplayStandList | ShelfView | 1 + 버튼 5 |
| CakeButton_1~5 | ShelfButton | 각 1 (enum) |
| ToastMessage | ToastController | 4 (+ 색 2, 문구 2) |
| Screen_Result | ResultView | 6 |
| Screen_Title / Play / Pause / Result, ConfirmPopup | 각 State | 각 1 (firstSelected) |
| OrderPrefab / ChoicePrefab | DessertIconView | 각 2~3 |

인스펙터 연결 대신 `GameObject.Find` 를 쓰고 싶어지는 순간이 옵니다. 쓰지 마세요.
이름을 바꾸는 즉시 조용히 깨지고, 어디서 깨졌는지 알려주지 않습니다.
