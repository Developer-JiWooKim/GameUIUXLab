using System;
using System.Threading;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// Screen_Play 에 붙인다. FirstSelected = CakeButton_1
    ///
    /// Enter() 가 새 판을 시작하는 지점이다. Pause 에서 돌아올 때는 Pop 이라 Enter() 가
    /// 다시 불리지 않으므로, 여기에 시작 처리를 두어도 게임이 재시작되지 않는다.
    /// (평면 FSM 이었다면 여기서 재시작 버그가 났을 자리다.)
    ///
    /// 진입 순서: Prepare(초기화, 시계 정지) → 카운트다운 → StartGame(시계 시작)
    ///
    /// 버튼 onClick 배선:
    ///   PauseButton → ScreenFlowController.ShowPause()
    /// </summary>
    public class PlayState : UIStateBase
    {
        [Header("게임 데이터")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("시작 카운트다운")]
        [SerializeField] private CountdownView countdown;

        [Tooltip("카운트다운 동안 잠글 일시정지 버튼. 비워도 동작한다")]
        [SerializeField] private Button pauseButton;

        private CancellationTokenSource introCts;
        private bool isCountingDown;

        private void Awake()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": PlayState 에 GamePlayController 가 꽂히지 않았습니다.", this);
            }
        }

        private void OnDestroy() => CancelIntro();

        public override void Enter()
        {
            if (gamePlay == null)
            {
                return;
            }

            gamePlay.OnGameOver += HandleGameOver;

            // 시계는 멈춘 채로 값만 맞춰 둔다. 카운트다운 동안 HUD 가 120초를 보여준다.
            gamePlay.Prepare();

            RunIntro();
        }

        public override void Exit()
        {
            // 카운트다운 도중에 나가면, 남아 있던 대기가 끝난 뒤 StartGame() 이 불려
            // 타이틀 화면에서 시계가 도는 상태가 된다. 반드시 먼저 끊는다.
            CancelIntro();

            if (gamePlay == null)
            {
                return;
            }

            gamePlay.OnGameOver -= HandleGameOver;

            // Result 로 가든 Title 로 가든, 어느 경로로 나가도 시계가 확실히 멈추게 한다.
            gamePlay.SetRunning(false);
        }

        public override void OnCancel()
        {
            // 카운트다운 중 일시정지를 허용하면 팝업 뒤에서 숫자가 계속 흐르고,
            // 닫는 순간 카운트가 끝나 버린다. 3초는 그냥 기다리게 한다.
            if (isCountingDown)
            {
                return;
            }

            Screens.ShowPause();
        }

        private async void RunIntro()
        {
            CancelIntro();
            introCts = new CancellationTokenSource();

            isCountingDown = true;
            SetPauseInteractable(false);

            try
            {
                if (countdown != null)
                {
                    await countdown.Play(introCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 화면을 떠났다. 시계를 돌리면 안 되므로 여기서 끝낸다.
                return;
            }
            finally
            {
                isCountingDown = false;
                SetPauseInteractable(true);
            }

            gamePlay.StartGame();
        }

        private void CancelIntro()
        {
            if (introCts == null)
            {
                return;
            }

            introCts.Cancel();
            introCts.Dispose();
            introCts = null;
        }

        /// <summary>
        /// 카운트다운 중 잠금. CountPanel 이 화면 전체를 덮어 클릭은 이미 막히지만,
        /// 키보드·게임패드 Submit 은 패널을 통과하므로 버튼 자체를 잠가야 한다.
        /// </summary>
        private void SetPauseInteractable(bool interactable)
        {
            if (pauseButton != null)
            {
                pauseButton.interactable = interactable;
            }
        }

        /// <summary>
        /// 영업시간 종료. 화면 전환을 ScreenFlowController 가 아니라 여기서 트리거하는 이유는,
        /// 화면 전환기가 게임 규칙을 구독하기 시작하면 두 책임이 섞이기 때문이다.
        /// </summary>
        private void HandleGameOver()
        {
            Screens.ShowResult();
        }
    }
}
