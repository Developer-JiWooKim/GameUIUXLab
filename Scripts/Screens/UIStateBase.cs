using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Screens
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIStateBase : MonoBehaviour, IState
    {
        [Header("처음 진입 시 포커스 설정")]
        [SerializeField] private GameObject firstSelected;

        public GameObject FirstSelected => firstSelected;

        protected IScreenController Screens { get; private set; }

        private CanvasGroup group;

        public void Bind(IScreenController screens) => Screens = screens;

        public void SetInteractable(bool value)
        {
            if (group == null)
            {
                group = GetComponent<CanvasGroup>();

                // ConfirmPopup 은 Screen_Pause 의 자손이다. 위에 열렸다는 이유로 아래(Screen_Pause)를
                // 잠그면 자기 자신까지 같이 잠긴다. 부모 그룹을 무시해야 팝업 버튼이 살아 있다.
                group.ignoreParentGroups = true;
            }

            group.interactable = value;
        }

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void OnCancel() { }
    }
}
