using System;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core.Rules
{
    /// <summary>
    /// 점수와 성공·실패 건수의 계산 규칙. (Score >= 0)
    /// </summary>
    public sealed class ScoreBoard
    {
        private int scorePerSuccess;
        private int scorePenaltyPerFail;

        private int score;
        private int successCount;
        private int failCount;

        public int Score => score;

        public int SuccessCount => successCount;

        public int FailCount => failCount;

        public void Reset(int scorePerSuccess, int scorePenaltyPerFail)
        {
            this.scorePerSuccess = scorePerSuccess;
            this.scorePenaltyPerFail = scorePenaltyPerFail;

            score = 0;
            successCount = 0;
            failCount = 0;
        }

        public void Apply(bool success)
        {
            if (success)
            {
                score += scorePerSuccess;
                successCount++;
            }
            else
            {
                score = Math.Max(0, score - scorePenaltyPerFail);
                failCount++;
            }
        }
    }
}
