using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// Screen_Pause 에 붙인다. FirstSelected = ResumeButton
    ///
    /// Play 위에 Push 로 겹친다. 아래층 PlayState 는 Exit() 되지 않으므로
    /// 여기서는 시계만 세웠다 다시 돌리면 된다.
    ///
    /// 버튼 onClick 배선:
    ///   ResumeButton  → ScreenFlowController.HidePause()
    ///   GoTitleButton → ScreenFlowController.OpenConfirm()
    /// </summary>
    public class PauseState : UIStateBase
    {
        [Header("게임 데이터")]
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
                // Time.timeScale = 0 을 쓰지 않는다. 토스트 소멸 연출까지 같이 멈춘다.
                gamePlay.SetRunning(false);
            }
        }

        public override void Exit()
        {
            if (gamePlay == null)
            {
                return;
            }

            // Pop 으로 Play 에 돌아가는 경우다. Title 로 나가는 경우에도 불리지만
            // 뒤이어 PlayState.Exit() 이 다시 false 로 만들므로 결과는 맞다.
            // 단 이 순서에 의존하므로, SetScreen 은 반드시 위에서부터 역순으로 닫아야 한다.
            gamePlay.SetRunning(true);
        }

        public override void OnCancel()
        {
            Screens.HidePause();
        }
    }
}
