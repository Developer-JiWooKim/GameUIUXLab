namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// Screen_Title 에 붙인다. FirstSelected = MenuButton_Start
    ///
    /// 버튼 onClick 배선:
    ///   MenuButton_Start → ScreenFlowController.ShowPlay()
    ///   MenuButton_Quit  → ScreenFlowController.Quit()
    /// </summary>
    public class TitleState : UIStateBase
    {
        public override void OnCancel()
        {
            // 타이틀에서 Esc 는 무시한다.
            // 실수로 누른 Esc 가 게임을 종료시키면 되돌릴 방법이 없다.
        }
    }
}
