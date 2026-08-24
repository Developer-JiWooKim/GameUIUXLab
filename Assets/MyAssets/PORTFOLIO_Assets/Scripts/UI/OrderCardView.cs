using System.Collections.Generic;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    // 현재 손님의 주문을 표시한다. OrderPanel 에 붙인다.
    public class OrderCardView : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GamePlayController gamePlay;
        [SerializeField] private DessertTable dessertTable;

        [Header("연결")]
        [SerializeField] private Transform gridRoot;
        [SerializeField] private DessertIconView iconPrefab;
        [SerializeField] private TextMeshProUGUI customerNameText;

        [Tooltip("손님 번호 앞에 붙는 고정 문구")]
        [SerializeField] private string customerNamePrefix = "손님 ";

        // 종류별 개수를 세는 임시 통. 주문이 바뀔 때마다 새로 만들지 않으려고 재사용한다.
        private readonly Dictionary<DessertType, int> counts = new Dictionary<DessertType, int>();

        private void OnEnable()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": OrderCardView 에 GamePlayController 가 꽂히지 않았습니다.", this);
                return;
            }

            gamePlay.OnOrderChanged += Refresh;
            // 화면이 켜지는 시점의 값을 한 번 그린다. PlayState.Enter() 의 Prepare() 가
            // 곧바로 이벤트를 발행하지만, 그 순서에 의존하지 않도록 여기서도 그린다.
            Refresh(gamePlay.CurrentOrder, gamePlay.CustomerNumber);
        }

        // 해제를 빼먹으면 화면을 껐다 켤 때마다 구독이 쌓여 아이콘이 중복 생성된다.
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

            ClearGrid();

            if (order == null || gridRoot == null || iconPrefab == null)
            {
                return;
            }

            // 주문 카드는 종류별로 묶어서 보여준다(초코 ×2). 개별 나열인 쟁반과 대비되어
            // "주문한 것" 과 "담은 것" 이 한눈에 구분된다.
            counts.Clear();
            foreach (DessertType type in order)
            {
                counts.TryGetValue(type, out int count);
                counts[type] = count + 1;
            }

            foreach (KeyValuePair<DessertType, int> pair in counts)
            {
                DessertIconView icon = Instantiate(iconPrefab, gridRoot);
                icon.SetIcon(dessertTable != null ? dessertTable.GetSprite(pair.Key) : null, pair.Value);
            }
        }

        private void ClearGrid()
        {
            if (gridRoot == null)
            {
                return;
            }

            for (int i = gridRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(gridRoot.GetChild(i).gameObject);
            }
        }
    }
}
