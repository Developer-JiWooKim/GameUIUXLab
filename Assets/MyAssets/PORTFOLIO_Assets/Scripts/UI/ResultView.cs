using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// 최종 결과를 표시한다. Screen_Result 에 붙인다.
    ///
    /// 이벤트를 구독하지 않는다. 결과 화면은 켜지는 순간 한 번만 그리면 되고, 그 시점에
    /// GamePlayController 의 값은 이미 확정되어 더 변하지 않는다.
    ///
    /// 이게 성립하는 이유는 GamePlay 가 Screen_Play 의 자식이 아니기 때문이다.
    /// 데이터 소유자가 화면과 함께 꺼지면 여기서 읽을 값이 남아 있지 않다.
    /// </summary>
    public class ResultView : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GamePlayController gamePlay;
        [SerializeField] private RankTable rankTable;

        [Header("연결")]
        [SerializeField] private TextMeshProUGUI successText;
        [SerializeField] private TextMeshProUGUI failText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI rankText;

        [Header("문구")]
        [SerializeField] private string successPrefix = "성공 : ";
        [SerializeField] private string failPrefix = "실패 : ";
        [SerializeField] private string scorePrefix = "점수 : ";

        private void OnEnable()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": ResultView 에 GamePlayController 가 꽂히지 않았습니다.", this);
                return;
            }

            if (successText != null)
            {
                successText.text = successPrefix + gamePlay.SuccessCount;
            }

            if (failText != null)
            {
                failText.text = failPrefix + gamePlay.FailCount;
            }

            if (scoreText != null)
            {
                scoreText.text = scorePrefix + gamePlay.Score;
            }

            ApplyRank();
        }

        /// <summary>
        /// 등급 판정 규칙을 여기에 두지 않는다. if (score > 500) "A" 같은 하드코딩은 값을 바꿀
        /// 때마다 코드를 고쳐야 하고, 무엇보다 View 가 판단을 하게 된다.
        ///
        /// 등급은 점수가 아니라 성공·실패 건수로 계산한다. 두 값을 하나로 합치지 말 것.
        /// </summary>
        private void ApplyRank()
        {
            if (rankText == null)
            {
                return;
            }

            if (rankTable == null)
            {
                Debug.LogWarning(name + ": ResultView 에 RankTable 이 꽂히지 않았습니다.", this);
                return;
            }

            RankRule rule = rankTable.Evaluate(gamePlay.SuccessCount, gamePlay.FailCount);

            rankText.text = rule.label;

            // 등급별로 색이 달라지는 것이 요건 7 의 "색 변화" 피드백 항목이다.
            rankText.color = rule.color;
        }
    }
}
