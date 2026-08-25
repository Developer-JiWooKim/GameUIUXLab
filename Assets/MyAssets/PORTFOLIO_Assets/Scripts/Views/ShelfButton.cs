using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    [RequireComponent(typeof(Button))]
    public sealed class ShelfButton : MonoBehaviour
    {
        [Header("이 버튼이 담당하는 디저트")]
        [SerializeField] private DessertType type;

        private Button button;

        public DessertType Type => type;

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
