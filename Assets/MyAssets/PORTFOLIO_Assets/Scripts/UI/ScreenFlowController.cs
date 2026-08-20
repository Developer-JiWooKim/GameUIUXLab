using System;
using System.Collections.Generic;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.States;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenFlowController : MonoBehaviour, IScreenController
{
    [Header("Screen")]
    [SerializeField] private UIStateBase[] Screens;
    [SerializeField] private TitleState _titleState;
    [SerializeField] private PlayState _playState;
    [SerializeField] private PauseState _pauseState;
    [SerializeField] private ConfirmState _confirmState;
    [SerializeField] private ResultState _resultState;

    [Header("Fade Effect")]
    [SerializeField] private ScreenFade _fade;

    private readonly Stack<UIStateBase> _stateStack = new();

    private bool _isTransitioning;

    public bool IsTransitioning => _isTransitioning;

    public UIStateBase Top => _stateStack.Count > 0 ? _stateStack.Peek() : null;

    public GameObject CurrentFirstSelected
    {
        get
        {
            UIStateBase top = Top;
            return top != null ? top.FirstSelected : null;
        }
    }

    private void Awake()
    {
        // 패널 연결 및 비활성화
        BindAndHide(_titleState);
        BindAndHide(_playState);
        BindAndHide(_pauseState);
        BindAndHide(_confirmState);
        BindAndHide(_resultState);
    }

    private void Start() => ShowTitle();


    private void BindAndHide(UIStateBase state)
    {
        if (state == null)
        {
            return;
        }

        state.Bind(this);
        state.gameObject.SetActive(false);
    }

    #region 버튼 이벤트
    public void CloseConfirm() => Pop();

    public void HidePause() => Pop();

    public void OpenConfirm() => Push(_confirmState);

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void ShowPause() => SetScreen(_pauseState);

    public void ShowPlay() => SetScreen(_playState);

    public void ShowResult() => SetScreen(_resultState);

    public void ShowTitle() => SetScreen(_titleState);

    #endregion

    /// <summary>ESC / 게임 패드 B.</summary>
    public void HandleCancel()
    {
        if (_isTransitioning)
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
        if (_isTransitioning || next == null)
        {
            return;
        }

        _isTransitioning = true;

        try
        {
            ClearFocus();

            if (_fade != null)
            {
                await _fade.FadeIn();
            }

            while (_stateStack.Count > 0)
            {
                PopTop();
            }

            PushInternal(next);

            if (_fade != null)
            {
                await _fade.FadeOut();
            }
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            _isTransitioning = false;
        }
    }

    #region Stack
    private void Pop()
    {
        if (_isTransitioning || _stateStack.Count == 0)
        {
            return;
        }

        PopTop();

        ApplyFocus();
    }

    private void Push(UIStateBase next)
    {
        if (_isTransitioning || next == null)
        {
            return;
        }

        PushInternal(next);
    }
    private void PopTop()
    {
        if (_stateStack.Count == 0)
        {
            return;
        }

        UIStateBase top = _stateStack.Pop();
        top.Exit();
        top.gameObject.SetActive(false);
    }

    private void PushInternal(UIStateBase next)
    {
        _stateStack.Push(next);

        next.gameObject.SetActive(true);
        next.Enter();

        ApplyFocus();
    }

    #endregion

    #region Focus
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
    #endregion
}
