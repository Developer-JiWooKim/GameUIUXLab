namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Screens
{
    public sealed class ConfirmState : UIStateBase
    {
        public override void OnCancel() => Screens.CloseConfirm();
    }
}
