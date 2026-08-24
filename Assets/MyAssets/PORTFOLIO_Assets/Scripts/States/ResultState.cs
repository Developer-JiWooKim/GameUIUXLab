namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// Screen_Result 에 붙인다. FirstSelected = ReplayButton
    ///
    /// 최종 값 표시는 ResultView 가 OnEnable 에서 처리한다.
    /// 결과 화면은 켜지는 순간 한 번만 그리면 되므로 이벤트 구독이 필요 없다.
    ///
    /// 버튼 onClick 배선:
    ///   ReplayButton  → ScreenFlowController.ShowPlay()   ← Set 이므로 PlayState.Enter() 가 다시 불린다
    ///   GoTitleButton → ScreenFlowController.ShowTitle()
    /// </summary>
    public class ResultState : UIStateBase
    {
        public override void OnCancel()
        {
            // 결과 화면에서 Esc 는 무시한다. 다시하기 / 타이틀로 중 하나를 명시적으로 고르게 한다.
        }
    }
}
