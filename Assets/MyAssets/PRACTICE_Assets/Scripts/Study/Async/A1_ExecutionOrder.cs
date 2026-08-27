using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Async
{
    /// <summary>
    /// A1. 코드를 짜지 않는 문제.
    ///
    /// 재생하기 전에 콘솔에 어떤 순서로 찍힐지 종이에 적어보고, 그 다음에 재생한다.
    /// Update 가 그동안 프레임 번호를 찍는 것도 같이 볼 것.
    /// </summary>
    public sealed class A1_ExecutionOrder : MonoBehaviour
    {
        private int frame;
        private bool logFrames;

        private void Awake()
        {
            Debug.Log("1) Awake 시작");
            RunTest();
            Debug.Log("3) Awake 끝");
        }

        private async void RunTest()
        {
            Debug.Log("2) RunTest 시작");
            logFrames = true;

            await Awaitable.WaitForSecondsAsync(2f);

            logFrames = false;
            Debug.Log("4) RunTest 끝 (2초 뒤)");
        }

        private void Update()
        {
            if (!logFrames)
            {
                return;
            }

            frame++;

            // 30프레임마다 한 번씩만 (콘솔 도배 방지)
            if (frame % 30 == 0)
            {
                Debug.Log("   ... Update 는 계속 돌고 있다 (frame " + frame + ")");
            }
        }

    }
}
