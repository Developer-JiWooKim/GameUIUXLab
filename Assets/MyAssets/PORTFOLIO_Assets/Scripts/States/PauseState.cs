namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// Screen_Pause 에 붙인다. FirstSelected = ResumeButton
    ///
    /// Play 위에 Push로 겹쳐 올라간다. Screen_Play는 켜진 채로 남아야 뒤에 게임 화면이 비쳐
    /// "일시정지"로 읽힌다. 다만 화면이 보이더라도 시계는 멈춰야 한다.
    ///
    /// 버튼 연결(인스펙터 onClick):
    ///   ResumeButton   → ScreenManager.HidePause()
    ///   GoTitleButton  → ScreenManager.OpenConfirm()
    /// </summary>
    public class PauseState : UiStateBase
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
            //   Pop으로 Play에 돌아가는 경우다. Title로 나가는 경우에도 불리지만
            //   PlayState.Exit()이 뒤이어 다시 false로 만들므로 문제되지 않는다.
        }

        public override void OnCancel()
        {
            Screens.HidePause();
        }
    }
}
