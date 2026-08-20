using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    public abstract class UIStateBase : MonoBehaviour, IState
    {
        [Header("처음 진입 시 포커스")]
        [SerializeField] private GameObject firstSelected;

        public GameObject FirstSelected => firstSelected;

        protected IScreenController Screens { get; private set; }

        public void Bind(IScreenController screens) => Screens = screens;

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void OnCancel() { }
    }
}
