using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UI
{
    public class UIScreenFlowController : MonoBehaviour
    {
        private enum ScreenState
        {
            Title,
            Option,
            Play,
            Pause,
            Result,
        }

        [Header("Panel Objects")]
        [SerializeField] private GameObject screen_Title;
        [SerializeField] private GameObject screen_Option;
        [SerializeField] private GameObject screen_PlayHud;
        [SerializeField] private GameObject screen_Pause;
        [SerializeField] private GameObject screen_Result;

        private ScreenState currentState;

        private void Start() => ShowTitle();

        public void ShowTitle() => ChangeScreen(ScreenState.Title);

        public void ShowOption() => ChangeScreen(ScreenState.Option);

        public void StartGame() => ChangeScreen(ScreenState.Play);

        public void PauseGame() => ChangeScreen(ScreenState.Pause);

        public void ResumeGame() => ChangeScreen(ScreenState.Play);

        public void ShowResult() => ChangeScreen(ScreenState.Result);

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit(); 
#endif
        }

        private void ChangeScreen(ScreenState nextState)
        {
            currentState = nextState;

            screen_Title.SetActive(currentState == ScreenState.Title);
            screen_Option.SetActive(currentState == ScreenState.Option);
            screen_PlayHud.SetActive(currentState == ScreenState.Play);
            screen_Pause.SetActive(currentState == ScreenState.Pause);
            screen_Result.SetActive(currentState == ScreenState.Result);
        }
    }
}