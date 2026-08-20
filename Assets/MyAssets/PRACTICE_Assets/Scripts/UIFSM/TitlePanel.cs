namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UIFSM
{
    public class TitlePanel : BasePanel
    {
        public override PanelName PanelName => PanelName.Title;

        public override void Enter()
        {
            UnityEngine.Debug.Log("Title Panel 진입");
        }

        public override void Exit()
        {
            UnityEngine.Debug.Log("Title Panel 퇴장");
        }
    }
}
