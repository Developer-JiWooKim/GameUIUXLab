using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Screens
{
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
