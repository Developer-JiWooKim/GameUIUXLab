using System;
using System.Collections.Generic;
using System.Text;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.States;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// 화면 스택을 소유하고 패널 활성화·키보드 포커스를 전담한다. UIRoot 에 붙인다.
    ///
    /// 평면 FSM 이 아니라 스택인 이유:
    ///   Play - Pause - Confirm 은 전환이 아니라 겹침이다. Push 는 아래층의 Exit() 를
    ///   부르지 않으므로, Pause 에서 돌아올 때 PlayState.Enter() 가 재호출되어 게임이
    ///   다시 시작되는 문제가 구조적으로 생기지 않는다.
    ///
    /// 연산 3개로 전체 흐름이 덮인다.
    ///   Set   - 스택을 비우고 새로 쌓는다 (Title / Play / Result 간 이동)
    ///   Push  - 아래층을 그대로 두고 겹친다 (Play to Pause, Pause to Confirm)
    ///   Pop   - 맨 위 한 겹만 닫는다 (Cancel 의 기본 동작)
    /// </summary>
    public class ScreenManager : MonoBehaviour, IScreenController
    {
        [Header("화면 상태 (각 Screen_ 패널의 컴포넌트)")]
        [SerializeField] private TitleState titleState;
        [SerializeField] private PlayState playState;
        [SerializeField] private PauseState pauseState;
        [SerializeField] private ConfirmState confirmState;
        [SerializeField] private ResultState resultState;

        [Header("전환 연출")]
        [Tooltip("비워두면 페이드 없이 즉시 전환한다. 동기 동작을 먼저 검증할 때 비워두면 편하다")]
        [SerializeField] private ScreenFade fade;

        [Header("디버그")]
        [Tooltip("전환할 때마다 스택 상태를 콘솔에 찍는다. 배선 검증이 끝나면 끈다")]
        [SerializeField] private bool logTransitions = true;

        private readonly Stack<UIStateBase> stack = new Stack<UIStateBase>();

        private bool isTransitioning;

        /// <summary>스택 맨 위. 비어 있으면 null.</summary>
        public UIStateBase Top => stack.Count > 0 ? stack.Peek() : null;

        /// <summary>UiInputRouter 의 선택 복구가 사용한다. 현재 화면의 첫 버튼.</summary>
        public GameObject CurrentFirstSelected
        {
            get
            {
                UIStateBase top = Top;
                return top != null ? top.FirstSelected : null;
            }
        }

        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            // 씬에 어떤 패널이 켜진 채 저장돼 있어도 시작 상태를 일정하게 만든다.
            BindAndHide(titleState);
            BindAndHide(playState);
            BindAndHide(pauseState);
            BindAndHide(confirmState);
            BindAndHide(resultState);
        }

        // Awake 가 아닌 이유: 다른 컴포넌트의 Awake 가 끝난 뒤 포커스를 잡아야 안전하다.
        private void Start() => ShowTitle();

        // 버튼 onClick 에서 부르는 진입점

        public void ShowTitle() => SetScreen(titleState);

        public void ShowPlay() => SetScreen(playState);

        public void ShowResult() => SetScreen(resultState); // 얘만 유일하게 버튼 클릭 호출이 아닌 게임 종료 시점 호출

        public void ShowPause() => Push(pauseState);

        public void HidePause() => Pop();

        public void OpenConfirm() => Push(confirmState);

        public void CloseConfirm() => Pop();

        public void Quit()
        {
            // 에디터에서는 Application.Quit() 이 아무 일도 하지 않으므로 분기가 필요하다.
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Esc / 게임패드 B. UiInputRouter 가 호출한다.</summary>
        public void HandleCancel()
        {
            if (isTransitioning)
            {
                return;
            }

            // 분기를 여기에 두지 않는다. 화면별 동작은 각 상태의 OnCancel 이 안다.
            // 그래서 "가장 위 레이어 하나만 닫는다" 가 저절로 지켜진다.
            UIStateBase top = Top;
            if (top != null)
            {
                top.OnCancel();
            }
        }

        // 스택 연산

        /// <summary>스택을 전부 비우고 하나만 쌓는다. 화면 간 이동에 사용.</summary>
        private async void SetScreen(UIStateBase next)
        {
            if (isTransitioning || next == null)
            {
                return;
            }

            isTransitioning = true;

            try
            {
                // 커튼은 포인터 입력만 막는다. 키보드·게임패드는 선택을 비워 끊는다.
                ClearFocus();

                if (fade != null)
                {
                    await fade.FadeIn();
                }

                // 위에서부터 역순으로 닫는다. PauseState.Exit() 이 PlayState.Exit() 보다
                // 먼저 불려야 시계가 확실히 멈춘 상태로 끝난다.
                while (stack.Count > 0)
                {
                    PopTop();
                }

                PushInternal(next);

                if (fade != null)
                {
                    await fade.FadeOut();
                }
            }
            catch (OperationCanceledException)
            {
                // 오브젝트 파괴로 페이드가 취소된 경우. 정상 종료 경로다.
            }
            finally
            {
                isTransitioning = false;
            }
        }

        /// <summary>아래층을 그대로 두고 겹쳐 올린다. 일시정지·팝업에 사용.</summary>
        private void Push(UIStateBase next)
        {
            if (isTransitioning || next == null)
            {
                return;
            }

            PushInternal(next);
        }

        /// <summary>맨 위 한 겹만 닫는다.</summary>
        private void Pop()
        {
            if (isTransitioning || stack.Count == 0)
            {
                return;
            }

            PopTop();

            // 새 top 의 첫 버튼으로 포커스를 되돌린다.
            // 빠뜨리면 확인 팝업을 닫았을 때 포커스가 사라진 채 Pause 에 남는다.
            ApplyFocus();
            LogStack();
        }

        private void PushInternal(UIStateBase next)
        {
            stack.Push(next);

            // 패널을 켜기 전에 포커스를 주면 비활성 오브젝트라 선택이 먹지 않는다. 순서 주의.
            next.gameObject.SetActive(true);
            next.Enter();

            ApplyFocus();
            LogStack();
        }

        /// <summary>포커스를 건드리지 않고 한 겹만 걷어낸다. SetScreen 이 반복 호출한다.</summary>
        private void PopTop()
        {
            if (stack.Count == 0)
            {
                return;
            }

            UIStateBase top = stack.Pop();
            top.Exit();
            top.gameObject.SetActive(false);
        }

        // 포커스

        /// <summary>현재 top 의 첫 버튼으로 키보드 포커스를 지정한다.</summary>
        private void ApplyFocus()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return;
            }

            // null 을 한 번 거치는 이유: 같은 오브젝트를 다시 지정하면 OnSelect 가 발생하지
            // 않아 하이라이트가 다시 그려지지 않는 경우가 있다.
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(CurrentFirstSelected);
        }

        private void ClearFocus()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        // 초기화 · 디버그

        private void BindAndHide(UIStateBase state)
        {
            if (state == null)
            {
                Debug.LogWarning(name + ": ScreenManager 에 꽂히지 않은 상태 슬롯이 있습니다.", this);
                return;
            }

            state.Bind(this);
            state.gameObject.SetActive(false);
        }

        private void LogStack()
        {
            if (!logTransitions)
            {
                return;
            }

            // Stack<T> 는 top 부터 순회하므로 그대로 찍으면 위에서 아래 순서가 된다.
            StringBuilder builder = new StringBuilder("[Screens] ");
            foreach (UIStateBase state in stack)
            {
                builder.Append(state.gameObject.name).Append(" < ");
            }

            Debug.Log(builder.ToString());
        }
    }
}
