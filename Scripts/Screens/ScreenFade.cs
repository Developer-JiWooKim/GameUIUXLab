using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Screens
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ScreenFade : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.5f;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        public async Awaitable FadeIn()
        {
            gameObject.SetActive(true);
            await Fade(0f, 1f, fadeDuration);
        }

        public async Awaitable FadeOut()
        {
            await Fade(1f, 0f, fadeDuration);
            gameObject.SetActive(false);
        }

        private async Awaitable Fade(float from, float to, float duration)
        {
            canvasGroup.alpha = from;

            float elapsed = 0;
            while (elapsed < duration)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);

                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            }

            canvasGroup.alpha = to;
        }
    }
}