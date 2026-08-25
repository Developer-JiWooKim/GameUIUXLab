using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Screens
{
    public sealed class ScreenFlowController : MonoBehaviour, IScreenController
    {
        [Header("Screen 패널")]
        [SerializeField] private TitleState titleState;
        [SerializeField] private PlayState playState;
        [SerializeField] private PauseState pauseState;
        [SerializeField] private ConfirmState confirmState;
        [SerializeField] private ResultState resultState;

        [Header("Fade Effect")]
        [SerializeField] private ScreenFade fade;

        private readonly Stack<UIStateBase> stack = new();

        private bool isTransitioning;

        public UIStateBase Top => stack.Count > 0 ? stack.Peek() : null;

        /// <summary>현재 화면의 첫 Focus.</summary>
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
            BindAndHide(titleState);
            BindAndHide(playState);
            BindAndHide(pauseState);
            BindAndHide(confirmState);
            BindAndHide(resultState);
        }

        private void Start() => ShowTitle();

        public void ShowTitle() => SetScreen(titleState);

        public void ShowPlay() => SetScreen(playState);

        public void ShowResult() => SetScreen(resultState); // 게임 종료 시점에 호출, 버튼 클릭 X

        public void ShowPause() => Push(pauseState);

        public void HidePause() => Pop();

        public void OpenConfirm() => Push(confirmState);

        public void CloseConfirm() => Pop();

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Esc / 게임패드 B</summary>
        public void HandleCancel()
        {
            if (isTransitioning)
            {
                return;
            }

            UIStateBase top = Top;
            if (top != null)
            {
                top.OnCancel();
            }
        }

        private async void SetScreen(UIStateBase next)
        {
            if (isTransitioning || next == null)
            {
                return;
            }

            isTransitioning = true;

            try
            {
                ClearFocus();

                if (fade != null)
                {
                    await fade.FadeIn();
                }

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

            }
            finally
            {
                isTransitioning = false;
            }
        }

        private void Push(UIStateBase next)
        {
            if (isTransitioning || next == null)
            {
                return;
            }

            PushInternal(next);
        }

        private void Pop()
        {
            if (isTransitioning || stack.Count == 0)
            {
                return;
            }

            PopTop();

            ApplyFocus();
        }

        private void PushInternal(UIStateBase next)
        {
            UIStateBase below = Top;

            if (below != null)
            {
                below.SetInteractable(false);
            }

            stack.Push(next);

            next.gameObject.SetActive(true);
            next.SetInteractable(true);
            next.Enter();

            ApplyFocus();
        }

        private void PopTop()
        {
            if (stack.Count == 0)
            {
                return;
            }

            UIStateBase top = stack.Pop();
            top.Exit();
            top.gameObject.SetActive(false);

            UIStateBase below = Top;

            if (below != null)
            {
                below.SetInteractable(true);
            }
        }

        private void ApplyFocus()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return;
            }

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

        private void BindAndHide(UIStateBase state)
        {
            if (state == null)
            {
                Debug.LogWarning(name + ": ScreenFlowController 에 꽂히지 않은 상태 슬롯이 있습니다.", this);
                return;
            }

            state.Bind(this);
            state.gameObject.SetActive(false);
        }
    }
}
