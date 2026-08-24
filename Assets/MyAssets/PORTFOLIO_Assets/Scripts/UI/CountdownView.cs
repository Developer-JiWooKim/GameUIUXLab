using System.Threading;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// Screen_Play 중앙 오버레이 문구. 시작 카운트다운과 영업 종료 알림을 담당한다.
    /// Screen_Play 에 붙이고 CountPanel / CountText 를 꽂는다.
    ///
    /// CountPanel 자신이 아니라 Screen_Play 에 붙이는 이유: 패널을 꺼야 하는데
    /// 꺼진 오브젝트에 붙어 있으면 다시 켜 줄 주체가 사라진다.
    ///
    /// 시작과 종료가 같은 패널을 쓰는 이유: 둘 다 "판이 아직/이미 진행 중이 아니다" 를 알리는
    /// 같은 성격의 오버레이다. 패널을 따로 두면 위치·크기·폰트를 두 번 맞춰야 한다.
    /// </summary>
    public class CountdownView : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] private GameObject countPanel;
        [SerializeField] private TextMeshProUGUI countText;

        [Header("시작 카운트다운")]
        [SerializeField] private string[] introSteps = { "3", "2", "1", "Start!" };
        [SerializeField] private float introStepSeconds = 0.7f;

        [Header("영업 종료")]
        [SerializeField] private string timeoutMessage = "Timeout!!";

        [Tooltip("이 문구를 보여준 뒤 결과 화면으로 전환한다. 전환 자체의 페이드는 ScreenFade 가 맡는다")]
        [SerializeField] private float timeoutSeconds = 1f;

        // 화면이 켜질 때 패널이 남아 있으면 안 된다. Enter() 보다 먼저 불린다.
        private void OnEnable() => SetPanelActive(false);

        /// <summary>
        /// 3 → 2 → 1 → Start! 를 보여주고 패널을 끈다.
        /// 중간에 화면을 떠나면 token 이 취소되어 OperationCanceledException 이 나간다.
        /// </summary>
        public async Awaitable PlayIntro(CancellationToken token)
        {
            await ShowSteps(introSteps, introStepSeconds, true, token);
        }

        /// <summary>
        /// Timeout!! 을 보여준다. 끝난 뒤 패널을 끄지 않는 것이 의도다 —
        /// 여기서 끄면 페이드가 시작되기 전 한 프레임 동안 플레이 화면이 드러난다.
        /// 패널은 Screen_Play 가 꺼질 때 함께 사라지고, 다시 들어올 때 OnEnable 이 정리한다.
        /// </summary>
        public async Awaitable PlayTimeout(CancellationToken token)
        {
            await ShowSteps(new[] { timeoutMessage }, timeoutSeconds, false, token);
        }

        private async Awaitable ShowSteps(string[] steps, float secondsPerStep, bool hideWhenDone,
            CancellationToken token)
        {
            if (countPanel == null || countText == null || steps == null)
            {
                return;
            }

            // 오브젝트가 파괴되는 경우까지 같이 끊어야 파괴된 패널을 건드리지 않는다.
            using CancellationTokenSource linked =
                CancellationTokenSource.CreateLinkedTokenSource(token, destroyCancellationToken);

            SetPanelActive(true);

            try
            {
                foreach (string step in steps)
                {
                    countText.text = step;
                    await Awaitable.WaitForSecondsAsync(secondsPerStep, linked.Token);
                }
            }
            finally
            {
                // 취소로 빠져나갈 때도 패널은 반드시 꺼져야 한다.
                if (hideWhenDone)
                {
                    SetPanelActive(false);
                }
            }
        }

        private void SetPanelActive(bool active)
        {
            if (countPanel != null)
            {
                countPanel.SetActive(active);
            }
        }
    }
}
