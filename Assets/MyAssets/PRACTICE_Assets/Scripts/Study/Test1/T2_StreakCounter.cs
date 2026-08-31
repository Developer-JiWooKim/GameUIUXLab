using System;
using System.Threading;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Test1
{
    /// <summary>
    /// 테스트 2. 콤보 스트릭 카운터.  [Unity C#]
    ///
    /// 격투 게임의 콤보 시스템과 비슷하다. Hit() 을 부를 때마다 스트릭이 오른다.
    /// 마지막 Hit() 로부터 1초 안에 다음 Hit() 이 없으면 스트릭이 0으로 초기화된다.
    ///
    /// 규칙
    ///   1. Hit() → StreakCount++ , OnStreakChanged(StreakCount) 발행
    ///   2. Hit() 은 1초짜리 유예 타이머를 (다시) 건다.
    ///      Hit() 이 새로 오면, 진행 중이던 이전 유예 타이머는 취소되고 새로 시작한다
    ///   3. 유예 타이머가 취소되지 않고 끝까지 가면 → StreakCount = 0, OnStreakChanged(0) 발행
    ///   4. Reset() → StreakCount 를 즉시 0으로. 단, 이미 0이면 이벤트를 또 쏘지 않는다
    ///      (값이 실제로 바뀔 때만 이벤트가 나간다)
    ///   5. OnDestroy 에서 타이머를 정리한다
    ///
    /// 검증 (컴포넌트를 씬에 올리고 재생)
    ///   int changedCount = 0;
    ///   counter.OnStreakChanged += _ => changedCount++;
    ///
    ///   counter.Hit();  await 0.2초;
    ///   counter.Hit();  await 0.2초;
    ///   counter.Hit();  await 0.2초;
    ///     StreakCount == 3
    ///     changedCount == 3          ← Hit 마다 값이 실제로 바뀌었으니 매번 발행
    ///
    ///   (마지막 Hit 로부터) await 1.2초 더;
    ///     StreakCount == 0           ← 유예 시간 초과로 리셋
    ///     changedCount == 4          ← 리셋도 값 변경이니 한 번 더 발행
    ///
    ///   counter.Reset();             ← 이미 0인 상태에서 Reset
    ///     changedCount == 4          ← 안 바뀌었으니 이벤트 추가 발행 없음
    ///
    /// 힌트는 없다. 지금까지 배운 것 중 어떤 패턴이 여기 맞는지 스스로 판단할 것.
    /// </summary>
    public sealed class T2_StreakCounter : MonoBehaviour
    {
        [SerializeField] private float graceSeconds = 1f;

        public int StreakCount { get; private set; }

        public event Action<int> OnStreakChanged;

        private CancellationTokenSource cts;

        public void Hit()
        {
            StreakCount++;
            OnStreakChanged?.Invoke(StreakCount);

            _ = GraceCounter(graceSeconds);
        }

        public void Reset()
        {
            if (StreakCount == 0)
            {
                return;
            }

            StreakCount = 0;
            OnStreakChanged?.Invoke(StreakCount);
        }

        private async Awaitable GraceCounter(float count)
        {
            Cancel();

            cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            try
            {
                await Awaitable.WaitForSecondsAsync(count, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Reset();
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

        private void OnDestroy()
        {
            Reset();
        }
    }
}
