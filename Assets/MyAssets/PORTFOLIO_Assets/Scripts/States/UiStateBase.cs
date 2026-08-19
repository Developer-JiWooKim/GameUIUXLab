using Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// 모든 화면 상태의 공통 부모. 각 Screen_ 패널 오브젝트에 붙인다.
    /// 상태를 패널에 붙여 두면 첫 버튼 참조를 인스펙터로 해결할 수 있어 배선 코드가 사라진다.
    /// </summary>
    public abstract class UiStateBase : MonoBehaviour, IState
    {
        [Header("포커스")]
        [Tooltip("이 화면이 스택 맨 위가 됐을 때 선택될 버튼")]
        [SerializeField] private GameObject firstSelected;

        public GameObject FirstSelected => firstSelected;

        /// <summary>ScreenManager가 자신을 넘겨준다. 상태가 전환을 요청할 때 사용한다.</summary>
        protected ScreenManager Screens { get; private set; }

        public void Bind(ScreenManager screens)
        {
            Screens = screens;
        }

        // 아무것도 하지 않는 화면이 많으므로 virtual 빈 구현을 기본으로 둔다.
        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void OnCancel() { }
    }
}
