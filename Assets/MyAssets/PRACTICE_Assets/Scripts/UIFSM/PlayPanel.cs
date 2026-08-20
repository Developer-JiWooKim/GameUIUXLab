namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UIFSM
{
    public class PlayPanel : BasePanel
    {
        public override PanelName PanelName => PanelName.Play;

        public override void Enter()
        {
            UnityEngine.Debug.Log("Play Panel 진입");
        }

        public override void Exit()
        {
            UnityEngine.Debug.Log("Play Panel 퇴장");
        }
    }
}
