namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// ConfirmPopup 에 붙인다. FirstSelected = NoButton
    ///
    /// 되돌릴 수 없는 선택이므로 기본 포커스를 YesButton 에 두지 않는다.
    /// Pause 위에 Push 로 겹치며, Esc 는 팝업만 닫고 Pause 는 남긴다.
    ///
    /// 버튼 onClick 배선:
    ///   NoButton  → ScreenManager.CloseConfirm()
    ///   YesButton → ScreenManager.ShowTitle()   ← Set 이므로 스택이 통째로 비워진다
    /// </summary>
    public class ConfirmState : UIStateBase
    {
        public override void OnCancel()
        {
            Screens.CloseConfirm();
        }
    }
}
