# 실습 문제집 — 이 프로젝트의 기법을 맨손으로 다시 짜기

## 사용법

1. 새 Unity 프로젝트(또는 이 프로젝트 안의 `Sandbox` 폴더)를 만들고, **원본 코드를 보지 않은 채** 문제를 푼다.
2. 각 문제는 이렇게 구성된다.
   - **문제** — 무엇을 만들어야 하는가
   - **요구사항** — 반드시 만족해야 하는 스펙 (체크박스)
   - **함정** — 대충 짜면 반드시 밟게 되는 버그. 여기서 막히면 그게 그 문제의 핵심이다
   - **통과 기준** — 스스로 검증하는 방법
   - **정답 대조** — 다 풀고 나서 열어볼 원본 파일
3. **함정을 미리 읽지 말 것.** 먼저 짜고, 30분 이상 막힐 때만 펼친다.
4. 문제는 `L1 → L6` 순서대로 난이도가 올라간다. 앞 문제의 결과물을 뒤 문제에서 재사용한다.

## 난이도 표

| 레벨 | 주제 | 핵심 기법 | 체감 난이도 |
|---|---|---|---|
| L1 | 순수 규칙 클래스 | MonoBehaviour 없는 도메인 모델, 멀티셋 판정 | ★★☆☆☆ |
| L2 | 이벤트 기반 UI | C# event, 구독 생명주기, 갱신 최소화 | ★★★☆☆ |
| L3 | 비동기 연출과 취소 | Awaitable, CancellationToken, 세대 토큰 | ★★★★★ |
| L4 | 화면 스택 FSM | 인터페이스 의존 역전, Push/Pop, CanvasGroup | ★★★★☆ |
| L5 | 입력과 포커스 | Input System 직접 구독, 프레임 순서, EventSystem | ★★★★★ |
| L6 | 풀링과 해상도 | ObjectPool, GetWorldCorners, camera.rect | ★★★☆☆ |

---

# L1. 순수 규칙 클래스 — "MonoBehaviour 없이 게임 규칙 짜기"

> 이 레벨의 목표: **게임 규칙에서 Unity를 완전히 걷어내기.**
> 여기서 만든 클래스들은 `using UnityEngine;` 이 한 줄도 없어야 한다.

## 문제 1-1. PlayClock (카운트다운)

**문제**
제한 시간을 재는 순수 C# 클래스 `PlayClock` 을 만든다. `MonoBehaviour` 를 상속하지 않는다.

**요구사항**
- [ ] `Reset(float durationSeconds)` — 시간을 설정하고 되감는다
- [ ] `Tick(float deltaTime)` — 시간을 흘려보낸다. **반환값이 있다**
- [ ] `Duration`, `Remaining`, `IsExpired` 를 읽기 전용으로 노출
- [ ] `Remaining` 은 절대 음수가 되지 않는다
- [ ] `Reset` 에 음수를 넣어도 터지지 않는다

**핵심 질문 — `Tick` 의 반환값은 무엇이어야 하는가?**
"시간이 다 됐다(`IsExpired`)" 를 그대로 돌려주면 안 된다. 왜 안 되는지 스스로 답할 수 있어야 한다.
힌트: 이 값을 쓰는 쪽은 `Update()` 안에서 매 프레임 호출한다. 그리고 시간이 다 되면 게임오버 화면으로 넘긴다.

<details><summary>함정 (막히면 펼치기)</summary>

`Tick` 이 `IsExpired` 를 그대로 반환하면, 남은 시간이 0이 된 뒤 **매 프레임 계속 true** 가 나온다.
게임오버 연출이 초당 60번 시작된다.

`Tick` 은 **"이번 호출로 방금 0이 되었는가"** 라는 *에지(edge)* 를 반환해야 한다.
이미 0이었다면 `false`. 이걸 '레벨 트리거 vs 에지 트리거' 라고 부른다.
</details>

**통과 기준**

```
clock.Reset(1f);
clock.Tick(0.5f);   // false
clock.Tick(0.5f);   // true   ← 이번에 0이 됨
clock.Tick(0.5f);   // false  ← 이미 0이었음
clock.Remaining;    // 0f  (음수 아님)
```

**정답 대조** → `Core/Rules/PlayClock.cs`

---

## 문제 1-2. ScoreBoard (점수 계산)

**문제**
성공/실패 건수와 점수를 관리하는 순수 C# 클래스 `ScoreBoard` 를 만든다.

**요구사항**
- [ ] `Reset(int scorePerSuccess, int scorePenaltyPerFail)` — 가중치를 받고 0으로 초기화
- [ ] `Apply(bool success)` — 성공/실패 1건을 반영
- [ ] `Score`, `SuccessCount`, `FailCount` 읽기 전용 노출
- [ ] **`Score` 는 절대 음수가 되지 않는다**
- [ ] 반면 `SuccessCount` / `FailCount` 는 누적된 사실이므로 감점과 무관하게 계속 쌓인다

**생각해볼 것**
"점수는 0에서 멈추는데 실패 횟수는 계속 쌓인다" 는 규칙 때문에, **점수만 보고는 등급을 매길 수 없다.**
0성공 20실패와 0성공 0실패는 둘 다 점수 0이다. 이 문제는 L1-4에서 해결한다.

**정답 대조** → `Core/Rules/ScoreBoard.cs`

---

## 문제 1-3. OrderSession (주문 판정) ★ 이 레벨의 핵심

**문제**
손님 한 명의 주문을 판정하는 순수 C# 클래스 `OrderSession` 을 만든다.

주문은 디저트 종류의 목록이다. 예: `[초코케이크, 초코케이크, 레몬케이크]`
플레이어가 버튼을 눌러 하나씩 담고, **주문에 없는 것을 담으면 즉시 실패**다.

**요구사항**
- [ ] `Begin(IReadOnlyList<DessertType> newOrder)` — 새 주문 시작
- [ ] `Pick(DessertType type)` — 하나 담기. **결과를 3가지로 구분해서 반환**한다
  - `Accepted` — 맞게 담았고, 아직 남았다
  - `Completed` — 맞게 담았고, 이걸로 주문이 완성됐다
  - `Rejected` — 주문에 없는(또는 이미 다 담은) 것을 담았다
- [ ] `Order`, `Tray` 를 `IReadOnlyList<T>` 로 노출 (외부에서 수정 불가)
- [ ] `IsComplete` 프로퍼티
- [ ] `Begin(null)` 을 넣어도 터지지 않는다

**핵심 질문 — 판정을 어떤 자료구조로 하는가?**

순진하게 짜면 `List` 를 순회하며 `Contains` 로 검사하게 된다. 그러면 이 케이스가 깨진다.

> 주문이 `[초코, 초코, 레몬]` 인데 플레이어가 `초코, 초코, 초코` 를 담았다.
> 세 번째 초코는 **Rejected** 여야 한다. 하지만 `Contains(초코)` 는 여전히 true다.

**"순서는 상관없지만 개수는 정확해야 하는" 컬렉션**을 뭐라고 부르는지, 그걸 C#에서 뭘로 구현하는지가 이 문제의 전부다.

<details><summary>함정 (막히면 펼치기)</summary>

이건 **멀티셋(multiset / bag)** 판정이다. `Dictionary<DessertType, int>` 로 "아직 안 담은 개수" 를 들고 있어야 한다.

- `Begin` 에서 주문을 순회하며 종류별 카운트를 센다
- `Pick` 에서 해당 종류의 남은 개수가 0 이하면 `Rejected`
- 아니면 1 깎고, 전부 0이 됐으면 `Completed`, 아니면 `Accepted`

`_remaining.Remove(type)` 로 지우는 방식도 되지만, 카운트를 0으로 남겨두는 편이 `IsComplete` 를 단순하게 만든다.
`TryGetValue(type, out int count)` 로 "없으면 0" 을 받아내는 패턴을 익혀둘 것.
</details>

**통과 기준**

```
session.Begin(new[]{ 초코, 초코, 레몬 });
session.Pick(초코);   // Accepted
session.Pick(레몬);   // Accepted
session.Pick(레몬);   // Rejected  ← 레몬은 1개뿐이었다
session.Pick(초코);   // Completed
session.Tray.Count;   // 3
```

**정답 대조** → `Core/Rules/OrderSession.cs`, `Core/Rules/PickResult.cs`

---

## 문제 1-4. RankTable (등급 판정)

**문제**
성공/실패 건수로 최종 등급(A~F)을 매기는 컴포넌트를 만든다.
등급 기준은 **인스펙터에서 디자이너가 수정할 수 있어야** 한다.

**요구사항**
- [ ] 등급 하나를 표현하는 `[Serializable] struct RankRule { label, minRankScore, color }`
- [ ] `RankRule[]` 을 `[SerializeField]` 로 노출하고, 코드에 기본값 테이블을 넣어둔다
- [ ] `Evaluate(int successCount, int failCount)` → `RankRule` 반환
- [ ] 내부 점수 = `성공 × 가중치 − 실패 × 페널티` (음수 허용)
- [ ] **어떤 입력이 들어와도 반드시 등급 하나가 나온다.** 빈 문자열이 화면에 뜨는 일이 없어야 한다
- [ ] 배열이 비어 있거나 null 이어도 터지지 않는다

**핵심 질문 — "어떤 입력이든 반드시 하나에 걸린다" 를 어떻게 보장하는가?**
`minRankScore = 0` 을 최하위로 두면 안 된다. L1-2에서 봤듯 내부 점수는 음수가 될 수 있다(0성공 20실패 = −1000).

<details><summary>함정 (막히면 펼치기)</summary>

최하위 등급의 임계값을 **`int.MinValue`** 로 둔다. 이걸 **센티넬 값(sentinel)** 이라고 한다.
"모든 입력을 반드시 포획하는 마지막 규칙" 은 예외 처리 대신 쓰는 아주 흔한 방어 기법이다.

추가 함정: `RankRule best = default;` 로 초기화하면 `color` 의 알파가 **0** 이다.
루프에서 한 번도 갱신되지 않으면 투명한 글자가 화면에 뜬다. 반환 직전에 알파를 검사해야 한다.
</details>

**정답 대조** → `Core/RankTable.cs`

---

# L2. 이벤트 기반 UI — "View는 Controller를 모른 채 갱신된다"

> 이 레벨의 목표: **게임 로직이 UI를 직접 건드리지 않게 만들기.**
> `GamePlayController` 안에 `scoreText.text = ...` 가 단 한 줄도 없어야 한다.

## 문제 2-1. GamePlayController — 규칙 조립과 이벤트 발행 ★ 핵심

**문제**
L1에서 만든 `PlayClock` / `OrderSession` / `ScoreBoard` 를 **소유**하고,
상태가 바뀔 때마다 **C# `event`** 로 바깥에 알리는 `MonoBehaviour` 를 만든다.

**요구사항**
- [ ] 세 규칙 클래스를 `private readonly` 필드로 소유
- [ ] `Update()` 에서 진행 중일 때만 시계를 흘린다
- [ ] 아래 이벤트를 노출한다 (전부 `event Action<...>`)

| 이벤트 | 시그니처 | 언제 |
|---|---|---|
| `OnTimeChanged` | `(남은시간, 전체시간)` | 매 프레임 |
| `OnScoreChanged` | `(점수)` | 판정 시 |
| `OnCountChanged` | `(성공, 실패)` | 판정 시 |
| `OnOrderChanged` | `(주문목록, 손님번호)` | 새 손님 |
| `OnTrayChanged` | `(쟁반목록)` | 담을 때 / 새 손님 |
| `OnJudged` | `(성공여부)` | 판정 순간 |
| `OnJudgingChanged` | `(잠금여부)` | 판정 연출 시작/끝 |
| `OnRunningChanged` | `(진행여부)` | 일시정지 / 종료 |
| `OnGameOver` | `()` | 시간 만료 |

- [ ] `Pick(type)` — 진행 중이 아니거나 판정 중이면 **무시**한다
- [ ] `Prepare()` / `StartGame()` / `SetRunning(bool)` 공개 메서드
- [ ] `SetRunning` 은 **값이 실제로 바뀔 때만** 이벤트를 쏜다

**핵심 질문 1 — 왜 `event` 인가? 그냥 `public Action` 이면 안 되는가?**
`event` 키워드가 막아주는 것이 무엇인지 한 문장으로 답할 수 있어야 한다.

**핵심 질문 2 — 왜 `OnJudged` 와 `OnJudgingChanged` 를 따로 두는가?**
둘 다 판정 시점에 발생하는데 왜 나눴는지 생각해볼 것. (구독자가 서로 다르다)

<details><summary>함정</summary>

- `event` 는 외부에서 `+=` / `-=` 만 허용한다. `public Action` 이면 남이 `= null` 로 **전체 구독자를 날려버릴 수 있다.**
- `Invoke` 는 반드시 `?.Invoke(...)` — 구독자가 0명이면 `Action` 은 null이다.
- `OnJudged(bool)` 는 "토스트 띄워라"(일회성 사건), `OnJudgingChanged(bool)` 는 "버튼 잠가라"(지속 상태).
  사건과 상태를 한 이벤트에 섞으면 구독자가 자기 관심사만 골라내지 못한다.
- `SetRunning` 에서 `if (isRunning == running) return;` 을 빼먹으면, 일시정지 화면을 열 때마다 잠금 이벤트가 중복으로 나간다.
</details>

**정답 대조** → `Core/GamePlayController.cs`

---

## 문제 2-2. HudController — 구독 생명주기와 즉시 동기화

**문제**
점수 / 성공·실패 건수 / 타이머(게이지 + 숫자) 를 표시하는 HUD를 만든다.
`GamePlayController` 의 이벤트만 구독하고, **직접 `Update()` 를 돌리지 않는다.**

**요구사항**
- [ ] `OnEnable` 에서 구독, `OnDisable` 에서 **반드시** 해제
- [ ] 구독 직후 **현재 값으로 한 번 즉시 갱신**한다
- [ ] 게이지(Slider)는 매 프레임 갱신해도 되지만, **숫자 텍스트는 초가 바뀔 때만** 갱신한다
- [ ] 남은 시간이 임계값 이하면 게이지가 경고색으로 **깜빡인다**
- [ ] 단, 텍스트 색은 **경고 상태가 전환되는 순간에만** 바꾼다
- [ ] 참조가 하나라도 비어 있으면 `NullReferenceException` 대신 경고 로그

**핵심 질문 — 왜 구독 직후 한 번 즉시 갱신해야 하는가?**
이걸 빼먹으면 정확히 언제 버그가 보이는지 설명할 수 있어야 한다.

<details><summary>함정</summary>

- **즉시 동기화 누락**: 이벤트는 "값이 바뀔 때" 온다. HUD가 켜진 순간부터 첫 이벤트가 올 때까지는
  **프리팹에 저장된 더미 텍스트("999", "Score")** 가 그대로 보인다. 일시정지 후 복귀 시 특히 티가 난다.
  → `OnEnable` 끝에서 핸들러를 직접 한 번 호출한다.
- **해제 누락**: `OnDisable` 에서 `-=` 를 빼먹으면 파괴된 오브젝트가 계속 구독자로 남아
  `MissingReferenceException` 이 난다. `+=` 를 쓴 순간 `-=` 를 먼저 적는 습관을 들일 것.
- **캐시 값 초기화**: `lastDisplayedSecond` 같은 캐시는 `OnEnable` 에서 `-1` 로 되돌려야 한다.
  안 그러면 재진입 시 "값이 같다" 고 판단해 갱신을 건너뛴다.
- 깜빡임은 `Mathf.PingPong(t, 1f)` + `Color.Lerp` 조합. `Mathf.Sin` 보다 의도가 명확하다.
</details>

**통과 기준**
- 일시정지 → 복귀를 10번 반복해도 로그에 예외가 없다
- 타이머 숫자가 1초에 한 번만 바뀐다 (프로파일러에서 TMP 재빌드 확인)

**정답 대조** → `Views/HudController.cs`

---

## 문제 2-3. ShelfView — 클로저 캡처

**문제**
디저트 버튼 5개를 배열로 받아, 각 버튼이 자기가 담당하는 디저트를 `Pick` 하도록 런타임에 연결한다.

**요구사항**
- [ ] `ShelfButton` 컴포넌트가 `DessertType` 을 들고 있게 한다
- [ ] `foreach` 로 순회하며 `onClick.AddListener(...)` 로 람다를 등록
- [ ] `OnJudgingChanged` + `OnRunningChanged` 를 구독해 버튼 전체를 잠그고 푼다
- [ ] 잠금 조건: `IsRunning && !IsJudging` 일 때만 누를 수 있다

**핵심 질문**
`foreach` 변수를 람다 안에서 그대로 캡처하면 무슨 일이 일어나는가?
C# 버전에 따라 답이 다르다는 것까지 알고 있는가?

<details><summary>함정</summary>

C# 5 이전에는 `foreach` 변수가 루프 전체에서 하나로 공유돼서 **모든 버튼이 마지막 디저트를 Pick** 했다.
C# 5부터 `foreach` 변수는 반복마다 새로 만들어져 이 버그는 사라졌다.

그런데도 원본 코드가 `ShelfButton captured = shelfButton;` 으로 한 번 더 복사한 이유는
**의도를 명시하기 위해서**다. `for (int i = ...)` 로 바꾸면 즉시 되살아나는 버그이기 때문에,
"이 변수는 캡처된다" 를 코드에 남겨둔 것. 방어적 코딩의 사례로 기억해둘 것.
</details>

**정답 대조** → `Views/ShelfView.cs`, `Views/ShelfButton.cs`

---

# L3. 비동기 연출과 취소 — 이 프로젝트에서 가장 어려운 부분

> 이 레벨의 목표: **"기다리는 코드" 가 남긴 유령을 없애기.**
> 여기서 나오는 버그들은 전부 "0.8초 뒤에 깨어난 코드가 이미 사라진 세상을 건드린다" 는 한 문장으로 요약된다.

## 문제 3-1. ScreenFade — Awaitable 기초

**문제**
검은 `CanvasGroup` 의 알파를 0→1, 1→0 으로 보간하는 페이드 컴포넌트를 만든다.
코루틴(`IEnumerator`)이 **아닌** Unity 6의 `Awaitable` 로 만든다.

**요구사항**
- [ ] `public async Awaitable FadeIn()` / `FadeOut()`
- [ ] 호출부에서 `await fade.FadeIn();` 으로 기다릴 수 있어야 한다
- [ ] `FadeOut` 이 끝나면 오브젝트를 꺼서 레이캐스트를 막지 않게 한다
- [ ] **일시정지 중에도 페이드가 진행된다**

**핵심 질문 2가지**
1. 프레임을 기다릴 때 `Time.deltaTime` 을 쓰면 어떤 상황에서 페이드가 멈추는가?
2. 페이드 도중 이 오브젝트가 파괴되면 어떻게 되는가?

<details><summary>함정</summary>

1. **`Time.timeScale = 0` 으로 일시정지를 구현하면 `Time.deltaTime` 이 0이 되어 페이드가 영원히 안 끝난다.**
   → `Time.unscaledDeltaTime` 을 쓴다. UI 연출은 거의 항상 unscaled 다.
2. **`await Awaitable.NextFrameAsync()` 를 인자 없이 쓰면**, 오브젝트가 파괴된 뒤에도 코드가 깨어나
   파괴된 `canvasGroup` 에 접근한다. → `destroyCancellationToken` 을 넘긴다.

`destroyCancellationToken` 은 `MonoBehaviour` 에 내장된 프로퍼티로,
그 오브젝트가 파괴될 때 자동으로 취소된다. **Unity에서 async를 쓸 때의 기본 안전장치**다.
</details>

**정답 대조** → `Screens/ScreenFade.cs`

---

## 문제 3-2. ToastController — CTS 재생성 패턴

**문제**
판정 결과("성공!" / "실패..")를 0.5초 띄웠다 지우는 토스트를 만든다.
`OnJudged` 이벤트를 구독한다.

**요구사항**
- [ ] 성공/실패에 따라 문구·배경색·글자색이 바뀐다
- [ ] 0.5초 뒤 자동으로 꺼진다
- [ ] **토스트가 떠 있는 동안 새 토스트가 오면, 새 토스트가 0.5초를 온전히 채운다**
- [ ] 화면을 떠나거나(`OnDisable`) 파괴돼도 안전하다
- [ ] 오브젝트 풀링을 쓰지 않는다 (왜 안 써도 되는지 주석으로 근거를 남길 것)

**핵심 질문 — 왜 `destroyCancellationToken` 하나로는 부족한가?**

<details><summary>함정</summary>

이게 이 문제의 전부다:

> 첫 토스트가 0.3초 지났을 때 두 번째 토스트가 뜬다.
> **첫 번째의 타이머가 0.2초 뒤에 깨어나 두 번째 토스트를 꺼버린다.**

`destroyCancellationToken` 은 "오브젝트가 죽을 때" 만 취소된다. "새 토스트가 왔을 때" 는 취소해주지 않는다.
→ **직접 만든 `CancellationTokenSource` 를 필드로 들고 있다가, 새 요청이 오면 이전 것을 `Cancel()` 한다.**

```csharp
private CancellationTokenSource hideCts;

private async void RunHide()
{
    CancelHide();                       // ① 이전 대기를 반드시 끊는다
    hideCts = CancellationTokenSource
        .CreateLinkedTokenSource(destroyCancellationToken);   // ② 파괴도 함께 감시

    try { await Awaitable.WaitForSecondsAsync(showSeconds, hideCts.Token); }
    catch (OperationCanceledException) { return; }            // ③ 취소는 정상 흐름
    SetVisible(false);
}
```

- `CreateLinkedTokenSource` — 여러 취소 사유를 **하나의 토큰으로 합치는** API. 반드시 익혀둘 것.
- `CancelHide()` 는 `Cancel()` **과 `Dispose()` 를 둘 다** 부르고 필드를 `null` 로 만든다. CTS는 `IDisposable` 이다.
- `catch (OperationCanceledException)` 은 **에러 처리가 아니라 정상 종료 경로**다. 로그를 찍지 말 것.
- `OnDisable` 과 `OnDestroy` 양쪽에서 취소해야 한다.
</details>

**통과 기준**
디저트 버튼을 최대한 빠르게 연타했을 때, 토스트가 깜빡이거나 조기에 사라지지 않는다.

**정답 대조** → `Views/ToastController.cs`

---

## 문제 3-3. CountdownView — 연결된 토큰(linked token)

**문제**
`3 → 2 → 1 → Start!` 를 0.7초 간격으로 보여주는 오버레이를 만든다.
게임 종료 시엔 `Timeout!!` 을 1초 보여준다. **두 연출을 같은 내부 메서드로 처리한다.**

**요구사항**
- [ ] `PlayIntro(CancellationToken token)` / `PlayTimeout(CancellationToken token)` — 둘 다 `Awaitable` 반환
- [ ] **외부에서 넘어온 토큰**과 **자기 파괴 토큰**을 **둘 다** 감시한다
- [ ] 인트로는 끝나면 패널을 끄고, 타임아웃은 패널을 **켠 채로 둔다** (그 위로 페이드가 덮이므로)
- [ ] 중간에 취소되어도 패널 상태가 어긋나지 않는다

**핵심 질문**
왜 호출자가 토큰을 넘겨주는데도, 내부에서 토큰을 하나 더 합쳐야 하는가?

<details><summary>함정</summary>

- 호출자(PlayState)의 토큰은 "화면을 떠났다" 를 알린다. 자기 `destroyCancellationToken` 은 "내가 파괴됐다" 를 알린다.
  **둘은 서로 다른 사건이고, 둘 다 대기를 끊어야 한다.**
  → `using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(token, destroyCancellationToken);`
  `using` 선언문을 쓰면 메서드 끝에서 자동 `Dispose` 된다.
- 패널 켜기/끄기는 `try { ... } finally { ... }` 로 감싼다.
  취소로 `OperationCanceledException` 이 던져져도 `finally` 는 실행되므로 패널이 켜진 채 남지 않는다.
- `hideWhenDone` 같은 bool 파라미터로 두 연출의 차이를 흡수하면 메서드 하나로 합칠 수 있다.
</details>

**정답 대조** → `Views/CountdownView.cs`

---

## 문제 3-4. 세대 토큰(generation token) ★★ 이 문서에서 가장 어려운 문제

**문제**
`GamePlayController` 에 **판정 딜레이**를 넣는다.
성공/실패 판정이 나면 0.8초 동안 입력을 잠그고, 그 뒤 다음 손님으로 넘어간다.

**요구사항**
- [ ] 판정 직후 `OnJudgingChanged(true)` → 0.8초 대기 → 다음 손님 → `OnJudgingChanged(false)`
- [ ] 대기 중에는 `Pick` 이 무시된다
- [ ] **대기 중에 `Prepare()`(다시하기)가 호출되면, 그 대기는 아무 일도 하지 않고 조용히 끝나야 한다**

**마지막 요구사항이 이 문제의 전부다. 재현 시나리오:**

> 실패 판정이 나서 0.8초 대기가 시작됐다.
> 플레이어가 0.3초 만에 일시정지 → 다시하기를 눌렀다. `Prepare()` 가 새 판을 깔고 손님 1번을 세웠다.
> 0.5초 뒤, **죽은 줄 알았던 이전 판의 대기가 깨어나 `NextCustomer()` 를 부른다.**
> 새 판이 시작하자마자 손님이 2번으로 건너뛴다.

`CancellationToken` 으로 풀 수도 있지만, 여기서는 **더 가벼운 기법**을 쓴다.
정수 필드 하나로 이걸 막을 수 있다. 어떻게?

<details><summary>함정 / 정답 방향</summary>

**세대 토큰(generation token)** 또는 **에포크(epoch) 카운터** 라고 부르는 기법이다.

```csharp
private int judgeToken;

private async void BeginJudgeDelay()
{
    int token = ++judgeToken;          // ① 이번 대기의 "세대 번호" 를 찍어둔다
    SetJudging(true);

    try { await Awaitable.WaitForSecondsAsync(judgeDelaySeconds, destroyCancellationToken); }
    catch (OperationCanceledException) { return; }

    if (token != judgeToken) return;   // ② 깨어나 보니 세대가 바뀌었다 → 나는 유령이다
    NextCustomer();
    SetJudging(false);
}

public void Prepare()
{
    judgeToken++;                      // ③ 세대를 넘겨 진행 중인 대기를 전부 무효화
    SetJudging(false);
    // ...
}
```

**왜 CTS 대신 int 인가?**
CTS는 `Dispose` 관리가 필요하고, 여기서는 "취소" 가 아니라 **"결과를 버린다"** 가 맞는 의미다.
대기 자체는 자연히 끝나게 두고, 깨어난 뒤 자기가 유효한지 검사만 한다.
네트워크 응답 폐기, 검색어 자동완성, 씬 로딩 등 **"늦게 온 응답 버리기"** 에 널리 쓰이는 패턴이다.

**꼭 짚을 것**: `async void` 는 예외를 잡을 수 없어서 원래 위험하다.
Unity 이벤트 핸들러에서만 예외적으로 허용되며, 반드시 `try/catch (OperationCanceledException)` 로 감싸야 한다.
</details>

**통과 기준**
판정 직후 0.8초 안에 "다시하기" 를 20번 연타해도 손님 번호가 항상 1부터 시작한다.

**정답 대조** → `Core/GamePlayController.cs` 의 `judgeToken`, `BeginJudgeDelay()`, `Prepare()`

---

## 문제 3-5. PlayState — CTS 수명 관리

**문제**
게임 화면에 들어오면 인트로 카운트다운을 재생하고, 끝나면 게임을 시작한다.
시간이 다 되면 `Timeout!!` 을 보여준 뒤 결과 화면으로 넘긴다.
**연출 중에는 일시정지 버튼이 눌리지 않아야 한다.**

**요구사항**
- [ ] `Enter()` 에서 구독 + `Prepare()` + 인트로 재생
- [ ] `Exit()` 에서 구독 해제 + **진행 중인 연출 취소** + 시계 정지
- [ ] `BeginSequence()` / `EndSequence()` / `CancelSequence()` 3종 세트로 CTS 수명을 관리
- [ ] 연출 중 `OnCancel()`(ESC)이 들어와도 일시정지 화면이 열리지 않는다
- [ ] 일시정지 **버튼 컴포넌트 자체**도 `interactable = false` 로 잠근다

**핵심 질문 — 카운트다운 패널이 화면 전체를 덮고 있어서 클릭은 이미 막힌다. 그런데 왜 버튼을 또 잠그는가?**

<details><summary>함정</summary>

**패널은 마우스 클릭(레이캐스트)만 막는다. 키보드 Enter / 게임패드 A 는 패널을 그냥 통과한다.**
`EventSystem` 의 Submit은 레이캐스트를 거치지 않고 `currentSelectedGameObject` 에게 직접 간다.
카운트다운 중에 Enter를 치면 뒤에 가려진 일시정지 버튼이 눌린다.
→ 시각적 차단(패널)과 논리적 차단(`interactable`)은 **별개**다. UI 작업에서 반복해서 마주치는 원칙.

`BeginSequence()` 안에서 **먼저 `CancelSequence()` 를 부른 뒤** 새 CTS를 만드는 순서도 중요하다.
인트로가 끝나기 전에 타임아웃이 겹치는 상황에서 CTS가 새는 것을 막는다.
</details>

**정답 대조** → `Screens/PlayState.cs`

---

# L4. 화면 스택 FSM — "팝업 위에 팝업"

> 이 레벨의 목표: **화면 전환을 `SetActive` 난사 대신 자료구조로 다루기.**

## 문제 4-1. 인터페이스 2개 설계 ★ 설계 문제

**문제**
화면 시스템을 두 개의 인터페이스로 나눈다. **코드보다 설계 의도를 답하는 문제다.**

- `IState` — 화면 하나가 지켜야 할 계약
- `IScreenController` — 화면들이 "다음으로 가 줘" 라고 부탁하는 창구

**요구사항**
- [ ] `IState` : `FirstSelected` 프로퍼티, `Enter()`, `Exit()`, `OnCancel()`
- [ ] `IScreenController` : `ShowTitle/ShowPlay/ShowResult/ShowPause/HidePause/OpenConfirm/CloseConfirm/Quit`
- [ ] 추상 클래스 `UIStateBase : MonoBehaviour, IState` 를 만들고, 구체 화면들은 이걸 상속
- [ ] `UIStateBase` 는 `Bind(IScreenController)` 로 창구를 주입받는다

**핵심 질문 — 왜 `PauseState` 가 `ScreenFlowController` 를 직접 `[SerializeField]` 로 참조하면 안 되는가?**

<details><summary>답</summary>

**순환 의존**이 생긴다. `ScreenFlowController` 는 모든 State를 알고, State도 컨트롤러를 알면
둘이 한 덩어리가 되어 어느 쪽도 따로 테스트하거나 교체할 수 없다.

인터페이스를 사이에 끼우면 State는 **"화면을 넘기는 능력"** 에만 의존하고,
그걸 누가 구현하는지는 모른다. 이게 **의존성 역전 원칙(DIP)** 이다.

효과: 튜토리얼 전용 `TutorialScreenFlow` 를 만들어 끼워도 State 코드는 한 줄도 안 바뀐다.

`Bind()` 라는 별도 메서드로 주입하는 이유는, 인터페이스는 `[SerializeField]` 로 인스펙터에 꽂을 수 없기 때문이다.
Unity에서 DI를 흉내 낼 때의 표준 우회법이다.
</details>

**정답 대조** → `Screens/IState.cs`, `Screens/IScreenController.cs`, `Screens/UIStateBase.cs`

---

## 문제 4-2. ScreenFlowController — Stack 기반 전환

**문제**
화면 전환을 `Stack<UIStateBase>` 로 관리하는 컨트롤러를 만든다.

**전환은 두 종류다. 이 구분이 이 문제의 핵심이다.**

| | 동작 | 예 |
|---|---|---|
| **교체(SetScreen)** | 스택을 **전부 비우고** 새 화면 하나만 | 타이틀 → 플레이 → 결과 |
| **쌓기(Push/Pop)** | 아래 화면을 **살려둔 채** 위에 얹기 | 플레이 → 일시정지 → 종료확인 |

**요구사항**
- [ ] `SetScreen` 은 페이드 인 → 전부 팝 → 새 화면 푸시 → 페이드 아웃 (`async`)
- [ ] `Push` / `Pop` 은 페이드 없이 즉시
- [ ] `PushInternal` 에서 **아래 화면을 `SetInteractable(false)`** 로 잠근다
- [ ] `PopTop` 에서 아래 화면을 다시 푼다
- [ ] 전환 중(`isTransitioning`)에는 모든 입력을 무시한다
- [ ] `HandleCancel()` — 스택 맨 위 화면의 `OnCancel()` 만 부른다
- [ ] 화면이 바뀔 때마다 `EventSystem` 의 선택을 그 화면의 `FirstSelected` 로 옮긴다

**핵심 질문 — ESC를 눌렀을 때 "어느 화면이 반응해야 하는가" 를 어떻게 결정하는가?**

<details><summary>함정</summary>

- ESC 처리는 **스택 최상단에게만** 위임한다(`Top.OnCancel()`). 각 화면이 자기 ESC 동작을 스스로 정의하므로
  `if (일시정지중) ... else if (확인창) ...` 같은 분기가 통째로 사라진다. **상태 패턴의 실질적 이득**이 여기 있다.
- `TitleState.OnCancel()` 과 `ResultState.OnCancel()` 은 **의도적으로 비워둔다.**
  "여기선 ESC로 뒤로 갈 곳이 없다" 를 코드로 명시하는 것. 빈 override에 그 의도를 주석으로 남겨야 한다.
- `isTransitioning` 가드를 빼면, 페이드 0.5초 동안 버튼을 연타해서 화면 두 개를 동시에 켤 수 있다.
- `ApplyFocus()` 에서 `SetSelectedGameObject(null)` 을 **먼저** 부른 뒤 대상을 넣어야 한다.
  같은 오브젝트를 다시 선택할 때 `EventSystem` 이 갱신을 건너뛰는 경우가 있다.
</details>

**정답 대조** → `Screens/ScreenFlowController.cs`

---

## 문제 4-3. CanvasGroup.ignoreParentGroups ★ 함정 문제

**문제**
`Screen_Pause` 안의 자식으로 `ConfirmPopup` 이 들어 있다.
확인 팝업이 열리면 아래의 일시정지 화면 버튼들은 눌리지 않아야 한다.

L4-2의 `PushInternal` 규칙대로 아래 화면(`Screen_Pause`)의 `CanvasGroup.interactable = false` 를 줬다.
**그랬더니 확인 팝업의 "예 / 아니오" 버튼까지 같이 죽었다.** 왜인가? 어떻게 고치는가?

<details><summary>답</summary>

`CanvasGroup.interactable` 은 **자손 전체에 상속된다.**
`ConfirmPopup` 이 `Screen_Pause` 의 자손이므로, 부모를 잠그면 자기 자신까지 잠긴다.

→ 팝업 자신의 `CanvasGroup` 에 **`ignoreParentGroups = true`** 를 준다.
"부모 그룹의 판정을 무시하고 내 설정만 따른다" 는 뜻이다.

원본에서는 `UIStateBase.SetInteractable()` 안에서 `CanvasGroup` 을 지연 획득할 때
**모든 화면에 일괄로** 이 플래그를 켠다. 화면은 어차피 서로 독립적으로 잠기고 풀려야 하므로,
개별 예외를 두는 것보다 규칙을 통일하는 편이 안전하다.

이 문제를 못 풀면 대개 계층 구조를 바꿔서(팝업을 바깥으로 빼서) 우회하게 된다.
그것도 답이지만, `ignoreParentGroups` 를 알고 있는 것과 모르는 것은 다르다.
</details>

**정답 대조** → `Screens/UIStateBase.cs` 의 `SetInteractable()`

---

# L5. 입력과 포커스 — 마우스와 키보드를 함께 지원하기

> 이 레벨의 목표: **"마우스로도 되고 키보드로도 되는" UI의 실제 난이도를 체감하기.**
> 여기 나오는 버그는 전부 **한 프레임 안의 실행 순서** 때문에 생긴다.

## 문제 5-1. UIInputRouter — 프레임 순서 ★★★ 가장 어려운 문제

**문제**
마우스와 키보드/게임패드를 함께 지원하는 입력 라우터를 만든다.
`InputSystemUIInputModule` 이 이미 붙어 있는 상태에서, **액션을 직접 구독**해 다음 3가지를 처리한다.

1. **Cancel** — ESC / 게임패드 B → `ScreenFlowController.HandleCancel()`
2. **선택 해제** — 마우스나 터치로 조작하면 키보드 선택 테두리를 지운다
3. **선택 복구** — 선택이 비어 있을 때 방향키나 Enter가 들어오면 현재 화면의 첫 버튼을 다시 선택

**요구사항**
- [ ] `InputActionReference` 4개를 인스펙터로 받는다 (cancel / click / navigate / submit)
- [ ] `OnEnable` 구독 / `OnDisable` 해제
- [ ] `IsSelectionMode` 프로퍼티 — 마지막 조작이 키보드 계열이면 true (L5-2에서 쓴다)
- [ ] 스틱을 놓을 때 오는 0 근처 값을 복구 신호로 오인하지 않는다(데드존)
- [ ] 클릭 판정은 **마우스/터치/펜에서 온 입력만** 인정한다

**그리고 여기가 핵심이다:**
- [ ] **선택 변경을 액션 콜백 안에서 하지 말고 `LateUpdate` 로 미룬다**

**핵심 질문 — 왜 콜백에서 바로 `SetSelectedGameObject` 를 부르면 안 되는가? 두 가지 증상을 각각 설명하라.**

순진하게 짜서 아래 두 버그를 **직접 재현해보고** 원인을 알아내는 것이 이 문제의 목표다.

> **증상 A** — 마우스로 버튼을 클릭했는데 노란 테두리가 안 지워진다.
> **증상 B** — 마우스로 아무 데나 클릭한 뒤 Enter를 치면, 화면의 첫 버튼이 **저절로 눌린다.**

<details><summary>정답 (충분히 헤맨 뒤에 열 것)</summary>

한 프레임 안의 실행 순서는 이렇다:

```
[입력 갱신] → 액션 콜백 발생        ← 우리 코드가 여기서 실행됨
[Update]    → EventSystem / InputSystemUIInputModule 이 여기서 돔
[LateUpdate]→ 우리가 미뤄둔 코드
```

**증상 A**: 우리가 콜백에서 선택을 `null` 로 지운다.
그 **직후** Update에서 `Selectable.OnPointerDown` 이 돌면서 **자기 자신을 다시 선택한다.**
우리 코드가 먼저 실행돼서 그대로 덮어써진다.

**증상 B**: 모듈은 Submit 을 "**처리 시점의** `currentSelectedGameObject`" 에게 보낸다.
콜백에서 미리 첫 버튼을 복구해두면, 그 프레임의 Update에서 모듈이 **방금 복구한 버튼에게 Submit을 배달**한다.
복구용으로 친 Enter가 그대로 버튼 실행이 되어버린다.

**해결**: 콜백에서는 `clearRequested` / `restoreRequested` **플래그만 세우고**,
실제 `SetSelectedGameObject` 는 `LateUpdate` 에서 한다.
그러면 그 프레임 입력은 모듈이 "선택 없음" 으로 처리해 아무 일도 일어나지 않고, 다음 프레임부터 정상 동작한다.

**이것은 버그가 아니라 콘솔 UI의 표준 동작이다.** 마우스를 쓰다가 패드를 잡으면
첫 입력은 커서를 되살리는 데 쓰이고, 그 입력으로 버튼이 실행되지는 않는다.

추가로 알아야 할 것:
- `context.control.device` 로 **어느 장치에서 왔는지** 판별한다 (`is Mouse || is Touchscreen || is Pen`).
  게임패드의 A버튼도 Click 액션에 바인딩돼 있을 수 있어서, 장치 검사 없이는 패드 조작이 마우스로 오인된다.
- 클릭 해제 시 `topAtPointerPress` 에 당시의 최상단 화면을 기록해뒀다가,
  `LateUpdate` 에서 화면이 바뀌었으면 해제를 취소한다.
  버튼을 눌러 화면이 전환된 경우, 새 화면이 방금 세팅한 포커스를 지워버리기 때문이다.
</details>

**통과 기준**
- 마우스로 버튼 클릭 → 테두리가 사라진다
- 이어서 Enter → **아무 버튼도 실행되지 않고** 테두리만 첫 버튼에 나타난다
- 한 번 더 Enter → 그때 실행된다

**정답 대조** → `Screens/UIInputRouter.cs`

---

## 문제 5-2. FocusRing — 런타임 UI 생성과 좌표 변환

**문제**
현재 선택된 버튼 주위에 사각형 테두리를 그리는 컴포넌트를 만든다.
**Sprite를 쓰지 않고 `Image` 4개를 런타임에 생성**해서 상하좌우 변으로 쓴다.

**요구사항**
- [ ] `Awake` 에서 `Image` 4개를 코드로 생성하고 앵커를 잡아 각각 상/하/좌/우 변이 되게 한다
- [ ] `LateUpdate` 에서 `EventSystem.current.currentSelectedGameObject` 를 추적
- [ ] 선택된 버튼의 사각형에 맞춰 위치·크기를 조정
- [ ] `IsSelectionMode` 가 false(마우스 조작 중)면 숨긴다
- [ ] **`interactable == false` 인 버튼에는 테두리를 두르지 않는다**
- [ ] 생성한 `Image` 의 `raycastTarget` 을 끈다
- [ ] **해상도가 바뀌어도 테두리 두께·위치가 어긋나지 않는다**
- [ ] 선택이 바뀌지 않는 프레임에는 재계산하지 않는다

**핵심 질문 2가지**
1. 다른 `RectTransform` 의 화면상 사각형을 어떻게 알아내는가?
2. Canvas Scaler가 켜져 있을 때, 그 값을 그대로 쓰면 왜 크기가 어긋나는가?

<details><summary>함정</summary>

1. **`target.GetWorldCorners(corners)`** — `Vector3[4]` 를 채워준다.
   순서는 **0=좌하, 1=좌상, 2=우상, 3=우하**. 이 순서를 외우고 있어야 한다.
   중심은 `(corners[0] + corners[2]) * 0.5f`.

2. **Canvas Scaler(Scale With Screen Size)가 캔버스 전체를 확대·축소**하므로,
   월드 좌표에서 잰 길이를 `sizeDelta`(로컬 단위)에 그대로 넣으면 해상도에 따라 배율만큼 어긋난다.
   → **`canvas.transform.lossyScale.x` 로 나눠서** 되돌린다.
   `lossyScale` 이 0일 수 있으니(`Mathf.Approximately`) 방어할 것.

3. `raycastTarget = false` 를 안 끄면 테두리가 자기가 감싼 버튼의 클릭을 막는다.
   **런타임에 만드는 모든 장식용 Image의 필수 설정.**

4. 매 프레임 `GetWorldCorners` + `SetVisible` 을 부르는 대신,
   `needsFit` 더티 플래그와 `lastScreenWidth/Height` 비교로 **바뀐 프레임에만** 재계산한다.
   `SetVisible` 도 `visible` 캐시와 비교해 실제로 바뀔 때만 4개 Image를 순회한다.
   이 "**바뀐 것만 반영**" 패턴은 L2의 HUD, 여기, 그리고 L6의 레터박스에 모두 반복해서 등장한다.
</details>

**정답 대조** → `Screens/FocusRing.cs`

---

## 문제 5-3. 숫자키 단축키 — 바인딩 인덱스 매핑

**문제**
숫자키 `1~5` 로 진열대 버튼 5개를 곧바로 누를 수 있게 한다.
**하나의 Input Action** 에 5개 바인딩(1,2,3,4,5)을 걸어두고 처리한다.

**요구사항**
- [ ] 어떤 키가 눌렸는지 알아내 해당 인덱스의 버튼을 실행
- [ ] 마우스로 누른 것과 **완전히 동일하게** 동작한다 (버튼 눌림 연출 포함)
- [ ] 잠긴 버튼(`interactable == false`)은 무시하고, **포커스 테두리도 옮기지 않는다**
- [ ] `onClick.Invoke()` 를 직접 부르지 않는다

**핵심 질문 2가지**
1. `performed` 콜백에서 "몇 번째 바인딩이 눌렸는가" 를 어떻게 알아내는가?
2. `onClick.Invoke()` 를 직접 부르면 안 되는 이유는?

<details><summary>답</summary>

1. **`context.action.GetBindingIndexForControl(context.control)`**
   눌린 컨트롤이 그 액션의 **몇 번째 바인딩**인지 돌려준다.
   바인딩을 인스펙터에 1,2,3,4,5 순서로 넣어뒀다면 그대로 배열 인덱스로 쓸 수 있다.
   (`Vector2Composite` 같은 복합 바인딩에서는 인덱스가 밀리므로 주의)

2. `onClick.Invoke()` 는 **로직만 실행하고 시각 피드백이 없다.**
   버튼이 눌린 색으로 변하지 않아 "먹었는지 안 먹었는지" 를 알 수 없다.

   → `EventSystem.current.SetSelectedGameObject(target)` 로 포커스를 옮긴 뒤
   `ExecuteEvents.Execute(target, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler)`
   로 **Submit 이벤트를 정식으로 발행**한다.
   그러면 `Button` 이 `OnSubmit` 을 받아 눌림 연출까지 스스로 재생한다.

   `ExecuteEvents.Execute` 는 **"UI 이벤트를 코드로 위조해서 보내는"** 공식 API다.
   자동 테스트나 튜토리얼의 "여기를 누르세요" 데모에도 그대로 쓰인다.
</details>

**정답 대조** → `Views/ShelfView.cs` 의 `HandleSlotShortcut`

---

# L6. 풀링과 해상도 대응

## 문제 6-1. DessertIconPool — ObjectPool&lt;T&gt; 래핑

**문제**
주문 카드와 쟁반이 **함께 쓰는** 아이콘 풀을 만든다.
`UnityEngine.Pool.ObjectPool<T>` 를 감싼 **순수 C# 클래스**로 만든다 (`MonoBehaviour` 아님).

**요구사항**
- [ ] 생성자 `(DessertIconView prefab, Transform root, int capacity)`
- [ ] `Get()` / `ReleaseAll()` / `Dispose()`
- [ ] **`ReleaseAll()` 로 한 번에 전부 반납**할 수 있어야 한다 (개별 반납 API는 필요 없다)
- [ ] `collectionCheck: true` 로 이중 반납을 잡는다
- [ ] 사용하는 쪽(`TrayView`, `OrderCardView`)은 `OnDestroy` 에서 `Dispose()`

**핵심 질문 — 풀에서 꺼낸 아이콘이 Layout Group 안에서 엉뚱한 자리에 나타난다. 왜인가?**

<details><summary>답</summary>

**재사용된 오브젝트는 예전 `sibling index`(부모 안에서의 순서)를 그대로 들고 온다.**
`HorizontalLayoutGroup` / `GridLayoutGroup` 은 계층 순서대로 배치하므로,
3번째였던 아이콘을 꺼내면 첫 번째로 꺼냈는데도 3번째 칸에 놓인다.

→ `actionOnGet` 에서 **`icon.transform.SetAsLastSibling()`** 을 부른다.
꺼낸 순서 = 배치 순서가 보장된다.

`ObjectPool<T>` 의 4개 콜백을 정확히 구분할 것:

| 콜백 | 언제 |
|---|---|
| `createFunc` | 풀이 비어서 새로 만들 때 (`Instantiate`) |
| `actionOnGet` | 꺼낼 때 (`SetActive(true)`, `SetAsLastSibling`) |
| `actionOnRelease` | 반납할 때 (`SetActive(false)`) |
| `actionOnDestroy` | `maxSize` 초과분 폐기 / `Clear()` (`Destroy`) |

`ReleaseAll` 을 위해 **밖에 나가 있는 것들의 `List` 를 따로 들고 있어야 한다.**
`ObjectPool<T>` 는 대여 중인 인스턴스를 추적해주지 않는다.
순회 중 `icon == null` 검사도 필요하다 — 씬 언로드로 이미 파괴됐을 수 있다.
</details>

**정답 대조** → `Views/DessertIconPool.cs`

---

## 문제 6-2. OrderCardView — 표시용 집계

**문제**
주문 `[초코, 초코, 레몬]` 을 화면에는 **아이콘 2개**로 보여준다: `초코 ×2`, `레몬`.
반면 쟁반(`TrayView`)은 담은 순서대로 **3개를 그대로** 보여준다.

**요구사항**
- [ ] 주문은 종류별로 묶어서 개수를 표시하고, `×1` 은 표시하지 않는다
- [ ] 쟁반은 담긴 순서대로 하나씩 표시한다
- [ ] **두 화면이 같은 프리팹과 같은 `DessertIconView` 컴포넌트를 공유한다**
- [ ] 집계용 `Dictionary` 를 매번 `new` 하지 않는다

**생각해볼 것**
같은 데이터를 **모델은 리스트로, 뷰는 집계해서** 다루고 있다.
집계를 `OrderSession` 안에 넣지 않고 View에 둔 이유는 무엇인가?

<details><summary>답</summary>

집계는 **표시 규칙**이지 게임 규칙이 아니다.
"×2로 묶어서 보여준다" 는 디자인 결정이고, 판정에는 아무 영향이 없다.
모델에 넣으면 "UI를 바꾸려는데 판정 코드를 건드려야 하는" 상황이 생긴다.

**모델은 사실을, 뷰는 표현을 담는다.** 이 경계를 어디에 긋느냐가 L2의 이벤트 설계와 함께
이 프로젝트 아키텍처의 골격이다.

`private readonly Dictionary<DessertType, int> counts = new();` 를 필드로 두고
`counts.Clear()` 로 재사용하면 주문이 바뀔 때마다 발생하는 GC 할당을 없앨 수 있다.
초당 수십 번 도는 코드가 아니라 효과는 작지만, **재사용 가능한 임시 버퍼는 필드로** 라는 습관은 들여둘 가치가 있다.
</details>

**정답 대조** → `Views/OrderCardView.cs`, `Views/TrayView.cs`, `Views/DessertIconView.cs`

---

## 문제 6-3. AspectRatioLetterbox — 카메라 뷰포트 계산

**문제**
목표 비율(예: 1080×1920 세로)을 유지하고, 남는 부분은 검은 레터박스로 채우는 컴포넌트를 만든다.

**요구사항**
- [ ] `[RequireComponent(typeof(Camera))]`
- [ ] 창 크기가 **바뀐 프레임에만** 재계산한다
- [ ] 창이 목표보다 **가로로 넓으면** 좌우에 검은 띠 (필러박스)
- [ ] 창이 목표보다 **세로로 길면** 상하에 검은 띠 (레터박스)
- [ ] 인스펙터를 비워두거나 창이 최소화돼도 **0으로 나누지 않는다**

**힌트**
`camera.rect` 는 0~1 정규화 좌표의 `Rect(x, y, width, height)` 다.
`scale = 현재비율 / 목표비율` 을 구한 뒤 1보다 큰지 작은지로 두 경우를 나눈다.
검은 배경은 카메라의 Background Color(검정) + Clear Flags 설정으로 자동으로 생긴다.

**정답 대조** → `Views/AspectRatioLetterbox.cs`

---

# 최종 통합 과제

문제를 다 풀었다면, 아래를 **처음부터 혼자** 만들어본다. 원본을 열지 않는다.

> **"제한 시간 안에 손님 주문에 맞는 아이템을 담는 게임"** 을 만든다.
>
> - 타이틀 → 플레이 → 결과 화면 (페이드 전환)
> - 플레이 중 ESC로 일시정지, 일시정지에서 "그만두기" → 확인 팝업
> - 인트로 3-2-1-Start! 카운트다운
> - 주문 표시 / 쟁반 표시 / 성공·실패 토스트 / 남은 시간 경고 연출
> - 마우스·키보드·게임패드 전부 지원, 포커스 테두리 표시
> - 숫자키 단축키
> - 결과 화면에 A~F 등급

**자가 채점표**

| 항목 | 확인 방법 | ✓ |
|---|---|---|
| 규칙 클래스에 `using UnityEngine` 이 없다 | 파일 상단 확인 | |
| 게임 로직에 `.text =` 가 없다 | `GamePlayController` 전체 검색 | |
| 모든 `+=` 에 짝이 되는 `-=` 가 있다 | `+=` 검색 후 개수 대조 | |
| 판정 직후 다시하기 연타 → 손님 번호가 1부터 | 20회 연타 | |
| 디저트 버튼 연타 → 토스트가 조기에 사라지지 않음 | 연타 | |
| 일시정지 중에도 페이드가 진행됨 | `timeScale=0` 으로 확인 | |
| 마우스 클릭 후 첫 Enter가 버튼을 실행하지 않음 | 직접 조작 | |
| 창 크기를 바꿔도 포커스 테두리 두께가 일정 | 창 리사이즈 | |
| 확인 팝업의 버튼이 살아 있다 | 팝업 열고 클릭 | |
| 카운트다운 중 Enter로 일시정지가 안 열림 | 인트로 중 Enter | |
| 씬을 여러 번 재진입해도 예외 로그가 없다 | 콘솔 | |

---

# 부록 A. 이 프로젝트에서 뽑아낸 기법 목록

문제를 풀지 않더라도, 아래 항목에 대해 **"이게 뭐고 왜 쓰는지"** 를 말로 설명할 수 있는지 스스로 점검해볼 것.

**설계**
- 도메인 로직과 MonoBehaviour 분리 (순수 C# 규칙 클래스)
- C# `event` 기반 단방향 데이터 흐름 (Model → View)
- 인터페이스를 통한 의존성 역전 (`IScreenController`)
- 상태 패턴 (`IState` + `UIStateBase` + Stack)
- 사건(event)과 상태(state) 이벤트의 분리

**비동기**
- `Awaitable` (Unity 6) — 코루틴 대체
- `destroyCancellationToken` — 파괴 안전장치
- `CancellationTokenSource.CreateLinkedTokenSource` — 취소 사유 합치기
- CTS 재생성 패턴 (`Cancel` → `Dispose` → `null` → 새로 생성)
- `catch (OperationCanceledException)` — 정상 종료 경로
- 세대 토큰 (generation token / epoch counter) — 늦게 온 응답 폐기
- `try/finally` 로 취소 시에도 상태 복구
- `async void` 의 허용 범위

**Unity UI**
- `CanvasGroup.interactable` 상속과 `ignoreParentGroups`
- 시각적 차단(패널)과 논리적 차단(`interactable`)의 분리
- `EventSystem.SetSelectedGameObject` 와 프레임 실행 순서
- `ExecuteEvents.Execute(..., submitHandler)` — UI 이벤트 위조
- `RectTransform.GetWorldCorners` (0=좌하, 1=좌상, 2=우상, 3=우하)
- `canvas.transform.lossyScale` 로 Canvas Scaler 배율 되돌리기
- `raycastTarget = false` — 장식용 Image
- `Transform.SetAsLastSibling()` 과 Layout Group 배치 순서
- `camera.rect` 로 레터박스/필러박스

**Input System**
- `InputActionReference` 직접 구독 (`performed += `)
- `context.control.device` 로 입력 장치 판별
- `GetBindingIndexForControl` 로 바인딩 인덱스 → 슬롯 매핑
- 아날로그 스틱 데드존
- 포인터 모드 / 셀렉션 모드 구분

**성능·방어**
- `UnityEngine.Pool.ObjectPool<T>` 4개 콜백
- 더티 플래그 (`needsFit`, `lastDisplayedSecond`, `lastDangerState`)
- 재사용 버퍼 (`readonly Dictionary` + `Clear()`)
- 센티넬 값 (`int.MinValue`)
- 에지 트리거 vs 레벨 트리거
- 멀티셋 판정 (`Dictionary<T,int>`)
- `IReadOnlyList<T>` 로 컬렉션 노출
- null 검사 + `Debug.LogWarning(msg, this)` — 두 번째 인자로 클릭 시 해당 오브젝트 하이라이트

---

# 부록 B. 추천 진행 순서

**1주차 — L1 (규칙 클래스)**
Unity를 열지 않고 콘솔 프로젝트에서 푼다. 가능하면 유닛 테스트도 붙인다.
이 클래스들이 `MonoBehaviour` 가 아니기 때문에 **테스트가 가능하다** 는 것을 몸으로 느낄 것.

**2주차 — L2 + L4 (이벤트 + 화면 전환)**
동작하는 게임의 뼈대가 여기서 완성된다. 연출은 아직 없어도 된다.

**3주차 — L3 (비동기)**
가장 오래 걸린다. 문제 3-4(세대 토큰)는 하루를 잡고 붙어도 좋다.
**먼저 버그를 재현하는 것**부터 시작할 것. 버그를 못 만들면 해법도 와닿지 않는다.

**4주차 — L5 + L6 (입력/포커스 + 풀링)**
L5-1은 순진하게 짠 뒤 두 증상을 직접 재현해보는 과정이 핵심이다.
증상을 눈으로 못 보면 `LateUpdate` 지연이 왜 필요한지 절대 이해되지 않는다.

**5주차 — 최종 통합 과제**

---

*문제집 작성 기준 코드: `Assets/MyAssets/PORTFOLIO_Assets/Scripts` (32개 파일, 약 2,460줄)*

---
---

# PART 2. 코딩테스트 형식 문제집

## 이 파트는 위(PART 1)와 무엇이 다른가

| | PART 1 (위) | PART 2 (여기) |
|---|---|---|
| 형식 | "원본 클래스를 재현하라" | **입출력이 명시된 독립 문제** |
| 채점 | 원본 파일과 대조 | **예시 입출력 + 배점표로 자가 채점** |
| 범위 | 프로젝트 구조에 의존 | **파일 하나로 완결** (앞 문제를 안 풀어도 됨) |
| 목적 | 아키텍처 감각 | **기법 하나를 정확히 구사하기** |

같은 기법을 다루더라도, 여기서는 **"이 입력에 이 출력이 나오는가"** 로만 판단한다.
면접 코딩테스트나 과제 전형에서 마주칠 형태에 가깝게 다듬었다.

## 규칙

- 제출 위치: `Assets/MyAssets/PRACTICE_Assets/Scripts/Practice/`
- 파일명: 문제 번호를 그대로 (`B1_DessertAggregator.cs`)
- **`[순수 C#]` 표시가 붙은 문제는 `using UnityEngine;` 을 쓰면 0점이다.** 콘솔에서도 돌아가야 한다
- **`[Unity C#]` 문제는 `MonoBehaviour` 로 만든다**
- 각 문제의 **엣지 케이스는 전부 통과해야 한다.** 정상 케이스만 맞으면 절반도 못 받는다
- 힌트는 접어뒀다. 30분 이상 막혔을 때만 펼친다

## 난이도 로드맵

| 단계 | 문제 | 주제 | 상태 |
|---|---|---|---|
| **초급 B** | B1~B3 | 컬렉션 집계 · 구간 판정 · 생명주기와 갱신 최소화 | ✅ 아래 |
| 중급 I | I1~I3 | 이벤트 발행/구독 · 상태 머신 · 오브젝트 풀 | 예정 |
| 고급 A | A1~A3 | 비동기 취소 · 세대 토큰 · 프레임 순서 제어 | 예정 |

---

# 초급 B — 3문제

---

## B1. 주문 집계기 `[순수 C#]`

**난이도** ★☆☆☆☆ · **제한시간** 30분 · **출제 의도** `Dictionary` 카운팅, 열거 순서 함정

### 문제

손님의 주문 목록을 화면에 표시하기 좋게 **종류별로 묶어서** 집계하는 정적 클래스를 만든다.

주문 `[초코, 초코, 레몬]` 은 아이콘 3개가 아니라 **2개**로 표시된다 — `초코 ×2`, `레몬`.
개수가 1이면 `×1` 을 붙이지 않고 숫자를 아예 감춘다.

### 구현할 것

```csharp
public static class DessertAggregator
{
    // 종류별 개수를 집계한다.
    public static IReadOnlyList<(DessertType Type, int Count)> Aggregate(IReadOnlyList<DessertType> order);

    // 아이콘 옆에 붙일 개수 문자열을 만든다.
    public static string ToLabel(int count);
}
```

`DessertType` 은 프로젝트에 이미 있는 enum 을 그대로 쓴다 (`Core/DessertType.cs`).
이 enum 은 `using UnityEngine` 이 없으므로 순수 C# 규칙을 깨지 않는다.

```csharp
public enum DessertType
{
    ChocoCake, KiwiBigCake, LemonRectangularCake, RainbowCupCake, SkullCakePiece
}
```

### 규칙

1. 같은 종류는 하나로 묶고 개수를 센다
2. **결과 순서는 "주문 목록에서 그 종류가 처음 등장한 순서"** 를 따른다
3. `ToLabel` 은 `count >= 2` 일 때만 `"×N"` 을 반환하고, 그 외에는 **빈 문자열** 을 반환한다

### 입출력 예시

> 지면상 `Choco` = `ChocoCake`, `Lemon` = `LemonRectangularCake`, `Kiwi` = `KiwiBigCake` 로 줄여 적는다.

| # | 입력 `order` | 기대 출력 |
|---|---|---|
| 1 | `[Choco, Choco, Lemon]` | `[(Choco,2), (Lemon,1)]` |
| 2 | `[Lemon, Choco, Lemon]` | `[(Lemon,2), (Choco,1)]` ← **Lemon 이 먼저** |
| 3 | `[Kiwi, Kiwi, Kiwi]` | `[(Kiwi,3)]` |
| 4 | `[Choco, Lemon, Kiwi]` | `[(Choco,1), (Lemon,1), (Kiwi,1)]` |
| 5 | `[]` | `[]` |
| 6 | `null` | `[]` ← 예외를 던지지 않는다 |

| # | 입력 `count` | 기대 출력 |
|---|---|---|
| 7 | `3` | `"×3"` |
| 8 | `1` | `""` |
| 9 | `0` | `""` |

### 제약

- 주문 길이 ≤ 10, 종류 ≤ 5 — **성능 최적화는 채점 대상이 아니다.** 정확성만 본다
- 입력 `order` 를 수정하면 안 된다
- 반환한 컬렉션을 호출자가 `Add` 할 수 있으면 감점 (`IReadOnlyList<T>` 로 노출)

### 함정 — 예시 2번이 이 문제의 전부다

<details><summary>힌트 (막히면 펼치기)</summary>

`Dictionary<DessertType, int>` 하나만 쓰고 `foreach (var pair in dict)` 로 결과를 만들면 **2번에서 깨질 수 있다.**

`Dictionary` 의 열거 순서는 **명세상 보장되지 않는다.** 지금 .NET 구현에서는 삽입 순서대로 나오는 경우가 많지만, 그건 우연이지 계약이 아니다. 키를 지웠다가 다시 넣으면 빈 슬롯이 재사용돼 순서가 뒤집힌다.

해결은 둘 중 하나:

- **`Dictionary`(개수) + `List`(첫 등장 순서)** 를 같이 들고 간다
- 종류가 5개뿐이므로 그냥 **`List<(DessertType, int)>` 선형 탐색** 으로 끝낸다 — 이 규모에선 이게 더 단순하고 빠르다

그리고 개수를 셀 때는 `ContainsKey` 로 먼저 검사하지 말 것. 해시를 두 번 계산한다.

```csharp
dict.TryGetValue(type, out int count);   // 없으면 count == 0
dict[type] = count + 1;
```

`TryGetValue` 는 실패해도 `out` 에 `default` 를 넣어준다. 이 "없으면 0" 패턴이 뒤 문제에서도 계속 나온다.
</details>

### 배점

| 항목 | 배점 |
|---|---|
| 기본 집계 (예시 1, 3, 4) | 30 |
| **첫 등장 순서 유지 (예시 2)** | 25 |
| `ToLabel` 규칙 (예시 7~9) | 15 |
| `null` / 빈 입력 방어 (예시 5, 6) | 15 |
| `IReadOnlyList<T>` 로 노출 · 입력 미변경 | 10 |
| `using UnityEngine` 없음 | 5 |

> **이 프로젝트 어디서 왔나** — `Views/OrderCardView.cs` 의 `Refresh()`, `Views/DessertIconView.cs` 의 `SetIcon()`

---

## B2. 등급 판정기 `[순수 C#]`

**난이도** ★★☆☆☆ · **제한시간** 40분 · **출제 의도** 구간 탐색, 센티넬 값, 초기화 함정

### 문제

점수를 등급(A~F)으로 변환하는 정적 메서드를 만든다.
등급 기준은 **호출자가 배열로 넘긴다** — 기획자가 수정할 수 있어야 하기 때문이다.

각 규칙은 `"이 점수 이상이면 이 등급"` 을 뜻한다. 점수가 여러 규칙에 걸리면 **가장 높은 기준** 의 등급을 준다.

### 구현할 것

```csharp
public readonly struct GradeRule
{
    public readonly string Label;
    public readonly int MinScore;   // 이 값 이상이면 이 등급

    public GradeRule(string label, int minScore)
    {
        Label = label;
        MinScore = minScore;
    }
}

public static class GradeEvaluator
{
    public static string Evaluate(IReadOnlyList<GradeRule> table, int score);
}
```

### 규칙

1. `score >= rule.MinScore` 인 규칙들 중 **`MinScore` 가 가장 큰** 것의 `Label` 을 반환한다
2. **테이블은 정렬돼 있다고 가정하면 안 된다**
3. 아무 규칙에도 걸리지 않으면 `"-"` 를 반환한다
4. `table` 이 `null` 이거나 비어 있으면 `"-"` 를 반환한다
5. **어떤 입력에도 `null` 이나 빈 문자열을 반환하지 않는다**

### 입출력 예시

기준 테이블 (**일부러 뒤섞어 놨다**):

```csharp
var table = new[]
{
    new GradeRule("C", 1600),
    new GradeRule("F", int.MinValue),
    new GradeRule("A", 2800),
    new GradeRule("E", 400),
    new GradeRule("B", 2200),
    new GradeRule("D", 1000),
};
```

| # | `score` | 기대 출력 | 비고 |
|---|---|---|---|
| 1 | `3000` | `"A"` | |
| 2 | `2800` | `"A"` | **경계 포함** (`>=`) |
| 3 | `2799` | `"B"` | 경계 바로 아래 |
| 4 | `1600` | `"C"` | |
| 5 | `400` | `"E"` | |
| 6 | `399` | `"F"` | |
| 7 | `0` | `"F"` | |
| 8 | `-1000` | `"F"` | **음수** |

센티넬이 **없는** 테이블:

```csharp
var noSentinel = new[] { new GradeRule("A", 2800), new GradeRule("B", 2200) };
```

| # | 호출 | 기대 출력 |
|---|---|---|
| 9 | `Evaluate(noSentinel, 3000)` | `"A"` |
| 10 | `Evaluate(noSentinel, 100)` | `"-"` |
| 11 | `Evaluate(null, 500)` | `"-"` |
| 12 | `Evaluate(new GradeRule[0], 500)` | `"-"` |

### 제약

- 테이블 길이 ≤ 20
- `LINQ` 사용 금지 — `foreach` 로 직접 순회한다 (한 번의 순회로 풀 수 있다)
- 테이블을 정렬하거나 수정하면 안 된다 (입력은 읽기 전용)

### 함정 — 8번이 이 문제의 핵심이다

<details><summary>힌트 (막히면 펼치기)</summary>

**함정 1 — 정렬 가정**
"위에서부터 훑다가 처음 걸리는 걸 반환" 하면 테이블 순서에 따라 답이 달라진다.
예시 테이블에서 `score=3000` 이면 `C(1600)` 이 먼저 걸려서 `"C"` 가 나온다.
**전부 순회하면서 최댓값을 갱신** 해야 한다.

**함정 2 — `best` 초기화 (8번에서 터진다)**

```csharp
GradeRule best = default;                      // MinScore == 0
foreach (var rule in table)
{
    if (score < rule.MinScore) continue;
    if (rule.MinScore > best.MinScore) best = rule;   // ← 여기
}
return best.Label;
```

`score = -1000` 이면 통과하는 규칙은 `F(int.MinValue)` 하나뿐이다.
그런데 `int.MinValue > 0` 은 **false** 라서 `best` 가 한 번도 갱신되지 않는다.
`default` 의 `Label` 은 `null` — 화면에 빈 등급이 뜬다.

**"아직 아무것도 못 찾았다" 를 `default` 값으로 표현하면 안 된다.** `bool found` 플래그를 따로 둔다.

```csharp
if (!found || rule.MinScore > best.MinScore) { best = rule; found = true; }
```

**함정 3 — 센티넬의 의미**
`int.MinValue` 를 최하위 등급의 기준으로 두면 **모든 정수가 반드시 하나에 걸린다.**
이걸 센티넬 값(sentinel)이라고 하고, "못 찾았을 때" 분기를 아예 없애는 데 쓴다.
다만 호출자가 센티넬을 안 넣을 수도 있으므로(예시 10번) `"-"` 반환 경로는 여전히 필요하다.
</details>

### 배점

| 항목 | 배점 |
|---|---|
| 기본 구간 판정 (예시 1, 4, 5) | 25 |
| 경계값 포함 `>=` (예시 2, 3) | 15 |
| **정렬 안 된 테이블에서 최댓값 선택** | 20 |
| **음수 · 센티넬 처리 (예시 6~8)** | 25 |
| 미매칭 · `null` · 빈 배열 → `"-"` (예시 10~12) | 15 |

> **이 프로젝트 어디서 왔나** — `Core/RankTable.cs` 의 `Evaluate()`

---

## B3. 타이머 라벨 `[Unity C#]`

**난이도** ★★☆☆☆ · **제한시간** 40분 · **출제 의도** 컴포넌트 생명주기, 더티 플래그, null 가드

### 문제

남은 시간을 초 단위 숫자로 표시하는 UI 컴포넌트를 만든다.
`SetTime()` 은 **매 프레임 호출된다** (초당 60회). 하지만 화면의 글자는 **1초에 한 번만** 바뀌어야 한다.

`TextMeshProUGUI.text` 에 대입하면 같은 값이어도 메시가 다시 만들어진다.
초당 60번 재빌드하는 것과 1번 하는 것은 모바일에서 체감 차이가 난다.

### 구현할 것

```csharp
public sealed class TimerLabel : MonoBehaviour
{
    // 남은 시간(초). 외부에서 매 프레임 호출한다.
    public void SetTime(float remainingSeconds);

    // 채점용. label.text 에 실제로 대입한 횟수.
    public int WriteCount { get; private set; }
}
```

### 요구사항

1. `TextMeshProUGUI` 참조를 **`[SerializeField] private`** 필드로 받는다 (`public` 필드 금지)
2. 표시 문자열은 **올림** 한 정수 — `Mathf.CeilToInt(remainingSeconds)`
   (`1.2초` 남았으면 `"2"`. 내림하면 아직 시간이 남았는데 `0` 이 뜬다)
3. **표시할 정수 초가 직전과 같으면 `text` 에 대입하지 않는다**
4. 대입할 때마다 `WriteCount` 를 1 증가시킨다
5. **`OnEnable` 에서 캐시를 초기화해, 재활성화 직후 첫 호출은 반드시 대입한다**
6. 참조가 `null` 이면 `NullReferenceException` 대신 **경고 로그를 한 번만** 남기고, 이후 조용히 무시한다
   - 로그는 `Debug.LogWarning(name + ": ...", this)` 형태로 남긴다 (두 번째 인자 필수)

### 검증 하니스

아래 스크립트를 빈 GameObject 에 붙이고 재생해서 콘솔을 확인한다.

```csharp
using UnityEngine;

public sealed class TimerLabelTest : MonoBehaviour
{
    [SerializeField] private TimerLabel target;

    private void Start()
    {
        // ── 테스트 1 : 초가 바뀔 때만 쓰는가
        float[] steps = { 3.0f, 2.5f, 2.0f, 1.5f, 1.0f, 0.5f, 0.0f };
        //  CeilToInt →    3     3     2     2     1     1     0
        //  대입       →   O     X     O     X     O     X     O   = 4회
        foreach (float t in steps)
        {
            target.SetTime(t);
        }

        Debug.Log($"[테스트1] WriteCount = {target.WriteCount}  (기대: 4)");

        // ── 테스트 2 : 재활성화 후 첫 호출은 반드시 쓰는가
        int before = target.WriteCount;

        target.gameObject.SetActive(false);
        target.gameObject.SetActive(true);

        target.SetTime(0.0f);   // 직전과 같은 0초. 그래도 대입돼야 한다.

        Debug.Log($"[테스트2] 증가분 = {target.WriteCount - before}  (기대: 1)");
    }
}
```

| 테스트 | 기대 결과 |
|---|---|
| 1 | `WriteCount == 4` |
| 2 | 증가분 `== 1` |
| 3 | 참조를 비워둔 채 `SetTime` 을 100번 호출 → 경고 로그 **1줄**, 예외 없음 |

### 제약

- `Update()` 를 만들지 않는다. 시간은 **외부에서 밀어 넣는다**
- `remainingSeconds` 가 음수여도 예외가 나지 않는다
- 문자열 포맷에 `$"{...}"` 보간이나 `string.Format` 을 쓰지 않는다 — 매 프레임 도는 경로라 불필요한 할당이 생긴다. `int.ToString()` 만 쓴다

### 함정

<details><summary>힌트 (막히면 펼치기)</summary>

**함정 1 — 캐시 초기값**

```csharp
private int lastDisplayedSecond;   // 기본값 0
```

이러면 남은 시간이 정확히 0초일 때 "직전과 같다" 고 판단해 **첫 대입을 건너뛴다.**
화면에는 프리팹에 저장된 더미 텍스트(`"999"` 같은)가 그대로 남는다.

**`-1` 같은 "불가능한 값" 을 `아직 아무것도 안 썼다` 의 표식으로 쓴다.** B2 의 `found` 플래그와 같은 발상이다.

**함정 2 — 초기화 위치**
`Awake` 가 아니라 **`OnEnable`** 에서 되돌려야 한다. `Awake` 는 오브젝트 생애에 한 번뿐이라,
비활성화 → 재활성화(일시정지 후 복귀 등)했을 때 옛 캐시가 그대로 남는다. 테스트 2가 이걸 잡는다.

**함정 3 — 경고 로그 폭주**
`SetTime` 안에서 매번 `LogWarning` 을 찍으면 초당 60줄이 쌓여 콘솔이 마비된다.
`bool warned` 플래그로 한 번만 남긴다.

`Debug.LogWarning` 의 **두 번째 인자 `this`** 는 습관을 들여둘 것.
콘솔에서 그 로그를 클릭하면 하이어라키의 해당 오브젝트가 하이라이트된다.
참조 누락 버그를 찾는 시간이 크게 줄어든다.
</details>

### 배점

| 항목 | 배점 |
|---|---|
| `CeilToInt` 로 올림 표시 | 10 |
| **초가 바뀔 때만 대입 (테스트 1)** | 30 |
| **`OnEnable` 캐시 초기화 (테스트 2)** | 25 |
| null 가드 + 경고 1회 (테스트 3) | 20 |
| `[SerializeField] private` 사용 | 10 |
| 음수 입력 방어 · 문자열 할당 없음 | 5 |

> **이 프로젝트 어디서 왔나** — `Views/HudController.cs` 의 `HandleTimeChanged()`, `lastDisplayedSecond`

---

## 초급 정리 — 세 문제를 관통하는 것

세 문제 모두 같은 질문을 다른 옷을 입혀 물었다.

> **"아직 값이 없다" 를 어떻게 표현할 것인가?**

| 문제 | "값 없음" 상황 | 잘못된 표현 | 올바른 표현 |
|---|---|---|---|
| B1 | 딕셔너리에 키가 없다 | `ContainsKey` 두 번 조회 | `TryGetValue` + `default` 0 |
| B2 | 아직 후보를 못 찾았다 | `default` 구조체 (`MinScore == 0`) | `bool found` 플래그 |
| B3 | 아직 아무것도 안 그렸다 | `0` (실제 가능한 초) | `-1` 센티넬 |

**실제 데이터로 나올 수 있는 값을 "없음" 의 표식으로 쓰면 반드시 터진다.**
`0` 은 유효한 점수이고 유효한 남은 초다. 그래서 `0` 으로는 "없음" 을 나타낼 수 없다.

`null`, `-1`, `int.MinValue`, 별도 `bool` — 어느 것을 고르든 **그 값이 실제 데이터 범위 밖** 이어야 한다는 원칙은 같다. 중급·고급에서 다룰 `judgeToken` 세대 검사와 `CancellationToken` 도 결국 이 원칙의 확장이다.

---

*PART 2 초급 끝. 중급(I1~I3)은 이벤트 발행/구독, 상태 머신, 오브젝트 풀을 다룬다.*

---
---

# PART 3. 5단계 사다리 (난이도 재조정판)

## 왜 다시 만들었나

PART 1은 "완성된 포트폴리오 코드를 재현하라" 였고, PART 2는 `초급` 이라 써놓고 실제로는
`Dictionary` 열거 순서 계약 같은 걸 물었다. **둘 다 라벨이 잘못 붙어 있었다.**

실제 난이도를 다시 매기면 이렇다.

| | 실제 위치 |
|---|---|
| PART 2 초급 (B1~B3) | **3~4단계** |
| PART 1 L1 (규칙 클래스) | **3단계** |
| PART 1 L2·L4 | **4단계** |
| PART 1 L3·L5 | **5단계** |

그래서 아래로 1·2단계를 새로 깔았다. **1단계는 함정이 하나도 없다.**

## 5단계 구성

| 단계 | 이름 | 한 문제에 담기는 것 | 코드량 | 함정 |
|---|---|---|---|---|
| **1단계** | 표현식 | 개념 **1개**. 계산식 하나 | 1~5줄 | **없음** |
| 2단계 | 분기와 방어 | 경계값 · 빈 입력 · `null` | 5~15줄 | 1개 |
| 3단계 | 상태를 가진 클래스 | 필드 + 여러 메서드, 호출 순서 | 15~40줄 | 2개 |
| 4단계 | Unity 컴포넌트 | 생명주기 · 참조 · 이벤트 구독 | 30~80줄 | 2~3개 |
| 5단계 | 설계 | 여러 클래스 협업 · 비동기 · 취소 | 80줄+ | 다수 |

**언어 배분** — 1·2단계는 순수 C# 만. Unity C# 은 3단계부터 나온다.
Unity 를 늦게 넣는 이유는, `MonoBehaviour` 생명주기가 섞이면 "내 로직이 틀린 건지 Unity 가 그런 건지"
구분이 안 돼서 디버깅 난이도가 갑자기 뛰기 때문이다.

---

# 1단계 — 표현식 5문제

**이 단계의 목표는 정답률 100% 다.** 막히면 그건 문제가 잘못된 것이니 그냥 물어보면 된다.

- 전부 **순수 C#** — `using UnityEngine;` 없이 컴파일돼야 한다
- 전부 **`static` 메서드 하나**, 몸통은 길어야 5줄
- **`null` 입력은 들어오지 않는다고 가정한다** (`null` 방어는 2단계에서 다룬다)
- 제출: `Assets/MyAssets/PRACTICE_Assets/Scripts/Study/Level1/L1_Basics.cs`
  (스텁 파일을 만들어 뒀다. 메서드 5개의 `throw` 를 지우고 채우면 된다)

**통과 조건** — 각 문제의 예시를 **전부** 통과하면 합격. 부분 점수 없음.

---

## 1-1. 점수는 음수가 되지 않는다

점수를 받아서, 음수면 `0` 으로 바꿔 돌려준다.

```csharp
public static int ClampScore(int score)
```

| 입력 | 출력 |
|---|---|
| `100` | `100` |
| `0` | `0` |
| `-50` | `0` |
| `-1` | `0` |

**힌트** — `if` 로 써도 되고, `Math.Max` 를 쓰면 한 줄이다. 둘 다 정답.

> 원본: `Core/Rules/ScoreBoard.cs`

---

## 1-2. 개수 라벨 만들기

아이콘 옆에 붙일 개수 문자열을 만든다. **2개 이상일 때만** 표시하고, 1개 이하면 빈 문자열.

```csharp
public static string ToCountLabel(int count)
```

| 입력 | 출력 |
|---|---|
| `3` | `"×3"` |
| `2` | `"×2"` |
| `1` | `""` |
| `0` | `""` |

**힌트** — `×` 는 알파벳 `x` 가 아니라 곱셈 기호(U+00D7)다. 이 줄을 복사해서 쓰면 된다: `"×"`

> 이미 B1 에서 푼 문제다. 워밍업으로 다시 한 번.

---

## 1-3. 남은 시간을 비율로

게이지(`Slider`)에 넣을 `0~1` 비율을 계산한다. **전체 시간이 0 이하면 `0` 을 돌려준다.**

```csharp
public static float ToRatio(float remaining, float total)
```

| 입력 | 출력 |
|---|---|
| `remaining=60, total=120` | `0.5f` |
| `remaining=120, total=120` | `1f` |
| `remaining=0, total=120` | `0f` |
| `remaining=10, total=0` | `0f` ← 0으로 나누지 않는다 |

**힌트** — `total` 이 0 이하인지 **먼저** 검사하고, 아닐 때만 나눈다.
(C# 에서 `float` 을 0으로 나누면 예외 대신 `Infinity` 가 나온다. 게이지가 이상해질 뿐 크래시는 안 나서 더 찾기 어렵다)

> 원본: `Views/HudController.cs` 의 `HandleTimeChanged()`

---

## 1-4. 경고 상태인가

남은 시간이 임계값 **이하** 면 `true`. 게이지를 빨갛게 만들지 판단하는 데 쓴다.

```csharp
public static bool IsDanger(float remaining, float threshold)
```

| 입력 | 출력 |
|---|---|
| `remaining=15, threshold=10` | `false` |
| `remaining=10, threshold=10` | `true` ← **같을 때도 경고** |
| `remaining=3, threshold=10` | `true` |
| `remaining=0, threshold=10` | `true` |

**힌트** — `<` 인지 `<=` 인지만 정확히 고르면 끝. 표의 2번째 줄이 그걸 정한다.

> 원본: `Views/HudController.cs` 의 `ApplyTimeWarning()`

---

## 1-5. 특정 종류가 몇 개인가

주문 목록에서 지정한 디저트가 몇 개 들어 있는지 센다.

```csharp
public static int CountOf(IReadOnlyList<DessertType> order, DessertType target)
```

> `DessertType` 은 프로젝트에 이미 있는 enum 이다 (`ChocoCake`, `KiwiBigCake`, `LemonRectangularCake`, `RainbowCupCake`, `SkullCakePiece`). 스텁 파일에 `using` 이 걸려 있다.

지면상 `초코` = `ChocoCake`, `레몬` = `LemonRectangularCake` 로 줄여 적는다.

| 입력 | 출력 |
|---|---|
| `[초코, 초코, 레몬]`, `target=초코` | `2` |
| `[초코, 초코, 레몬]`, `target=레몬` | `1` |
| `[초코, 초코, 레몬]`, `target=키위` | `0` |
| `[]`, `target=초코` | `0` |

**힌트** — `foreach` 로 하나씩 보면서 `target` 과 같으면 카운터를 1 올린다. `Dictionary` 필요 없다.

```csharp
int count = 0;
foreach (DessertType menu in order)
{
    // 여기서 비교하고 count++
}
return count;
```

> 원본: `Core/Rules/OrderSession.cs` 의 개수 세기 부분을 가장 단순하게 자른 것

---

## 자가 확인

다 짰으면 아래를 콘솔 프로젝트나 Unity 아무 스크립트의 `Start()` 에 넣고 돌려본다.

```csharp
Debug.Log(L1_Basics.ClampScore(-50));                 // 0
Debug.Log(L1_Basics.ToCountLabel(3));                 // ×3
Debug.Log(L1_Basics.ToCountLabel(1));                 // (빈 줄)
Debug.Log(L1_Basics.ToRatio(60f, 120f));              // 0.5
Debug.Log(L1_Basics.ToRatio(10f, 0f));                // 0
Debug.Log(L1_Basics.IsDanger(10f, 10f));              // True
Debug.Log(L1_Basics.CountOf(
    new[] { DessertType.ChocoCake, DessertType.ChocoCake, DessertType.LemonRectangularCake },
    DessertType.ChocoCake));                          // 2
```

일곱 줄이 전부 기대값과 같으면 1단계 통과다.

**1단계는 "할 수 있다" 를 확인하는 단계지 배우는 단계가 아니다.** 30분 안에 끝나야 정상이고,
오래 걸렸다면 문법(메서드 선언, `foreach`, 삼항 연산자) 쪽을 먼저 보는 게 낫다.

---

*2단계는 여기에 경계값·빈 입력·`null` 을 하나씩 얹는다. 1단계를 통과하면 이어서 낸다.*

---

# 2단계 — 분기와 방어 5문제

1단계가 "계산식 하나" 였다면, 2단계는 거기에 **비정상 입력 하나** 를 얹는다.

- 전부 **순수 C#** (`using UnityEngine;` · `using UnityEditor;` 둘 다 금지)
- 전부 **`static` 메서드 하나**, 몸통 5~15줄
- **함정은 문제당 정확히 1개.** 예시 표에서 `←` 로 표시한 줄이 그 함정이다
- 제출: `Assets/MyAssets/PRACTICE_Assets/Scripts/Study/Level2/L2_Guards.cs` (스텁 있음)

**통과 조건** — 예시를 전부 통과하면 합격. `←` 줄을 못 넘기면 불합격.

이 단계의 한 문장:

> **정상 입력으로 짠 코드는 절반만 짠 것이다.**

---

## 2-1. 안전한 배열 조회

배열에서 이름 하나를 꺼낸다. 꺼낼 수 없으면 `null` 을 돌려준다. **예외를 던지지 않는다.**

```csharp
public static string GetName(string[] names, int index)
```

| 입력 | 출력 |
|---|---|
| `["A","B","C"]`, `1` | `"B"` |
| `["A","B","C"]`, `0` | `"A"` |
| `["A","B","C"]`, `3` | `null` |
| `["A","B","C"]`, `-1` | `null` ← |
| `null`, `0` | `null` |
| `[]`, `0` | `null` |

<details><summary>힌트</summary>

범위 검사는 **양쪽** 이다. 대부분 `index >= names.Length` 만 쓰고 `index < 0` 을 빼먹는다.

"인덱스가 음수일 리가 있나?" 싶지만, 실제 프로젝트에서는 `enum` 을 `int` 로 캐스팅해 인덱스로 쓴다.
`(int)someType` 이 음수가 되는 경로가 생기면 그대로 배열에 들어간다. 그래서 원본도 양쪽을 다 막는다.

조건 세 개(`names == null`, `index < 0`, `index >= names.Length`)를 `||` 로 한 번에 묶어도 되고,
`if` 를 나눠 써도 된다. 둘 다 정답.
</details>

> 원본: `Core/DessertTable.cs` 의 `GetSprite()`

---

## 2-2. 누적 점수 계산 ★ 이 단계의 핵심

성공/실패 기록을 순서대로 받아 최종 점수를 계산한다.

- `true` = 성공 → `scorePerSuccess` 만큼 **더한다**
- `false` = 실패 → `penaltyPerFail` 만큼 **뺀다**
- **점수는 매 건마다 0 밑으로 내려가지 않는다**

```csharp
public static int CalculateScore(IReadOnlyList<bool> results, int scorePerSuccess, int penaltyPerFail)
```

아래 예시는 전부 `scorePerSuccess = 100`, `penaltyPerFail = 20` 기준이다.

| 입력 `results` | 출력 |
|---|---|
| `[true, true]` | `200` |
| `[true, false]` | `80` |
| `[false]` | `0` |
| `[false, false, false, true]` | `100` ← |
| `[]` | `0` |
| `null` | `0` |

<details><summary>힌트</summary>

`←` 줄이 답을 가른다. 두 가지로 짤 수 있는데 **결과가 다르다.**

```
방법 A — 매 건마다 0에서 멈춘다
  0 → 실패 0 → 실패 0 → 실패 0 → 성공 100     = 100  ✅

방법 B — 다 더하고 마지막에 한 번만 0으로 자른다
  0 - 20 - 20 - 20 + 100 = 40                  = 40   ❌
```

한 번 0에 닿으면 그 아래로 판 손실은 **사라진다.** 그래서 나중에 성공해도 0에서 다시 시작한다.
방법 B는 "마이너스 구덩이" 를 기억했다가 나중에 메우게 만든다.

**루프 안에서 매번 자르면 된다.** 뺀 직후에 바로.
</details>

> 원본: `Core/Rules/ScoreBoard.cs` 의 `Apply()`

---

## 2-3. 남은 시간 → 표시 문자열

남은 시간(초)을 화면에 쓸 문자열로 바꾼다.

```csharp
public static string ToSecondText(float remainingSeconds)
```

| 입력 | 출력 | 이유 |
|---|---|---|
| `1.2f` | `"2"` | 아직 1초 넘게 남았으니 2 |
| `1.0f` | `"1"` | |
| `0.1f` | `"1"` | 조금이라도 남았으면 1 |
| `0.0f` | `"0"` | |
| `-3.0f` | `"0"` | ← |

<details><summary>힌트</summary>

**올림(ceiling)** 이다. `Math.Ceiling` 을 쓰고 `int` 로 캐스팅한다.

```csharp
int seconds = (int)Math.Ceiling(remainingSeconds);
```

내림이나 반올림을 쓰면 `0.1초` 가 `"0"` 으로 뜬다. 아직 시간이 남았는데 화면에 0이 찍히는 것이다.
타이머는 항상 올림이라고 기억해 두면 된다.

음수는 그 다음 문제다. `Math.Ceiling(-3.0)` 은 `-3` 이라 그대로 `"-3"` 이 나온다.
1단계 1-1의 `ClampScore` 와 똑같이 처리하면 된다.
</details>

> 원본: `Views/HudController.cs` 의 `HandleTimeChanged()`

---

## 2-4. 가장 큰 값의 위치 찾기

정수 목록에서 **가장 큰 값이 있는 인덱스** 를 돌려준다.

- 같은 최댓값이 여러 개면 **가장 앞** 인덱스
- 목록이 비었거나 `null` 이면 `-1`

```csharp
public static int FindMaxIndex(IReadOnlyList<int> values)
```

| 입력 | 출력 |
|---|---|
| `[3, 7, 5]` | `1` |
| `[7, 7, 5]` | `0` |
| `[10]` | `0` |
| `[-5, -2, -9]` | `1` ← |
| `[]` | `-1` |
| `null` | `-1` |

<details><summary>힌트</summary>

`←` 줄은 **전부 음수** 다. 이렇게 짜면 여기서 터진다.

```csharp
int max = 0;              // ← 문제
int bestIndex = -1;
foreach ... if (values[i] > max) { max = values[i]; bestIndex = i; }
```

`-5`, `-2`, `-9` 는 전부 `0` 보다 작아서 `if` 가 한 번도 참이 되지 않는다.
결과는 `-1`. "못 찾았다" 가 나와버린다.

**`0` 은 실제로 들어올 수 있는 값이라 "아직 못 찾았다" 의 표식으로 쓸 수 없다.**
1단계 정리에서 말한 그 원칙이다. 해결은 둘 중 하나.

- 첫 원소(`values[0]`)를 최댓값 초기값으로 잡고 인덱스 1부터 비교한다
- `bool found` 플래그를 따로 둔다

동점일 때 앞을 고르려면 비교를 `>` 로 해야 한다 (`>=` 로 하면 뒤엣것이 이긴다).
</details>

> 원본: `Core/RankTable.cs` 의 `Evaluate()` — 거기서 어려운 부분만 떼어냈다

---

## 2-5. 남은 게 없는가

디저트별 "아직 안 담은 개수" 를 보고, **전부 다 담았는지** 판정한다.

```csharp
public static bool IsAllDone(IReadOnlyDictionary<DessertType, int> remaining)
```

| 입력 | 출력 |
|---|---|
| `{초코:0, 레몬:0}` | `true` |
| `{초코:1, 레몬:0}` | `false` |
| `{초코:2, 레몬:3}` | `false` |
| `{초코:0, 레몬:-1}` | `true` |
| `{}` (빈 딕셔너리) | `true` ← |
| `null` | `true` |

<details><summary>힌트</summary>

**함정 1 — 루프 안에서 `true` 를 반환하면 안 된다.**

```csharp
foreach (int left in remaining.Values)
{
    if (left > 0) return false;
    else return true;      // ← 첫 원소만 보고 끝나버린다
}
```

`{초코:0, 레몬:5}` 를 넣으면 초코(0)에서 바로 `true` 가 나온다. 레몬은 보지도 않는다.

**"모두 ~이다" 를 판정할 때는 반례만 찾고 나온다.** 루프 안에서는 `false` 만 반환하고,
루프를 끝까지 다 돌았다면 그때 `true` 를 반환한다.

**함정 2 — 빈 딕셔너리는 `true` 다.**

"아무것도 없는데 왜 참이지?" 싶지만, 질문이 *"0보다 큰 게 하나라도 있나?"* 이기 때문이다.
빈 목록에는 그런 게 없으니 답은 "없다" = `true`. 루프를 한 번도 안 돌고 마지막 `return true` 로 떨어지면
자연히 이렇게 된다. **따로 처리할 필요가 없다는 게 핵심이다.**

`null` 도 같은 이유로 `true` 다. 맨 앞에서 `if (remaining == null) return true;` 하나만 두면 된다.
</details>

> 원본: `Core/Rules/OrderSession.cs` 의 `IsComplete`

---

## 자가 확인

```csharp
Debug.Log(L2_Guards.GetName(new[]{"A","B","C"}, -1) ?? "null");   // null
Debug.Log(L2_Guards.GetName(null, 0) ?? "null");                  // null

Debug.Log(L2_Guards.CalculateScore(new[]{ false, false, false, true }, 100, 20));  // 100
Debug.Log(L2_Guards.CalculateScore(new[]{ true, false }, 100, 20));                // 80
Debug.Log(L2_Guards.CalculateScore(null, 100, 20));                                // 0

Debug.Log(L2_Guards.ToSecondText(0.1f));      // 1
Debug.Log(L2_Guards.ToSecondText(-3.0f));     // 0

Debug.Log(L2_Guards.FindMaxIndex(new[]{ -5, -2, -9 }));   // 1
Debug.Log(L2_Guards.FindMaxIndex(new[]{ 7, 7, 5 }));      // 0
Debug.Log(L2_Guards.FindMaxIndex(new int[0]));            // -1

var empty = new Dictionary<DessertType, int>();
Debug.Log(L2_Guards.IsAllDone(empty));        // True
Debug.Log(L2_Guards.IsAllDone(null));         // True

var left = new Dictionary<DessertType, int>
{
    { DessertType.ChocoCake, 0 },
    { DessertType.LemonRectangularCake, 5 },
};
Debug.Log(L2_Guards.IsAllDone(left));         // False
```

13줄이 전부 기대값과 같으면 2단계 통과다.

---

## 2단계가 가르치는 것

다섯 문제의 함정을 나란히 놓으면 하나로 모인다.

| 문제 | 함정 | 정체 |
|---|---|---|
| 2-1 | 음수 인덱스 | 범위는 **양쪽** 이 있다 |
| 2-2 | 마지막에 한 번만 자르기 | **언제** 자르냐가 규칙을 바꾼다 |
| 2-3 | 음수 시간 | 계산 결과가 표시 범위를 벗어난다 |
| 2-4 | `int max = 0` | **`0` 은 실제 값이라 "없음" 이 될 수 없다** |
| 2-5 | 빈 컬렉션 | 원소가 0개인 경우를 안 세어봤다 |

전부 **"내가 생각한 정상 범위 밖의 입력"** 이다.
2-4는 1단계 정리에서 본 것과 정확히 같은 문제고, 3단계 이후로도 계속 나온다.

---

*3단계부터 상태를 가진 클래스(필드 + 여러 메서드)로 넘어간다. Unity C# 도 여기서 시작한다.*

---

# 3단계 — 상태를 가진 클래스 5문제

1·2단계는 **입력을 받아 결과를 돌려주는 함수** 였다. 같은 입력이면 언제나 같은 출력이 나온다.

3단계는 다르다. **클래스가 필드를 들고 있고, 메서드가 그걸 고친다.**
그래서 같은 메서드를 같은 인자로 불러도 **몇 번째 호출이냐에 따라 답이 달라진다.**

```
clock.Tick(0.5f);   // false
clock.Tick(0.5f);   // true    ← 인자가 같은데 결과가 다르다
clock.Tick(0.5f);   // false
```

이게 어려워지는 지점이고, 앞으로 나올 모든 버그의 뿌리다.

- 3-1 ~ 3-3 은 **순수 C#**, 3-4 ~ 3-5 는 **Unity C#** (첫 `MonoBehaviour`)
- 클래스당 15~40줄, **함정 2개**
- 제출: `Assets/MyAssets/PRACTICE_Assets/Scripts/Study/Level3/` (스텁 3개 있음)

**이 단계의 한 문장:**

> **"아직 아무 일도 없었다" 는 상태를, 실제로 나올 수 있는 값으로 표현하면 안 된다.**

2단계 2-4(`int max = 0`)에서 본 그 원칙이다. 이번엔 함수 안이 아니라 **필드** 에서 같은 일이 벌어진다.

---

## 3-1. ChangeTracker — 값이 바뀌었을 때만 알려주는 클래스 `[순수 C#]`

**함정 2개** · 15줄

### 문제

같은 값을 계속 넣어도 **바뀐 순간에만** `true` 를 돌려주는 클래스를 만든다.
UI 갱신을 줄일 때 쓴다 — 초 단위 숫자를 매 프레임 다시 그리지 않으려고.

```csharp
public sealed class ChangeTracker
{
    /// 직전 값과 다르면 저장하고 true. 같으면 아무것도 안 하고 false.
    public bool TryUpdate(int value);

    /// "아직 아무 값도 안 받았다" 상태로 되돌린다.
    public void Reset();
}
```

### 호출 순서대로 따라가는 예시

```csharp
var t = new ChangeTracker();

t.TryUpdate(0);    // true    ← 첫 호출은 무조건 true
t.TryUpdate(0);    // false
t.TryUpdate(5);    // true
t.TryUpdate(5);    // false
t.TryUpdate(0);    // true
t.Reset();
t.TryUpdate(0);    // true    ← Reset 직후 첫 호출도 무조건 true
t.TryUpdate(0);    // false
```

<details><summary>함정 (막히면 펼치기)</summary>

**함정 1 — `private int last;` 로 시작하면 안 된다**

`int` 필드는 자동으로 `0` 이 된다. 그 상태에서 `TryUpdate(0)` 을 부르면
"직전 값과 같다" 고 판단해 `false` 를 돌려준다. **첫 호출인데 변화 없음이 나온다.**

`0` 은 실제로 들어올 수 있는 값이라 "아직 없음" 을 나타낼 수 없다. 2단계 2-4와 똑같다.
`bool hasValue` 필드를 하나 더 두면 된다.

```csharp
private int last;
private bool hasValue;   // ← 이 한 개가 핵심
```

**함정 2 — `Reset()` 이 값을 지우는 게 아니다**

`last = 0;` 으로 되돌리면 위와 같은 문제가 다시 생긴다.
되돌려야 하는 건 값이 아니라 **`hasValue = false`** 다.
"값이 0이다" 와 "값이 없다" 는 다른 상태다.
</details>

> 원본: `Views/HudController.cs` 의 `lastDisplayedSecond` — 거기서는 `-1` 을 표식으로 썼다.
> `-1` 도 답이지만, "표시 가능한 초가 절대 음수가 아니다" 라는 도메인 지식에 기댄다.
> `bool` 플래그는 그런 가정 없이 항상 통한다.

---

## 3-2. Countdown — 멈출 수 있는 카운트다운 `[순수 C#]`

**함정 2개** · 30줄

### 문제

제한 시간을 재되, **일시정지할 수 있는** 시계를 만든다.

```csharp
public sealed class Countdown
{
    public float Remaining { get; }
    public bool IsRunning { get; }

    /// 시간을 설정하고 되감는다. 멈춘 상태가 된다.
    public void Reset(float seconds);

    /// 진행/정지를 바꾼다.
    public void SetRunning(bool running);

    /// 시간을 흘린다. 이번 호출로 0이 되면 true.
    public bool Tick(float deltaTime);
}
```

### 규칙

1. `Reset` 은 `Remaining` 을 설정하고 `IsRunning` 을 **`false`** 로 만든다
2. `Reset(-5f)` 처럼 음수를 넣어도 `Remaining` 은 0 밑으로 안 간다
3. **`IsRunning` 이 `false` 면 `Tick` 은 시간을 흘리지 않고 `false` 를 돌려준다**
4. `Tick` 은 **"이번 호출로 방금 0이 되었는가"** 를 돌려준다. 이미 0이었으면 `false`

### 호출 순서대로 따라가는 예시

```csharp
var c = new Countdown();

c.Reset(1f);
// Remaining = 1f, IsRunning = false

c.Tick(0.5f);        // false   ← 멈춰 있으니 시간이 안 간다
// Remaining = 1f                  (0.5f 가 아니다)

c.SetRunning(true);
c.Tick(0.5f);        // false
// Remaining = 0.5f

c.Tick(0.5f);        // true    ← 이번 호출로 0이 됐다
// Remaining = 0f

c.Tick(0.5f);        // false   ← 이미 0이었다
// Remaining = 0f

c.Reset(2f);
// Remaining = 2f, IsRunning = false  ← Reset 하면 다시 멈춘다
```

<details><summary>함정 (막히면 펼치기)</summary>

**함정 1 — `Tick` 이 `Remaining <= 0` 을 그대로 돌려주면 안 된다**

시간이 다 된 뒤에도 `Tick` 은 매 프레임 계속 불린다.
그때마다 `true` 가 나오면 게임오버 처리가 초당 60번 실행된다.

**"이미 0이었으면 아무 일도 안 하고 `false`"** 를 맨 앞에서 걸러야 한다.
그러고 나면 나머지 경로는 "0이 아니었다" 가 보장되므로, 감소 후 `<= 0` 검사가 곧 "방금 0이 됐다" 가 된다.

이걸 **에지 트리거** 라고 부른다. "지금 0이다(레벨)" 가 아니라 "방금 0이 됐다(에지)" 를 알리는 것.

**함정 2 — 검사 순서**

`Tick` 안에서 걸러야 할 게 둘이다. 순서가 있다.

```
① IsRunning 이 false 인가?     → false 반환, 시간 안 흘림
② Remaining 이 이미 0인가?      → false 반환, 시간 안 흘림
③ 여기까지 왔으면 진짜로 흘린다
```

`Remaining` 을 먼저 깎고 나서 `IsRunning` 을 검사하면, 멈춘 상태에서도 시간이 줄어든다.
**"할지 말지" 판단은 전부 "하기" 앞에 모아둔다** — 이 습관이 이후 단계에서 계속 쓰인다.
</details>

> 원본: `Core/Rules/PlayClock.cs` + `Core/GamePlayController.cs` 의 `isRunning`

---

## 3-3. OrderSession — 주문 판정 `[순수 C#]` ★ 이 단계의 핵심

**함정 2개** · 40줄

### 문제

손님 한 명의 주문을 받아, 플레이어가 하나씩 담을 때마다 판정한다.

주문은 디저트 목록이다. 예: `[초코, 초코, 레몬]`
**순서는 상관없지만 개수는 정확해야 한다.** 주문에 없는 걸 담거나, 이미 다 담은 걸 또 담으면 실패다.

```csharp
public enum PickOutcome
{
    Accepted,    // 맞게 담았고, 아직 남았다
    Completed,   // 맞게 담았고, 이걸로 주문이 완성됐다
    Rejected,    // 주문에 없거나, 이미 다 담았다
}

public sealed class OrderSession
{
    public IReadOnlyList<DessertType> Order { get; }   // 이번 주문
    public IReadOnlyList<DessertType> Tray  { get; }   // 지금까지 담은 것
    public bool IsComplete { get; }

    /// 새 주문을 시작한다. 이전 주문의 흔적은 전부 지운다.
    public void Begin(IReadOnlyList<DessertType> newOrder);

    /// 하나 담는다.
    public PickOutcome Pick(DessertType type);
}
```

### 호출 순서대로 따라가는 예시

```csharp
var s = new OrderSession();

s.Begin(new[]{ 초코, 초코, 레몬 });
s.IsComplete;          // false

s.Pick(초코);          // Accepted
s.Pick(레몬);          // Accepted
s.Pick(레몬);          // Rejected   ← 레몬은 1개뿐이었다
s.Pick(키위);          // Rejected   ← 주문에 아예 없다
s.Pick(초코);          // Completed  ← 마지막 한 개
s.IsComplete;          // true
s.Tray.Count;          // 3          ← Rejected 는 담기지 않는다

// 두 번째 손님
s.Begin(new[]{ 키위 });
s.Order.Count;         // 1          ← 이전 주문이 남아 있으면 안 된다
s.Tray.Count;          // 0
s.IsComplete;          // false

s.Begin(null);         // 예외가 나지 않는다
s.IsComplete;          // true       ← 주문이 없으면 담을 것도 없다
```

### 힌트 — 무엇을 필드로 들고 있어야 하나

`Order` 와 `Tray` 만으로는 판정할 수 없다. **"각 종류가 아직 몇 개 남았는가"** 를 따로 들고 있어야 한다.
`Dictionary<DessertType, int>` 다. B1에서 개수를 셌던 것과 같은 방식으로 `Begin` 에서 채운다.

<details><summary>함정 (막히면 펼치기)</summary>

**함정 1 — 깎기 전에 검사한다**

```csharp
// ❌ 이러면 안 된다
remaining[type] = count - 1;
if (remaining[type] <= 0) return PickOutcome.Completed;
```

이미 0개 남은 걸 또 담으면 `-1` 이 되고, `-1 <= 0` 이라 `Completed` 가 나온다.
**`Rejected` 여야 할 게 성공으로 판정된다.**

순서를 뒤집는다. **먼저 검사하고, 통과했을 때만 깎는다.**

```csharp
// ✅
if (남은 개수가 없다) return PickOutcome.Rejected;
남은 개수를 1 깎는다;
Tray 에 추가한다;
return IsComplete ? Completed : Accepted;
```

"주문에 아예 없는 종류" 와 "이미 다 담은 종류" 를 같은 조건으로 묶을 수 있다.
`TryGetValue` 가 실패하거나, 성공했는데 값이 0 이하거나 — 둘 다 `Rejected` 다.

**함정 2 — `Begin` 이 지워야 할 게 3개다**

`Order`, `Tray`, 그리고 개수 딕셔너리. 하나라도 빼먹으면 **두 번째 손님부터** 티가 난다.
첫 손님만 테스트하면 절대 안 걸리는 버그다. 위 예시에 두 번째 `Begin` 이 들어 있는 이유가 이것.

**보너스 — `IsComplete` 는 필드가 아니라 계산이다**

`bool isComplete` 필드를 두고 `Pick` 에서 갱신하면, 갱신을 빼먹는 경로가 반드시 생긴다.
딕셔너리 값들이 전부 0 이하인지 **매번 계산해서** 돌려주는 프로퍼티로 만든다.
2단계 2-5의 `IsAllDone` 이 그대로 들어간다.
</details>

> 원본: `Core/Rules/OrderSession.cs`

---

## 3-4. ShelfButton — 첫 MonoBehaviour `[Unity C#]`

**함정 2개** · 25줄

### 문제

진열대 버튼 하나가 "내가 담당하는 디저트" 를 들고 있게 만든다.
그리고 자기 `Button` 컴포넌트를 **효율적으로** 내어준다.

```csharp
[RequireComponent(typeof(Button))]
public sealed class L3_ShelfButton : MonoBehaviour
{
    [SerializeField] private DessertType type;

    public DessertType Type { get; }

    /// 이 오브젝트의 Button. 몇 번을 읽어도 GetComponent 는 한 번만 부른다.
    public Button Button { get; }

    /// 채점용. GetComponent 를 실제로 호출한 횟수.
    public int GetComponentCallCount { get; private set; }
}
```

### 요구사항

1. `type` 은 **`[SerializeField] private`** — 인스펙터에서 정하고 코드로는 못 바꾼다
2. `Button` 을 100번 읽어도 `GetComponentCallCount` 는 **1**
3. **`Awake()` 가 아직 안 불린 시점에 `Button` 을 읽어도 정상 동작한다**
4. `[RequireComponent]` 를 붙인다

### 검증

빈 GameObject 에 `Button` 과 이 스크립트를 붙이고, 다른 스크립트에서:

```csharp
for (int i = 0; i < 100; i++)
{
    var _ = shelfButton.Button;
}
Debug.Log(shelfButton.GetComponentCallCount);   // 1
```

<details><summary>함정 (막히면 펼치기)</summary>

**함정 1 — 매번 `GetComponent` 를 부르면 안 된다**

```csharp
public Button Button => GetComponent<Button>();   // ❌
```

동작은 한다. 하지만 `GetComponent` 는 컴포넌트 목록을 훑는 작업이라 공짜가 아니다.
매 프레임 도는 코드에서 이러면 프로파일러에 잡힌다. **한 번 찾으면 필드에 저장한다.**

**함정 2 — `Awake()` 에서만 캐싱하면 안 된다**

```csharp
private Button button;
private void Awake() => button = GetComponent<Button>();   // ❌ 위험
public Button Button => button;
```

**Unity 는 오브젝트들의 `Awake()` 실행 순서를 보장하지 않는다.**
다른 스크립트가 자기 `Awake()` 에서 `shelfButton.Button` 을 읽으면,
이 컴포넌트의 `Awake()` 가 아직 안 돌았을 수 있다. 그러면 `null` 이 나간다.

**"쓰이는 순간에 없으면 그때 찾는다"** 로 바꾼다. 이걸 지연 초기화(lazy initialization) 라고 한다.

```csharp
public Button Button
{
    get
    {
        if (button == null)
        {
            button = GetComponent<Button>();
            GetComponentCallCount++;
        }
        return button;
    }
}
```

이러면 언제 읽히든 안전하고, 두 번째 호출부터는 캐시가 나간다.
**실행 순서에 의존하지 않는 코드** 가 되는 것이 핵심이다. 5단계에서 훨씬 사나운 형태로 다시 만난다.
</details>

> 원본: `Views/ShelfButton.cs`

---

## 3-5. TimerLabel — 생명주기와 갱신 최소화 `[Unity C#]`

**함정 2개** · 40줄

### 문제

남은 시간을 초 단위로 표시하는 컴포넌트. `SetTime()` 은 **매 프레임 불린다.**
하지만 글자는 **1초에 한 번만** 바뀌어야 한다.

```csharp
public sealed class L3_TimerLabel : MonoBehaviour
{
    /// 외부에서 매 프레임 호출한다.
    public void SetTime(float remainingSeconds);

    /// 채점용. text 에 실제로 대입한 횟수.
    public int WriteCount { get; private set; }
}
```

### 요구사항

1. `TextMeshProUGUI` 를 `[SerializeField] private` 로 받는다
2. 표시 문자열은 **올림한 정수 초** — 2단계 2-3의 `ToSecondText` 규칙 그대로 (음수는 `"0"`)
3. **표시할 초가 직전과 같으면 `text` 에 대입하지 않는다**
4. **`OnEnable` 에서 캐시를 초기화해, 재활성화 직후 첫 호출은 반드시 대입한다**
5. 참조가 `null` 이면 예외 대신 **경고 로그를 한 번만** 남기고 조용히 무시한다
   (`Debug.LogWarning(name + ": ...", this)` — 두 번째 인자 필수)

**3-1의 `ChangeTracker` 를 그대로 가져다 쓰면 3번과 4번이 거의 공짜로 풀린다.**
이게 순수 C# 클래스를 따로 빼놓는 이유다.

### 검증

```csharp
// ── 테스트 1 : 초가 바뀔 때만 쓰는가
float[] steps = { 3.0f, 2.5f, 2.0f, 1.5f, 1.0f, 0.5f, 0.0f };
//  올림  →       3     3     2     2     1     1     0
//  대입  →       O     X     O     X     O     X     O   = 4회
foreach (float t in steps) label.SetTime(t);
Debug.Log(label.WriteCount);            // 4

// ── 테스트 2 : 재활성화 후 첫 호출은 반드시 쓰는가
int before = label.WriteCount;
label.gameObject.SetActive(false);
label.gameObject.SetActive(true);
label.SetTime(0.0f);                    // 직전과 같은 0초인데도 대입돼야 한다
Debug.Log(label.WriteCount - before);   // 1

// ── 테스트 3 : 참조를 비워두고 100번 호출 → 경고 1줄, 예외 없음
```

<details><summary>함정 (막히면 펼치기)</summary>

**함정 1 — 초기화는 `Awake` 가 아니라 `OnEnable` 에서**

`Awake` 는 오브젝트 생애에 **한 번만** 불린다.
비활성화 → 재활성화(일시정지 후 복귀 등) 하면 옛 캐시가 그대로 남아,
"직전과 같다" 고 판단하고 첫 대입을 건너뛴다. 화면에는 프리팹의 더미 텍스트가 남는다.

`OnEnable` 은 켜질 때마다 불린다. 여기서 `ChangeTracker.Reset()` 을 부르면 된다. 테스트 2가 이걸 잡는다.

**함정 2 — 경고 로그 폭주**

`SetTime` 은 초당 60번 불린다. 그 안에서 매번 `LogWarning` 을 찍으면 콘솔이 마비되고,
정작 중요한 다른 로그가 스크롤에 묻힌다. `bool warned` 플래그로 한 번만 남긴다.

`Debug.LogWarning` 의 두 번째 인자 `this` 는 꼭 넣을 것.
콘솔에서 그 줄을 클릭하면 하이어라키의 해당 오브젝트가 하이라이트된다.
"참조 안 꽂힘" 버그를 찾는 시간이 확 줄어든다.
</details>

> 원본: `Views/HudController.cs`

---

## 3단계가 가르치는 것

다섯 문제의 함정 10개가 두 덩어리로 모인다.

**① "아직 없음" 을 실제 값으로 표현하지 마라**

| 문제 | 잘못된 표현 | 올바른 표현 |
|---|---|---|
| 3-1 | `int last = 0` | `bool hasValue` |
| 3-5 | `Awake` 에서 한 번만 초기화 | `OnEnable` 마다 `Reset()` |

**② 판단은 전부 실행 앞에 모아라**

| 문제 | 잘못된 순서 | 올바른 순서 |
|---|---|---|
| 3-2 | 시간 깎고 → `IsRunning` 검사 | 검사 전부 → 깎기 |
| 3-3 | 개수 깎고 → 0 검사 | 0 검사 → 깎기 |
| 3-4 | `Awake` 에 캐싱 (순서 의존) | 쓰일 때 캐싱 (순서 무관) |

**"되돌릴 수 없는 일을 하기 전에, 해도 되는지 전부 확인한다."**
3-3에서 이 순서를 틀리면 실패가 성공으로 판정되고,
3-4에서 틀리면 `null` 이 새어 나간다. 4·5단계의 버그는 대부분 이 규칙의 변주다.

---

*4단계는 Unity 이벤트 구독(`OnEnable`/`OnDisable` 짝), 여러 컴포넌트 협업으로 넘어간다.*

---

# 4단계 — 이벤트와 구독 4문제

3단계까지는 **내가 만든 클래스를 내가 직접 불렀다.** 호출 시점을 내가 안다.

4단계는 다르다. **누가 언제 부를지 모르는 코드**를 쓴다.
`GameSession` 은 자기 구독자가 몇 명인지, 누구인지 모른 채 이벤트를 쏜다.
`TimerHud` 는 언제 이벤트가 올지 모른 채 기다린다.

여기서 생기는 버그는 3단계까지와 성격이 다르다.
**컴파일도 되고 겉보기에 잘 돌아가는데, 씬을 몇 번 껐다 켜면 조용히 망가진다.**

- 전부 **Unity C#**. 3단계의 `Countdown` · `OrderSession` 을 재사용한다
- 클래스당 30~80줄, **함정 2~3개**
- 제출: `Assets/MyAssets/PRACTICE_Assets/Scripts/Study/Level4/` (스텁 4개 있음)

**이 단계의 한 문장:**

> **`+=` 를 쓴 순간 `-=` 를 어디에 쓸지 정해야 한다.**

---

## 4-1. GameSession — 이벤트를 발행하는 쪽

**함정 3개** · 60줄

### 문제

제한 시간을 재고, **상태가 바뀔 때마다 바깥에 알리는** 컴포넌트를 만든다.
이 클래스는 **UI를 전혀 모른다.** `text` 도 `Image` 도 건드리지 않는다.

```csharp
public sealed class L4_GameSession : MonoBehaviour
{
    [SerializeField] private float playTimeSeconds = 10f;

    private readonly Countdown clock = new();      // 3-2 재사용

    public float Remaining { get; }
    public float PlayTime  { get; }
    public bool  IsRunning { get; }

    /// (남은 시간, 전체 시간)
    public event Action<float, float> OnTimeChanged;

    /// 진행/정지가 바뀐 순간
    public event Action<bool> OnRunningChanged;

    /// 시간이 다 된 순간
    public event Action OnFinished;

    public void StartGame();                 // 시계를 되감고 시작
    public void SetRunning(bool running);
    public void Tick(float deltaTime);       // Update 가 부르지만, 테스트도 직접 부른다
}
```

### 규칙

1. `Update()` 는 `Tick(Time.deltaTime)` 만 부른다. 실제 로직은 `Tick` 에
2. `Tick` 은 **시간이 실제로 흐른 프레임에만** `OnTimeChanged` 를 쏜다 (멈춰 있으면 안 쏨)
3. **`SetRunning` 은 값이 실제로 바뀔 때만** `OnRunningChanged` 를 쏜다
4. `OnFinished` 는 시간이 다 된 순간 **정확히 한 번**. 그 뒤로 `Tick` 을 아무리 불러도 다시 안 쏜다
5. 시간이 다 되면 `IsRunning` 이 `false` 가 된다

### 검증

```csharp
int timeCount = 0, runningCount = 0, finishedCount = 0;

session.OnTimeChanged    += (r, t) => timeCount++;
session.OnRunningChanged += _      => runningCount++;
session.OnFinished       += ()     => finishedCount++;

// ── 같은 값으로 SetRunning 을 반복해도 이벤트가 안 나가야 한다
session.SetRunning(false);
session.SetRunning(false);
Debug.Log(runningCount);              // 0

session.StartGame();                  // playTimeSeconds = 10f 라고 하자
Debug.Log(runningCount);              // 1
Debug.Log(session.IsRunning);         // True

// ── 시간 만료는 한 번만
session.Tick(6f);
Debug.Log(finishedCount);             // 0
session.Tick(6f);                     // 여기서 만료
Debug.Log(finishedCount);             // 1
Debug.Log(session.IsRunning);         // False
Debug.Log(runningCount);              // 2   ← 만료로 정지된 것도 상태 변화다

session.Tick(6f);
session.Tick(6f);
Debug.Log(finishedCount);             // 1   ← 계속 1
```

<details><summary>함정</summary>

**함정 1 — `public Action` 이 아니라 `event Action` 이다**

```csharp
public Action<float, float> OnTimeChanged;    // ❌
public event Action<float, float> OnTimeChanged;   // ✅
```

`event` 키워드가 있으면 **바깥에서는 `+=` 와 `-=` 만** 할 수 있다.
없으면 남이 `session.OnTimeChanged = null;` 로 **모든 구독자를 한 번에 날려버릴 수 있고**,
`session.OnTimeChanged(1, 2);` 로 남의 이벤트를 대신 쏠 수도 있다.

**함정 2 — 구독자가 0명이면 `null` 이다**

```csharp
OnTimeChanged(remaining, playTime);      // ❌ 구독자 없으면 NullReferenceException
OnTimeChanged?.Invoke(remaining, playTime);   // ✅
```

`Action` 은 델리게이트라서 아무도 `+=` 하지 않았으면 그냥 `null` 이다.
**모든 이벤트 발행은 `?.Invoke(...)` 로 한다.** 예외 없다.

**함정 3 — "값이 바뀔 때만" 을 지켜야 한다**

```csharp
public void SetRunning(bool running)
{
    if (IsRunning == running) return;     // ← 이 줄이 없으면
    ...
    OnRunningChanged?.Invoke(running);
}
```

이 가드가 없으면 일시정지 화면을 열 때마다, 판정이 끝날 때마다 **같은 값으로 이벤트가 중복 발행**된다.
구독자 쪽에서는 "바뀌었다" 는 신호를 받고 갱신 작업을 하는데, 실제로는 안 바뀌었으니 낭비다.
더 나쁜 건 구독자가 이 신호로 애니메이션을 재생하는 경우 — 같은 연출이 계속 다시 시작된다.

`OnFinished` 를 한 번만 쏘는 건 **3-2의 에지 트리거가 이미 해준다.**
`clock.Tick()` 이 `true` 를 돌려주는 건 만료된 그 한 번뿐이니, 그 반환값을 그대로 쓰면 된다.
</details>

> 원본: `Core/GamePlayController.cs`

---

## 4-2. TimerHud — 구독하는 쪽 ★ 이 단계의 핵심

**함정 3개** · 50줄

### 문제

`L4_GameSession` 의 이벤트를 구독해 남은 시간을 표시하는 HUD.
**`Update()` 를 만들지 않는다.** 이벤트가 올 때만 일한다.

```csharp
public sealed class L4_TimerHud : MonoBehaviour
{
    [SerializeField] private L4_GameSession session;
    [SerializeField] private TextMeshProUGUI label;

    /// 채점용. 화면을 실제로 갱신한 횟수.
    public int RefreshCount { get; private set; }
}
```

### 규칙

1. `OnEnable` 에서 `OnTimeChanged` 를 구독하고, **`OnDisable` 에서 반드시 해제**한다
2. **구독 직후 현재 값으로 한 번 즉시 갱신**한다
3. 갱신할 때마다 `RefreshCount` 를 1 올린다
4. `session` 이나 `label` 이 `null` 이면 경고 한 번 남기고 조용히 넘어간다

> 표시 형식은 자유다. 3-5의 `L3_TimerLabel` 을 가져다 쓰거나, `label.text` 에 직접 써도 된다.

### 검증 — 여기가 진짜다

```csharp
// ── 테스트 A : 껐다 켜도 구독이 쌓이지 않는가
for (int i = 0; i < 3; i++)
{
    hud.gameObject.SetActive(false);
    hud.gameObject.SetActive(true);
}

int before = hud.RefreshCount;
session.Tick(0.1f);                        // OnTimeChanged 를 딱 1번 발행
Debug.Log(hud.RefreshCount - before);      // 1   ← 4가 나오면 구독이 4겹 쌓인 것

// ── 테스트 B : 켜지는 순간 즉시 동기화하는가
hud.gameObject.SetActive(false);
int before2 = hud.RefreshCount;
hud.gameObject.SetActive(true);            // 이벤트는 아직 안 왔다
Debug.Log(hud.RefreshCount - before2);     // 1   ← OnEnable 에서 스스로 한 번 갱신
```

<details><summary>함정</summary>

**함정 1 — 해제를 빼먹으면 구독이 겹쳐 쌓인다 (테스트 A)**

```csharp
private void OnEnable() => session.OnTimeChanged += HandleTime;
// OnDisable 없음                                             ❌
```

`OnEnable` 은 켜질 때마다 불린다. 해제를 안 하면 **켤 때마다 같은 핸들러가 하나씩 더 등록**된다.
3번 껐다 켜면 구독자가 4명이 되고, 이벤트 한 번에 핸들러가 4번 실행된다.

증상이 고약하다. **처음엔 멀쩡하다가 씬을 오갈수록 점점 느려진다.**
그리고 오브젝트를 파괴해도 `GameSession` 이 죽은 핸들러를 계속 붙들고 있어서
`MissingReferenceException` 이 뜬다 — 게다가 그 예외는 `GameSession` 쪽 스택에서 터지므로
원인이 여기 있다는 걸 알아채기 어렵다.

**`+=` 를 쓰는 순간 `-=` 를 먼저 적는 습관을 들일 것.**

**함정 2 — 구독만으로는 화면이 안 채워진다 (테스트 B)**

이벤트는 **"값이 바뀔 때"** 온다. HUD가 켜진 순간부터 첫 이벤트가 도착할 때까지는
프리팹에 저장된 더미 텍스트(`"999"` 같은)가 그대로 보인다.

게임이 멈춰 있으면 `OnTimeChanged` 가 아예 안 오니 **영원히** 더미가 보인다.
일시정지 후 복귀했을 때 특히 티가 난다.

→ `OnEnable` 끝에서 핸들러를 **직접 한 번 호출**한다.

```csharp
session.OnTimeChanged += HandleTime;
HandleTime(session.Remaining, session.PlayTime);   // ← 즉시 동기화
```

`GameSession` 이 `Remaining` / `PlayTime` 프로퍼티를 공개하는 이유가 이것이다.
**이벤트는 변화를 알리고, 프로퍼티는 현재를 알려준다. 둘 다 필요하다.**

**함정 3 — `OnDisable` 에서도 `null` 검사**

`OnDisable` 은 씬이 닫힐 때도 불린다. 그 시점엔 `session` 이 먼저 파괴됐을 수 있다.
`if (session != null)` 로 감싸지 않으면 씬 전환 때마다 예외가 뜬다.
</details>

> 원본: `Views/HudController.cs` 의 `OnEnable` / `OnDisable`

---

## 4-3. ShelfView — 버튼 배열과 조합 조건

**함정 3개** · 60줄

### 문제

디저트 버튼 여러 개를 배열로 받아, 각 버튼이 **자기가 담당하는 디저트**를 알리게 한다.
그리고 두 군데서 오는 신호를 **조합해서** 버튼을 잠근다.

```csharp
public sealed class L4_ShelfView : MonoBehaviour
{
    [SerializeField] private L4_GameSession session;
    [SerializeField] private L4_OrderRunner runner;
    [SerializeField] private L3_ShelfButton[] shelfButtons;   // 3-4 재사용

    /// 마지막으로 눌린 디저트. 채점용.
    public DessertType LastPicked { get; private set; }
}
```

### 규칙

1. 각 버튼의 `onClick` 에 **런타임으로** 리스너를 건다. 눌리면 `LastPicked` 에 그 버튼의 `Type` 을 넣는다
2. 리스너 등록은 **`Start` 에서 한 번만** 한다 (`OnEnable` 에 두면 켤 때마다 중복 등록된다)
3. **잠금 조건: `session.IsRunning && !runner.IsLocked` 일 때만 버튼이 눌린다**
   - `session.OnRunningChanged` 와 `runner.OnLockChanged` 를 **둘 다** 구독한다
4. 배열에 `null` 칸이 있어도 터지지 않는다

### 검증

```csharp
// ── 테스트 A : 각 버튼이 자기 디저트를 알리는가
shelfButtons[2].Button.onClick.Invoke();
Debug.Log(shelfView.LastPicked);      // shelfButtons[2].Type 과 같아야 한다
shelfButtons[0].Button.onClick.Invoke();
Debug.Log(shelfView.LastPicked);      // shelfButtons[0].Type

// ── 테스트 B : 두 조건의 조합
session.SetRunning(true);
runner.SetLocked(false);
Debug.Log(shelfButtons[0].Button.interactable);   // True

runner.SetLocked(true);                            // 판정 연출 중
Debug.Log(shelfButtons[0].Button.interactable);   // False

runner.SetLocked(false);
session.SetRunning(false);                         // 일시정지
Debug.Log(shelfButtons[0].Button.interactable);   // False
```

<details><summary>함정</summary>

**함정 1 — `for` 루프 인덱스 캡처 (테스트 A)**

```csharp
for (int i = 0; i < shelfButtons.Length; i++)
{
    shelfButtons[i].Button.onClick.AddListener(() => LastPicked = shelfButtons[i].Type);   // ❌
}
```

람다가 **`i` 라는 변수 자체**를 붙든다. 값이 아니라 변수다.
루프가 끝나면 `i` 는 `Length` 가 되어 있고, 나중에 버튼을 누르면 `shelfButtons[5]` 를 읽어
`IndexOutOfRangeException` 이 난다.

`foreach` 는 C# 5부터 반복마다 변수를 새로 만들어서 이 버그가 없다. 하지만 `for` 는 **여전히** 있다.

```csharp
foreach (L3_ShelfButton shelfButton in shelfButtons)
{
    L3_ShelfButton captured = shelfButton;    // 의도를 명시
    captured.Button.onClick.AddListener(() => LastPicked = captured.Type);
}
```

`foreach` 면 복사가 필요 없지만, 원본 코드가 굳이 한 줄 더 쓴 이유는
**"이 변수는 캡처된다" 를 코드에 남겨두기 위해서**다. 나중에 누가 `for` 로 바꾸면 즉시 되살아나는 버그니까.

**함정 2 — 두 신호를 각자 반영하면 안 된다 (테스트 B)**

```csharp
private void HandleRunning(bool running) => SetInteractable(running);          // ❌
private void HandleLock(bool locked)     => SetInteractable(!locked);          // ❌
```

이러면 **나중에 온 신호가 앞의 것을 덮어쓴다.**
일시정지(`running=false`)로 잠근 뒤 판정이 끝나(`locked=false`) 신호가 오면 버튼이 풀려버린다.

**두 핸들러가 같은 메서드를 부르고, 그 메서드가 두 값을 다시 읽어서 조합해야 한다.**

```csharp
private void HandleAnyChanged(bool _) => ApplyLock();

private void ApplyLock()
{
    bool interactable = session.IsRunning && !runner.IsLocked;   // 매번 둘 다 읽는다
    ...
}
```

이벤트 인자를 버리고(`bool _`) 프로퍼티에서 현재 값을 다시 읽는 게 핵심이다.
**이벤트는 "뭔가 바뀌었다" 는 알림일 뿐, 판단 재료는 현재 상태에서 가져온다.**

**함정 3 — 등록은 `Start`, 구독은 `OnEnable`**

`onClick.AddListener` 를 `OnEnable` 에 두면 켤 때마다 리스너가 쌓여 한 번 클릭에 여러 번 실행된다.
버튼 리스너는 생애에 한 번(`Start`)만 걸고, **이벤트 구독만** `OnEnable`/`OnDisable` 짝으로 관리한다.

둘의 수명이 다르다는 걸 구분하는 게 이 함정의 핵심이다.
</details>

> 원본: `Views/ShelfView.cs`

---

## 4-4. OrderRunner — 사건과 상태를 구분해서 알리기

**함정 2개** · 70줄

### 문제

3단계의 `OrderSession` 을 감싸서, **두 가지 성격의 이벤트**를 발행한다.

```csharp
public sealed class L4_OrderRunner : MonoBehaviour
{
    private readonly OrderSession session = new();     // 3-3 재사용

    public IReadOnlyList<DessertType> Order { get; }
    public IReadOnlyList<DessertType> Tray  { get; }
    public bool IsLocked { get; }

    /// 【사건】 한 번 담을 때마다. 토스트를 띄우는 쪽이 듣는다.
    public event Action<PickOutcome> OnPicked;

    /// 【상태】 쟁반 내용이 실제로 바뀌었을 때만. 아이콘을 다시 그리는 쪽이 듣는다.
    public event Action<IReadOnlyList<DessertType>> OnTrayChanged;

    /// 【상태】 새 주문이 시작됐을 때.
    public event Action<IReadOnlyList<DessertType>> OnOrderChanged;

    /// 【상태】 입력 잠금이 바뀌었을 때. 4-3 이 듣는다.
    public event Action<bool> OnLockChanged;

    public void Begin(IReadOnlyList<DessertType> newOrder);
    public void Pick(DessertType type);
    public void SetLocked(bool locked);
}
```

### 규칙

1. `Begin` 은 `OnOrderChanged` 와 `OnTrayChanged` 를 각각 한 번 쏜다
2. `Pick` 은 **결과와 무관하게** `OnPicked` 를 한 번 쏜다 (사건은 일어났으니까)
3. **`OnTrayChanged` 는 쟁반이 실제로 바뀌었을 때만 쏜다.** `Rejected` 면 안 쏜다
4. `IsLocked` 가 `true` 면 `Pick` 은 **아무것도 하지 않는다** (이벤트도 안 쏨)
5. `SetLocked` 는 값이 바뀔 때만 `OnLockChanged` 를 쏜다

### 검증

```csharp
int picked = 0, trayChanged = 0, orderChanged = 0;

runner.OnPicked       += _ => picked++;
runner.OnTrayChanged  += _ => trayChanged++;
runner.OnOrderChanged += _ => orderChanged++;

runner.Begin(new[]{ 초코, 초코, 레몬 });
Debug.Log($"{orderChanged} {trayChanged}");    // 1 1

int t = trayChanged;
runner.Pick(키위);                              // Rejected
Debug.Log(picked);                              // 1   ← 사건은 일어났다
Debug.Log(trayChanged - t);                     // 0   ← 쟁반은 안 바뀌었다

runner.Pick(초코);                              // Accepted
Debug.Log(picked);                              // 2
Debug.Log(trayChanged - t);                     // 1

// ── 잠금 중에는 아무 일도 없어야 한다
runner.SetLocked(true);
int p = picked;
runner.Pick(초코);
Debug.Log(picked - p);                          // 0
```

<details><summary>함정</summary>

**함정 1 — 사건과 상태를 한 이벤트에 섞지 마라**

`OnPicked` 와 `OnTrayChanged` 를 하나로 합치고 싶어진다. 둘 다 `Pick` 에서 나가니까.
하지만 **구독자가 다르다.**

| 이벤트 | 성격 | 듣는 쪽 | 하는 일 |
|---|---|---|---|
| `OnPicked(결과)` | **사건** — 지금 일어난 일 | 토스트 | "성공!" / "실패.." 를 0.5초 띄운다 |
| `OnTrayChanged(목록)` | **상태** — 지금 어떤 모습인지 | 쟁반 아이콘 | 목록대로 아이콘을 다시 그린다 |

`Rejected` 일 때 토스트는 **떠야 하고**, 쟁반은 **다시 그릴 필요가 없다.**
합쳐두면 쟁반이 매번 헛되이 다시 그려지거나, 토스트가 안 뜬다. 둘 중 하나는 반드시 손해다.

구분하는 기준: **"이건 지금 벌어진 일인가(사건), 아니면 지금의 모습인가(상태)?"**
사건은 놓치면 안 되고, 상태는 마지막 것만 맞으면 된다.

**함정 2 — 잠금 검사는 맨 앞에**

```csharp
public void Pick(DessertType type)
{
    if (IsLocked) return;      // ← 맨 앞. 이벤트도 안 나간다
    ...
}
```

`session.Pick()` 을 먼저 부르고 나서 잠금을 검사하면, 이미 쟁반이 바뀐 뒤라 되돌릴 수 없다.
3-2·3-3에서 계속 나온 **"판단은 전부 실행 앞에"** 가 여기서도 그대로다.
</details>

> 원본: `Core/GamePlayController.cs` 의 `OnJudged` / `OnTrayChanged` / `OnJudgingChanged`

---

## 4단계가 가르치는 것

**① 구독에는 수명이 있다**

| | 언제 | 짝 |
|---|---|---|
| `onClick.AddListener` | `Start` — 생애 한 번 | (보통 불필요) |
| `event +=` | `OnEnable` — 켜질 때마다 | **`OnDisable` 에서 `-=`** |

수명이 다른 걸 같은 자리에 두면 4-3의 함정 3이 된다.

**② 이벤트는 알림, 판단은 현재 상태에서**

4-3에서 `bool _` 로 인자를 버리고 프로퍼티를 다시 읽은 게 이것이다.
이벤트 인자만 믿으면 **여러 신호가 서로를 덮어쓴다.**

**③ 사건과 상태는 다른 이벤트다**

4-4의 `OnPicked` vs `OnTrayChanged`.
"놓치면 안 되는 것" 과 "마지막만 맞으면 되는 것" 은 성격이 다르다.

---

*5단계는 비동기 연출과 취소다. "0.8초 뒤에 깨어난 코드가 이미 사라진 세상을 건드리는" 문제를 다룬다.*

---

# 5단계 — 비동기와 취소 4문제

4단계까지의 버그는 전부 **"코드가 지금 틀렸다"** 였다. 읽으면 보이고, 돌리면 바로 드러난다.

5단계는 다르다.

> **0.8초 뒤에 깨어난 코드가, 이미 사라진 세상을 건드린다.**

`await` 앞뒤로 시간이 흐른다. 그 사이에 플레이어가 화면을 나갔을 수도, 오브젝트가 파괴됐을 수도,
새 게임이 시작됐을 수도 있다. **깨어난 코드는 그걸 모른 채 하던 일을 계속한다.**

- 전부 **Unity C#**. `Awaitable`(Unity 6)을 쓴다. 코루틴은 안 쓴다
- 클래스당 40~80줄
- 제출: `Assets/MyAssets/PRACTICE_Assets/Scripts/Study/Level5/` (스텁 4개 있음)

## 시작 전에 — 알아야 할 도구 3개

**① `Awaitable`** — Unity 6의 대기 도구. 코루틴을 대체한다.

```csharp
await Awaitable.NextFrameAsync();          // 한 프레임 기다린다
await Awaitable.WaitForSecondsAsync(0.5f); // 0.5초 기다린다
```

메서드에 `async` 를 붙이고 반환형을 `Awaitable` 로 하면, 호출부에서 `await` 할 수 있다.

```csharp
public async Awaitable FadeIn() { ... }     // 만드는 쪽
await fader.FadeIn();                        // 쓰는 쪽
```

**② `destroyCancellationToken`** — `MonoBehaviour` 에 내장된 프로퍼티.
그 오브젝트가 파괴되면 자동으로 취소 신호를 보낸다.

```csharp
await Awaitable.WaitForSecondsAsync(0.5f, destroyCancellationToken);
```

이걸 넘기면, 오브젝트가 파괴됐을 때 대기가 **`OperationCanceledException` 을 던지며 끝난다.**
안 넘기면 파괴된 뒤에도 코드가 깨어나 죽은 컴포넌트를 건드린다.

**③ `catch (OperationCanceledException)` 은 에러 처리가 아니다**

```csharp
try
{
    await Awaitable.WaitForSecondsAsync(0.5f, token);
}
catch (OperationCanceledException)
{
    return;              // 로그 찍지 말 것. 이건 정상 종료다.
}
```

취소는 **기대된 정상 흐름**이다. "화면을 떠났으니 하던 일을 그만둔다" 는 성공적인 동작이지 실패가 아니다.
여기에 `Debug.LogError` 를 넣으면 정상 동작할 때마다 빨간 줄이 뜬다.

---

## 5-1. Fader — Awaitable 기초

**함정 2개** · 40줄

### 문제

`CanvasGroup` 의 알파를 부드럽게 바꾸는 페이드 컴포넌트. **코루틴이 아니라 `Awaitable`** 로 만든다.

```csharp
[RequireComponent(typeof(CanvasGroup))]
public sealed class L5_Fader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;

    public float Alpha { get; }          // 채점용
    public bool IsFading { get; private set; }

    public async Awaitable FadeIn();     // 0 → 1
    public async Awaitable FadeOut();    // 1 → 0
}
```

### 규칙

1. 호출부에서 `await fader.FadeIn();` 으로 끝날 때까지 기다릴 수 있어야 한다
2. **`Time.timeScale == 0` 에서도 페이드가 진행된다**
3. 페이드 도중 오브젝트가 파괴돼도 예외가 콘솔에 뜨지 않는다
4. `FadeOut` 이 끝나면 `gameObject` 를 꺼서 레이캐스트를 막지 않게 한다

### 검증

```csharp
private async void Start()
{
    // ── 테스트 A : 일시정지 중에도 끝나는가
    Time.timeScale = 0f;
    await fader.FadeIn();
    Debug.Log("[A] 완료");             // 이 줄에 도달하면 통과. 안 뜨면 영원히 대기 중
    Time.timeScale = 1f;

    // ── 테스트 B : 파괴돼도 안전한가
    L5_Fader temp = Instantiate(faderPrefab);
    _ = temp.FadeIn();                  // await 하지 않고 시작만
    Destroy(temp.gameObject);           // 즉시 파괴
    await Awaitable.WaitForSecondsAsync(1f);
    Debug.Log("[B] 완료");             // 콘솔에 MissingReferenceException 이 없어야 통과
}
```

<details><summary>함정</summary>

**함정 1 — `Time.deltaTime` 을 쓰면 일시정지에서 영원히 안 끝난다**

일시정지를 `Time.timeScale = 0` 으로 구현하면 `Time.deltaTime` 이 **0** 이 된다.
경과 시간이 안 쌓이니 `while (elapsed < duration)` 이 무한 루프가 되고,
`await` 하던 호출부는 영원히 다음 줄로 못 간다. **화면이 검은 채로 굳는다.**

→ **`Time.unscaledDeltaTime`** 을 쓴다. UI 연출은 거의 항상 unscaled 다.

**함정 2 — 토큰 없는 `await` 는 파괴 후에도 깨어난다**

```csharp
await Awaitable.NextFrameAsync();               // ❌
await Awaitable.NextFrameAsync(destroyCancellationToken);   // ✅
```

토큰을 안 넘기면, 오브젝트가 파괴된 다음 프레임에도 코드가 깨어나
`canvasGroup.alpha = ...` 를 실행한다. `canvasGroup` 은 이미 파괴됐으니 `MissingReferenceException`.

**씬 전환 중에 터지기 때문에 원인 추적이 어렵다.** 예외 스택이 이 파일을 가리키긴 하는데,
"왜 파괴된 뒤에 실행됐지?" 를 이해하려면 async 의 동작을 알아야 한다.

구조는 이렇게 된다.

```csharp
private async Awaitable Fade(float from, float to, float duration)
{
    canvasGroup.alpha = from;

    float elapsed = 0f;
    while (elapsed < duration)
    {
        await Awaitable.NextFrameAsync(destroyCancellationToken);
        elapsed += Time.unscaledDeltaTime;
        canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
    }

    canvasGroup.alpha = to;
}
```
</details>

> 원본: `Screens/ScreenFade.cs`

---

## 5-2. Toast — CTS 재생성 패턴 ★

**함정 3개** · 60줄

### 문제

판정 결과를 0.5초 띄웠다 지우는 토스트. 4-4의 `OnPicked` 를 구독한다.

```csharp
public sealed class L5_Toast : MonoBehaviour
{
    [SerializeField] private L4_OrderRunner runner;
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float showSeconds = 0.5f;

    public bool IsVisible { get; }        // 채점용
    public int HideCount { get; private set; }
}
```

### 규칙

1. `OnEnable` 에서 `runner.OnPicked` 구독, `OnDisable` 에서 해제 (4단계 그대로)
2. `Rejected` 면 "실패..", 아니면 "성공!" 을 띄운다
3. 0.5초 뒤 자동으로 꺼지고 `HideCount` 가 1 오른다
4. **토스트가 떠 있는 동안 새 토스트가 오면, 새 토스트가 0.5초를 온전히 채운다**
5. `OnDisable` · `OnDestroy` 에서 대기를 취소한다

### 검증

```csharp
private async void Start()
{
    runner.Begin(new[]{ DessertType.ChocoCake });

    runner.Pick(DessertType.KiwiBigCake);        // Rejected → 토스트 1
    await Awaitable.WaitForSecondsAsync(0.3f);

    runner.Pick(DessertType.KiwiBigCake);        // Rejected → 토스트 2 (덮어씀)
    await Awaitable.WaitForSecondsAsync(0.3f);

    Debug.Log(toast.IsVisible);   // True  ← 첫 타이머가 껐으면 False
    Debug.Log(toast.HideCount);   // 0     ← 1이면 첫 타이머가 살아 있었다는 뜻

    await Awaitable.WaitForSecondsAsync(0.3f);
    Debug.Log(toast.IsVisible);   // False
    Debug.Log(toast.HideCount);   // 1
}
```

<details><summary>함정</summary>

**함정 1 — `destroyCancellationToken` 만으로는 부족하다**

이게 이 문제의 전부다.

> 첫 토스트가 0.3초 지났을 때 두 번째 토스트가 뜬다.
> **첫 번째의 타이머가 0.2초 뒤에 깨어나 두 번째 토스트를 꺼버린다.**

`destroyCancellationToken` 은 **"오브젝트가 죽을 때"** 만 취소된다.
"새 토스트가 왔을 때" 는 아무도 안 알려준다. 그 취소는 **직접 만들어야** 한다.

```csharp
private CancellationTokenSource hideCts;

private async void RunHide()
{
    CancelHide();                                   // ① 이전 대기를 반드시 끊는다
    hideCts = CancellationTokenSource
        .CreateLinkedTokenSource(destroyCancellationToken);   // ② 파괴도 함께 감시

    try
    {
        await Awaitable.WaitForSecondsAsync(showSeconds, hideCts.Token);
    }
    catch (OperationCanceledException)
    {
        return;                                     // ③ 새 토스트가 덮었거나 화면을 떠났다
    }

    SetVisible(false);
    HideCount++;
}

private void CancelHide()
{
    if (hideCts == null) return;

    hideCts.Cancel();
    hideCts.Dispose();      // CTS 는 IDisposable 이다. 반드시 버린다
    hideCts = null;         // 다음 CancelHide 가 죽은 CTS 를 건드리지 않게
}
```

**함정 2 — `Cancel()` 만 하고 `Dispose()` 를 빼먹는다**

`CancellationTokenSource` 는 `IDisposable` 이다. 내부에 타이머 핸들 등을 들고 있어서
버리지 않으면 누적된다. **`Cancel` → `Dispose` → `null` 세 줄이 한 세트다.**

`null` 대입도 중요하다. 안 하면 다음 호출에서 이미 `Dispose` 된 CTS 에 `Cancel()` 을 불러
`ObjectDisposedException` 이 난다.

**함정 3 — `OnDisable` 과 `OnDestroy` 둘 다에서 취소**

`OnDisable` 은 화면을 떠날 때, `OnDestroy` 는 파괴될 때 불린다. **다른 사건이고 둘 다 막아야 한다.**
`OnDisable` 에서는 토스트를 끄기도 해야 한다 — 다시 켰을 때 옛 토스트가 남아 있으면 안 되니까.
</details>

> 원본: `Views/ToastController.cs`

---

## 5-3. CountdownOverlay — 연결된 토큰

**함정 2개** · 50줄

### 문제

`3 → 2 → 1 → Start!` 를 0.7초 간격으로 보여주는 오버레이.
게임 종료 시엔 `Timeout!!` 을 1초 보여준다. **두 연출을 같은 내부 메서드로 처리한다.**

```csharp
public sealed class L5_CountdownOverlay : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private string[] introSteps = { "3", "2", "1", "Start!" };
    [SerializeField] private float introStepSeconds = 0.7f;
    [SerializeField] private float timeoutSeconds = 1f;

    public bool IsPanelActive { get; }        // 채점용

    public async Awaitable PlayIntro(CancellationToken token);
    public async Awaitable PlayTimeout(CancellationToken token);
}
```

### 규칙

1. **호출자가 넘긴 토큰**과 **자기 `destroyCancellationToken`** 을 **둘 다** 감시한다
2. 인트로는 끝나면 패널을 **끈다**
3. 타임아웃은 끝나도 패널을 **켠 채로 둔다** (그 위로 페이드가 덮이므로)
4. **중간에 취소되어도 패널이 켜진 채 남지 않는다**

### 검증

```csharp
private async void Start()
{
    // ── 테스트 A : 정상 재생
    await overlay.PlayIntro(CancellationToken.None);
    Debug.Log(overlay.IsPanelActive);    // False  ← 인트로는 끝나면 끈다

    // ── 테스트 B : 중간 취소
    var cts = new CancellationTokenSource();
    Awaitable playing = overlay.PlayIntro(cts.Token);

    await Awaitable.WaitForSecondsAsync(1f);   // "2" 쯤 재생 중
    cts.Cancel();

    try { await playing; } catch (OperationCanceledException) { }

    Debug.Log(overlay.IsPanelActive);    // False  ← 취소돼도 패널이 남으면 안 된다
    cts.Dispose();
}
```

<details><summary>함정</summary>

**함정 1 — 토큰이 둘인데 `await` 는 하나만 받는다**

호출자의 토큰은 **"화면을 떠났다"** 를 알린다.
자기 `destroyCancellationToken` 은 **"내가 파괴됐다"** 를 알린다.
**둘은 서로 다른 사건이고, 어느 쪽이든 대기를 끊어야 한다.**

`await` 에는 토큰을 하나만 넘길 수 있으니, **두 개를 하나로 합친다.**

```csharp
using CancellationTokenSource linked =
    CancellationTokenSource.CreateLinkedTokenSource(token, destroyCancellationToken);

await Awaitable.WaitForSecondsAsync(seconds, linked.Token);
```

`using` **선언문**(중괄호 없는 형태)을 쓰면 메서드가 끝날 때 자동으로 `Dispose` 된다.
5-2처럼 필드로 들고 있을 필요가 없는 경우엔 이게 가장 간단하다.

**함정 2 — 취소돼도 패널은 정리돼야 한다**

취소되면 `await` 지점에서 `OperationCanceledException` 이 던져지고, **그 아래 줄은 실행되지 않는다.**
패널을 끄는 코드가 그 아래에 있으면 영영 안 불리고, **검은 패널이 화면에 남는다.**

```csharp
SetPanelActive(true);

try
{
    foreach (string step in steps)
    {
        text.text = step;
        await Awaitable.WaitForSecondsAsync(secondsPerStep, linked.Token);
    }
}
finally
{
    if (hideWhenDone) SetPanelActive(false);   // 예외가 나가도 반드시 실행된다
}
```

`finally` 는 예외가 통과해 나가도 실행된다. **"어떻게 끝나든 반드시 해야 하는 정리"** 는 여기에 넣는다.

두 연출의 차이(`인트로는 끄고, 타임아웃은 켠 채로 둔다`)는 `bool hideWhenDone` 파라미터
하나로 흡수하면 메서드 하나로 합칠 수 있다.
</details>

> 원본: `Views/CountdownView.cs`

---

## 5-4. JudgeRunner — 세대 토큰 ★★ 이 문서에서 가장 어려운 문제

**함정 3개** · 70줄

### 문제

판정이 나면 **0.8초 동안 입력을 잠그고**, 그 뒤 다음 손님으로 넘어간다.
연타로 실패가 줄줄이 나는 걸 막고, 플레이어가 무엇을 잘못 눌렀는지 볼 시간을 준다.

```csharp
public sealed class L5_JudgeRunner : MonoBehaviour
{
    [SerializeField] private float judgeDelaySeconds = 0.8f;

    public int CustomerNumber { get; private set; }
    public bool IsJudging { get; private set; }

    /// 새 판을 시작한다. (다시하기)
    public void Prepare();

    /// 판정이 났다. 잠그고 0.8초 뒤 다음 손님으로.
    public void Judge(bool success);
}
```

### 규칙

1. `Prepare()` 는 `CustomerNumber` 를 **1** 로 만들고 잠금을 푼다
2. `Judge()` 는 `IsJudging = true` → 0.8초 대기 → `CustomerNumber++` → `IsJudging = false`
3. 대기 중 `Judge()` 가 또 불려도 무시한다 (이미 잠겨 있으니)
4. **대기 중에 `Prepare()` 가 불리면, 그 대기는 아무 일도 하지 않고 조용히 끝난다** ← 이게 전부다

### 재현 시나리오 — 규칙 4가 없으면

> 실패 판정이 나서 0.8초 대기가 시작됐다.
> 플레이어가 0.3초 만에 "다시하기" 를 눌렀다. `Prepare()` 가 새 판을 깔고 손님 1번을 세웠다.
> 0.5초 뒤, **죽은 줄 알았던 이전 판의 대기가 깨어나 `CustomerNumber++` 를 실행한다.**
> **새 판이 시작하자마자 손님이 2번으로 건너뛴다.**

### 검증

```csharp
private async void Start()
{
    runner.Prepare();
    Debug.Log(runner.CustomerNumber);        // 1

    runner.Judge(false);                     // 0.8초 대기 시작
    await Awaitable.WaitForSecondsAsync(0.3f);

    runner.Prepare();                        // 다시하기
    Debug.Log(runner.CustomerNumber);        // 1
    Debug.Log(runner.IsJudging);             // False  ← 잠금도 즉시 풀려야 한다

    await Awaitable.WaitForSecondsAsync(1.0f);   // 옛 대기가 깨어날 시간
    Debug.Log(runner.CustomerNumber);        // 1      ← 2면 유령이 깨어난 것
    Debug.Log(runner.IsJudging);             // False

    // ── 정상 경로도 확인
    runner.Judge(true);
    await Awaitable.WaitForSecondsAsync(1.0f);
    Debug.Log(runner.CustomerNumber);        // 2
}
```

### 힌트

`CancellationToken` 으로도 풀 수 있다. 하지만 여기서는 **더 가벼운 기법**이 있다.
**정수 필드 하나**로 막을 수 있다. 어떻게?

<details><summary>정답 방향 (충분히 헤맨 뒤에 열 것)</summary>

**세대 토큰(generation token)** 또는 **에포크 카운터** 라고 부른다.

```csharp
private int judgeToken;

private async void BeginJudgeDelay()
{
    int token = ++judgeToken;        // ① 이번 대기의 "세대 번호" 를 찍어둔다
    SetJudging(true);

    try
    {
        await Awaitable.WaitForSecondsAsync(judgeDelaySeconds, destroyCancellationToken);
    }
    catch (OperationCanceledException)
    {
        return;
    }

    if (token != judgeToken)          // ② 깨어나 보니 세대가 바뀌었다 → 나는 유령이다
    {
        return;
    }

    CustomerNumber++;
    SetJudging(false);
}

public void Prepare()
{
    judgeToken++;                     // ③ 세대를 넘겨 진행 중인 대기를 전부 무효화
    SetJudging(false);
    CustomerNumber = 1;
}
```

**왜 CTS 대신 `int` 인가?**

여기서 하고 싶은 건 "취소" 가 아니라 **"결과를 버린다"** 이다.
대기 자체는 자연히 끝나게 두고, 깨어난 뒤 **자기가 아직 유효한지 검사만** 한다.
CTS 는 `Dispose` 관리가 필요한데, 이 패턴은 정수 하나면 끝난다.

네트워크 응답 폐기, 검색어 자동완성, 씬 로딩 등 **"늦게 온 응답 버리기"** 에 널리 쓰인다.
"요청 3번을 보냈는데 2번 응답이 3번보다 늦게 왔다" 같은 상황을 이걸로 막는다.

**함정 — `async void`**

`BeginJudgeDelay` 는 `async void` 다. `async Awaitable` 이 아니다.
`async void` 는 **예외를 호출자가 잡을 수 없어서** 일반적으로는 쓰면 안 되는 형태다.
예외가 나면 그대로 앱을 타고 올라간다.

Unity 이벤트 핸들러(버튼 콜백, 여기처럼 "쏘고 잊는" 작업)에서만 예외적으로 허용되며,
**반드시 `try/catch (OperationCanceledException)` 로 감싸야 한다.**
안 그러면 오브젝트가 파괴될 때마다 콘솔에 예외가 뜬다.

**규칙 3(대기 중 `Judge` 무시)** 은 `Judge` 맨 앞에서 `if (IsJudging) return;` 하면 된다.
3-2·3-3·4-4에서 계속 나온 "판단은 실행 앞에" 다.
</details>

> 원본: `Core/GamePlayController.cs` 의 `judgeToken`, `BeginJudgeDelay()`, `Prepare()`

---

## 5단계가 가르치는 것

네 문제의 함정을 한 줄로 줄이면 전부 같은 질문이다.

> **`await` 에서 깨어났을 때, 내가 시작할 때의 세상이 아직 남아 있는가?**

세상이 바뀌는 경로는 셋이고, 대응이 각각 다르다.

| 무엇이 바뀌었나 | 어떻게 아나 | 어디서 |
|---|---|---|
| 오브젝트가 파괴됐다 | `destroyCancellationToken` | 5-1 |
| 새 요청이 나를 덮었다 | **직접 만든 CTS 를 `Cancel()`** | 5-2 |
| 호출자가 그만두랬다 | 넘겨받은 토큰 (+ 합치기) | 5-3 |
| 판 자체가 바뀌었다 | **세대 번호 비교** | 5-4 |

그리고 **어떻게 끝나든 정리는 해야 한다** — 5-3의 `finally`, 5-2의 `Dispose`.

이 네 가지를 구분해서 쓸 수 있으면, 이 프로젝트에서 가장 어려운 코드를 읽을 수 있다.

---

*5단계까지 끝나면 PART 1의 L3·L5(비동기·입력 프레임 순서)로 넘어갈 수 있다.
거기가 이 프로젝트 실제 코드의 마지막 난관이다.*

---

# 부록. 비동기 준비 트랙 (A1~A6)

**5단계를 풀기 전에 거치는 준비 과정.** 5단계는 async 를 안다는 전제로 낸 문제라, 모르는 상태에서
부딪히면 "문법이 낯선 건지 로직이 틀린 건지" 구분이 안 된다.

이 트랙은 다르다.

- **A1~A3 은 코드를 거의 안 짠다.** 완성된 코드를 **돌려보고 출력 순서를 확인**하는 게 과제다
- A4~A6 에서 조금씩 쓴다
- 전부 `Assets/MyAssets/PRACTICE_Assets/Scripts/Study/Async/` 에 있다
- 각 파일을 **빈 GameObject 에 붙이고 재생**하면 콘솔에 결과가 찍힌다

| # | 파일 | 배우는 것 | 코딩량 |
|---|---|---|---|
| A1 | `A1_ExecutionOrder` | `await` 는 게임이 아니라 **그 메서드만** 멈춘다 | 없음 (예측만) |
| A2 | `A2_FirstAsync` | `async Awaitable` 메서드 만들고 `await` 하기 | 3줄 |
| A3 | `A3_Sequence` | 여러 번 순차 대기 (`3 → 2 → 1`) | 10줄 |
| A4 | `A4_FrameLoop` | 프레임 단위 대기 + 보간 → **5-1의 뼈대** | 15줄 |
| A5 | `A5_DestroyToken` | 파괴 후 깨어나는 버그 **재현** → 토큰으로 수정 | 5줄 |
| A6 | `A6_Cancel` | 직접 취소하기 (CTS) → **5-2의 뼈대** | 20줄 |

---

## A1. 실행 순서 — 코드를 짜지 않는 문제

`A1_ExecutionOrder.cs` 를 빈 GameObject 에 붙인다.

**재생하기 전에** 콘솔에 어떤 순서로 찍힐지 종이에 적어보고, 그 다음에 재생한다.

```csharp
private void Awake()
{
    Debug.Log("1) Awake 시작");
    RunTest();
    Debug.Log("?) Awake 끝");
}

private async void RunTest()
{
    Debug.Log("2) RunTest 시작");
    await Awaitable.WaitForSecondsAsync(2f);
    Debug.Log("?) RunTest 끝 (2초 뒤)");
}
```

<details><summary>정답과 해설 (예측을 적은 뒤에 열 것)</summary>

```
1) Awake 시작
2) RunTest 시작
3) Awake 끝            ← 2초를 기다리지 않는다
  ... 2초 ...
4) RunTest 끝
```

**`await` 를 만나는 순간 `RunTest` 는 호출자(`Awake`)에게 제어를 돌려준다.**
`Awake` 는 하던 일을 계속하고, `RunTest` 의 나머지는 2초 뒤에 이어진다.

즉 **메서드 하나가 `await` 지점에서 두 조각으로 잘려, 서로 다른 시점에 실행된다.**

```
[프레임 0]   Awake: "1" → RunTest: "2" → (await 만남, 돌아감) → Awake: "3" → Awake 끝
[프레임 120] RunTest 의 나머지: "4"
```

이게 5단계 전체의 뿌리다. **두 번째 조각이 실행될 때는 세상이 바뀌어 있을 수 있다.**

같은 파일의 `Update()` 가 그동안 계속 돌면서 프레임 번호를 찍는 것도 확인할 것.
**`await` 는 게임을 멈추지 않는다.**
</details>

---

## A2. 첫 async 메서드

`A2_FirstAsync.cs` 의 `WaitAndLog` 를 완성한다. **3줄이다.**

```csharp
/// seconds 초 기다린 뒤 message 를 찍는다.
private async Awaitable WaitAndLog(float seconds, string message)
{
    // TODO
}
```

`Start` 는 이미 이렇게 되어 있다.

```csharp
private async void Start()
{
    Debug.Log("시작");
    await WaitAndLog(1f, "1초 지남");
    await WaitAndLog(1f, "2초 지남");
    Debug.Log("끝");
}
```

**기대 출력** — `시작` → (1초) → `1초 지남` → (1초) → `2초 지남` → `끝`

<details><summary>힌트</summary>

```csharp
await Awaitable.WaitForSecondsAsync(seconds);
Debug.Log(message);
```

두 줄이면 된다. `async Awaitable` 로 선언했으니 호출부가 `await` 할 수 있다.

**`await` 를 빼고 `WaitAndLog(1f, ...)` 만 부르면 어떻게 되는지도 해보라.**
두 메시지가 동시에 찍히고 `끝` 이 맨 먼저 나온다 — A1에서 본 그 동작이다.
`await` 는 "이게 끝날 때까지 여기서 기다린다" 는 뜻이다.
</details>

---

## A3. 순차 대기

`A3_Sequence.cs` 에서 `3 → 2 → 1 → Start!` 를 0.5초 간격으로 찍는다.

```csharp
[SerializeField] private string[] steps = { "3", "2", "1", "Start!" };
[SerializeField] private float stepSeconds = 0.5f;

private async Awaitable PlaySteps()
{
    // TODO: steps 를 순회하며 하나씩 찍고 stepSeconds 만큼 기다린다
}
```

**기대 출력** — 0.5초 간격으로 `3`, `2`, `1`, `Start!`

<details><summary>힌트</summary>

```csharp
foreach (string step in steps)
{
    Debug.Log(step);
    await Awaitable.WaitForSecondsAsync(stepSeconds);
}
```

**`foreach` 안에서 `await` 를 해도 된다.** 루프가 한 바퀴 돌 때마다 메서드가 잠시 멈췄다 이어진다.
이게 코루틴 없이 연출을 쓰는 방식이고, **5-3이 정확히 이 모양이다.**
</details>

---

## A4. 프레임 단위 대기와 보간 — 5-1의 뼈대

`A4_FrameLoop.cs` 에서 **2초 동안 0에서 100까지 올라가는 숫자**를 매 프레임 찍는다.

```csharp
private async Awaitable CountUp(float duration)
{
    // TODO
}
```

**규칙**
- `Awaitable.NextFrameAsync()` 로 한 프레임씩 기다린다
- `Time.unscaledDeltaTime` 을 누적한다
- 진행률을 `Mathf.Lerp(0f, 100f, elapsed / duration)` 으로 계산해 `Value` 에 넣는다
- 끝나면 `Value` 가 정확히 `100` 이다

**검증** — 재생하면 `Value` 가 0→100으로 부드럽게 오르고, 마지막에 `완료: 100` 이 찍힌다.

<details><summary>힌트</summary>

```csharp
float elapsed = 0f;

while (elapsed < duration)
{
    await Awaitable.NextFrameAsync();      // 한 프레임 쉰다
    elapsed += Time.unscaledDeltaTime;
    Value = Mathf.Lerp(0f, 100f, elapsed / duration);
}

Value = 100f;      // 루프가 딱 떨어지지 않으므로 마지막에 확정한다
```

**`await Awaitable.NextFrameAsync()` 는 `Update` 가 한 번 도는 것과 같다.**
`while` 루프가 프레임마다 한 바퀴씩 돈다.

**왜 `deltaTime` 이 아니라 `unscaledDeltaTime` 인가?**
`Time.timeScale = 0` (일시정지)이면 `deltaTime` 이 **0** 이 된다. `elapsed` 가 안 쌓여
**`while` 이 영원히 안 끝난다.** 스크립트에 `Time.timeScale = 0f;` 를 넣고 직접 확인해보라.

마지막 줄에서 `Value = 100f` 를 확정하는 이유도 중요하다. 루프는 `elapsed` 가 `duration` 을
**넘어설 때** 끝나므로, 마지막 계산값이 100을 살짝 넘거나 못 미칠 수 있다.
</details>

---

## A5. 파괴 후 깨어나는 버그 — 재현부터

**이 문제의 목표는 버그를 직접 보는 것이다.** 고치는 건 그 다음이다.

`A5_DestroyToken.cs` 를 붙이고 재생하면, 3초 뒤 콘솔에 **빨간 예외**가 뜬다.

```
MissingReferenceException: The object of type 'A5_Worker' has been destroyed
but you are still trying to access it.
```

무슨 일이 일어났는지 파일 안 주석에 적혀 있다. 순서는 이렇다.

```
0.0초   임시 오브젝트를 만들고 Worker.Run() 시작 → 3초 대기 진입
0.5초   그 오브젝트를 Destroy 한다
3.0초   대기가 끝나 코드가 깨어난다 → 이미 없는 자기 transform 을 건드린다 → 예외
```

**과제** — `A5_Worker.Run()` 의 `await` 에 `destroyCancellationToken` 을 넘기고,
`try/catch (OperationCanceledException)` 로 감싸서 예외를 없앤다.

<details><summary>힌트</summary>

```csharp
try
{
    await Awaitable.WaitForSecondsAsync(3f, destroyCancellationToken);
}
catch (OperationCanceledException)
{
    return;          // 파괴됐다. 조용히 끝낸다.
}

Debug.Log(transform.position);    // 여기까지 왔으면 살아 있다
```

**`destroyCancellationToken`** 은 `MonoBehaviour` 에 내장된 프로퍼티다.
그 오브젝트가 파괴되면 자동으로 취소 신호를 보내고, 대기 중이던 `await` 가
`OperationCanceledException` 을 던지며 즉시 끝난다. **그 아래 줄은 실행되지 않는다.**

**`catch (OperationCanceledException)` 은 에러 처리가 아니다.**
"화면을 떠났으니 하던 일을 그만둔다" 는 **성공적인 동작**이다.
여기에 `Debug.LogError` 를 넣으면 정상 동작할 때마다 빨간 줄이 뜬다. 조용히 `return` 한다.

> `catch` 를 안 쓰고 싶다면? `await` 앞에 `if (this == null) return;` 을 넣는 건 소용없다.
> 문제는 **대기하는 3초 동안** 파괴되는 것이지, 시작 시점이 아니기 때문이다.
</details>

---

## A6. 직접 취소하기 — 5-2의 뼈대

`destroyCancellationToken` 은 **"오브젝트가 죽을 때"** 만 알려준다.
**"새 요청이 왔으니 이전 걸 그만둬라"** 는 아무도 안 알려준다. 그건 직접 만들어야 한다.

`A6_Cancel.cs` 에서 "1초 뒤 메시지를 찍는" 작업을 만들되, **새 요청이 오면 이전 걸 취소**한다.

```csharp
private CancellationTokenSource cts;

/// 1초 뒤 message 를 찍는다. 이전 요청이 진행 중이면 그건 취소한다.
public void Request(string message)
{
    // TODO
}
```

`Start` 는 이렇게 부른다.

```csharp
Request("A");
await 0.3초
Request("B");        // A 는 취소돼야 한다
await 0.3초
Request("C");        // B 도 취소돼야 한다
```

**기대 출력** — `C` **하나만** 찍힌다 (마지막 요청으로부터 1초 뒤). `A` 나 `B` 가 찍히면 실패.

<details><summary>힌트</summary>

```csharp
public void Request(string message)
{
    RunRequest(message);
}

private async void RunRequest(string message)
{
    Cancel();                                          // ① 이전 대기를 끊는다
    cts = CancellationTokenSource
        .CreateLinkedTokenSource(destroyCancellationToken);   // ② 파괴도 함께 감시

    try
    {
        await Awaitable.WaitForSecondsAsync(1f, cts.Token);
    }
    catch (OperationCanceledException)
    {
        return;                                        // ③ 새 요청이 덮었다
    }

    Debug.Log(message);
}

private void Cancel()
{
    if (cts == null) return;

    cts.Cancel();
    cts.Dispose();     // CTS 는 IDisposable 이다
    cts = null;        // 다음 Cancel 이 죽은 CTS 를 건드리지 않게
}
```

**`Cancel()` → `Dispose()` → `null` 세 줄이 한 세트다.**
`Dispose` 를 빼면 내부 자원이 쌓이고, `null` 을 빼면 다음 호출에서
이미 버린 CTS 에 `Cancel()` 을 불러 `ObjectDisposedException` 이 난다.

**`CreateLinkedTokenSource`** 는 여러 취소 사유를 **하나의 토큰으로 합친다.**
여기서는 "새 요청이 왔다"(내 CTS)와 "오브젝트가 파괴됐다"(destroyCancellationToken) 둘 다 감시한다.

**왜 `RunRequest` 가 `async void` 인가?**
`Request` 는 "쏘고 잊는" 호출이다. 호출자가 결과를 기다리지 않는다.
`async void` 는 **예외를 호출자가 잡을 수 없어서** 일반적으로는 피해야 하지만,
Unity 이벤트 핸들러나 이런 fire-and-forget 에서만 예외적으로 쓴다.
**반드시 `try/catch` 로 감싸는 게 조건이다.**

| | 언제 |
|---|---|
| `async Awaitable` | 호출부가 `await` 로 **기다려야 할 때** (A2~A4) |
| `async void` | 쏘고 잊을 때. 반드시 `try/catch` 동반 (A6, 5-2, 5-4) |
</details>

---

## 이 트랙을 끝내면

| 배운 것 | 쓰이는 곳 |
|---|---|
| A1 실행이 `await` 에서 잘린다 | 5단계 전체의 전제 |
| A2·A3 `async Awaitable` 과 순차 대기 | **5-3** |
| A4 프레임 루프 + `unscaledDeltaTime` | **5-1** |
| A5 `destroyCancellationToken` | **5-1, 5-2, 5-4** |
| A6 CTS 재생성 + `async void` | **5-2** |

A6까지 하면 5-1은 A4 + A5 를 합치는 것뿐이고, 5-2는 A6 를 토스트에 입히는 것뿐이다.
**5-3은 A3 + 토큰 합치기, 5-4만 새 개념(세대 토큰)이 하나 남는다.**
