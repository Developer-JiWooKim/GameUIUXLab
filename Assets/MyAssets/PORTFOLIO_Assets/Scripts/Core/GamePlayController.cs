using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core.Rules;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core
{
    public sealed class GamePlayController : MonoBehaviour
    {
        [Header("게임 플레이 시간 설정")]
        [SerializeField] private float playTimeSeconds = 120f;

        [Header("주문 생성기")]
        [SerializeField] private OrderGenerator orderGenerator;

        [Header("쟁반 슬롯 수")]
        [SerializeField] private int trayCapacity = 3;

        [Header(" 성공 시 획득 점수")]
        [SerializeField] private int scorePerSuccess = 100;

        [Header("실패 시 잃는 점수")]
        [SerializeField] private int scorePenaltyPerFail = 20;

        [Header("판정 연출")]
        [SerializeField] private float judgeDelaySeconds = 0.8f;

        // 게임 규칙. 
        // (제한 시간, 주문 판정(정확한 메뉴를 담았는가, 이번 선택이 해당 주문의 마지막 메뉴였는가 등), 성공/실패 점수 계산)
        private readonly PlayClock clock = new();
        private readonly OrderSession session = new();
        private readonly ScoreBoard scoreBoard = new();

        /// <summary>현재 게임이 진행중인지 판별 bool. Pause 구분용.</summary>
        private bool isRunning;
        private int customerNumber;

        /// <summary>주문과 쟁반에 담은 종류가 일치하는지 판정 여부.</summary>
        private bool isJudging;

        /// <summary>판정 대기가 끝났을 때 그 사이 새 판이 시작됐는지 구분하는 표식.</summary>
        private int judgeToken;

        // 외부 공개용 프로퍼티
        public float RemainingTime => clock.Remaining; // 남은 시간

        public float PlayTimeSeconds => playTimeSeconds;

        public bool IsRunning => isRunning; // 게임 진행 여부

        public int Score => scoreBoard.Score;

        public int SuccessCount => scoreBoard.SuccessCount;

        public int FailCount => scoreBoard.FailCount;

        public bool IsJudging => isJudging; // 판정 결과

        public int CustomerNumber => customerNumber;

        public IReadOnlyList<DessertType> CurrentOrder => session.Order; // 현재 주문 리스트

        public IReadOnlyList<DessertType> Tray => session.Tray;

        // 이벤트 Actions
        public event Action<float, float> OnTimeChanged;

        public event Action<int> OnScoreChanged;

        public event Action<int, int> OnCountChanged;

        public event Action<IReadOnlyList<DessertType>, int> OnOrderChanged;

        public event Action<IReadOnlyList<DessertType>> OnTrayChanged;

        /// <summary>true = 성공.</summary>
        public event Action<bool> OnJudged;

        /// <summary>true = 입력 잠금.</summary>
        public event Action<bool> OnJudgingChanged;

        /// <summary>false = 시계가 멈춤(카운트다운 중, 일시정지, 영업 종료).</summary>
        public event Action<bool> OnRunningChanged;

        public event Action OnGameOver;

        private void Awake()
        {
            clock.Reset(playTimeSeconds);
            scoreBoard.Reset(scorePerSuccess, scorePenaltyPerFail);

            if (orderGenerator != null && orderGenerator.OrderCount != trayCapacity)
            {
                Debug.LogWarning(name + ": 주문 개수(" + orderGenerator.OrderCount
                    + ") 와 쟁반 칸 수(" + trayCapacity + ") 가 다릅니다.", this);
            }
        }

        private void Update()
        {
            if (isRunning)
            {
                Tick(Time.deltaTime);
            }
        }

        public void Prepare()
        {
            clock.Reset(playTimeSeconds);
            SetRunning(false);

            scoreBoard.Reset(scorePerSuccess, scorePenaltyPerFail);
            customerNumber = 0;

            // 진행 중이던 판정 대기를 무효. 
            // 다시하기를 0.8초 안에 누르면 이전 판의 대기가 뒤늦게 깨어나 새 판의 손님을 넘겨 버린다.
            judgeToken++;
            SetJudging(false);

            NextCustomer();

            OnTimeChanged?.Invoke(clock.Remaining, playTimeSeconds);
            OnScoreChanged?.Invoke(scoreBoard.Score);
            OnCountChanged?.Invoke(scoreBoard.SuccessCount, scoreBoard.FailCount);
        }

        public void StartGame()
        {
            Prepare();
            SetRunning(true);
        }

        public void SetRunning(bool running)
        {
            if (isRunning == running)
            {
                return;
            }

            isRunning = running;
            OnRunningChanged?.Invoke(running);
        }

        public void Tick(float deltaTime)
        {
            if (!isRunning)
            {
                return;
            }

            bool expired = clock.Tick(deltaTime);
            OnTimeChanged?.Invoke(clock.Remaining, playTimeSeconds);

            if (expired)
            {
                SetRunning(false);
                OnGameOver?.Invoke();
            }
        }

        public void Pick(DessertType type)
        {
            if (!isRunning || isJudging)
            {
                return;
            }

            PickResult result = session.Pick(type);
            if (result == PickResult.Rejected)
            {
                Judge(false);
                return;
            }

            OnTrayChanged?.Invoke(session.Tray);

            if (result == PickResult.Completed)
            {
                Judge(true);
            }
        }

        private void Judge(bool success)
        {
            scoreBoard.Apply(success);

            OnJudged?.Invoke(success);
            OnScoreChanged?.Invoke(scoreBoard.Score);
            OnCountChanged?.Invoke(scoreBoard.SuccessCount, scoreBoard.FailCount);

            BeginJudgeDelay();
        }

        /// <summary>
        /// 바로 다음 손님으로 넘어가지 않고 딜레이를 줌. 
        /// 
        /// 연타하면 실패 → 새 주문 → 또 실패가 0.1초 안에 줄줄이 일어나는 현상 방지.
        /// 실패한 순간 주문이 바뀌면 무엇을 잘못 눌렀는지 확인 불가능하므로 플레이어가 알 수 있도록 약간의 딜레이 줌.
        /// </summary>
        private async void BeginJudgeDelay()
        {
            int token = ++judgeToken;
            SetJudging(true);

            try
            {
                await Awaitable.WaitForSecondsAsync(judgeDelaySeconds, destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            // 대기 중에 Prepare() 가 불렸다면(다시하기) 이 대기는 이미 다른 판이 되었으므로 대기를 종료.
            if (token != judgeToken)
            {
                return;
            }

            NextCustomer();
            SetJudging(false);
        }

        private void NextCustomer()
        {
            customerNumber++;

            session.Begin(orderGenerator != null
                ? orderGenerator.Generate()
                : null);

            OnOrderChanged?.Invoke(session.Order, customerNumber);
            OnTrayChanged?.Invoke(session.Tray);
        }

        private void SetJudging(bool judging)
        {
            isJudging = judging;
            OnJudgingChanged?.Invoke(judging);
        }
    }
}
