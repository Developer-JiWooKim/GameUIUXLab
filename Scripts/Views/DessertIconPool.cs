using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    /// <summary>
    /// 주문 카드와 쟁반이 함께 쓰는 DessertIcon 풀.
    /// </summary>
    public sealed class DessertIconPool
    {
        private readonly DessertIconView prefab;
        private readonly Transform root;
        private readonly ObjectPool<DessertIconView> pool;

        /// <summary>지금 화면에 나가 있는 것들. 갱신할 때 한 번에 되돌림.</summary>
        private readonly List<DessertIconView> active = new();

        /// <param name="capacity">동시에 켜지는 최대 개수. 주문·쟁반 모두 슬롯 수와 같음.</param>
        public DessertIconPool(DessertIconView prefab, Transform root, int capacity)
        {
            this.prefab = prefab;
            this.root = root;

            pool = new ObjectPool<DessertIconView>(
                createFunc: Create,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroyIcon,
                collectionCheck: true,
                defaultCapacity: capacity,
                maxSize: capacity);
        }

        public DessertIconView Get()
        {
            DessertIconView icon = pool.Get();
            active.Add(icon);
            return icon;
        }

        public void ReleaseAll()
        {
            foreach (DessertIconView icon in active)
            {
                if (icon == null)
                {
                    continue;
                }

                pool.Release(icon);
            }

            active.Clear();
        }

        public void Dispose()
        {
            ReleaseAll();
            pool.Clear();
        }

        private DessertIconView Create()
        {
            return Object.Instantiate(prefab, root);
        }

        private void OnGet(DessertIconView icon)
        {
            // 재사용된 것은 예전 자리(sibling index)를 그대로 들고 오므로 맨 뒤로 보내야 Layout Group 이 Get 한 순서대로 늘어놓게됨.
            icon.transform.SetAsLastSibling();
            icon.gameObject.SetActive(true);
        }

        private void OnRelease(DessertIconView icon)
        {
            icon.gameObject.SetActive(false);
        }

        private void OnDestroyIcon(DessertIconView icon)
        {
            if (icon != null)
            {
                Object.Destroy(icon.gameObject);
            }
        }
    }
}
