using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    public sealed class ShelfView : MonoBehaviour
    {
        [Header("GamePlayController")]
        [SerializeField] private GamePlayController gamePlay;

        [Header("진열대 메뉴(케이크) 버튼")]
        [SerializeField] private ShelfButton[] shelfButtons;

        [Header("키보드 단축키(1,2,3,4,5)")]
        [SerializeField] private InputActionReference shelfSlotAction;

        private void Start()
        {
            if (gamePlay == null || shelfButtons == null)
            {
                return;
            }

            foreach (ShelfButton shelfButton in shelfButtons)
            {
                if (shelfButton == null)
                {
                    Debug.LogWarning(name + ": ShelfView 의 shelfButtons 에 빈 칸이 있습니다.", this);
                    continue;
                }

                ShelfButton captured = shelfButton;
                captured.Button.onClick.AddListener(() => gamePlay.Pick(captured.Type));
            }
        }

        private void OnEnable()
        {
            if (gamePlay == null)
            {
                Debug.LogWarning(name + ": ShelfView 에 GamePlayController 가 꽂히지 않았습니다.", this);
                return;
            }

            gamePlay.OnJudgingChanged += HandleLockChanged;
            gamePlay.OnRunningChanged += HandleLockChanged;

            if (shelfSlotAction != null && shelfSlotAction.action != null)
            {
                shelfSlotAction.action.performed += HandleSlotShortcut;

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

        private void HandleLockChanged(bool _) => ApplyLock();

        /// <summary>
        /// 숫자 키 1~5 로 진열대 버튼을 곧바로 누를 수 있게 키 입력 대응.
        /// </summary>
        private void HandleSlotShortcut(InputAction.CallbackContext context)
        {
            if (shelfButtons == null || EventSystem.current == null)
            {
                return;
            }

            int index = context.action.GetBindingIndexForControl(context.control);

            if (index < 0 || index >= shelfButtons.Length || shelfButtons[index] == null)
            {
                return;
            }

            // 잠금 중에는 FocusRing도 옮기지 않게.
            if (!shelfButtons[index].Button.IsInteractable())
            {
                return;
            }

            GameObject target = shelfButtons[index].gameObject;

            EventSystem.current.SetSelectedGameObject(target);

            ExecuteEvents.Execute(target, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

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
