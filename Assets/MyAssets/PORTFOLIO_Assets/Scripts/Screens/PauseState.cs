using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Screens
{
    public sealed class PauseState : UIStateBase
    {
        [Header("GamePlayController")]
        [SerializeField] private GamePlayController gamePlay;

        private void Awake()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": PauseState 에 GamePlayController 가 꽂히지 않았습니다.", this);
            }
        }

        public override void Enter()
        {
            if (gamePlay != null)
            {
                gamePlay.SetRunning(false);
            }
        }

        public override void Exit()
        {
            if (gamePlay == null)
            {
                return;
            }

            gamePlay.SetRunning(true);
        }

        public override void OnCancel()
        {
            Screens.HidePause();
        }
    }
}
