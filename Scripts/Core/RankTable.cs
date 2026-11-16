using System;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core
{
    [Serializable]
    public struct RankRule
    {
        [Tooltip("RankText 에 표시할 문자")]
        public string label;

        [Tooltip("이 값 이상이면 이 등급")]
        public int minRankScore;

        [Tooltip("등급별 RankText 색")]
        public Color color;
    }

    /// <summary>
    /// 성공/실패 건수로 최종 등급을 판정.
    /// </summary>
    public sealed class RankTable : MonoBehaviour
    {
        [Header("가중치")]
        [SerializeField] private int successWeight = 100;
        [SerializeField] private int failPenalty = 50;

        [Header("등급 테이블")]
        [SerializeField]
        private RankRule[] rules =
        {
            new RankRule { label = "A", minRankScore = 2800, color = new Color(1f, 0.824f, 0.290f) },
            new RankRule { label = "B", minRankScore = 2200, color = new Color(0.435f, 0.820f, 1f) },
            new RankRule { label = "C", minRankScore = 1600, color = new Color(0.494f, 0.827f, 0.494f) },
            new RankRule { label = "D", minRankScore = 1000, color = new Color(1f, 0.690f, 0.404f) },
            new RankRule { label = "E", minRankScore = 400, color = new Color(1f, 0.541f, 0.420f) },

            // 최하위는 int.MinValue 로 둬서 모든 입력이 반드시 하나에 걸리게. 빈 문자열이 표시될 여지를 없앰.
            new RankRule { label = "F", minRankScore = int.MinValue, color = new Color(0.604f, 0.627f, 0.651f) }
        };

        /// <summary>
        /// rankScore = 성공 × successWeight − 실패 × failPenalty.
        /// 음수가 될 수 있음(0성공 20실패 = −1000). 정상이며, 이럴 경우 결과는 F.
        /// </summary>
        public RankRule Evaluate(int successCount, int failCount)
        {
            if (rules == null || rules.Length == 0)
            {
                return new RankRule { label = "-", minRankScore = int.MinValue, color = Color.white };
            }

            int rankScore = successCount * successWeight - failCount * failPenalty;

            bool found = false;
            RankRule best = default;

            foreach (RankRule rule in rules)
            {
                if (rankScore < rule.minRankScore)
                {
                    continue;
                }

                if (!found || rule.minRankScore > best.minRankScore)
                {
                    best = rule;
                    found = true;
                }
            }

            if (!found)
            {
                return new RankRule { label = "-", minRankScore = int.MinValue, color = Color.white };
            }

            // Default를 대입하면 alpha값이 0이므로 위에서 best가 바뀌지 않을 경우를 대비.
            if (best.color.a <= 0f)
            {
                best.color = Color.white;
            }

            return best;
        }
    }
}
