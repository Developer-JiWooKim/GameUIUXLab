using System.Threading;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Async
{
    /// <summary>
    /// A2. 첫 async 메서드. WaitAndLog 를 완성한다. (2줄)
    ///
    /// 기대 출력
    ///   시작 → (1초) → 1초 지남 → (1초) → 2초 지남 → 끝
    ///
    /// 해본 뒤 : Start 의 await 를 지우고 돌려보라. 순서가 어떻게 바뀌는가?
    /// A: 시작, 끝이 출력되고 1초 뒤에 메세지 두개가 동시에 출력
    /// </summary>
    public sealed class A2_FirstAsync : MonoBehaviour
    {
        private async void Start()
        {
            Debug.Log("시작");

            // _ = WaitAndLog(1f, "1초 지남");
            // _ = WaitAndLog(1f, "2초 지남");

            await WaitAndLog(1f, "1초 지남");
            await WaitAndLog(1f, "2초 지남");

            Debug.Log("끝");
        }

        /// <summary>seconds 초 기다린 뒤 message 를 찍는다.</summary>
        private async Awaitable WaitAndLog(float seconds, string message)
        {
            await Awaitable.WaitForSecondsAsync(seconds);
            Debug.Log(message);
        }
    }
}
