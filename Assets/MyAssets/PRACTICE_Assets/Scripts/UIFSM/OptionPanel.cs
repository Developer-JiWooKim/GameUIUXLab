namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UIFSM
{
    public class OptionPanel : BasePanel
    {
        public override PanelName PanelName => PanelName.Option;

        public override void Enter()
        {
            UnityEngine.Debug.Log("Option Panel 진입");
        }
        public override void Exit()
        {
            UnityEngine.Debug.Log("Option Panel 퇴장");
        }
    }
}
