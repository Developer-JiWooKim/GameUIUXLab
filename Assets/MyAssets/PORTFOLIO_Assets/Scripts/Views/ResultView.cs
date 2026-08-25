using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    public sealed class ResultView : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GamePlayController gamePlay;
        [SerializeField] private RankTable rankTable;

        [Header("결과 텍스트(TMP)")]
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
            rankText.color = rule.color;
        }
    }
}
