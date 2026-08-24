using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// 진열대 버튼 5개의 입력을 GamePlayController 로 넘기고, 잠금을 일괄 제어한다.
    /// DisplayStandList 에 붙인다.
    ///
    /// 판정 자체는 하지 않는다. 무엇이 정답인지 아는 것은 GamePlayController 뿐이다.
    /// </summary>
    public class ShelfView : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("연결")]
        [Tooltip("CakeButton_1 ~ CakeButton_5")]
        [SerializeField] private ShelfButton[] shelfButtons;

        private void Start()
        {
            if (gamePlay == null || shelfButtons == null)
            {
                return;
            }

            // onClick 을 인스펙터에서 손으로 5개 꽂지 않는 이유: 하나를 빠뜨려도 오류가 나지 않고,
            // 그 버튼만 조용히 반응하지 않는다. 코드로 걸면 5개가 같이 붙거나 같이 빠진다.
            foreach (ShelfButton shelfButton in shelfButtons)
            {
                if (shelfButton == null)
                {
                    Debug.LogWarning(name + ": ShelfView 의 shelfButtons 에 빈 칸이 있습니다.", this);
                    continue;
                }

                // 지역 변수로 복사한다. 람다가 순회 변수를 그대로 캡처하면 전부 마지막 값이 된다.
                ShelfButton captured = shelfButton;
                captured.Button.onClick.AddListener(() => gamePlay.Pick(captured.Type));
            }
        }

        // Screen_Play 가 켜질 때 같이 켜진다. 첫 활성화에서는 Start 보다 먼저 불리지만,
        // 여기서 하는 일은 잠금 상태 반영뿐이라 순서에 영향받지 않는다.
        private void OnEnable()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": ShelfView 에 GamePlayController 가 꽂히지 않았습니다.", this);
                return;
            }

            gamePlay.OnJudgingChanged += HandleLockChanged;
            gamePlay.OnRunningChanged += HandleLockChanged;

            ApplyLock();
        }

        private void OnDisable()
        {
            if (gamePlay == null)
            {
                return;
            }

            gamePlay.OnJudgingChanged -= HandleLockChanged;
            gamePlay.OnRunningChanged -= HandleLockChanged;
        }

        // 두 이벤트가 같은 처리로 모인다. 어느 쪽이 바뀌었는지는 중요하지 않고,
        // 결과 상태만 다시 계산하면 된다.
        private void HandleLockChanged(bool _) => ApplyLock();

        /// <summary>
        /// 시계가 도는 동안, 판정 연출이 아닐 때만 진열대를 누를 수 있다.
        ///
        /// GamePlayController.Pick() 이 이미 같은 조건을 검사하므로 데이터는 안전하다.
        /// 그래도 버튼을 끄는 이유는 피드백이다 — 눌리는데 아무 일도 일어나지 않는 것이
        /// 가장 나쁘다. 이 잠금이 요건 7 의 "비활성화" 피드백 항목을 담당한다.
        ///
        /// 대상이 판정 중(0.8초)뿐 아니라 시작 카운트다운과 일시정지까지인 이유:
        /// 그 구간에도 Pick 은 거부되는데, 키보드·게임패드 Submit 은 CountPanel 이나
        /// PausePopup 을 통과해 뒤쪽 버튼에 닿는다.
        /// </summary>
        private void ApplyLock()
        {
            bool interactable = gamePlay.IsRunning && !gamePlay.IsJudging;

            if (shelfButtons == null)
            {
                return;
            }

            foreach (ShelfButton shelfButton in shelfButtons)
            {
                if (shelfButton != null)
                {
                    shelfButton.Button.interactable = interactable;
                }
            }
        }
    }
}
