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
    /// 성공·실패 건수로 최종 등급을 판정한다. GamePlay 에 붙인다.
    /// DessertTable 과 같은 성격의 조회 컴포넌트다.
    ///
    /// 점수를 입력으로 받지 않는다. 현재 설계에서 score 는 successCount × 100 과 같은 값이라,
    /// 점수와 성공 건수를 둘 다 넣으면 같은 정보를 두 번 세는 셈이 된다.
    /// 실질 축은 처리량(success) 과 정확도(fail) 두 개다.
    /// </summary>
    public class RankTable : MonoBehaviour
    {
        [Header("가중치")]
        [SerializeField] private int successWeight = 100;

        [Tooltip("실패 가중치를 성공의 절반으로 둔 이유: 실패는 1탭이면 끝나 시간을 거의 쓰지 않는다. "
            + "감점이 없으면 '일단 아무거나 눌러보기' 가 무비용이 된다")]
        [SerializeField] private int failPenalty = 50;

        [Header("등급 테이블")]
        [Tooltip("잠정치. 한 판 정직하게 플레이해 나온 성공 건수 S 로 튜닝할 것 — "
            + "A=S×1.15, B=S×0.95, C=S×0.75, D=S×0.5, E=S×0.25 (각각 ×100). "
            + "C 가 '평범하게 하면 나오는 등급' 이 되도록 잡는 것이 기준")]
        [SerializeField]
        private RankRule[] rules =
        {
            new RankRule { label = "A", minRankScore = 2800, color = new Color(1f, 0.824f, 0.290f) },
            new RankRule { label = "B", minRankScore = 2200, color = new Color(0.435f, 0.820f, 1f) },
            new RankRule { label = "C", minRankScore = 1600, color = new Color(0.494f, 0.827f, 0.494f) },
            new RankRule { label = "D", minRankScore = 1000, color = new Color(1f, 0.690f, 0.404f) },
            new RankRule { label = "E", minRankScore = 400, color = new Color(1f, 0.541f, 0.420f) },

            // 최하위는 int.MinValue 로 둬서 모든 입력이 반드시 하나에 걸리게 한다.
            // 빈 문자열이 표시될 여지를 없앤다.
            new RankRule { label = "F", minRankScore = int.MinValue, color = new Color(0.604f, 0.627f, 0.651f) }
        };

        /// <summary>
        /// rankScore = 성공 × successWeight − 실패 × failPenalty.
        /// 음수가 될 수 있다(0성공 20실패 = −1000). 정상이며 F 다.
        /// </summary>
        public RankRule Evaluate(int successCount, int failCount)
        {
            // 인스펙터를 안 채운 채 실행해도 예외가 나지 않아야 한다.
            if (rules == null || rules.Length == 0)
            {
                return new RankRule { label = "-", minRankScore = int.MinValue, color = Color.white };
            }

            int rankScore = successCount * successWeight - failCount * failPenalty;

            // 배열 순서를 신뢰하지 않고 조건을 만족하는 것 중 가장 높은 기준값을 고른다.
            // 인스펙터에서 항목을 옮겨도 판정이 조용히 어긋나지 않는다.
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
                // 모든 기준값보다 낮다. 인스펙터에 int.MinValue 항목이 없는 경우다.
                Debug.LogWarning(name + ": rankScore " + rankScore
                    + " 가 어느 등급에도 걸리지 않았습니다. 최하위 등급의 minRankScore 를 int.MinValue 로 두세요.", this);
                return new RankRule { label = "-", minRankScore = int.MinValue, color = Color.white };
            }

            // Serializable struct 의 Color 기본값은 알파 0 이다. 배열에 항목을 추가하고 색을
            // 안 만지면 글자가 투명해져 아무것도 안 보인다. 원인을 찾기 어려운 자리라 보정한다.
            if (best.color.a <= 0f)
            {
                best.color = Color.white;
            }

            return best;
        }
    }
}
