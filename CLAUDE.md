# 디저트 가게 주문 받기 — Unity UI/UX 포트폴리오

## 이 프로젝트가 무엇인가

학교 과제(NCS `0803020529_24v5` 게임 UI/UX 프로그래밍)로 제출하는 Unity 6 UI 프로토타입입니다.
**평가 대상은 게임의 재미나 규모가 아니라 UI/UX 설계·구현·검증 과정입니다.**

제한된 영업시간(기본 120초) 안에 손님의 주문을 정확히 처리해 점수를 얻는 캐주얼 세로형 게임입니다.

```
손님 1명 등장 → 주문 카드 표시(5종 중 중복 허용 3개) → 진열대에서 디저트를 누름 → 누른 즉시 판정
  ├ 주문에 있는 품목 → 쟁반에 담김. 주문 3개를 다 채우면 +100점, 처리 +1
  └ 주문에 없는 품목 → 그 자리에서 실패, −20점(0 하한), 실패 +1
→ 영업시간 120초(인스펙터 조정 가능) 종료 → 결과 화면
```

점수는 성공 +100 / 실패 −20이며 **0 아래로 내려가지 않습니다.**
결과 화면의 등급은 점수가 아니라 성공·실패 **건수**로 따로 계산합니다.

마감은 2026-08-26입니다. **완성도보다 요건 충족이 우선입니다.**

---

## 절대 하지 말 것

이 프로젝트에서 가장 큰 위험은 버그가 아니라 **범위 확장**입니다. 아래는 요청받지 않는 한 절대 구현하지 마세요. "있으면 좋을 것 같아서" 추가하는 것도 금지입니다.

- 난이도 상승 곡선, 시간에 따른 스폰 가속
- 콤보, 보너스 점수, 배수 시스템
- 점수 감소를 알리는 별도 연출 (점수 텍스트 점멸·흔들림 등. 실패 토스트가 이미 그 역할을 합니다)
- 시간이 줄수록 빨라지는 가속 점멸 (초당 3회 초과는 광과민성 발작 기준에 걸립니다)
- 손님 캐릭터 스프라이트, 표정, 등장 애니메이션
- 최고 점수 저장, `PlayerPrefs`, 세이브/로드
- 효과음, BGM, `AudioSource`
- 튜토리얼 화면, 도움말
- 동시 손님(대기줄), 손님 선택 상태 관리
- 드래그 앤 드롭
- `DOTween` 등 외부 라이브러리
  (아이콘 풀링은 예외. `UnityEngine.Pool.ObjectPool` 로 이미 구현했다 — `DessertIconPool`)
- 씬 분리 (`SceneManager.LoadScene`)
- Safe Area(노치) 대응

추가 기능이 필요해 보이면 **구현하지 말고 먼저 물어보세요.**

---

## 기술 스택 / 제약

| 항목 | 값 |
| :--- | :--- |
| Unity | 6 |
| UI | UGUI (`UnityEngine.UI`) |
| 텍스트 | **TextMeshPro 필수** (`TextMeshProUGUI`). 레거시 `Text` 사용 금지 |
| 입력 | Input System (`InputSystem_Actions` 에셋 이미 존재) |
| 씬 | **단일 씬** (`PortfolioAssignment`). 화면 전환은 패널 활성/비활성 |
| 해상도 | 1080 × 1920 세로 |
| Canvas Scaler | Scale With Screen Size, Match **0** (Width 기준) |

과제 요건상 UGUI와 TextMeshPro 사용이 명시되어 있습니다. UI Toolkit은 사용하지 않습니다.

---

## 폴더 구조

```
Assets/
├── MyAssets/
│   └── PORTFOLIO_Assets/
│       ├── Scene/
│       ├── Scripts/          ← 여기에 코드 작성
│       │   ├── Core/         데이터·규칙 (Core/Rules 는 UnityEngine 도 안 씀)
│       │   ├── Views/        패널 바인딩
│       │   └── Screens/      화면 흐름·상태·입력 라우팅
│       ├── Sprites/CakeIcon/  ← 디저트 아이콘 5종
│       ├── Font/             ← KERISKEDU_R SDF (한글)
│       └── Prefabs/          ← UI 프리팹
├── Skyden_Games/             ← Free Casual GUI (버튼/패널/게이지 배경)
├── TextMesh Pro/
└── InputSystem_Actions/
```

`Skyden_Games`와 에셋 스토어 폴더는 **수정하지 마세요.** 읽기만 합니다.

참조는 `Screens → Views → Core` 한 방향입니다. 역방향을 만들지 마세요. 특히
`Views` 안에서 `ScreenFlowController`·`UIInputRouter` 같은 `Screens` 타입을 참조하면
순환이 됩니다. 선택 테두리를 그리는 `FocusRing` 이 `Views` 가 아니라 `Screens` 에 있는
이유가 이것입니다 — 게임 데이터는 안 읽고 `UIInputRouter` 를 읽습니다.

---

## 디저트 품목

스프라이트 5종이 `Sprites/CakeIcon/`에 있습니다. 형태가 서로 다르도록 선별한 것이므로 늘리거나 줄이지 마세요.

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

스프라이트 파일명: `Choco_Cake`, `Kiwi_BigCake`, `Lemon_RectangularCake`, `Rainbow_CupCake`, `Skull_CakePiece`

---

## 아키텍처

**데이터와 표시를 분리합니다.** 이건 과제 요건("HUD 표시 값이 실제 게임 상태와 연결되어 있다")과 직결되므로 반드시 지켜주세요.

```
GamePlayController        ← 실제 데이터 소유 (점수, 시간, 주문, 쟁반)
   ↓ 읽기
UI Controller    ← GamePlayController를 읽어 View에 반영
   ↓
UGUI View        ← TextMeshProUGUI, Image, Button
```

UI 스크립트가 점수 같은 게임 데이터를 직접 소유하면 안 됩니다. `HudController`는 `GamePlayController.score`를 읽어 표시할 뿐이고, 점수를 바꾸는 주체는 `GamePlayController`입니다.

### 씬 오브젝트 배치

한 오브젝트에 컴포넌트를 몰아 붙이지 마세요.

| 오브젝트 | 붙는 것 |
| :--- | :--- |
| `GamePlay` | `GamePlayController`, `OrderGenerator`, `DessertTable`, `RankTable` |
| `UIRoot` | `ScreenFlowController`, `UIInputRouter` |
| 각 패널 | View 스크립트 (`HudController`, `OrderCardView`, `TrayView`, `ToastController`, `ShelfView`, `CountdownView`) |

`GamePlay`를 `Screen_Play`의 자식으로 두지 마세요. 결과 화면에서 `Screen_Play`가 꺼진 뒤에도 `ResultView`가 최종 점수를 읽어야 합니다. 데이터 소유자의 수명이 화면 활성 상태에 묶이면 원인 찾기 어려운 버그가 생깁니다.

### 스크립트 구성

| 스크립트 | 역할 |
| :--- | :--- |
| `GamePlayController` | score, remainingTime, currentOrder, remaining, tray, successCount, failCount, isJudging 보유 |
| `OrderGenerator` | 무작위 주문 생성 (5종 중 중복 허용으로 3개) |
| `ScreenFlowController` | 화면 패널 전환, 전환 시 초기 선택 버튼 지정 |
| `HudController` | GamePlayController 값을 HUD에 반영 |
| `OrderCardView` | 현재 주문을 DessertIcon으로 표시 |
| `TrayView` | 쟁반 슬롯 표시 (표시 전용. 클릭을 받지 않음) |
| `ShelfView` | 진열대 버튼 입력 수신, 판정 중 버튼 잠금 |
| `ToastController` | 판정 알림 문구 표시·소멸 (ChoiceListPanel 에 붙는다) |
| `RankTable` | 성공·실패 건수 → 등급 A~F + 등급 색 판정 |
| `ResultView` | 최종 성공·실패·점수·등급 표시 (켜질 때 한 번, 이벤트 구독 없음) |
| `CountdownView` | Screen_Play 중앙 오버레이 — 시작 카운트다운, `Timeout!!` |

### 화면

```
Screen_Title   — 시작, 종료
Screen_Play    — HUD + 주문카드 + 쟁반 + 진열대
Screen_Pause   — 계속하기, 타이틀로(확인 팝업 경유)
Screen_Result  — 최종 점수, 처리/실패, 다시하기, 타이틀로
```

전환 흐름:
```
Title → Play ⇄ Pause
         ↓ (시간 종료 → "Timeout!!" 1초 → 페이드)
       Result → Play(재시작) / Title
Pause → ConfirmPopup → Title
```

---

## 프리팹

| 프리팹 | 재사용 지점 | 상태 |
| :--- | :--- | :--- |
| `OrderPrefab` | 주문 카드 슬롯 | 제작 완료 |
| `ChoicePrefab` | 쟁반 슬롯 | 제작 완료 |
| `Score` | 상단 HUD 점수 | 제작 완료 |
| `Progress Slider_Yellow` | 영업시간 게이지 | 제작 완료 |

`OrderPrefab`과 `ChoicePrefab`은 구조(`CakeIcon` + `CountText`)가 같으므로 **`DessertIconView` 컴포넌트를 공유**합니다. 스프라이트 교체·개수 표시 로직이 한 곳에만 존재하는 것이 요건 6번의 증거입니다. 두 프리팹을 하나로 합치지는 마세요 — 배치가 이미 끝났고 되돌리는 비용이 이득보다 큽니다.

판정 알림(`ToastMessage`)은 **프리팹으로 만들지 않습니다.** 판정 잠금 0.8초가 표시 시간 0.5초보다 길어 토스트가 동시에 둘 존재할 수 없으므로, `ChoiceListPanel` 아래 텍스트 하나를 켜고 끕니다. 생성·파괴가 없으니 오브젝트 풀 이야기도 나오지 않습니다. 재사용 프리팹 요건은 위 4개로 이미 충족입니다.

진열대 버튼(`CakeButton_1~5`)도 프리팹으로 만들지 않습니다. 5개가 스프라이트·`ShelfButton.type`·Explicit Navigation 이 전부 달라 공유할 것이 거의 없고, 프리팹 애셋은 씬 참조(Navigation 대상)를 담을 수 없어 실수로 Apply 하면 배선이 한꺼번에 날아갑니다. 5개를 동시에 고칠 일은 하이라키 다중 선택으로 처리하세요.

---

## 판정 로직

주문을 "남은 개수" 딕셔너리로 들고 있으면 판정이 이게 전부입니다. 복잡하게 만들지 마세요.

```
Dictionary<DessertType,int> remaining   // 주문 생성 시 채움

Pick(type):
  isJudging 이면            → 무시
  remaining[type] == 0 이면 → 실패 확정
  remaining[type]--, tray에 추가
  remaining 이 전부 0 이면  → 성공 확정
```

순서는 무관합니다. 개수 초과(초코 2개 주문에서 세 번째 초코)도 자동으로 걸립니다.

판정이 확정되면 **바로 다음 손님으로 넘기지 마세요.** 0.8초 지연 + 진열대 버튼 잠금이
필요합니다. 안 하면 왜 틀렸는지 볼 시간이 없고, 연타로 주문이 줄줄이 실패합니다.

---

## 입력

| 입력 | 처리 |
| :--- | :--- |
| 마우스 클릭 | UGUI Button `onClick` |
| 터치 | **동일 경로. 추가 구현 불필요** |
| 키보드 Navigate | 방향키 / WASD (Tab은 Input System UI 모듈이 지원하지 않음) |
| 키보드 Submit | Enter (`*/{Submit}`. Space는 기본 바인딩에 없음) |
| 키보드 숫자 1~5 | 진열대 버튼을 곧바로 누름 (`UI/ShelfSlot`). `ShelfView` 가 해당 버튼에 Submit 이벤트를 보내므로 판정 잠금·눌림 피드백이 클릭과 동일하게 적용됨 |
| 키보드 Cancel | Esc |
| 게임패드 | Navigate 좌스틱·우스틱·D-pad / Submit A / Cancel B — **기존 UI 맵 바인딩 그대로, 추가 구현 불필요** |

### 반드시 지킬 것

- **화면을 활성화할 때마다** `EventSystem.current.SetSelectedGameObject(firstButton)`을 호출하세요. 안 하면 화면 전환 후 키보드 포커스가 사라집니다. 과제 요건 5번 직결입니다.
- 진열대 버튼은 가로 1열이므로 Navigation을 `Horizontal` 또는 `Explicit`로 지정하세요. `Automatic`은 밀집 배치에서 이동 순서가 어긋납니다.
- 포인터 입력이 발생하면 `SetSelectedGameObject(null)`로 선택 하이라이트를 해제하세요. 안 하면 마우스로 누른 뒤 엉뚱한 버튼에 테두리가 남습니다.
- **위 규칙과 짝이 되는 선택 복구가 반드시 필요합니다.** `Navigate` 또는 `Submit`이 들어왔는데 선택이 `null`이면 현재 화면의 첫 버튼을 다시 선택하세요. 게임패드에는 포인터가 없어서, 이게 없으면 마우스로 한 번 클릭한 뒤 패드가 완전히 먹통이 됩니다.
- 진열대 `CakeButton_5`의 Up을 `PauseButton`으로 배선하세요. 선택 계열 입력은 배선된 경로로만 이동할 수 있어, 배선이 없으면 보이는데 도달할 수 없는 버튼이 됩니다.
- `Cancel` 동작은 상황별로 다릅니다: Play → 일시정지 열기 / Pause → 닫기 / Popup → 팝업만 닫기. 이 분기 하나가 Esc와 패드 B를 동시에 처리합니다.

---

## 피드백 (요건 7번)

아래는 요건이므로 반드시 구현합니다.

| 상황 | 피드백 |
| :--- | :--- |
| 판정 연출 중 | 진열대 버튼 `interactable = false` + Disabled 색 |
| 주문 성공 | Success 색 토스트 |
| 주문 불일치 | Danger 색 토스트 |
| 남은 시간 10초 이하 | 게이지가 평상시 색 ↔ Danger 색으로 **점멸** (위상은 `Time.time`이 아니라 `remainingTime`) |
| 최종 등급 표시 | 등급 A~F별로 RankText 색이 달라짐 |
| 버튼 hover/선택/눌림 | Normal / Highlighted / Pressed |
| 쟁반 담김 / 빈칸 | 실선 슬롯 / 점선 슬롯 |

---

## 코드 스타일

- C# 표준 명명 규칙: 클래스·메서드 `PascalCase`, 필드 `camelCase`, 상수 `PascalCase`
- 인스펙터 노출은 `public` 대신 `[SerializeField] private`
- 매직 넘버는 `[SerializeField]` 상수로 빼서 인스펙터에서 조정 가능하게
- `Update()`에서 매 프레임 문자열을 만들지 마세요. 값이 바뀔 때만 텍스트를 갱신합니다.
- `GameObject.Find`, `Camera.main` 반복 호출 금지. 참조는 `[SerializeField]`로 연결
- 주석은 한국어로, **왜** 그렇게 했는지만. 코드가 설명하는 내용은 주석 불필요
- 한 스크립트가 200줄을 넘으면 책임이 섞인 것이므로 분리를 제안하세요

---

## 작업 순서

**완료**: 에셋 임포트, 스프라이트 5종 선별·설정, TMP 한글 폰트(`KERISKEDU_R SDF`) 생성,
9-Slice 설정, UI 배치, 프리팹 4종 제작, 설계 문서 작성
(`docs/uiux-design.md`, `docs/script-design.md`)

| 순서 | 작업 |
| :--- | :--- |
| ~~1~~ | ~~`ScreenFlowController` + 4개 화면 전환 골격, 화면별 첫 포커스 지정~~ 완료 |
| ~~2~~ | ~~`UIInputRouter` — Cancel(Esc·패드B), 포인터 시 선택 해제, **선택 복구**~~ 완료 (인스펙터 배선 남음) |
| ~~3~~ | ~~`GamePlayController` 시간 + `HudController` 게이지·타이머 바인딩~~ 완료 (+ 시작 카운트다운) |
| ~~4~~ | ~~`OrderGenerator` + `OrderCardView`~~ 완료 (인스펙터 배선 남음) |
| ~~5~~ | ~~`ShelfView` / `ShelfButton` / `TrayView` — 즉시 판정~~ 완료 (인스펙터 배선 남음) + HUD 점수·처리수 바인딩 |
| ~~6~~ | ~~판정 지연·진열대 잠금 + `ToastController`~~ 완료 (인스펙터 배선 남음. ToastMessage 는 프리팹 아님 — 텍스트 1개) |
| ~~7~~ | ~~`ResultView` + `RankTable`(등급 A~F·등급 색) + 다시하기 초기화~~ 완료 (인스펙터 배선 남음. 등급 경계값 튜닝 필요) |
| 8 | Navigation 배선(진열대 `Horizontal`, CakeButton_5 Up → PauseButton), PauseButton 144px, 팝업 설명문 48px |
| 9 | 해상도 4종 + 입력 3계열(마우스·키보드·게임패드) 테스트 |
| 10 | 체크리스트 작성, 캡처 |

---

## 테스트 중 발견한 문제는 기록해야 합니다

과제 요건 8번이 **"기대 결과 / 실제 결과 / 수정 내용 / 다시 확인한 결과"** 기록을 요구합니다. 사후에 지어내면 티가 납니다.

작업 중 문제를 발견하고 고쳤다면, 그 내용을 `docs/checklist.md`에 아래 형식으로 추가해 주세요.

```markdown
### [UI 요소명]
- 기대 결과:
- 실제 결과:
- 원인:
- 수정 내용:
- 다시 확인한 결과:
```

예상되는 항목: 9-Slice 미설정으로 인한 버튼 모서리 깨짐, 화면 전환 후 키보드 포커스 소실, 마우스 클릭 후 선택 하이라이트 잔존, 마우스 클릭 직후 게임패드 입력 무반응, 진열대에서 상단 HUD로 이동 불가, 태블릿 비율(3:4)에서 레이아웃 붕괴, 긴 한글 라벨 넘침

---

## 요건 체크

구현 중 아래를 기준으로 판단하세요. 이 목록에 없으면 만들지 않습니다.

- [ ] UGUI + TextMeshPro 사용
- [ ] Canvas Scaler 기준 해상도 1080×1920, Match 0
- [ ] HUD 3개 이상이 실제 게임 상태와 연결 (시간·점수·주문 안내·처리/실패)
- [ ] 화면 3개 이상 + 버튼으로 전환 가능
- [ ] 마우스로 UI 조작 가능
- [ ] 키보드·게임패드로 Navigate / Submit / Cancel 확인 가능
- [ ] 재사용 UI Prefab 2개 이상
- [ ] 피드백 2개 이상 (버튼 상태·알림·비활성화·색 변화 2종)
- [ ] 해상도 변경 테스트 수행
- [ ] 체크리스트에 기대/실제/수정/재확인 기록
