using System.Threading;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// 시작 카운트다운. Screen_Play 에 붙이고 CountPanel / CountText 를 꽂는다.
    ///
    /// CountPanel 자신이 아니라 Screen_Play 에 붙이는 이유: 패널을 꺼야 하는데
    /// 꺼진 오브젝트에 붙어 있으면 다시 켜 줄 주체가 사라진다.
    /// </summary>
    public class CountdownView : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] private GameObject countPanel;
        [SerializeField] private TextMeshProUGUI countText;

        [Header("설정")]
        [SerializeField] private string[] steps = { "3", "2", "1", "Start!" };
        [SerializeField] private float stepSeconds = 0.7f;

        // 화면이 켜질 때 패널이 남아 있으면 안 된다. Enter() 보다 먼저 불린다.
        private void OnEnable() => SetPanelActive(false);

        /// <summary>
        /// 3 → 2 → 1 → Start! 를 보여주고 패널을 끈다.
        /// 중간에 화면을 떠나면 token 이 취소되어 OperationCanceledException 이 나간다.
        /// </summary>
        public async Awaitable Play(CancellationToken token)
        {
            if (countPanel == null || countText == null)
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
                    await Awaitable.WaitForSecondsAsync(stepSeconds, linked.Token);
                }
            }
            finally
            {
                // 취소로 빠져나갈 때도 패널은 반드시 꺼져야 한다.
                SetPanelActive(false);
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
