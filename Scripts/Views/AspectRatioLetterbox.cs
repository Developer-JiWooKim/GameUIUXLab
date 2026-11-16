using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Views
{
    /// <summary>
    /// 해상도 대응 컴포넌트. (targetAspect를 넘어가는 부분은 검은색 레터박스 생기게)
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public sealed class AspectRatioLetterbox : MonoBehaviour
    {
        [Header("고정 화면 해상도")]
        [SerializeField] private Vector2 targetAspect = new Vector2(1080f, 1920f);

        private Camera camera;
        private int lastWidth;
        private int lastHeight;

        private void Awake()
        {
            camera = GetComponent<Camera>();
            Apply();
        }

        private void Update()
        {
            if (Screen.width == lastWidth && Screen.height == lastHeight)
            {
                return;
            }

            Apply();
        }

        private void Apply()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            // 인스펙터를 비워두거나 창이 최소화된 순간에 0 나눗셈이 나지 않게
            if (targetAspect.x <= 0f || targetAspect.y <= 0f || lastWidth <= 0 || lastHeight <= 0)
            {
                return;
            }

            float target = targetAspect.x / targetAspect.y;
            float current = (float)lastWidth / lastHeight;
            float scale = current / target;

            if (scale < 1f)
            {
                camera.rect = new Rect(0f, (1f - scale) * 0.5f, 1f, scale);
            }
            else
            {
                float width = 1f / scale;
                camera.rect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
            }
        }
    }
}
