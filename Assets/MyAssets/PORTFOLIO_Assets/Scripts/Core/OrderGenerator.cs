using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core
{
    public class OrderGenerator : MonoBehaviour
    {
        [Header("디저트 종류를 담고 있는 테이블")]
        [SerializeField] private DessertTable dessertTable;

        [Header("고정 주문 개수")]
        [SerializeField] private int orderCount = 3;

        public int OrderCount => orderCount;

        void Awake()
        {
            if (dessertTable == null)
            {
                Debug.LogWarning(this + ": 인스펙터 상에서 DessertTable이 연결되지 않음");
            }
        }

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
