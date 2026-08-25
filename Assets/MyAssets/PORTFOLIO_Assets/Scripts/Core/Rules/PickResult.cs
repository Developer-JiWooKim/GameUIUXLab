namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core.Rules
{
    /// <summary>
    /// 플레이어가 케이크 버튼을 하나를 눌렀을 때, 그 항목 하나 판정 체크 enum.
    /// </summary>
    public enum PickResult
    {
        /// <summary>채워야 될 항목에 해당하는 경우.</summary>
        Accepted,

        /// <summary>성공.</summary>
        Completed,

        /// <summary>실패.</summary>
        Rejected
    }
}
