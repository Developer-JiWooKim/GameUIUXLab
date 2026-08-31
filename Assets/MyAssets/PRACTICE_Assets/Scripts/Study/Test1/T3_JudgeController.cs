using System;
using System.Collections.Generic;
using System.Threading;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level3;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Test1
{
    /// <summary>
    /// 테스트 3. 원본 GamePlayController 의 Prepare()/Pick()/Judge() 를 재현한다.  [Unity C#]
    /// 지금까지 나온 것 중 가장 복잡한 조합이다. 힌트는 없다.
    ///
    /// 규칙
    ///   1. Prepare() — CustomerNumber=1 로, 새 주문을 받는다 (session.Begin 격),
    ///      OnOrderChanged·OnTrayChanged 를 발행한다. 잠금을 푼다.
    ///      진행 중이던 판정 대기가 있었다면 그 대기는 무효화된다.
    ///
    ///   2. Pick(type) — IsJudging 이 true 면 무시한다. 그 외엔 session.Pick() 결과에 따라:
    ///      · Rejected  → Judge(false) 를 유발한다 (트레이 이벤트는 없다 — 안 담겼으니)
    ///      · Accepted  → OnTrayChanged 만 발행한다 (아직 판정 아님, 담겼을 뿐)
    ///      · Completed → OnTrayChanged 발행 + Judge(true) 를 유발한다
    ///
    ///   3. Judge(success) — 호출되는 즉시 OnJudged(success) 를 발행한다 (【사건】).
    ///      그 직후 IsJudging=true 로 잠그고 OnJudgingChanged(true) 를 발행한다 (【상태】).
    ///      judgeDelaySeconds 뒤, 그 사이 무효화되지 않았다면:
    ///        CustomerNumber++ , 새 주문을 받는다, OnOrderChanged·OnTrayChanged 발행,
    ///        IsJudging=false 로 풀고 OnJudgingChanged(false) 발행.
    ///
    ///   4. 대기 중 Prepare() 가 불리면, 그 대기는 아무 것도 하지 않고 조용히 끝난다.
    ///
    ///   5. OnDestroy 에서 진행 중인 대기를 정리한다.
    ///
    /// 검증 (컴포넌트를 씬에 올리고 재생)
    ///   var judged = new List&lt;bool&gt;();
    ///   var judgingStates = new List&lt;bool&gt;();
    ///   controller.OnJudged += s =&gt; judged.Add(s);
    ///   controller.OnJudgingChanged += j =&gt; judgingStates.Add(j);
    ///
    ///   controller.Prepare();
    ///   controller.Begin(new[]{ 초코, 레몬 });     // 편의상 Begin 을 노출해도 되고, Prepare 인자로 받아도 된다.
    ///                                              // (테스트 편의를 위해 스스로 API 를 조금 조정해도 무방하다)
    ///
    ///   controller.Pick(키위);    // Rejected — 키위는 이 주문에 아예 없다
    ///     judged 마지막 값 == false
    ///     judgingStates 마지막 값 == true
    ///     CustomerNumber == 1        (아직 안 바뀜)
    ///
    ///   controller.Pick(초코);    // 잠긴 상태라 무시돼야 한다
    ///     judged.Count 그대로       (증가 없음)
    ///
    ///   judgeDelaySeconds 만큼 대기 후:
    ///     CustomerNumber == 2
    ///     judgingStates 마지막 값 == false
    ///
    ///   ── 무효화 시나리오
    ///   controller.Pick(...);     // Completed 가 나오도록 순서대로 담아 성공시킨다
    ///   0.3초 후 controller.Prepare();
    ///     CustomerNumber == 1     (Prepare 가 즉시 되돌림)
    ///   나머지 대기 시간이 다 지난 뒤에도
    ///     CustomerNumber == 1     (2 로 건너뛰면 안 된다 — 유령이 깨어난 것)
    /// </summary>
    public sealed class T3_JudgeController : MonoBehaviour
    {
        [SerializeField] private float judgeDelaySeconds = 0.8f;

        private readonly OrderSession session = new();   // 3-3 재사용

        public int CustomerNumber { get; private set; }

        public bool IsJudging { get; private set; }

        public IReadOnlyList<DessertType> Order => session.Order;

        public IReadOnlyList<DessertType> Tray => session.Tray;

        /// <summary>【사건】 판정 순간. true=성공.</summary>
        public event Action<bool> OnJudged;

        /// <summary>【상태】 입력 잠금 여부.</summary>
        public event Action<bool> OnJudgingChanged;

        /// <summary>새 손님의 주문.</summary>
        public event Action<IReadOnlyList<DessertType>, int> OnOrderChanged;

        public event Action<IReadOnlyList<DessertType>> OnTrayChanged;

        private DessertType[] pendingOrder;

        private CancellationTokenSource cts;

        /// <summary>다음 손님의 주문을 미리 정해둔다. 검증 편의용.</summary>
        public void SetNextOrder(DessertType[] order)
        {
            pendingOrder = order;
        }

        /// <summary>새 판을 시작한다. (다시하기)</summary>
        public void Prepare()
        {
            Cancel();

            CustomerNumber = 1;

            if (IsJudging)
            {
                IsJudging = false;
                OnJudgingChanged?.Invoke(IsJudging);
            }

            Begin();
        }

        public void Begin()
        {
            session.Begin(pendingOrder);

            OnOrderChanged?.Invoke(Order, CustomerNumber);
            OnTrayChanged?.Invoke(Tray);
        }

        private void Cancel()
        {
            if (cts == null)
            {
                return;
            }
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }


        /// <summary>하나 담는다.</summary>
        public async Awaitable Pick(DessertType type)
        {
            if (IsJudging)
            {
                return;
            }

            PickOutcome result = session.Pick(type);
            if (result == PickOutcome.Rejected)
            {
                await Judge(false);
            }
            else if (result == PickOutcome.Accepted)
            {

                OnTrayChanged?.Invoke(Tray);
            }
            else
            {

                OnTrayChanged?.Invoke(Tray);
                await Judge(true);
            }
        }

        private async Awaitable Judge(bool success)
        {
            OnJudged?.Invoke(success);

            Cancel();

            cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            IsJudging = true;
            OnJudgingChanged?.Invoke(true);

            try
            {
                await Awaitable.WaitForSecondsAsync(judgeDelaySeconds, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            CustomerNumber++;

            Begin();

            IsJudging = false;
            OnJudgingChanged?.Invoke(false);

        }

        private void OnDestroy()
        {
            Cancel();
        }
    }
}
