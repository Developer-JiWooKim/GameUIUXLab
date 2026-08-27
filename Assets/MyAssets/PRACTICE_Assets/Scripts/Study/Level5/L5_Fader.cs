using System;
using System.Threading;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level5
{
    /// <summary>
    /// 5-1. CanvasGroup 알파를 부드럽게 바꾼다. 코루틴이 아니라 Awaitable 로.
    ///
    /// 규칙
    ///   1. 호출부에서 await fader.FadeIn(); 으로 기다릴 수 있어야 한다
    ///   2. Time.timeScale == 0 에서도 페이드가 진행된다
    ///   3. 페이드 도중 오브젝트가 파괴돼도 예외가 콘솔에 뜨지 않는다
    ///   4. FadeOut 이 끝나면 gameObject 를 끈다
    ///
    /// 검증 A : Time.timeScale = 0f; await fader.FadeIn();  → 다음 줄에 도달하면 통과
    /// 검증 B : _ = temp.FadeIn(); Destroy(temp.gameObject); → 콘솔에 예외가 없어야 통과
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class L5_Fader : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.5f;

        private CanvasGroup canvasGroup;

        /// <summary>채점용.</summary>
        public float Alpha => canvasGroup != null ? canvasGroup.alpha : 0f;

        public bool IsFading { get; private set; }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>0 → 1</summary>
        public async Awaitable FadeIn()
        {
            if (!IsFading)
            {
                return;
            }
            gameObject.SetActive(true);
            await Fade(0f, 1f, fadeDuration);
            IsFading = !IsFading;
        }

        /// <summary>1 → 0. 끝나면 gameObject 를 끈다.</summary>
        public async Awaitable FadeOut()
        {
            if (!IsFading)
            {
                return;
            }
            await Fade(1f, 0f, fadeDuration);
            IsFading = !IsFading;
            gameObject.SetActive(false);
        }

        // TODO: private async Awaitable Fade(float from, float to, float duration)
        //       - Time.unscaledDeltaTime 을 쓴다 (deltaTime 이면 timeScale=0 에서 안 끝남)
        //       - await 에 destroyCancellationToken 을 넘긴다

        private async Awaitable Fade(float from, float to, float duration)
        {
            canvasGroup.alpha = from;
            int elasped;
            try
            {

            }
            catch (OperationCanceledException)
            {

            }
            finally
            {

            }
        }
    }
}
