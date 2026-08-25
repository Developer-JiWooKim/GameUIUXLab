using System.Collections.Generic;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    public sealed class OrderCardView : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GamePlayController gamePlay;
        [SerializeField] private DessertTable dessertTable;

        [Header("주문 프리팹")]
        [SerializeField] private Transform gridRoot;
        [SerializeField] private DessertIconView iconPrefab;

        [Header("손님")]
        [SerializeField] private TextMeshProUGUI customerNameText;
        [Tooltip("손님 번호 앞에 붙는 고정 문구")]
        [SerializeField] private string customerNamePrefix = "손님 ";

        [Header("동시에 켜지는 아이콘의 최대 개수.")]
        [SerializeField] private int iconPoolCapacity = 3;

        // 종류별 개수를 세는 임시 통. 주문이 바뀔 때마다 새로 만들지 않고 재사용.
        private readonly Dictionary<DessertType, int> counts = new();

        private DessertIconPool iconPool;

        private void Awake()
        {
            if (gridRoot != null && iconPrefab != null)
            {
                iconPool = new DessertIconPool(iconPrefab, gridRoot, iconPoolCapacity);
            }
        }

        private void OnDestroy() => iconPool?.Dispose();

        private void OnEnable()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": OrderCardView 에 GamePlayController 가 꽂히지 않았습니다.", this);
                return;
            }

            gamePlay.OnOrderChanged += Refresh;

            Refresh(gamePlay.CurrentOrder, gamePlay.CustomerNumber);
        }

        private void OnDisable()
        {
            if (gamePlay != null)
            {
                gamePlay.OnOrderChanged -= Refresh;
            }
        }

        private void Refresh(IReadOnlyList<DessertType> order, int customerNumber)
        {
            if (customerNameText != null)
            {
                customerNameText.text = customerNamePrefix + customerNumber;
            }

            if (iconPool == null)
            {
                return;
            }

            iconPool.ReleaseAll();

            if (order == null)
            {
                return;
            }

            // 주문 카드는 종류별로 묶어서 보여줌. ex)초코 케이크 아이콘 밑에 ×2
            counts.Clear();
            foreach (DessertType type in order)
            {
                counts.TryGetValue(type, out int count);
                counts[type] = count + 1;
            }

            foreach (KeyValuePair<DessertType, int> pair in counts)
            {
                DessertIconView icon = iconPool.Get();
                icon.SetIcon(dessertTable != null ? dessertTable.GetSprite(pair.Key) : null, pair.Value);
            }
        }
    }
}
