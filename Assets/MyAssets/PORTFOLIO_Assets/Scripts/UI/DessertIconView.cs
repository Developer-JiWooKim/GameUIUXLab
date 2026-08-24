using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    // OrderPrefab 과 ChoicePrefab 이 공유하는 컴포넌트. 두 프리팹은 구조가 같다(CakeIcon + CountText).
    // 스프라이트 교체와 개수 표시 로직이 한 곳에만 있으면, 표시 규칙을 바꿀 때 한 군데만 고치면 된다.
    public class DessertIconView : MonoBehaviour
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

            // "×1" 은 표시하지 않는다. 1개짜리가 대부분이라 전부 붙이면 시선만 분산된다.
            bool showCount = count > 1;
            countText.gameObject.SetActive(showCount);

            if (showCount)
            {
                countText.text = "×" + count;
            }
        }
    }
}
