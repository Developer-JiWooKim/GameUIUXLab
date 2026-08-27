using System;
using System.Collections.Generic;
using System.Linq;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level2
{
    /// <summary>
    /// PART 3 · 2단계 (분기와 방어) 5문제.
    /// 함정은 문제당 정확히 1개. 각 요약의 ← 표시가 그 함정이다.
    /// </summary>
    public static class L2_Guards
    {
        /// <summary>
        /// 2-1. 배열에서 이름 하나를 꺼낸다. 꺼낼 수 없으면 null. 예외를 던지지 않는다.
        /// (["A","B","C"], 1) → "B" | (…, 3) → null | (…, -1) → null ←
        /// (null, 0) → null | ([], 0) → null
        /// </summary>
        public static string GetName(string[] names, int index)
        {
            if (names == null || index >= names.Length || index < 0)
            {
                return null;
            }

            return names[index];
        }

        /// <summary>
        /// 2-2. 성공/실패 기록을 순서대로 반영해 최종 점수를 구한다.
        /// 점수는 매 건마다 0 밑으로 내려가지 않는다.
        /// 아래는 전부 scorePerSuccess=100, penaltyPerFail=20 기준.
        /// [true,true] → 200 | [true,false] → 80 | [false] → 0
        /// [false,false,false,true] → 100 ←   ([] 와 null 은 0)
        /// </summary>
        public static int CalculateScore(IReadOnlyList<bool> results, int scorePerSuccess, int penaltyPerFail)
        {
            // if (results == null)
            // {
            //     return 0;
            // }

            if (results is null or { Count: 0 })
            {
                return 0;
            }

            int score = 0;
            foreach (bool result in results)
            {
                if (result)
                {
                    score += scorePerSuccess;
                }
                else
                {
                    score = Math.Max(0, score - penaltyPerFail);
                }
            }
            return score;

            // return results.Aggregate(0, (score, isSuccess) => isSuccess
            //                                 ? score + scorePerSuccess
            //                                 : Math.Max(0, score - penaltyPerFail));
        }

        /// <summary>
        /// 2-3. 남은 시간(초)을 화면에 쓸 문자열로. 올림이다.
        /// 1.2f → "2" | 1.0f → "1" | 0.1f → "1" | 0.0f → "0" | -3.0f → "0" ←
        /// </summary>
        public static string ToSecondText(float remainingSeconds)
        {
            int seconds = (int)Math.Ceiling(remainingSeconds);

            return Math.Max(0, seconds).ToString();
        }

        /// <summary>
        /// 2-4. 가장 큰 값의 인덱스. 동점이면 가장 앞. 비었거나 null 이면 -1.
        /// [3,7,5] → 1 | [7,7,5] → 0 | [10] → 0 | [-5,-2,-9] → 1 ←
        /// [] → -1 | null → -1
        /// </summary>
        public static int FindMaxIndex(IReadOnlyList<int> values)
        {
            // if (values == null || values.Count == 0)
            // {
            //     return -1;
            // }

            if (values is null or { Count: 0 })
            {
                return -1;
            }

            // return values
            //         .Select((val, idx) => (val, idx))
            //         .Aggregate((max, next) => next.val > max.val ? next : max)
            //         .idx;

            int index = 0;
            int value = values[index];
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] > value)
                {
                    value = values[i];
                    index = i;
                }
            }
            return index;

        }

        /// <summary>
        /// 2-5. 남은 개수가 전부 0 이하인가.
        /// {초코:0, 레몬:0} → true | {초코:1, 레몬:0} → false
        /// {} → true ←  | null → true
        /// </summary>
        public static bool IsAllDone(IReadOnlyDictionary<DessertType, int> remaining)
        {
            // if (remaining == null)
            // {
            //     return true;
            // }

            // foreach (int count in remaining.Values)
            // {
            //     if (count > 0)
            //     {
            //         return false;
            //     }
            // }
            // return true;

            return remaining is null || remaining.Values.All(cnt => cnt <= 0);
        }
    }
}
