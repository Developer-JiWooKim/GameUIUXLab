using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Async
{
    /// <summary>
    /// A4. 프레임 단위 대기와 보간.  ★ 5-1 의 뼈대
    ///
    /// 2초 동안 Value 를 0 에서 100 까지 올린다.
    ///   - Awaitable.NextFrameAsync() 로 한 프레임씩 기다린다
    ///   - Time.unscaledDeltaTime 을 누적한다
    ///   - Mathf.Lerp(0f, 100f, elapsed / duration) 으로 계산
    ///   - 끝나면 Value 가 정확히 100
    ///
    /// 해본 뒤 : Start 맨 위의 Time.timeScale 주석을 풀고 돌려보라.
    ///           unscaledDeltaTime 대신 deltaTime 을 쓰면 영원히 안 끝난다.
    /// </summary>
    public sealed class A4_FrameLoop : MonoBehaviour
    {
        public float Value { get; private set; }

        private async void Start()
        {
            // Time.timeScale = 0f;      // ← 나중에 이 줄의 주석을 풀어볼 것

            await CountUp(2f);

            Debug.Log("완료: " + Value);   // 100 이어야 한다
            Time.timeScale = 1f;
        }

        private async Awaitable CountUp(float duration)
        {
            // TODO
            throw new System.NotImplementedException();
        }

        private void Update()
        {
            // 진행 상황을 눈으로 보려면 인스펙터 대신 여기서 찍어도 된다
        }
    }
}
