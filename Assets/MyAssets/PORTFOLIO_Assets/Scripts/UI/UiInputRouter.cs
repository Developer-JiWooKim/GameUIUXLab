using Assets.MyAssets.PORTFOLIO_Assets.Scripts.States;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// UI 액션을 직접 구독해 "선택된 오브젝트와 무관한" 입력을 처리한다. UIRoot 에 붙인다.
    ///
    /// 담당은 셋이다.
    ///   Cancel      - Esc / 게임패드 B → ScreenFlowController.HandleCancel()
    ///   선택 해제   - 마우스·터치로 조작하면 키보드 선택 테두리를 지운다
    ///   선택 복구   - 선택이 비었을 때 Navigate·Submit 이 들어오면 현재 화면 첫 버튼을 다시 선택
    ///
    /// 마우스·터치·게임패드 이동/확정 자체는 InputSystemUIInputModule 이 처리한다.
    /// 여기서 다시 구현하지 않는다.
    ///
    /// 선택 해제·복구를 콜백에서 바로 하지 않고 LateUpdate 로 미루는 이유는
    /// ApplyPendingSelection 주석 참고. 프레임 순서 때문에 반드시 필요하다.
    /// </summary>
    public class UiInputRouter : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private ScreenFlowController screenFlow;

        [Header("UI 액션")]
        [SerializeField] private InputActionReference cancelAction;
        [SerializeField] private InputActionReference clickAction;
        [SerializeField] private InputActionReference navigateAction;
        [SerializeField] private InputActionReference submitAction;

        [Header("설정")]
        [Tooltip("스틱을 놓을 때 오는 0 근처 값을 복구 신호로 오인하지 않게 걸러낸다")]
        [SerializeField] private float navigateDeadzone = 0.2f;

        private bool clearRequested;
        private bool restoreRequested;

        private UIStateBase topAtPointerPress;

        private void OnEnable()
        {
            Subscribe(cancelAction, OnCancel);
            Subscribe(clickAction, OnClick);
            Subscribe(navigateAction, OnNavigate);
            Subscribe(submitAction, OnSubmit);
        }

        private void OnDisable()
        {
            Unsubscribe(cancelAction, OnCancel);
            Unsubscribe(clickAction, OnClick);
            Unsubscribe(navigateAction, OnNavigate);
            Unsubscribe(submitAction, OnSubmit);
        }

        private static void Subscribe(InputActionReference reference, System.Action<InputAction.CallbackContext> handler)
        {
            if (reference != null && reference.action != null)
            {
                reference.action.performed += handler;
            }
        }

        private static void Unsubscribe(InputActionReference reference, System.Action<InputAction.CallbackContext> handler)
        {
            if (reference != null && reference.action != null)
            {
                reference.action.performed -= handler;
            }
        }

        /// <summary>Esc · 게임패드 B</summary>
        private void OnCancel(InputAction.CallbackContext context)
        {
            if (screenFlow != null)
            {
                screenFlow.HandleCancel();
            }
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() < 0.5f)
            {
                return;
            }

            InputDevice device = context.control.device;
            if (!(device is Mouse || device is Touchscreen || device is Pen))
            {
                return;
            }

            clearRequested = true;
            topAtPointerPress = screenFlow != null ? screenFlow.Top : null;
        }

        private void OnNavigate(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            if (value.sqrMagnitude < navigateDeadzone * navigateDeadzone)
            {
                return;
            }

            restoreRequested = true;
        }

        private void OnSubmit(InputAction.CallbackContext context) => restoreRequested = true;

        // 지연 적용

        /// <summary>
        /// 선택 변경을 한 프레임 안에서 "모듈보다 늦게" 적용한다.
        ///
        /// EventSystem 은 Update 에서 돌고 LateUpdate 는 그 뒤다. 액션 콜백은 반대로
        /// Update 보다 앞선 입력 갱신 단계에서 오므로, 콜백에서 곧바로 선택을 바꾸면
        /// 같은 프레임에 모듈이 그 결과를 덮어쓰거나 잘못 소비한다.
        ///
        ///   해제 - Selectable.OnPointerDown 이 자기 자신을 다시 선택한다.
        ///          콜백에서 null 로 지워도 그 직후 되살아나 테두리가 그대로 남는다.
        ///   복구 - 모듈은 Submit 을 "처리 시점의 currentSelectedGameObject" 에게 보낸다.
        ///          콜백에서 미리 복구해두면 방금 복구한 버튼이 그 프레임에 즉시 눌린다.
        ///          (마우스로 클릭한 뒤 Enter 를 치면 첫 버튼이 저절로 실행된다)
        ///
        /// LateUpdate 로 미루면 그 프레임 입력은 모듈이 선택 없음으로 처리해 아무 일도
        /// 일어나지 않고, 다음 프레임부터 정상 조작이 된다. 첫 입력이 복구에 쓰이는
        /// 콘솔 UI 의 일반적인 동작과 같다.
        /// </summary>
        private void LateUpdate()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null || screenFlow == null)
            {
                clearRequested = false;
                restoreRequested = false;
                return;
            }

            if (clearRequested)
            {
                clearRequested = false;
                restoreRequested = false;
                ClearSelection(eventSystem);
                return;
            }

            if (restoreRequested)
            {
                restoreRequested = false;
                RestoreSelection(eventSystem);
            }
        }

        private void ClearSelection(EventSystem eventSystem)
        {
            if (screenFlow.IsTransitioning || screenFlow.Top != topAtPointerPress)
            {
                return;
            }

            eventSystem.SetSelectedGameObject(null);
        }

        private void RestoreSelection(EventSystem eventSystem)
        {
            if (eventSystem.currentSelectedGameObject != null || screenFlow.IsTransitioning)
            {
                return;
            }

            GameObject first = screenFlow.CurrentFirstSelected;

            if (first == null || !first.activeInHierarchy)
            {
                return;
            }

            eventSystem.SetSelectedGameObject(first);
        }
    }
}
