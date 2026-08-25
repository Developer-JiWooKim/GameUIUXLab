using System;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core.Rules
{
    /// <summary>카운트다운 규칙.</summary>
    public sealed class PlayClock
    {
        private float duration;
        private float remaining;

        public float Duration => duration;

        public float Remaining => remaining;

        public bool IsExpired => remaining <= 0f; // 카운트 다운 종료 여부

        public void Reset(float durationSeconds)
        {
            duration = Math.Max(0f, durationSeconds);
            remaining = duration;
        }

        /// <returns>이번 호출로 시간이 다 됐으면 true, 이미 0 이었다면 false.</returns>
        public bool Tick(float deltaTime)
        {
            if (remaining <= 0f)
            {
                return false;
            }

            remaining = Math.Max(0f, remaining - deltaTime);
            return remaining <= 0f;
        }
    }
}
