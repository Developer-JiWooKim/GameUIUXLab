using System.Collections.Generic;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// 쟁반에 담긴 것을 표시한다. ChoiceListPanel 에 붙인다.
    ///
    /// 표시 전용이다. 클릭을 받지 않는다. 즉시 판정에서는 쟁반에 담긴 것이 항상 정답이므로,
    /// 슬롯을 눌러 빼는 행동은 순수한 손해일 뿐 의미가 없다.
    ///
    /// 주문 카드는 종류별 묶음(초코 ×2), 쟁반은 담은 순서대로 개별 나열이다.
    /// 표시 방식을 다르게 두면 "주문한 것" 과 "담은 것" 이 한눈에 구분된다.
    /// </summary>
    public class TrayView : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GamePlayController gamePlay;
        [SerializeField] private DessertTable dessertTable;

        [Header("연결")]
        [SerializeField] private Transform gridRoot;
        [SerializeField] private DessertIconView iconPrefab;

        private void OnEnable()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": TrayView 에 GamePlayController 가 꽂히지 않았습니다.", this);
                return;
            }

            gamePlay.OnTrayChanged += Refresh;

            // 화면이 켜지는 시점의 값을 한 번 그린다. 이벤트 발행 순서에 의존하지 않도록.
            Refresh(gamePlay.Tray);
        }

        // 해제를 빼먹으면 화면을 껐다 켤 때마다 구독이 쌓여 아이콘이 중복 생성된다.
        private void OnDisable()
        {
            if (gamePlay != null)
            {
                gamePlay.OnTrayChanged -= Refresh;
            }
        }

        private void Refresh(IReadOnlyList<DessertType> tray)
        {
            ClearGrid();

            if (tray == null || gridRoot == null || iconPrefab == null)
            {
                return;
            }

            // 담은 것만 그린다. 빈 칸은 아무것도 그리지 않는다.
            // 개수는 묶지 않으므로 항상 1 을 넘긴다(DessertIconView 가 ×1 을 숨긴다).
            foreach (DessertType type in tray)
            {
                DessertIconView icon = Instantiate(iconPrefab, gridRoot);
                icon.SetIcon(dessertTable != null ? dessertTable.GetSprite(type) : null, 1);
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
