using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Screens
{
    public interface IState
    {
        GameObject FirstSelected { get; }

        void Enter();
        void Exit();
        void OnCancel();
    }
}
