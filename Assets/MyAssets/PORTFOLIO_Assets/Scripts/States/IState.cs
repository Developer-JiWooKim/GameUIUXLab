using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    public interface IState
    {
        GameObject FirstSelected { get; }

        void Enter();
        void Exit();
        void OnCancel();
    }

    public interface IScreenController
    {
        void ShowTitle();

        void ShowPlay();

        void ShowResult();

        void ShowPause();
        void HidePause();

        void OpenConfirm();
        void CloseConfirm();

        void Quit();
    }
}
