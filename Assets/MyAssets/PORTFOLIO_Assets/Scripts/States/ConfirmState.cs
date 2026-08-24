namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    public class ConfirmState : UIStateBase
    {
        public override void OnCancel() => Screens.CloseConfirm();
    }
}
