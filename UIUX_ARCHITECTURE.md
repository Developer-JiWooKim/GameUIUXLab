# UI/UX 설계 문서 

## 게임 이름: 주문받아라!!

---

## 화면 목록

### Screen_Title

| 요소 | 오브젝트 | 역할 |
| :--- | :--- | :--- |
| 타이틀 라벨 | `TitleLabel` (TitleLabelText "주문 받아라!!", Logo, Image ×2) | 게임 정체성 |
| 시작 버튼 | `MenuButton_Start` | → Screen_Play |
| 종료 버튼 | `MenuButton_Quit`| 애플리케이션 종료 |

- 첫 키보드/게임 패드 포커스: **`MenuButton_Start`**

### Screen_Play

| 영역 | 오브젝트 | 구성 |
| :--- | :--- | :--- |
| 상단 HUD | `TopNav` 패널 | `Score`(점수), `Timer`(게이지+숫자), `CurrentSlot`(성공/실패), `PauseButton` |
| 중단 | `MidNav` 패널| `OrderPanel`(손님+주문 카드), `ChoiceListPanel`(쟁반) |
| 하단 | `BotNav` 패널 | `DisplayStandList`(진열대 텍스트 + 케이크 버튼 5개) |

- 첫 키보드 포커스: **`CakeButton_1`**
- `OrderGridLayout` / `ChoiceGridLayout` 에 Horizontal Layout Group 적용, 자식은 Prefab 인스턴스
- 판정 알림 `ToastMessage` 는 `ChoiceListPanel` 아래 텍스트 1개를 켜고 끈다 (프리팹 아님 X)

### Screen_Pause

| 요소 | 오브젝트 |
| :--- | :--- |
| 제목 | `PauseText` "일시정지" |
| 계속하기 | `ResumeButton` → Screen_Play |
| 타이틀로 | `GoTitleButton` → ConfirmPopup 열기 |
| 확인 팝업 | `ConfirmPopup` (기본 비활성) — ConfirmText, ConfirmDescriptionText, YesButton, NoButton |

- 첫 키보드 포커스: **`ResumeButton`**
- 팝업의 첫 포커스: **`NoButton`**

### 3.4 Screen_Result

| 요소 | 오브젝트 | 연결 데이터 |
| :--- | :--- | :--- |
| 제목 | `ResultText` "결과" | — |
| 성공 | `SuccessText` | `GamePlayController.successCount` |
| 실패 | `FailText` | `GamePlayController.failCount` |
| 점수 | `ScoreText` | `GamePlayController.score` |
| 랭크 | `RankText` | `RankTable.Evaluate(successCount, failCount)` → A~F + 등급 색 |
| 다시하기 | `ReplayButton` | → Screen_Play (상태 초기화) |
| 타이틀로 | `GoTitleButton` | → Screen_Title |

- 첫 키보드 포커스: **`ReplayButton`**

---

## 화면 전환 흐름

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

### Cancel(Esc) 의 상황별 동작

Esc 는 화면마다 다르게 동작.

| 현재 상태 | Esc |
| :--- | :--- |
| Play | Pause 열기 |
| Pause, 팝업 닫힘 | Pause 닫기 |
| Pause, 팝업 열림 | **팝업만** 닫기 |

---

## 기준 해상도와 Canvas Scaler

| 항목 | 값 |
| :--- | :--- |
| 기준 해상도 | **1080 × 1920** (세로) |
| UI Scale Mode | Scale With Screen Size |
| Screen Match Mode | Match Width Or Height | 
| Match | 0 (Width 기준) |

---

## HUD와 데이터 연결

| 표시 요소 | 오브젝트 | 연결 데이터 | 갱신 시점 |
| :--- | :--- | :--- | :--- |
| 시간 게이지 | `Progress Slider_Yellow` | `GamePlayController.remainingTime / playTimeSeconds` | 매 프레임 |
| 시간 숫자 | `TimerText` | `remainingTime` (정수 변환) | **초 단위가 바뀔 때만** |
| 점수 | `ScoreValueText` | `GamePlayController.score` | 판정 시 |
| 성공 / 실패 | `CurrentSlotValueText` | `successCount`, `failCount` | 판정 시 |
| 주문 카드 | `OrderGridLayout` | `GamePlayController.currentOrder` | 손님 교체 시 |
| 손님 번호 | `CustomerNameText` | `GamePlayController.customerNumber` | 손님 교체 시 |
| 쟁반 슬롯 | `ChoiceGridLayout` | `GamePlayController.tray` | 담길 때 |

---

## 입력 인터페이스

- **포인터 계열(마우스·터치)과 선택 계열(키보드·게임패드)** 두 갈래를 모두 지원.

### 입력 방식

```text
디저트 담기   : 마우스 좌클릭 / 터치 / Submit(선택 상태에서)
메뉴 이동     : 방향키·WASD / 게임패드 좌스틱·우스틱·D-pad   (Navigate)
확인          : Enter / 게임패드 A(buttonSouth)              (Submit)
일시정지·취소 : Esc / 게임패드 B(buttonEast)                 (Cancel)
```

| 입력 | 구현 방식 | 비고 |
| :--- | :--- | :--- |
| 마우스 | UGUI Button `onClick` | 기본 경로 |
| 터치 | UGUI Button `onClick` | **마우스와 동일 경로** |
| 키보드 Navigate / Submit | `InputSystemUIInputModule` | - |
| **게임패드 Navigate / Submit** | `InputSystemUIInputModule` | **기존 바인딩 그대로** |
| 키보드·게임패드 Cancel | 액션 직접 구독 | 선택된 오브젝트와 무관하게 받아야 하므로 |


### Input Actions 매핑

기존 `Assets/InputSystem_Actions.inputactions` 의 **`UI` 맵을 그대로 사용**.

| 액션 | 바인딩 | 게임패드 | 처리 |
| :--- | :--- | :--- | :--- |
| `Point` / `Click` | `<Mouse>/position`, 마우스 좌클릭, 터치 | 없음 (포인터 없음) | 모듈이 처리 → Button `onClick` |
| `Navigate` | 방향키 / WASD 2D 컴포짓 | `leftStick`, `rightStick`, `dpad` | 모듈이 처리 |
| `Submit` | `*/{Submit}` | `buttonSouth` (A) | 모듈이 처리 |
| `Cancel` | `*/{Cancel}` | `buttonEast` (B) | **`UIInputRouter` 가 직접 구독** |


### 선택 계열 입력을 위한 Navigation 배선


| 화면 | 배선 |
| :--- | :--- |
| Title | `MenuButton_Start` ↕ `MenuButton_Quit` (Vertical) |
| Play | `CakeButton_1~5` 가로 1열 (Horizontal 또는 Explicit). 5번 버튼의 Up → `PauseButton` 으로 연결해 상단 HUD 도달 경로를 확보 |
| Pause | `ResumeButton` ↕ `GoTitleButton` |
| ConfirmPopup | `NoButton` ↔ `YesButton` |
| Result | `ReplayButton` ↕ `GoTitleButton` |

---

## UI Prefab 후보

| Prefab | 구조 | 재사용 지점 |
| :--- | :--- | :--- |
| `OrderPrefab` | CakeIcon + CountText | 주문 카드 슬롯 |
| `ChoicePrefab` | CakeIcon + CountText | 쟁반 슬롯 |
| `Score` | ScoreText + ScoreValueText | 상단 HUD 점수 |
| `Progress Slider_Yellow` | Background + Fill Area/Fill | 영업시간 게이지 |

---

## UI 리소스 규칙

### 디저트 아이콘

| DessertType | 파일 | 실루엣 |
| :--- | :--- | :--- |
| `ChocoCake` | `Choco_Cake` | 둥근 홀케이크 |
| `KiwiBigCake` | `Kiwi_BigCake` | 큰 원형 |
| `LemonRectangularCake` | `Lemon_RectangularCake` | 사각형 |
| `RainbowCupCake` | `Rainbow_CupCake` | 컵케이크 |
| `SkullCakePiece` | `Skull_CakePiece` | 삼각 조각 |

enum 순서를 스프라이트 배열 인덱스로 사용하므로 **순서 바꾸기 X**

### 글꼴

| 항목 | 규칙 |
| :--- | :--- |
| 한국어 | `KERISKEDU_R SDF` (`MyAssets/PORTFOLIO_Assets/Font/`) |
| 숫자·영문 | `Baloo-Regular SDF` (Free Casual GUI 동봉) |
| Weight | Regular / Bold 2종만 |
| 굽기 방식 | TMP Font Asset Creator, Custom Characters (사용 문자만 포함해 아틀라스 절약) |
| TextMeshProUGUI | 모든 Text 전부 TextMeshProUGUI 사용 |

### 색 규칙

| 역할 | 용도 |
| :--- | :--- |
| Normal | 버튼 기본, 게이지 평상시 |
| Success | 성공 토스트 배경 |
| Danger | 실패 토스트 배경, 남은 시간 10초 이하 게이지 점멸의 한쪽 끝 |
| Disabled | 판정 중 잠긴 진열대 버튼 |
| 등급 색 A~F | 결과 화면 `RankText`|

### 버튼 상태

| 상태 | 표현 | 발생 조건 |
| :--- | :--- | :--- |
| Normal | 기본 색상 | 평상시 |
| Highlighted | 밝기 상승 | 마우스 hover / 키보드 선택 |
| Pressed | 축소 + 밝기 하강 | 누르는 중 |
| Disabled | Disabled 색 | 판정 연출 중 (진열대) |

---

## 피드백 설계

| 상황 | 피드백 |
| :--- | :--- |
| 주문 성공 | Success 색 및 ToastMessage Text="주문 성공!" |
| 주문에 없는 품목 선택 | Danger 색 및 ToastMessage Text="주문에 없는 품목이에요" |
| 판정 연출 중 | 진열대 버튼 5개 `interactable = false` + Disabled 색 |
| **남은 시간 10초 이하** | **게이지가 평상시 색 ↔ Danger 색으로 점멸** (색 변화) |
| **최종 등급 표시** | **등급별로 RankText 색이 달라짐** (색 변화) |
| 버튼 hover / 선택 / 눌림 | Normal / Highlighted / Pressed | UGUI Button |

### 등급 색

| 등급 | 색 |
| :-- | :--- |
| A | `#FFD24A` 금색 |
| B | `#6FD1FF` 하늘 |
| C | `#7ED37E` 초록 |
| D | `#FFB067` 주황 |
| E | `#FF8A6B` 연한 빨강 |
| F | `#9AA0A6` 회색 |


### 토스트 설계

- 매번 `Instantiate` 하지 않고 `Screen_Play` 아래 `ToastMessage` 오브젝트 1개를 켜고 끄기(따로 풀링 X).
- 표시 시간은 `GamePlayController.judgeDelaySeconds`(0.8초) **이하**로 둔다.
- 연속으로 뜰 수 있으므로 이전 코루틴을 반드시 중단하고 새로 시작한다.
- `Raycast Target` 을 꺼서 버튼 클릭을 가로채지 않게 한다.