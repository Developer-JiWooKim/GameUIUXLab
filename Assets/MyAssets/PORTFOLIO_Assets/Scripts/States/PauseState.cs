using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    public class PauseState : UIStateBase
    {
        // TODO: GameState 작성 후 참조 연결

        // [SerializeField] private GameState gameState;

        public override void Enter()
        {
            // TODO: gameState.SetRunning(false);
            //   Time.timeScale = 0 을 쓰지 말 것. 토스트 코루틴까지 같이 멈춘다.
        }

        public override void Exit()
        {
            // TODO: gameState.SetRunning(true);
            //   Pop 으로 Play 에 돌아가는 경우다. Title 로 나가는 경우에도 불리지만
            //   뒤이어 PlayState.Exit() 이 다시 false 로 만들므로 결과는 맞다.
            //   단 이 순서에 의존하므로, Set() 은 반드시 위에서부터 역순으로 닫아야 한다.
        }

        public override void OnCancel()
        {
            Screens.HidePause();
        }
    }
}
