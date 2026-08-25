using System.Threading;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    /// <summary>
    /// Screen_Play 중앙 오버레이 문구. 시작 카운트다운과 게임 종료 알림을 담당.
    /// </summary>
    public sealed class CountdownView : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] private GameObject countPanel;
        [SerializeField] private TextMeshProUGUI countText;

        [Header("시작 카운트다운")]
        [SerializeField] private string[] introSteps = { "3", "2", "1", "Start!" };
        [SerializeField] private float introStepSeconds = 0.7f;

        [Header("게임 종료")]
        [SerializeField] private string timeoutMessage = "Timeout!!";

        [Tooltip("이 문구를 보여준 뒤 결과 화면으로 전환. 전환 자체의 페이드는 ScreenFade가 담당.")]
        [SerializeField] private float timeoutSeconds = 1f;

        private void OnEnable() => SetPanelActive(false);

        /// <summary>
        /// 3 → 2 → 1 → Start! 를 보여주고 패널을 끔.
        /// 중간에 화면을 떠나면 token 이 취소되어 OperationCanceledException.
        /// </summary>
        public async Awaitable PlayIntro(CancellationToken token)
        {
            await ShowSteps(introSteps, introStepSeconds, true, token);
        }

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
