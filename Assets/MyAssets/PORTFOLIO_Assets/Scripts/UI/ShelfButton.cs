using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// 진열대 버튼 1개가 자기 DessertType 을 들고 있게 한다.
    /// CakeButton_1 ~ CakeButton_5 에 각각 붙이고, type 을 버튼마다 다르게 지정한다.
    ///
    /// 버튼 이미지의 스프라이트는 이미 씬에 배치되어 있으므로 DessertTable 로 덮어쓰지 않는다.
    /// 대신 인스펙터의 type 과 눈에 보이는 아이콘이 일치하는지 반드시 대조할 것.
    /// 어긋나면 "눌렀는데 다른 게 담기는" 버그가 되고, 코드만 봐서는 원인을 찾을 수 없다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ShelfButton : MonoBehaviour
    {
        [Tooltip("이 버튼이 담당하는 디저트. 버튼 아이콘과 일치해야 한다")]
        [SerializeField] private DessertType type;

        private Button button;

        public DessertType Type => type;

        /// <summary>
        /// 지연 조회로 캐시한다. Screen_Play 가 꺼진 채 시작하므로 ShelfView 가 이 버튼을
        /// 참조하는 시점과 이쪽 Awake 순서를 가정하지 않는 편이 안전하다.
        /// </summary>
        public Button Button
        {
            get
            {
                if (button == null)
                {
                    button = GetComponent<Button>();
                }

                return button;
            }
        }
    }
}
