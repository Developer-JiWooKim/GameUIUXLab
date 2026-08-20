using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UIFSM
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BasePanel : MonoBehaviour, IStateUI
    {
        private float fadeDuration = 0.5f;

        private CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup => canvasGroup;

        public abstract PanelName PanelName { get; }

        public void Initialize()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public async Awaitable FadeIn()
        {
            gameObject.SetActive(true);
            await Fade(0f, 1f);
        }

        public async Awaitable FadeOut()
        {
            await Fade(1f, 0f);
            gameObject.SetActive(false);
        }

        private async Awaitable Fade(float from, float to)
        {
            canvasGroup.alpha = from;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);

                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            }

            canvasGroup.alpha = to;
        }

        public virtual void Enter() { }

        public virtual void Exit() { }
    }
}
