using System;
using System.Threading;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Screens
{
    public sealed class PlayState : UIStateBase
    {
        [Header("GamePlayController")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("오버레이 문구")]
        [SerializeField] private CountdownView countdown;

        [Header("연출 중 잠글 일시정지 버튼")]
        [SerializeField] private Button pauseButton;

        private CancellationTokenSource sequenceCts;

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

            gamePlay.Prepare();

            RunIntro();
        }

        public override void Exit()
        {
            CancelSequence();

            if (gamePlay == null)
            {
                return;
            }

            gamePlay.OnGameOver -= HandleGameOver;

            gamePlay.SetRunning(false);
        }

        public override void OnCancel()
        {
            // 시작 카운트다운 중 or 종료 알림 중에는 일시정지 X
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
                return;
            }
            finally
            {
                EndSequence();
            }

            gamePlay.StartGame();
        }

        /// <summary>
        /// 제한 시간 종료. Timeout!! 을 1초 보여준 뒤 페이드로 넘김.
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
        /// 연출 중 잠금. 
        /// CountPanel 이 화면 전체를 덮어 클릭은 이미 막히지만, 키보드·게임패드 Submit 은 패널을 통과하므로 버튼 자체를 잠가야 됨.
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
