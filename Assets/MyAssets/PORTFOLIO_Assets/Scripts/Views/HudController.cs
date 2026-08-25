using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    public sealed class HudController : MonoBehaviour
    {
        [Header("GamePlayController")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("Score 텍스트")]
        [SerializeField] private TextMeshProUGUI scoreValueText;

        [Tooltip("CurrentSlotValueText — 성공 / 실패 건수")]
        [SerializeField] private TextMeshProUGUI countValueText;

        [Header("타이머")]
        [SerializeField] private Slider timerGauge;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("남은 시간 경고")]
        [Tooltip("Progress Slider_Yellow / Fill Area / Fill")]
        [SerializeField] private Image gaugeFillImage;

        [SerializeField] private Color dangerColor = new Color(0.85f, 0.23f, 0.23f);

        [Tooltip("이 시간 이하로 남으면 경고 표시로 바뀐다(초)")]
        [SerializeField] private float dangerTimeThreshold = 10f;

        [Header("초당 깜빡임 횟수 = pulseSpeed / 2")]
        [Range(0f, 6f)]
        [SerializeField] private float pulseSpeed = 2f;

        // 마지막으로 화면에 쓴 정수 초. -1 은 "아직 아무것도 안 썼다" 는 뜻.
        private int lastDisplayedSecond = -1;

        // 경고 상태가 바뀐 순간에만 글자 색을 변경. -1 은 아직 모름.
        private int lastDangerState = -1;

        // Default Color
        private Color gaugeNormalColor = Color.white;
        private Color timerNormalColor = Color.white;

        private void Awake()
        {
            if (timerGauge != null)
            {
                timerGauge.interactable = false;
            }

            if (gaugeFillImage != null)
            {
                gaugeNormalColor = gaugeFillImage.color;
            }

            if (timerText != null)
            {
                timerNormalColor = timerText.color;
            }
        }

        private void OnEnable()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": HudController 에 GamePlayController 가 꽂히지 않았습니다.", this);
                return;
            }

            lastDisplayedSecond = -1;
            lastDangerState = -1;
            gamePlay.OnTimeChanged += HandleTimeChanged;
            gamePlay.OnScoreChanged += HandleScoreChanged;
            gamePlay.OnCountChanged += HandleCountChanged;

            HandleTimeChanged(gamePlay.RemainingTime, gamePlay.PlayTimeSeconds);
            HandleScoreChanged(gamePlay.Score);
            HandleCountChanged(gamePlay.SuccessCount, gamePlay.FailCount);
        }

        private void OnDisable()
        {
            if (gamePlay != null)
            {
                gamePlay.OnTimeChanged -= HandleTimeChanged;
                gamePlay.OnScoreChanged -= HandleScoreChanged;
                gamePlay.OnCountChanged -= HandleCountChanged;
            }
        }

        private void HandleScoreChanged(int score)
        {
            if (scoreValueText != null)
            {
                scoreValueText.text = score.ToString();
            }
        }

        private void HandleCountChanged(int successCount, int failCount)
        {
            if (countValueText != null)
            {
                countValueText.text = successCount + " / " + failCount;
            }
        }

        private void HandleTimeChanged(float remainingTime, float totalTime)
        {
            if (timerGauge != null)
            {
                // Slider 의 MaxValue 가 1 이므로 비율을 그대로 넣는다. 매 프레임 갱신해도 된다.
                timerGauge.value = totalTime > 0f ? remainingTime / totalTime : 0f;
            }

            ApplyTimeWarning(remainingTime);

            int second = Mathf.CeilToInt(remainingTime);
            if (second == lastDisplayedSecond)
            {
                return;
            }

            lastDisplayedSecond = second;

            if (timerText != null)
            {
                timerText.text = second.ToString();
            }
        }

        private void ApplyTimeWarning(float remainingTime)
        {
            bool isDanger = remainingTime <= dangerTimeThreshold;

            if (gaugeFillImage != null)
            {
                gaugeFillImage.color = isDanger
                    ? Color.Lerp(gaugeNormalColor, dangerColor, Mathf.PingPong(remainingTime * pulseSpeed, 1f))
                    : gaugeNormalColor;
            }

            int dangerState = isDanger ? 1 : 0;
            if (dangerState == lastDangerState)
            {
                return;
            }

            lastDangerState = dangerState;

            if (timerText != null)
            {
                timerText.color = isDanger ? dangerColor : timerNormalColor;
            }
        }
    }
}
