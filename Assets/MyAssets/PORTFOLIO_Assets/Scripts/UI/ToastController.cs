using System;
using System.Threading;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// 판정 결과를 짧은 문구로 알린다. ChoiceListPanel 에 붙인다.
    ///
    /// 토스트 오브젝트 자신에게 붙이지 않는 이유: 평상시 꺼져 있는 오브젝트는 OnEnable 이
    /// 불리지 않아 OnJudged 를 구독할 기회가 없다. 켜 줄 주체가 꺼진 채로 기다리는 셈이다.
    /// CountdownView 가 CountPanel 이 아니라 Screen_Play 에 붙는 것과 같은 이유다.
    ///
    /// 프리팹으로 만들지 않는다. 판정 잠금(0.8초)이 표시 시간(0.5초)보다 길어 토스트가
    /// 동시에 둘 존재할 수 없으므로, 인스턴스 하나를 켜고 끄면 충분하다.
    /// 생성·파괴가 없으니 오브젝트 풀 이야기도 나오지 않는다.
    /// </summary>
    public class ToastController : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("연결")]
        [Tooltip("ChoiceListPanel 아래의 ToastMessage. 배경과 글자를 함께 켜고 끈다. 비활성 상태로 저장할 것")]
        [SerializeField] private GameObject root;

        [Tooltip("ToastMessage 의 Image(Splash4). Preserve Aspect 를 켜고 Raycast Target 은 꺼 둔다")]
        [SerializeField] private Image backgroundImage;

        [Tooltip("배경 위에 올라가는 문구. 색은 성공·실패에 따라 코드가 바꾼다")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("문구")]
        [SerializeField] private string successMessage = "성공!";
        [SerializeField] private string failMessage = "실패..";

        [Header("성공 색")]
        [Tooltip("Splash 배경에 곱해지는 색. 연하게 둘 것")]
        [SerializeField] private Color successColor = new Color(0.75f, 0.95f, 0.78f);

        [Tooltip("배경과 같은 계열의 진한 색. 배경과 완전히 같게 두면 글자가 묻힌다")]
        [SerializeField] private Color successTextColor = new Color(0.12f, 0.42f, 0.20f);

        [Header("실패 색")]
        [Tooltip("Splash 배경에 곱해지는 색. 연하게 둘 것")]
        [SerializeField] private Color failColor = new Color(0.99f, 0.80f, 0.78f);

        [Tooltip("배경과 같은 계열의 진한 색. 배경과 완전히 같게 두면 글자가 묻힌다")]
        [SerializeField] private Color failTextColor = new Color(0.64f, 0.13f, 0.13f);

        [Header("표시 시간")]
        [Tooltip("GamePlayController 의 judgeDelaySeconds(기본 0.8) 보다 짧거나 같아야 한다. "
            + "토스트가 다음 손님 주문 위로 넘어와 떠 있으면 어느 판정에 대한 알림인지 구분이 안 된다")]
        [SerializeField] private float showSeconds = 0.5f;

        private CancellationTokenSource hideCts;

        private void OnEnable()
        {
            // 영업 종료 직후에 판정이 있었다면 토스트가 뜬 채로 화면이 꺼진다.
            // 다시 들어올 때 지난 판의 문구가 남아 있지 않도록 먼저 지운다.
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

        /// <summary>
        /// 배경과 글자를 같은 색 계열로 함께 바꾼다.
        ///
        /// 배경(연한 쪽)과 글자(진한 쪽)를 따로 노출하는 이유: 두 색을 완전히 같게 두면
        /// 글자가 배경에 묻혀 사라진다. 밝기 차를 코드가 계산하지 않고 인스펙터에 맡기면,
        /// Splash 스프라이트를 바꿔도 색만 다시 잡으면 된다.
        ///
        /// 색은 보조 수단이다. 문구(성공! / 실패..)가 함께 바뀌므로 색각 이상이 있어도
        /// 판정 결과를 읽을 수 있다.
        /// </summary>
        private void HandleJudged(bool success)
        {
            if (messageText != null)
            {
                messageText.text = success ? successMessage : failMessage;
                messageText.color = success ? successTextColor : failTextColor;
            }

            if (backgroundImage != null)
            {
                // Image.color 는 스프라이트에 곱해진다. Splash4 의 밝은 부분이 이 색을 받는다.
                backgroundImage.color = success ? successColor : failColor;
            }

            SetVisible(true);
            RunHide();
        }

        // 이전 대기를 반드시 끊는다. 안 끊으면 두 번째 토스트가 첫 번째의 타이머에 걸려 일찍 사라진다.
        private async void RunHide()
        {
            CancelHide();

            // 오브젝트가 파괴되는 경우까지 같이 끊어 파괴된 토스트를 건드리지 않는다.
            hideCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            try
            {
                await Awaitable.WaitForSecondsAsync(showSeconds, hideCts.Token);
            }
            catch (OperationCanceledException)
            {
                // 새 토스트가 덮었거나 화면을 떠났다. 끄는 일은 그쪽이 맡는다.
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
