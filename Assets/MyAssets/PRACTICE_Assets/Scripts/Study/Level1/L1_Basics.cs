using System;
using System.Collections.Generic;
using System.Linq;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level1
{
    /// <summary>
    /// PART 3 · 1단계 (표현식) 5문제.
    /// 각 메서드의 throw 를 지우고 채우면 된다. 몸통은 길어야 5줄.
    /// null 입력은 들어오지 않는다고 가정한다.
    /// </summary>
    public static class L1_Basics
    {
        /// <summary>
        /// 1-1. 점수가 음수면 0으로 바꾼다.
        /// 100 → 100 | 0 → 0 | -50 → 0 | -1 → 0
        /// </summary>
        public static int ClampScore(int score)
        {
            return Math.Max(0, score);
        }

        /// <summary>
        /// 1-2. 2개 이상일 때만 "×N", 그 외에는 빈 문자열.
        /// 3 → "×3" | 2 → "×2" | 1 → "" | 0 → ""
        /// </summary>
        public static string ToCountLabel(int count)
        {
            return count >= 2 ? "×" + count.ToString() : string.Empty;
        }

        /// <summary>
        /// 1-3. 남은 시간을 0~1 비율로. total 이 0 이하면 0.
        /// (60, 120) → 0.5f | (120, 120) → 1f | (0, 120) → 0f | (10, 0) → 0f
        /// </summary>
        public static float ToRatio(float remaining, float total)
        {
            if (total <= 0) return 0f;

            return remaining / total;
        }

        /// <summary>
        /// 1-4. 남은 시간이 임계값 이하면 true. (같을 때도 true)
        /// (15, 10) → false | (10, 10) → true | (3, 10) → true | (0, 10) → true
        /// </summary>
        public static bool IsDanger(float remaining, float threshold)
        {
            return remaining <= threshold;
        }

        /// <summary>
        /// 1-5. 주문 목록에서 target 이 몇 개인지 센다.
        /// ([초코,초코,레몬], 초코) → 2 | ([초코,초코,레몬], 키위) → 0 | ([], 초코) → 0
        /// </summary>
        public static int CountOf(IReadOnlyList<DessertType> order, DessertType target)
        {
            if (order == null) return 0;

            return order.Count(menu => menu == target);
        }
    }
}
