using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UIFSM
{

    public class UIFSM : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private BasePanel[] panels;

        [Header("시작 Panel 설정")]
        [SerializeField] private PanelName startPanel = PanelName.Title;

        private readonly Dictionary<PanelName, BasePanel> panelTable = new();

        private BasePanel currentPanel;
        private bool isTransitioning;

        public PanelName CurrentPanel => currentPanel != null ? currentPanel.PanelName : startPanel;

        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            foreach (BasePanel panel in panels)
            {
                if (panel == null)
                {
                    continue;
                }

                panel.Initialize();
                panelTable[panel.PanelName] = panel;
            }
        }

        private void Start() => ChangeState(startPanel);

        public void ShowTitle() => ChangeState(PanelName.Title);

        public void ShowOption() => ChangeState(PanelName.Option);

        public void StartGame() => ChangeState(PanelName.Play);

        public void PauseGame() => ChangeState(PanelName.Pause);

        public void ResumeGame() => ChangeState(PanelName.Play);

        public void ShowResult() => ChangeState(PanelName.Result);

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        public async void ChangeState(PanelName next)
        {
            if (isTransitioning)
            {
                Debug.Log("전환이 진행중임");
                return;
            }

            if (currentPanel != null && currentPanel.PanelName == next)
            {
                return;
            }

            if (!panelTable.TryGetValue(next, out BasePanel nextPanel))
            {
                return;
            }

            isTransitioning = true;

            try
            {
                await Transition(nextPanel);
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                isTransitioning = false;
            }
        }

        private async Awaitable Transition(BasePanel nextPanel)
        {
            if (currentPanel != null)
            {
                currentPanel.Exit();
                await currentPanel.FadeOut();
            }

            currentPanel = nextPanel;

            currentPanel.Enter();
            await currentPanel.FadeIn();
        }
    }
}
