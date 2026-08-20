namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UIFSM
{
    public class PausePanel : BasePanel
    {
        public override PanelName PanelName => PanelName.Pause;
        public override void Enter()
        {
            UnityEngine.Debug.Log("Pause Panel 진입");
        }
        public override void Exit()
        {
            UnityEngine.Debug.Log("Pause Panel 퇴장");
        }
    }
}
