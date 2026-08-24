using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core
{
    /// <summary>
    /// DessertType 하나로 아이콘을 얻는 조회 테이블. GamePlay 에 붙인다.
    ///
    /// 이게 없으면 OrderCardView·TrayView 가 각자 스프라이트 배열을 들게 되고,
    /// 스프라이트를 교체할 때 여러 곳을 고쳐야 한다.
    /// </summary>
    public class DessertTable : MonoBehaviour
    {
        [Tooltip("Sprites/CakeIcon/ 의 5종을 DessertType 순서대로 꽂는다")]
        [SerializeField] private Sprite[] sprites = new Sprite[TypeCount];

        public const int TypeCount = 5;

        /// <summary>주문 생성기가 쓰는 품목 수. enum 을 늘리면 자동으로 따라간다.</summary>
        public int Count => TypeCount;

        private void Awake()
        {
            // 배열 길이가 어긋나면 인덱스로 접근하는 순간 전부 깨진다. 시작할 때 알려 준다.
            if (sprites == null || sprites.Length != TypeCount)
            {
                Debug.LogWarning(name + ": DessertTable 의 sprites 길이가 " + TypeCount + " 이 아닙니다.", this);
            }
        }

        public Sprite GetSprite(DessertType type)
        {
            int index = (int)type;
            if (sprites == null || index < 0 || index >= sprites.Length)
            {
                return null;
            }

            return sprites[index];
        }
    }
}
