using System;
using System.Collections.Generic;

namespace CodingTest
{
    // 2. [C# 언어 / 데이터 구조] 인벤토리 가방 정렬 및 빈 공간 압축
    // 문제 설명
    // 인벤토리 가방(Array) 시스템을 구현해야 합니다.인벤토리는 고정된 크기 capacity를 가지며, 빈 슬롯은 null로 표현됩니다.인벤토리를 아래 규칙에 따라 정렬하고 빈 공간을 뒤로 모으는 CompressAndSort 메서드를 작성하세요. (GC 할당을 줄이기 위해 원본 배열을 인플레이스(In - place)로 수정하거나 최적화하여 구현하는 것이 좋습니다.)

    // 정렬 규칙
    // 유효한 아이템들은 배열의 앞쪽에, 빈 슬롯(null)은 뒤쪽에 배치되어야 합니다.

    // 유효한 아이템 간의 정렬 순서:

    // 1순위: 아이템 등급(Rarity) 내림차순(Higher is better)

    // 2순위: 등급이 같다면 아이템 ID(Id) 오름차순

    // 배열 내 순서가 변경된 원본 Item[] 배열을 반환하세요.
    public enum ItemRarity { Common, Rare, Epic, Legendary }

    public class Item
    {
        public int Id;
        public string Name;
        public ItemRarity Rarity;
    }

    public static class InventorySystem
    {
        // 캐싱해서 딱 한 번만 만든다. 정적 필드라 타입이 처음 쓰일 때 한 번 생성되고,
        // 이후 CompressAndSort 를 몇 번을 부르든 재사용된다 — 호출당 할당 0.
        //
        // b.Rarity.CompareTo(a.Rarity) : a 가 b 보다 등급이 높으면 음수를 반환한다
        // → Array.Sort 는 음수를 "a 가 먼저" 로 해석하므로 등급 내림차순이 된다.
        private static readonly IComparer<Item> RarityThenIdComparer = Comparer<Item>.Create(
            (a, b) =>
            {
                int rarityCompare = b.Rarity.CompareTo(a.Rarity);
                return rarityCompare != 0 ? rarityCompare : a.Id.CompareTo(b.Id);
            });

        /// <summary>
        /// 원본 배열을 그 자리에서 고친다. 새 배열을 만들지 않는다.
        ///   ① 투 포인터로 null 이 아닌 것만 앞으로 밀어낸다 — O(n), 추가 배열 없음
        ///   ② 남은 뒷부분을 null 로 채운다
        ///   ③ 유효 아이템 구간만 Array.Sort 로 정렬한다 — 제자리 정렬, 추가 배열 없음
        /// </summary>
        // public static Item[] CompressAndSort(Item[] slots)
        // {
        //     if (slots == null)
        //     {
        //         return null;
        //     }

        //     int writeIndex = 0;

        //     for (int i = 0; i < slots.Length; i++)
        //     {
        //         if (slots[i] != null)
        //         {
        //             slots[writeIndex] = slots[i];
        //             writeIndex++;
        //         }
        //     }

        //     for (int i = writeIndex; i < slots.Length; i++)
        //     {
        //         slots[i] = null;
        //     }

        //     Array.Sort(slots, 0, writeIndex, RarityThenIdComparer);

        //     return slots;
        // }

        public static Item[] CompressAndSort(Item[] slots)
        {
            if (slots == null)
            {
                return null;
            }
            Array.Sort(slots, CompareItems);

            return slots;
        }

        private static int CompareItems(Item x, Item y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return 1;
            }

            if (y == null)
            {
                return -1;
            }

            int rarityCompare = y.Rarity.CompareTo(x.Rarity);
            if (rarityCompare != 0)
            {
                return rarityCompare;
            }

            return x.Id.CompareTo(y.Id);
        }
    }
}
