using System;
using UnityEngine;
using System.Threading.Tasks;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Async
{
    /// <summary>
    /// A7. 관찰되지 않는 예외 — Forget() 패턴.
    ///
    /// A5 에서 async void 메서드(A5_Worker.Run)가 예외를 던졌을 때, Unity 가 자동으로
    /// 스택 트레이스와 함께 콘솔에 찍어준 걸 봤다. async void 는 처리 안 된 예외를
    /// Unity 의 SynchronizationContext 를 통해 자동으로 표면화한다. 이미 안전하다.
    ///
    /// 그런데 "async Awaitable" 메서드를 _ = Foo(); 로 버리면(discard) 다르다.
    /// 아무도 그 결과를 관찰하지 않으니, 그 안의 예외는 콘솔에 아무 흔적도 안 남기고 사라진다.
    ///
    /// 과제
    ///   ① 지금 그냥 재생해서 확인한다.
    ///      BuggyWork() 안에 DivideByZeroException 이 있는데 콘솔엔 아무것도 안 뜬다.
    ///      (지금 당장 고칠 필요 없이 관찰만 한다 — 이게 문제 상황이다.)
    ///
    ///   ② AwaitableExtensions.Forget() 을 완성한다.
    ///      - awaitable 을 await 한다
    ///      - OperationCanceledException 은 조용히 넘긴다 (정상 취소)
    ///      - 그 외 예외는 Debug.LogException(e) 로 콘솔에 반드시 남긴다
    ///
    ///   ③ Start() 의 `_ = BuggyWork();` 를 `BuggyWork().Forget();` 로 바꾼다.
    ///
    ///   ④ 다시 재생한다. 이번엔 DivideByZeroException 이 스택 트레이스와 함께
    ///      콘솔에 떠야 한다. (오류가 늘어난 게 아니라, 원래 있던 오류가 이제 보이는 것이다.)
    ///
    /// 힌트는 없다.
    /// </summary>
    public sealed class A7_Forget : MonoBehaviour
    {
        private void Start()
        {
            // _ = BuggyWork();   // ← ③ 에서 BuggyWork().Forget(); 으로 바꿀 것
            BuggyWork().Forget();
        }

        private async Awaitable BuggyWork()
        {
            await Awaitable.NextFrameAsync(destroyCancellationToken);

            int zero = 0;
            int result = 10 / zero;   // 일부러 넣은 버그. DivideByZeroException

            Debug.Log("여기 도달하면 이상하다: " + result);
        }
    }

    public static class AwaitableExtensions
    {
        /// <summary>
        /// UniTask 의 Forget() 과 같은 역할.
        /// 결과를 안 기다리는 대신, 취소는 조용히 넘기고 그 외 예외는 반드시 로그를 남긴다.
        /// </summary>
        public static async void Forget(this Awaitable awaitable)
        {
            try
            {
                await awaitable;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    public static class TaskExtensions
    {
        public static async void Forget(this Task task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

}
