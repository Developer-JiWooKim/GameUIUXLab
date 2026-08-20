namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UIFSM
{
    public class ResultPanel : BasePanel
    {
        public override PanelName PanelName => PanelName.Result;

        public override void Enter()
        {
            UnityEngine.Debug.Log("Result Panel 진입");
        }

        public override void Exit()
        {
            UnityEngine.Debug.Log("Result Panel 퇴장");
        }
    }
}
