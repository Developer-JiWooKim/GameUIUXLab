namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UIFSM
{
  public enum PanelName
  {
    Title,
    Option,
    Play,
    Pause,
    Result,
  }

  public interface IStateUI
  {
    void Enter();
    void Exit();
  }
}
