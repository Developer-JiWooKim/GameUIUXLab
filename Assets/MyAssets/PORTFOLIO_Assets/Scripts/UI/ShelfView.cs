using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    public class ShelfView : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("연결")]
        [Tooltip("CakeButton_1 ~ CakeButton_5")]
        [SerializeField] private ShelfButton[] shelfButtons;

        [Header("키보드 단축키")]
        [Tooltip("UI/ShelfSlot. 바인딩 <Keyboard>/1~5 의 순서가 위 shelfButtons 순서와 "
            + "같아야 한다. 에셋에서 바인딩을 지우거나 사이에 끼워 넣으면 대응이 어긋난다")]
        [SerializeField] private InputActionReference shelfSlotAction;

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

            // Screen_Play 가 꺼지면 이 컴포넌트도 같이 꺼진다. 덕분에 타이틀·결과 화면에서
            // 숫자 키를 눌러도 아무 일이 없다. 구독 수명을 화면 수명에 맡기는 편이
            // 화면마다 조건을 검사하는 것보다 어긋날 자리가 적다.
            if (shelfSlotAction != null && shelfSlotAction.action != null)
            {
                shelfSlotAction.action.performed += HandleSlotShortcut;

                // 직접 켜야 한다. InputSystemUIInputModule 은 자기가 쓰는 10개 액션만 켜고
                // UI 맵 전체를 켜지 않는다(EnableAllActions). 지금은 이 에셋이 Project-wide
                // Actions 로 등록돼 있어 자동으로 켜지지만, 그 설정이 풀리면 숫자 키만
                // 조용히 죽는다. 이미 켜져 있으면 Enable 은 아무 일도 하지 않는다.
                shelfSlotAction.action.Enable();
            }

            ApplyLock();
        }

        private void OnDisable()
        {
            if (shelfSlotAction != null && shelfSlotAction.action != null)
            {
                shelfSlotAction.action.performed -= HandleSlotShortcut;
            }

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
        /// 숫자 키 1~5 로 진열대 버튼을 곧바로 누른다.
        ///
        /// gamePlay.Pick() 을 직접 부르지 않고 버튼에 Submit 이벤트를 보내는 이유가 핵심이다.
        /// Button.OnSubmit 은 Press() 안에서 IsInteractable() 을 먼저 검사하므로,
        /// ApplyLock 이 걸어 둔 잠금(판정 0.8초·시작 카운트다운·일시정지)이 그대로 적용된다.
        /// Pick 을 직접 부르면 잠금을 우회하는 두 번째 입력 경로가 생겨, 연타로 주문이
        /// 줄줄이 실패하는 버그가 되살아난다.
        ///
        /// 덤으로 눌림 색 전환(Pressed)도 Enter 로 눌렀을 때와 같아진다.
        /// 단축키용 피드백을 따로 만들 필요가 없다.
        /// </summary>
        private void HandleSlotShortcut(InputAction.CallbackContext context)
        {
            if (shelfButtons == null || EventSystem.current == null)
            {
                return;
            }

            // 어느 키가 눌렸는지는 바인딩 순서로 안다. <Keyboard>/1~5 가 0~4 로 돌아온다.
            // 바인딩되지 않은 컨트롤이면 -1 이다.
            int index = context.action.GetBindingIndexForControl(context.control);

            if (index < 0 || index >= shelfButtons.Length || shelfButtons[index] == null)
            {
                return;
            }

            // 잠금 중에는 선택도 옮기지 않는다. 눌리지 않는 버튼에 테두리만 옮겨 가면
            // 왜 반응이 없는지 더 헷갈린다.
            if (!shelfButtons[index].Button.IsInteractable())
            {
                return;
            }

            GameObject target = shelfButtons[index].gameObject;

            // 마우스로 누른 뒤라면 선택이 비어 있다. 포커스를 같이 옮겨 둬야
            // 이어서 방향키를 눌렀을 때 방금 고른 버튼에서 출발한다.
            EventSystem.current.SetSelectedGameObject(target);

            ExecuteEvents.Execute(target, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

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
