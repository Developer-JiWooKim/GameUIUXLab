namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// Screen_Title 에 붙인다. FirstSelected = MenuButton_Start
    ///
    /// 버튼 연결(인스펙터 onClick):
    ///   MenuButton_Start → ScreenManager.ShowPlay()
    ///   MenuButton_Quit  → ScreenManager.Quit()
    /// </summary>
    public class TitleState : UiStateBase
    {
        public override void OnCancel()
        {
            // 타이틀에서 Esc는 무시한다.
            // 실수로 누른 Esc가 게임을 종료시키면 되돌릴 방법이 없다.
        }
    }
}
