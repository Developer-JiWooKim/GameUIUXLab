using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// GamePlayController 의 값을 읽어 HUD 에 반영한다. TopNav 에 붙인다.
    ///
    /// 여기서는 데이터를 소유하지 않는다. 점수·남은 시간을 아는 것은 GamePlayController 이고,
    /// 이 스크립트는 이벤트를 받아 표시만 한다.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("점수 · 처리 수")]
        [SerializeField] private TextMeshProUGUI scoreValueText;

        [Tooltip("CurrentSlotValueText — 성공 / 실패 건수")]
        [SerializeField] private TextMeshProUGUI countValueText;

        [Header("영업시간")]
        [SerializeField] private Slider timerGauge;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("남은 시간 경고")]
        [Tooltip("Progress Slider_Yellow / Fill Area / Fill")]
        [SerializeField] private Image gaugeFillImage;

        [SerializeField] private Color dangerColor = new Color(0.85f, 0.23f, 0.23f);

        [Tooltip("이 시간 이하로 남으면 경고 표시로 바뀐다(초)")]
        [SerializeField] private float dangerTimeThreshold = 10f;

        [Tooltip("초당 깜빡임 횟수 = pulseSpeed / 2. 초당 3회(pulseSpeed 6)를 넘는 점멸은 "
            + "광과민성 발작 유발 기준(WCAG 2.3.1)에 걸리므로 상한을 6으로 막아 둔다")]
        [Range(0f, 6f)]
        [SerializeField] private float pulseSpeed = 2f;

        // 마지막으로 화면에 쓴 정수 초. -1 은 "아직 아무것도 안 썼다" 는 뜻이다.
        private int lastDisplayedSecond = -1;

        // 경고 상태가 바뀐 순간에만 글자 색을 만지려고 들고 있는다. -1 은 아직 모름.
        private int lastDangerState = -1;

        // 평상시 색은 인스펙터에 또 적지 않고 씬에 배치된 값을 그대로 쓴다.
        // 두 군데에 적어 두면 프리팹 색을 바꿨을 때 코드가 옛 색으로 되돌려 놓는다.
        private Color gaugeNormalColor = Color.white;
        private Color timerNormalColor = Color.white;

        private void Awake()
        {
            // 게이지는 표시 전용이다. Slider 는 Selectable 이라 interactable 이 켜져 있으면
            // 게임패드·키보드 Navigate 가 여기에 멈추고, 좌우 입력이 남은 시간을 바꿔 버린다.
            // 마우스로 게이지를 클릭해도 값이 튄다.
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

        // TopNav 는 Screen_Play 의 자식이라 화면이 켜질 때 같이 켜진다.
        // 구독을 Awake 가 아닌 OnEnable 에 두면 Play 화면일 때만 이벤트를 받는다.
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

            // 화면이 켜지는 시점의 값을 한 번 그려 둔다.
            // PlayState.Enter() 의 Prepare() 보다 OnEnable 이 먼저지만, 다시하기처럼
            // 이미 값이 있는 상태로 들어오는 경로에서는 이게 없으면 이전 값이 남는다.
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

        // 점수와 건수는 판정이 끝날 때만 바뀐다. 초당 한 번도 아니므로 그때마다 문자열을 만들어도 된다.
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

            // 텍스트는 정수 초가 바뀔 때만 만든다. 매 프레임 ToString() 하면
            // 눈에 보이는 변화 없이 프레임마다 문자열이 새로 생긴다.
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

        /// <summary>
        /// 남은 시간이 얼마 없다는 것을 게이지 색으로 알린다.
        ///
        /// 점멸 위상을 Time.time 이 아니라 remainingTime 으로 잡는 것이 핵심이다.
        /// Time.time 을 쓰면 일시정지 중에도 게이지가 계속 깜빡여서, 게임은 멈췄는데
        /// HUD 만 살아 움직이는 상태가 된다. remainingTime 을 쓰면 Tick 이 멈추는 순간
        /// 이벤트도 멈추므로 색이 그 자리에 얼어붙는다. 일시정지 대응이 따라온다.
        /// </summary>
        private void ApplyTimeWarning(float remainingTime)
        {
            bool isDanger = remainingTime <= dangerTimeThreshold;

            if (gaugeFillImage != null)
            {
                gaugeFillImage.color = isDanger
                    ? Color.Lerp(gaugeNormalColor, dangerColor, Mathf.PingPong(remainingTime * pulseSpeed, 1f))
                    // 평상시 색을 여기서 반드시 다시 대입한다.
                    // 빠뜨리면 다시하기를 눌러도 게이지가 빨간 채로 남는다.
                    : gaugeNormalColor;
            }

            // 숫자는 점멸시키지 않는다. 마지막 10초에 사용자가 하는 일은 숫자를 읽는 것인데,
            // 글자 색이 계속 변하면 대비가 흔들려 오히려 읽기 어려워진다.
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
