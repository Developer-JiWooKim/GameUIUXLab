using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core
{
    public class GamePlayController : MonoBehaviour
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

        private float remainingTime;
        private bool isRunning;

        private int score;
        private int successCount;
        private int failCount;
        private int customerNumber;

        private List<DessertType> currentOrder = new();
        private readonly List<DessertType> tray = new();

        /// <summary>주문에서 아직 안 담은 개수.</summary>
        private readonly Dictionary<DessertType, int> remaining = new();

        /// <summary>주문과 쟁반에 담은 종류가 일치하는지 판정 여부.</summary>
        private bool isJudging;

        /// <summary>판정 대기가 끝났을 때 그 사이 새 판이 시작됐는지 구분하는 표식.</summary>
        private int judgeToken;

        public float RemainingTime => remainingTime;

        public float PlayTimeSeconds => playTimeSeconds;

        public bool IsRunning => isRunning;

        public int Score => score;

        public int SuccessCount => successCount;

        public int FailCount => failCount;

        public bool IsJudging => isJudging;

        public int CustomerNumber => customerNumber;

        public IReadOnlyList<DessertType> CurrentOrder => currentOrder;

        public IReadOnlyList<DessertType> Tray => tray;

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

        // 초기화와 시작을 나눈 이유: 카운트다운이 도는 동안 시계는 멈춰 있어야 하는데
        // HUD 는 시작값(120초)을 이미 보여주고 있어야 한다.
        public void Prepare()
        {
            remainingTime = playTimeSeconds;
            SetRunning(false);

            score = 0;
            successCount = 0;
            failCount = 0;
            customerNumber = 0;

            // 진행 중이던 판정 대기를 무효로 만든다. 다시하기를 0.8초 안에 누르면
            // 이전 판의 대기가 뒤늦게 깨어나 새 판의 손님을 넘겨 버린다.
            judgeToken++;
            SetJudging(false);

            NextCustomer();

            // 시작 상태를 전부 발행한다. View 가 초기값을 따로 읽을 필요가 없다.
            OnTimeChanged?.Invoke(remainingTime, playTimeSeconds);
            OnScoreChanged?.Invoke(score);
            OnCountChanged?.Invoke(successCount, failCount);
        }

        public void StartGame()
        {
            Prepare();
            SetRunning(true);
        }

        // 값이 바뀔 때만 발행한다. 매 프레임 부르는 곳이 생기더라도 View 가 헛일하지 않는다.
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

            remainingTime = Mathf.Max(0f, remainingTime - deltaTime);
            OnTimeChanged?.Invoke(remainingTime, playTimeSeconds);

            if (remainingTime <= 0f)
            {
                SetRunning(false);
                OnGameOver?.Invoke();
            }
        }

        // 진열대 버튼이 부르는 유일한 진입점. 누른 즉시 여기서 판정이 끝난다.
        public void Pick(DessertType type)
        {
            // 데이터 층의 잠금. 표시 층(ShelfView 의 interactable)만으로는
            // 키보드 Submit 이나 빠른 연타가 뚫는다.
            if (!isRunning || isJudging)
            {
                return;
            }

            if (!remaining.TryGetValue(type, out int left) || left <= 0)
            {
                // 주문에 없거나 이미 필요한 만큼 담았다. 개수 초과도 여기서 같이 걸린다.
                Judge(false);
                return;
            }

            remaining[type] = left - 1;
            tray.Add(type);
            OnTrayChanged?.Invoke(tray);

            if (IsOrderComplete())
            {
                Judge(true);
            }
        }

        private bool IsOrderComplete()
        {
            foreach (int left in remaining.Values)
            {
                if (left > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void Judge(bool success)
        {
            if (success)
            {
                score += scorePerSuccess;
                successCount++;
            }
            else
            {
                score = Mathf.Max(0, score - scorePenaltyPerFail);
                failCount++;
            }

            OnJudged?.Invoke(success);
            OnScoreChanged?.Invoke(score);
            OnCountChanged?.Invoke(successCount, failCount);

            BeginJudgeDelay();
        }

        // 바로 다음 손님으로 넘기지 않는 이유 두 가지.
        //   1. 실패한 순간 주문이 바뀌면 무엇을 잘못 눌렀는지 확인할 수 없다.
        //   2. 연타하면 실패 → 새 주문 → 또 실패가 0.1초 안에 줄줄이 일어난다.
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

            // 대기 중에 Prepare() 가 불렸다면(다시하기) 이 대기는 이미 남의 판 이야기다.
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

            currentOrder = orderGenerator != null
                ? orderGenerator.Generate()
                : new List<DessertType>();

            remaining.Clear();
            foreach (DessertType type in currentOrder)
            {
                remaining.TryGetValue(type, out int count);
                remaining[type] = count + 1;
            }

            tray.Clear();

            OnOrderChanged?.Invoke(currentOrder, customerNumber);
            OnTrayChanged?.Invoke(tray);
        }

        private void SetJudging(bool judging)
        {
            isJudging = judging;
            OnJudgingChanged?.Invoke(judging);
        }
    }
}
