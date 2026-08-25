using System;
using System.Threading;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    /// <summary>
    /// 판정 결과(성공/실패)를 알리는 Toast 메세지를 활성화/비활성화하는 컨트롤러.
    ///
    /// 판정 잠금(0.8초)이 표시 시간(0.5초)보다 길어 Toast가 동시에 둘 존재할 수 없으므로, 인스턴스 하나를 켜고 끄면 충분.
    /// 생성·파괴가 없으니 오브젝트 풀링 X.
    /// </summary>
    public sealed class ToastController : MonoBehaviour
    {
        [Header("GamePlayController")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("ToastMessage 오브젝트")]
        [SerializeField] private GameObject root;

        [SerializeField] private Image backgroundImage;

        [SerializeField] private TextMeshProUGUI messageText;

        [Header("문구")]
        [SerializeField] private string successMessage = "성공!";
        [SerializeField] private string failMessage = "실패..";

        [Header("성공 색")]
        [SerializeField] private Color successColor = new Color(0.75f, 0.95f, 0.78f);
        [SerializeField] private Color successTextColor = new Color(0.12f, 0.42f, 0.20f);

        [Header("실패 색")]
        [SerializeField] private Color failColor = new Color(0.99f, 0.80f, 0.78f);
        [SerializeField] private Color failTextColor = new Color(0.64f, 0.13f, 0.13f);

        [Header("Toast 표시 시간")]
        [SerializeField] private float showSeconds = 0.5f;

        private CancellationTokenSource hideCts;

        private void OnEnable()
        {
            SetVisible(false);

            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": ToastController 에 GamePlayController 가 꽂히지 않았습니다.", this);
                return;
            }

            if (root == null)
            {
                Debug.LogWarning(name + ": ToastController 에 root 가 꽂히지 않았습니다.", this);
            }

            gamePlay.OnJudged += HandleJudged;
        }

        private void OnDisable()
        {
            if (gamePlay != null)
            {
                gamePlay.OnJudged -= HandleJudged;
            }

            CancelHide();
            SetVisible(false);
        }

        private void OnDestroy() => CancelHide();

        private void HandleJudged(bool success)
        {
            if (messageText != null)
            {
                messageText.text = success ? successMessage : failMessage;
                messageText.color = success ? successTextColor : failTextColor;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = success ? successColor : failColor;
            }

            SetVisible(true);
            RunHide();
        }

        private async void RunHide()
        {
            CancelHide(); // 이전 대기를 반드시 끊어야 됨. 안 끊으면 두 번째 Toast가 첫 번째의 타이머에 걸려 일찍 사라짐.

            // 오브젝트가 파괴되는 경우까지 같이 끊어 파괴된 Toast를 건드리지 않게.
            hideCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            try
            {
                await Awaitable.WaitForSecondsAsync(showSeconds, hideCts.Token);
            }
            catch (OperationCanceledException)
            {
                // 새 토스트가 덮었거나 화면을 떠남.
                return;
            }

            SetVisible(false);
        }

        private void CancelHide()
        {
            if (hideCts == null)
            {
                return;
            }

            hideCts.Cancel();
            hideCts.Dispose();
            hideCts = null;
        }

        private void SetVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }
    }
}
