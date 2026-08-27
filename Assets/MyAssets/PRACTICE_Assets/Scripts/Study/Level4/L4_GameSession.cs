using System;
using Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level3;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level4
{
    /// <summary>
    /// 4-1. 제한 시간을 재고, 상태가 바뀔 때마다 바깥에 알린다.
    /// 이 클래스는 UI 를 전혀 모른다. text 도 Image 도 건드리지 않는다.
    ///
    /// 규칙
    ///   1. Update() 는 Tick(Time.deltaTime) 만 부른다. 로직은 Tick 에
    ///   2. Tick 은 시간이 실제로 흐른 프레임에만 OnTimeChanged 를 쏜다
    ///   3. SetRunning 은 값이 실제로 바뀔 때만 OnRunningChanged 를 쏜다
    ///   4. OnFinished 는 시간이 다 된 순간 정확히 한 번
    ///   5. 시간이 다 되면 IsRunning 이 false 가 된다
    ///
    /// 검증
    ///   session.SetRunning(false); session.SetRunning(false);
    ///   runningCount == 0                      ← 값이 안 바뀌면 이벤트도 없다
    ///   session.StartGame();  runningCount == 1,  IsRunning == true
    ///   session.Tick(6f);     finishedCount == 0
    ///   session.Tick(6f);     finishedCount == 1,  IsRunning == false,  runningCount == 2
    ///   session.Tick(6f);     finishedCount == 1   ← 계속 1
    /// </summary>
    public sealed class L4_GameSession : MonoBehaviour
    {
        [SerializeField] private float playTimeSeconds = 10f;

        private readonly Countdown clock = new();   // 3-2 재사용

        public float Remaining => clock.Remaining;
        public bool IsRunning => clock.IsRunning;

        public float PlayTime => playTimeSeconds;


        // TODO: 이벤트 3개. public Action 이 아니라 event Action 이다.
        //       (남은 시간, 전체 시간) / (진행 여부) / (인자 없음)

        public event Action<float, float> OnTimeChanged;
        public event Action<bool> OnRunningChanged;
        public event Action OnFinished;

        /// <summary>시계를 되감고 시작한다.</summary>
        public void StartGame()
        {
            clock.Reset(PlayTime);
            SetRunning(true);
        }

        public void SetRunning(bool running)
        {
            if (IsRunning == running)
            {
                return;
            }
            clock.SetRunning(running);
            OnRunningChanged?.Invoke(running);
        }

        /// <summary>Update 가 부르지만, 테스트도 직접 부른다.</summary>
        public void Tick(float deltaTime)
        {
            if (!IsRunning)
            {
                return;
            }

            bool expired = clock.Tick(deltaTime);
            OnTimeChanged?.Invoke(Remaining, PlayTime);

            if (expired)
            {
                SetRunning(false);
                OnFinished?.Invoke();
            }
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }
    }
}
