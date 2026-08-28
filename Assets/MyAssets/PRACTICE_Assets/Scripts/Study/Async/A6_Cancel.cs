using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Async
{
    /// <summary>
    /// A6. 직접 취소하기.  ★ 5-2 의 뼈대
    ///
    /// destroyCancellationToken 은 "오브젝트가 죽을 때" 만 알려준다.
    /// "새 요청이 왔으니 이전 걸 그만둬라" 는 직접 만들어야 한다.
    ///
    /// 기대 출력 : "C" 하나만 찍힌다 (마지막 요청으로부터 1초 뒤).
    ///             A 나 B 가 찍히면 이전 대기를 안 끊은 것.
    /// </summary>
    public sealed class A6_Cancel : MonoBehaviour
    {
        private CancellationTokenSource cts;

        private async void Start()
        {
            Debug.Log("시작");

            Request("A");
            await Awaitable.WaitForSecondsAsync(0.3f);

            Request("B");
            await Awaitable.WaitForSecondsAsync(0.3f);

            Request("C");

            await Awaitable.WaitForSecondsAsync(2f);
            Debug.Log("끝. 위에 C 만 찍혔으면 성공.");
        }

        /// <summary>1초 뒤 message 를 찍는다. 이전 요청이 진행 중이면 그건 취소한다.</summary>
        public void Request(string message)
        {
            RunRequest(message);
        }

        // TODO: private async void RunRequest(string message)
        //         ① Cancel() 로 이전 대기를 끊는다
        //         ② cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        //         ③ try { await ...(1f, cts.Token); } catch (OperationCanceledException) { return; }
        //         ④ Debug.Log(message);

        private async void RunRequest(string message)
        {
            Cancel();
            cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            try
            {
                await Awaitable.WaitForSecondsAsync(1f, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Debug.Log(message);
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

        // TODO: private void Cancel()
        //         Cancel() → Dispose() → null   세 줄이 한 세트

        private void OnDestroy()
        {
            Cancel();
        }
    }
}
