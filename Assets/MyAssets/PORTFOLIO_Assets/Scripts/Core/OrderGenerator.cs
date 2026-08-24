using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core
{
    /// <summary>
    /// 무작위 주문을 만들기만 한다. 게임 상태는 건드리지 않는다. GamePlay 에 붙인다.
    /// </summary>
    public class OrderGenerator : MonoBehaviour
    {
        [SerializeField] private DessertTable dessertTable;

        [Tooltip("주문 1건의 총 개수. 쟁반 슬롯 수와 같아야 한다")]
        [SerializeField] private int orderCount = 3;

        public int OrderCount => orderCount;

        /// <summary>
        /// 5종에서 중복을 허용해 orderCount 개를 뽑는다.
        ///
        /// 종류를 먼저 정하고 종류별 개수를 다시 뽑는 2단계 방식이 아니다.
        /// 같은 종류가 두 번 나오면 그게 곧 "그 품목 2개" 인 주문이다.
        /// 전부 같은 종류가 나올 수도 있고(확률 1/25), 그것도 정상이다.
        /// </summary>
        public List<DessertType> Generate()
        {
            int typeCount = dessertTable != null ? dessertTable.Count : DessertTable.TypeCount;
            List<DessertType> order = new List<DessertType>(orderCount);

            for (int i = 0; i < orderCount; i++)
            {
                order.Add((DessertType)Random.Range(0, typeCount));
            }

            return order;
        }
    }
}
