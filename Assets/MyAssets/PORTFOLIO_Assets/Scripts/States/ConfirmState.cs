namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// ConfirmPopup 에 붙인다. FirstSelected = NoButton
    ///
    /// 되돌릴 수 없는 선택이므로 기본 포커스를 YesButton에 두지 않는다.
    /// Pause 위에 Push로 겹치며, Esc는 팝업만 닫고 Pause는 남긴다.
    ///
    /// 버튼 연결(인스펙터 onClick):
    ///   NoButton  → ScreenManager.CloseConfirm()
    ///   YesButton → ScreenManager.ShowTitle()   ← Set이므로 스택이 통째로 비워진다
    /// </summary>
    public class ConfirmState : UiStateBase
    {
        public override void OnCancel()
        {
            Screens.CloseConfirm();
        }
    }
}
