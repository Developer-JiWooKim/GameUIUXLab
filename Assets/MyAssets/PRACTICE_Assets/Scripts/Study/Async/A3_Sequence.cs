using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Async
{
    /// <summary>
    /// A3. 순차 대기. 3 → 2 → 1 → Start! 를 0.5초 간격으로 찍는다.
    /// foreach 안에서 await 를 해도 된다. 5-3 이 정확히 이 모양이다.
    /// </summary>
    public sealed class A3_Sequence : MonoBehaviour
    {
        [SerializeField] private string[] steps = { "3", "2", "1", "Start!" };
        [SerializeField] private float stepSeconds = 0.5f;

        private async void Start()
        {
            Debug.Log("카운트다운 시작");

            await PlaySteps();

            Debug.Log("카운트다운 끝");
        }

        private async Awaitable PlaySteps()
        {
            // TODO: steps 를 순회하며 하나씩 찍고 stepSeconds 만큼 기다린다
            foreach (string step in steps)
            {
                await Awaitable.WaitForSecondsAsync(stepSeconds);
                Debug.Log(step);
            }
        }
    }
}
