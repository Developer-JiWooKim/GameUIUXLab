# 디저트 가게 주문 받기 — Unity UI/UX 포트폴리오

## 이 프로젝트가 무엇인가

학교 과제(NCS `0803020529_24v5` 게임 UI/UX 프로그래밍)로 제출하는 Unity 6 UI 프로토타입입니다.
**평가 대상은 게임의 재미나 규모가 아니라 UI/UX 설계·구현·검증 과정입니다.**

제한 시간 60초 안에 손님의 주문을 정확히 처리해 점수를 얻는 캐주얼 세로형 게임입니다.

```
손님 1명 등장 → 주문 카드 표시 → 진열대에서 디저트를 눌러 쟁반에 담기 → 제출
  ├ 주문과 일치     → 점수 획득, 처리 +1
  └ 불일치/시간초과 → 실패 +1
→ 다음 손님 → (영업시간 60초 종료) → 결과 화면
```

마감은 2026-08-26입니다. **완성도보다 요건 충족이 우선입니다.**

---

## 절대 하지 말 것

이 프로젝트에서 가장 큰 위험은 버그가 아니라 **범위 확장**입니다. 아래는 요청받지 않는 한 절대 구현하지 마세요. "있으면 좋을 것 같아서" 추가하는 것도 금지입니다.

- 난이도 상승 곡선, 시간에 따른 스폰 가속
- 콤보, 보너스 점수, 배수 시스템
- 손님 캐릭터 스프라이트, 표정, 등장 애니메이션
- 최고 점수 저장, `PlayerPrefs`, 세이브/로드
- 효과음, BGM, `AudioSource`
- 튜토리얼 화면, 도움말
- 동시 손님(대기줄), 손님 선택 상태 관리
- 드래그 앤 드롭
- 오브젝트 풀링, `DOTween` 등 외부 라이브러리
- 씬 분리 (`SceneManager.LoadScene`)
- Safe Area(노치) 대응
- 게임패드 입력

추가 기능이 필요해 보이면 **구현하지 말고 먼저 물어보세요.**

---

## 기술 스택 / 제약

| 항목 | 값 |
| :--- | :--- |
| Unity | 6 |
| UI | UGUI (`UnityEngine.UI`) |
| 텍스트 | **TextMeshPro 필수** (`TextMeshProUGUI`). 레거시 `Text` 사용 금지 |
| 입력 | Input System (`InputSystem_Actions` 에셋 이미 존재) |
| 씬 | **단일 씬** (`GameUIUXLab`). 화면 전환은 패널 활성/비활성 |
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
│       ├── Sprite/           ← 디저트 아이콘 5종
│       └── Prefabs/          ← UI 프리팹
├── Skyden_Games/             ← Free Casual GUI (버튼/패널/게이지 배경)
├── TextMesh Pro/
└── InputSystem_Actions/
```

`Skyden_Games`와 에셋 스토어 폴더는 **수정하지 마세요.** 읽기만 합니다.

---

## 디저트 품목

스프라이트 5종이 `Sprite/`에 있습니다. 형태가 서로 다르도록 선별한 것이므로 늘리거나 줄이지 마세요.

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
GameState        ← 실제 데이터 소유 (점수, 시간, 주문, 쟁반)
   ↓ 읽기
UI Controller    ← GameState를 읽어 View에 반영
   ↓
UGUI View        ← TextMeshProUGUI, Image, Button
```

UI 스크립트가 점수 같은 게임 데이터를 직접 소유하면 안 됩니다. `HudController`는 `GameState.score`를 읽어 표시할 뿐이고, 점수를 바꾸는 주체는 `GameState`입니다.

### 스크립트 구성

| 스크립트 | 역할 |
| :--- | :--- |
| `GameState` | score, remainingTime, currentOrder, tray, successCount, failCount 보유 |
| `OrderGenerator` | 무작위 주문 생성 (품목 1~3종, 각 1~2개) |
| `ScreenManager` | 화면 패널 전환, 전환 시 초기 선택 버튼 지정 |
| `HudController` | GameState 값을 HUD에 반영 |
| `OrderCardView` | 현재 주문을 DessertIcon으로 표시 |
| `TrayView` | 쟁반 슬롯 표시, 담기·취소 처리 |
| `ShelfView` | 진열대 버튼 생성 및 입력 수신 |
| `ToastController` | 알림 메시지 표시·소멸 |

### 화면

```
Screen_Title   — 시작, 종료
Screen_Play    — HUD + 주문카드 + 쟁반 + 제출 + 진열대
Screen_Pause   — 계속하기, 타이틀로(확인 팝업 경유)
Screen_Result  — 최종 점수, 처리/실패, 다시하기, 타이틀로
```

전환 흐름:
```
Title → Play ⇄ Pause
         ↓ (시간 종료)
       Result → Play(재시작) / Title
Pause → ConfirmPopup → Title
```

---

## 프리팹

| 프리팹 | 재사용 지점 |
| :--- | :--- |
| `DessertIcon` | 주문 카드 / 쟁반 슬롯 / 진열대 버튼 — **3곳** |
| `MenuButton` | 4개 화면 전부 |
| `GaugeBar` | 영업시간 표시 |
| `ToastMessage` | 성공/오답 알림 |
| `ConfirmPopup` | 타이틀 복귀 확인 |

`DessertIcon`이 세 컨텍스트에서 재사용되는 것이 과제 요건 6번의 핵심 증거입니다. 컨텍스트별로 별도 프리팹을 만들지 말고 **하나를 크기·역할만 달리해 사용**하세요.

---

## 판정 로직

주문 일치 판정은 이게 전부입니다. 복잡하게 만들지 마세요.

```
List<DessertType> currentOrder  vs  List<DessertType> tray
→ 품목별 개수가 모두 일치하면 성공
```

순서는 무관합니다. 품목별 카운트를 세서 비교하면 됩니다.

---

## 입력

| 입력 | 처리 |
| :--- | :--- |
| 마우스 클릭 | UGUI Button `onClick` |
| 터치 | **동일 경로. 추가 구현 불필요** |
| 키보드 Navigate | 방향키 / Tab |
| 키보드 Submit | Enter / Space |
| 키보드 Cancel | Esc |

### 반드시 지킬 것

- **화면을 활성화할 때마다** `EventSystem.current.SetSelectedGameObject(firstButton)`을 호출하세요. 안 하면 화면 전환 후 키보드 포커스가 사라집니다. 과제 요건 5번 직결입니다.
- 진열대 버튼은 가로 1열이므로 Navigation을 `Horizontal` 또는 `Explicit`로 지정하세요. `Automatic`은 밀집 배치에서 이동 순서가 어긋납니다.
- 포인터 입력이 발생하면 `SetSelectedGameObject(null)`로 선택 하이라이트를 해제하세요. 안 하면 마우스로 누른 뒤 엉뚱한 버튼에 테두리가 남습니다.
- `Cancel` 동작은 상황별로 다릅니다: Play → 일시정지 열기 / Pause → 닫기 / Popup → 팝업만 닫기

---

## 피드백 (요건 7번)

아래는 요건이므로 반드시 구현합니다.

| 상황 | 피드백 |
| :--- | :--- |
| 쟁반이 비어 있음 | 제출 버튼 `interactable = false` + Disabled 색 |
| 주문 성공 | Success 색 토스트 |
| 주문 불일치 | Danger 색 토스트 |
| 남은 시간 10초 이하 | 게이지 색을 Danger로 전환 |
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

현재 완료: 에셋 임포트, 스프라이트 5종 선별·설정, 설계 문서 작성

| 순서 | 작업 |
| :--- | :--- |
| 1 | TMP 한글 폰트 에셋 생성 (Noto Sans KR) |
| 2 | Free Casual GUI 버튼·패널에 9-Slice Border 설정 |
| 3 | 프리팹 5종 제작 |
| 4 | `ScreenManager` + 4개 화면 골격 |
| 5 | `GameState`, `OrderGenerator`, 판정 로직 |
| 6 | HUD 바인딩 |
| 7 | Input System 연결, Navigation 설정 |
| 8 | 피드백 구현 |
| 9 | 해상도 4종 테스트 (1080×1920 / 1080×2340 / 1080×2400 / 1536×2048) |
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

예상되는 항목: 9-Slice 미설정으로 인한 버튼 모서리 깨짐, 화면 전환 후 키보드 포커스 소실, 마우스 클릭 후 선택 하이라이트 잔존, 태블릿 비율(3:4)에서 레이아웃 붕괴, 긴 한글 라벨 넘침

---

## 요건 체크

구현 중 아래를 기준으로 판단하세요. 이 목록에 없으면 만들지 않습니다.

- [ ] UGUI + TextMeshPro 사용
- [ ] Canvas Scaler 기준 해상도 1080×1920, Match 0
- [ ] HUD 3개 이상이 실제 게임 상태와 연결 (시간·점수·주문 안내·처리/실패)
- [ ] 화면 3개 이상 + 버튼으로 전환 가능
- [ ] 마우스로 UI 조작 가능
- [ ] 키보드로 Navigate / Submit / Cancel 중 2개 이상 확인 가능
- [ ] 재사용 UI Prefab 2개 이상
- [ ] 피드백 2개 이상 (버튼 상태·알림·비활성화·색 변화)
- [ ] 해상도 변경 테스트 수행
- [ ] 체크리스트에 기대/실제/수정/재확인 기록
