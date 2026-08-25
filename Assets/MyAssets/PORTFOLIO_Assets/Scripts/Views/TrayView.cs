using System.Collections.Generic;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    /// <summary>
    /// 쟁반에 담긴 것을 표시. (표시 전용, 클릭 X)
    /// </summary>
    public sealed class TrayView : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GamePlayController gamePlay;
        [SerializeField] private DessertTable dessertTable;

        [Header("표시할 디저트와 위치")]
        [SerializeField] private Transform gridRoot;
        [SerializeField] private DessertIconView iconPrefab;

        [Tooltip("동시에 켜지는 아이콘의 최대 개수. (=쟁반 슬롯 수)")]
        [SerializeField] private int iconPoolCapacity = 3;

        private DessertIconPool iconPool;

        private void Awake()
        {
            if (gridRoot != null && iconPrefab != null)
            {
                iconPool = new DessertIconPool(iconPrefab, gridRoot, iconPoolCapacity);
            }
        }

        private void OnDestroy()
        {
            iconPool?.Dispose();
        }

        private void OnEnable()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": TrayView 에 GamePlayController 가 꽂히지 않았습니다.", this);
                return;
            }

            gamePlay.OnTrayChanged += Refresh;

            // 켜지는 시점에 한번 Refresh
            Refresh(gamePlay.Tray);
        }

        private void OnDisable()
        {
            if (gamePlay != null)
            {
                gamePlay.OnTrayChanged -= Refresh;
            }
        }

        private void Refresh(IReadOnlyList<DessertType> tray)
        {
            if (iconPool == null)
            {
                return;
            }

            iconPool.ReleaseAll();

            if (tray == null)
            {
                return;
            }

            foreach (DessertType type in tray)
            {
                DessertIconView icon = iconPool.Get();
                icon.SetIcon(dessertTable != null ? dessertTable.GetSprite(type) : null, 1);
            }
        }
    }
}
