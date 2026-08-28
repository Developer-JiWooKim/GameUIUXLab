# 진행 상황 — 새 세션 인계 문서

이 문서는 **다른 컴퓨터/새 Claude Code 세션에서 [PRACTICE_PROBLEMS.md](PRACTICE_PROBLEMS.md) 학습을 이어가기 위한 인계 노트**다.
새 세션 시작 시 이 파일과 `PRACTICE_PROBLEMS.md` 를 같이 읽으면 지금까지의 맥락을 빠르게 파악할 수 있다.

## 이 문서를 어떻게 쓰나

새 세션에서 이렇게 요청하면 된다:

> "PROGRESS.md 와 PRACTICE_PROBLEMS.md 읽고 이어서 채점해줘. 다음은 [문제 번호]야."

또는 그냥 스텁 파일을 열고 "N번 문제 풀었어 채점해줘" 라고 하면, 채점 방식은 아래 "채점 프로토콜" 절을 따르면 된다.

---

## 전체 구조

`PRACTICE_PROBLEMS.md` 는 세 파트로 되어 있다.

| 파트 | 내용 | 상태 |
|---|---|---|
| **PART 1** (L1~L6) | 원본 포트폴리오 코드(`Assets/MyAssets/PORTFOLIO_Assets/Scripts`)를 재현하는 문제. "정답 대조" 방식 | 안 품 (난이도가 너무 높다고 판단해 PART 3로 재설계) |
| **PART 2** (B1~B3) | 코딩테스트 형식 3문제. `Study/Easy/` 에 있었으나 **PART 3 로 흡수되며 폴더 삭제됨** | B1만 풀었었음(중복이라 재풀이 불필요) |
| **PART 3** (1~5단계 + Async 트랙) | **현재 진행 중인 메인 트랙.** 난이도 재조정판. 실제 사용 중 | 아래 참조 |

**결론: 지금 실제로 진행 중인 건 PART 3 하나다.** PART 1·2는 참고용으로 문서에 남아있을 뿐, 안 풀어도 된다.

---

## PART 3 완료 현황 (파일의 `NotImplementedException` 유무로 확인됨)

파일 위치: `Assets/MyAssets/PRACTICE_Assets/Scripts/Study/`

### 1단계 `Level1/L1_Basics.cs` — ✅ 완료 (5/5)
순수 C#. `ClampScore`, `ToCountLabel`, `ToRatio`, `IsDanger`, `CountOf`.

### 2단계 `Level2/L2_Guards.cs` — ✅ 완료 (5/5)
순수 C#. `GetName`, `CalculateScore`, `ToSecondText`, `FindMaxIndex`, `IsAllDone`.

### 3단계 `Level3/` — ✅ 완료 (5/5)
- `L3_State.cs` — `ChangeTracker`, `Countdown`, `OrderSession` (순수 C#, 3클래스 한 파일)
- `L3_ShelfButton.cs` — 첫 MonoBehaviour, GetComponent 캐싱
- `L3_TimerLabel.cs` — `ChangeTracker` 재사용

### 4단계 `Level4/` — ✅ 완료 (4/4)
- `L4_GameSession.cs` — `Countdown`(3-2) 재사용, event 발행
- `L4_TimerHud.cs` — 구독/해제, 즉시 동기화
- `L4_ShelfView.cs` — 두 이벤트 조합 잠금
- `L4_OrderRunner.cs` — `OrderSession`(3-3) 재사용, 사건 vs 상태 이벤트 구분

### 5단계 `Level5/` — 🔶 진행 중 (2/4)
- `L5_Fader.cs` — ✅ 완료. `Awaitable`, `unscaledDeltaTime`, `destroyCancellationToken`
- `L5_Toast.cs` — ✅ 완료. CTS 재생성 패턴(A6 응용)
- `L5_CountdownOverlay.cs` — ❌ **미완료** (스텁 상태, `NotImplementedException` 2곳: `PlayIntro`, `PlayTimeout`)
- `L5_JudgeRunner.cs` — ❌ **미완료** (스텁 상태, `NotImplementedException` 2곳: `Prepare`, `Judge`) — **세대 토큰, 이 문서 전체에서 가장 어려운 문제**

### 비동기 준비 트랙 `Async/A1~A6` — ✅ 전부 완료
5단계를 풀기 전 async/await 기초를 다지려고 중간에 추가한 트랙. `A1`(관찰만) ~ `A6`(CTS 재생성)까지 전부 통과.

---

## 다음에 할 일

**`Level5/L5_CountdownOverlay.cs` 부터 이어가면 된다.**

`PRACTICE_PROBLEMS.md` 의 **"## 5-3. CountdownOverlay — 연결된 토큰"** 절 참고.
- 호출자 토큰 + `destroyCancellationToken` 을 `CreateLinkedTokenSource` 로 합친다
- 인트로는 끝나면 패널을 끄고, 타임아웃은 켠 채로 둔다 → `try/finally` 로 처리 (5-1과 같은 유형: 반드시 되돌려야 하는 상태가 있다)

그 다음 **`Level5/L5_JudgeRunner.cs`** (5-4, 세대 토큰) 을 풀면 PART 3 전체 완료.

---

## 채점 프로토콜 (새 세션이 지켜야 할 방식)

지금까지 이 톤과 형식으로 채점해왔다. 이어갈 때 동일하게 유지할 것.

1. **먼저 파일을 실제로 읽고**, 대화 기록의 기억에 의존하지 않는다 (파일은 세션 사이에도 계속 바뀌어왔다)
2. 요구사항 표 또는 검증 시나리오를 **직접 손으로 추적**해서 통과/실패를 판정한다 — 짐작하지 않는다
3. 실패한 항목은 **파일:줄번호** 를 정확히 짚고, "왜 틀렸는지"를 원리로 설명한다 (단순히 "이렇게 고쳐라"가 아니라)
4. 함정이 **이전 문제에서 이미 다룬 개념의 재등장**이면 그 연결을 명시한다 (예: "이건 2-4의 `int max=0` 함정과 같다")
5. 잘한 부분도 구체적으로 짚는다 (감점 없는 지적과 감점 있는 버그를 구분해서 제시)
6. 학생이 스스로 고치게 하고, **정답 코드를 먼저 주지 않는다** — 막혔을 때만 방향을 준다
7. 학생이 배운 개념을 다른 곳에 잘못 적용했을 때(예: A6의 CTS 패턴을 5-1에 불필요하게 가져온 경우), "틀렸다"만 말하지 않고 **왜 그 문제엔 안 맞는지** 설명한다

## 이 학생의 특징 (이어가는 세션이 알아두면 좋은 것)

- **매우 빠르게 배운다.** 지적하면 다음 문제에서 대부분 스스로 적용한다 (예: `null` 가드 습관, 클래스명 오타 → 재발 안 함)
- **가끔 "고친 척"만 하는 실수**를 한다 — 조건문 껍데기만 지우고 안의 로직까지 같이 지워버리는 식. 재확인이 필요할 때가 있다
- **배운 패턴을 과잉 적용**하는 경향이 있다 (A6에서 배운 CTS 재생성을 5-1에 불필요하게 가져옴). "이 패턴이 필요한 조건이 뭔가"를 물어보는 게 효과적이었다
- **LINQ를 이해 없이 쓰다가 되돌리는 패턴**이 초반(B1)에 있었다 — 맞는 코드를 주석 처리하고 틀린 코드를 남긴 사례. 이후엔 스스로 LINQ vs 명시적 루프의 가독성을 판단해서 고르게 됨
- **비동기 개념이 완전히 처음**이라 5단계 진입 전 Async 트랙(A1~A6)을 별도로 만들어야 했다. A1은 코드를 안 짜고 실행 순서를 예측만 하는 문제로 설계했고 효과적이었다
- 직접 실험해서 확인하는 걸 좋아한다 (`timeScale=0` 에서 `WaitForSecondsAsync` 가 scaled인지 직접 테스트해봄). 이런 질문엔 정답을 바로 주지 말고 실험을 유도하는 게 좋다

## 주의할 것 — `.claude` 폴더는 이 저장소에 넣지 말 것

이 저장소(`GameUIUXLab`)는 **공개(public) GitHub 저장소**다.
로컬의 `.claude` 폴더(대화 기록, 메모리, `.credentials.json` 인증 토큰 포함)를 여기에 커밋하면 안 된다.
백업이 필요하면 외장 드라이브나 완전히 별도의 **private** 저장소를 쓸 것.

---

*최종 갱신: 5단계 진행 중 (5-1, 5-2 완료 / 5-3, 5-4 남음), Async 트랙 완료 시점*
