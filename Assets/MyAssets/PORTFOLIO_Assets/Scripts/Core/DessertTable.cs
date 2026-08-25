using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core
{
    public sealed class DessertTable : MonoBehaviour
    {
        [Header("디저트 아이콘 스프라이트")]
        [SerializeField] private Sprite[] sprites = new Sprite[TypeCount];

        public const int TypeCount = 5;

        public int Count => TypeCount;

        private void Awake()
        {
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
