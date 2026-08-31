using System.Collections.Generic;

namespace CodingTest
{
    // 1. [자료구조/알고리즘] 몬스터 타겟팅 우선순위 시스템
    // 문제 설명
    // 유니티 턴제 RPG 전투에서 플레이어의 스킬이 타겟을 지정하려 합니다.
    // 전달받은 몬스터 목록에서 다음 우선순위 규칙에 따라 최종 타겟 1마리를 선별하여 반환하는 메서드를 작성하세요.
    //
    // 우선순위 규칙
    //   1. 도발(IsTaunting) 상태인 몬스터가 있다면 최우선 선택됩니다.
    //   2. 도발 상태인 몬스터가 여러 마리라면, 현재 체력(CurrentHp)이 가장 적은 몬스터를 선택합니다.
    //   3. 도발 상태인 몬스터가 하나도 없다면, 현재 체력 비율(CurrentHp / MaxHp)이 가장 적은 몬스터를 선택합니다.
    //   4. 체력 비율까지 동일하다면, 생성 순서(SpawnIndex)가 가장 먼저(값이 작음)인 몬스터를 선택합니다.
    //   생존한 몬스터(CurrentHp > 0)만 타겟 대상이 되며, 대상이 없으면 null 을 반환합니다.

    public sealed class Monster
    {
        public bool IsTaunting;
        public int CurrentHp;
        public int MaxHp;
        public int SpawnIndex;
    }

    public static class MonsterTargetSelector
    {
        public static Monster SelectTarget(IReadOnlyList<Monster> monsters)
        {
            if (monsters == null)
            {
                return null;
            }

            Monster best = null;

            for (int i = 0; i < monsters.Count; i++)
            {
                Monster m = monsters[i];

                if (m == null || m.CurrentHp <= 0)
                {
                    continue;
                }

                if (best == null || IsBetter(m, best))
                {
                    best = m;
                }
            }

            return best;
        }

        private static bool IsBetter(Monster candidate, Monster best)
        {
            if (candidate.IsTaunting != best.IsTaunting)
            {
                return candidate.IsTaunting;
            }

            if (candidate.IsTaunting)
            {
                if (candidate.CurrentHp != best.CurrentHp)
                {
                    return candidate.CurrentHp < best.CurrentHp;
                }

                return candidate.SpawnIndex < best.SpawnIndex;
            }

            long left = (long)candidate.CurrentHp * best.MaxHp;
            long right = (long)best.CurrentHp * candidate.MaxHp;

            if (left != right)
            {
                return left < right;
            }

            return candidate.SpawnIndex < best.SpawnIndex;
        }
    }
}
