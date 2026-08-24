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
    /// 종료 순서: OnGameOver → Timeout!! 1초 → ShowResult(페이드 전환)
    ///
    /// 버튼 onClick 배선:
    ///   PauseButton → ScreenFlowController.ShowPause()
    /// </summary>
    public class PlayState : UIStateBase
    {
        [Header("게임 데이터")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("오버레이 문구")]
        [SerializeField] private CountdownView countdown;

        [Header("연출 중 잠글 일시정지 버튼")]
        [SerializeField] private Button pauseButton;

        private CancellationTokenSource sequenceCts;

        // 시작 카운트다운과 종료 알림을 하나로 묶은 이유: 둘 다 "연출이 도는 동안 조작을
        // 받지 않는다" 는 같은 규칙이 적용된다. 플래그를 두 개 두면 한쪽만 검사하는 실수가 난다.
        private bool isPlayingSequence;

        private void Awake()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": PlayState 에 GamePlayController 가 꽂히지 않았습니다.", this);
            }
        }

        private void OnDestroy() => CancelSequence();

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
            CancelSequence();

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
            // 종료 알림 중에도 같다 — 이미 끝난 판을 일시정지할 이유가 없다.
            if (isPlayingSequence)
            {
                return;
            }

            Screens.ShowPause();
        }

        private async void RunIntro()
        {
            BeginSequence();

            try
            {
                if (countdown != null)
                {
                    await countdown.PlayIntro(sequenceCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 화면을 떠났다. 시계를 돌리면 안 되므로 여기서 끝낸다.
                return;
            }
            finally
            {
                EndSequence();
            }

            gamePlay.StartGame();
        }

        /// <summary>
        /// 영업시간 종료. 시간이 0 이 되는 순간 화면을 바꾸지 않는다 —
        /// 마지막 판정 결과를 볼 틈이 없고, 무엇 때문에 끝났는지도 전달되지 않는다.
        /// Timeout!! 을 1초 보여준 뒤 페이드로 넘긴다.
        ///
        /// 화면 전환을 ScreenFlowController 가 아니라 여기서 트리거하는 이유는,
        /// 화면 전환기가 게임 규칙을 구독하기 시작하면 두 책임이 섞이기 때문이다.
        /// </summary>
        private void HandleGameOver() => RunTimeout();

        private async void RunTimeout()
        {
            BeginSequence();

            try
            {
                if (countdown != null)
                {
                    await countdown.PlayTimeout(sequenceCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 알림 중에 화면을 떠났다(타이틀로 나감). 결과 화면으로 보내면 안 된다.
                return;
            }
            finally
            {
                EndSequence();
            }

            Screens.ShowResult();
        }

        private void BeginSequence()
        {
            CancelSequence();
            sequenceCts = new CancellationTokenSource();

            isPlayingSequence = true;
            SetPauseInteractable(false);
        }

        private void EndSequence()
        {
            isPlayingSequence = false;
            SetPauseInteractable(true);
        }

        private void CancelSequence()
        {
            if (sequenceCts == null)
            {
                return;
            }

            sequenceCts.Cancel();
            sequenceCts.Dispose();
            sequenceCts = null;
        }

        /// <summary>
        /// 연출 중 잠금. CountPanel 이 화면 전체를 덮어 클릭은 이미 막히지만,
        /// 키보드·게임패드 Submit 은 패널을 통과하므로 버튼 자체를 잠가야 한다.
        /// </summary>
        private void SetPauseInteractable(bool interactable)
        {
            if (pauseButton != null)
            {
                pauseButton.interactable = interactable;
            }
        }
    }
}
