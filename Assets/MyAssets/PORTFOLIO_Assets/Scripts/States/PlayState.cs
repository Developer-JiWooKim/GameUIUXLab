namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// Screen_Play 에 붙인다. FirstSelected = CakeButton_1
    ///
    /// Enter() 가 새 판을 시작하는 지점이다. Pause 에서 돌아올 때는 Pop 이라 Enter() 가
    /// 다시 불리지 않으므로, 여기에 StartGame() 을 두어도 게임이 재시작되지 않는다.
    /// (평면 FSM 이었다면 여기서 재시작 버그가 났을 자리다.)
    ///
    /// 버튼 onClick 배선:
    ///   PauseButton → ScreenManager.ShowPause()
    /// </summary>
    public class PlayState : UIStateBase
    {
        // TODO: GameState 작성 후 참조 연결
        // [SerializeField] private GameState gameState;

        public override void Enter()
        {
            // TODO: gameState.StartGame();
            //   점수·시간·성공/실패·쟁반·remaining 초기화 + 첫 손님 생성 + 이벤트 발행
        }

        public override void Exit()
        {
            // TODO: gameState.SetRunning(false);
            //   Result 로 가든 Title 로 가든, 어느 경로로 나가도 시계가 확실히 멈추게 한다.
        }

        public override void OnCancel()
        {
            Screens.ShowPause();
        }
    }
}
