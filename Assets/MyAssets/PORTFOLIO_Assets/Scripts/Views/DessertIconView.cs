using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    /// <summary>
    /// OrderPrefab 과 ChoicePrefab 이 공유하는 컴포넌트.
    /// </summary>
    public sealed class DessertIconView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;

        public void SetIcon(Sprite sprite, int count)
        {
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
            }

            if (countText == null)
            {
                return;
            }

            // "×1" 은 표시 X
            bool showCount = count > 1;
            countText.gameObject.SetActive(showCount);

            if (showCount)
            {
                countText.text = "×" + count;
            }
        }
    }
}
