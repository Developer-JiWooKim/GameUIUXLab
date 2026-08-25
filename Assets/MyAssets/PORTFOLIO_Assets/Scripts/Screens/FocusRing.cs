using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Screens
{
    /// <summary>
    /// 현재 선택된 버튼(Focus)에 포인트 줌. (사각형 테두리)
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class FocusRing : MonoBehaviour
    {
        [Header("UIInputRouter")]
        [SerializeField] private UIInputRouter inputRouter;

        [Header("사각형 테두리 색깔")]
        [SerializeField] private Color ringColor = new Color(1f, 0.78f, 0.2f);

        [Header("테두리 두께(px)")]
        [SerializeField] private float thickness = 6f;

        [Header("버튼 바깥으로 띄울 간격(px)")]
        [SerializeField] private float padding = 6f;

        private RectTransform rect;
        private Canvas canvas;

        private readonly Image[] edges = new Image[4];
        private readonly Vector3[] corners = new Vector3[4];

        private bool visible = true;

        private GameObject lastSelected;
        private Selectable lastSelectable;

        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            rect = (RectTransform)transform;
            canvas = GetComponentInParent<Canvas>();

            BuildEdges();
            SetVisible(false);
        }

        private void BuildEdges()
        {
            // 위·아래는 가로로, 좌·우는 세로로 늘어남.
            edges[0] = CreateEdge("Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, thickness));
            edges[1] = CreateEdge("Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, thickness));
            edges[2] = CreateEdge("Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(thickness, 0f));
            edges[3] = CreateEdge("Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(thickness, 0f));
        }

        private Image CreateEdge(string edgeName, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            GameObject edge = new GameObject(edgeName, typeof(RectTransform), typeof(Image));
            edge.transform.SetParent(rect, false);

            RectTransform edgeRect = (RectTransform)edge.transform;
            edgeRect.anchorMin = anchorMin;
            edgeRect.anchorMax = anchorMax;
            edgeRect.pivot = new Vector2(0.5f, 0.5f);
            edgeRect.anchoredPosition = Vector2.zero;
            edgeRect.sizeDelta = sizeDelta;

            Image image = edge.GetComponent<Image>();
            image.color = ringColor;

            image.raycastTarget = false;

            return image;
        }

        private void LateUpdate()
        {
            if (inputRouter != null && !inputRouter.IsSelectionMode)
            {
                SetVisible(false);
                return;
            }

            GameObject selected = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject
                : null;

            if (selected == null || !selected.activeInHierarchy)
            {
                SetVisible(false);
                return;
            }

            bool selectionChanged = selected != lastSelected;
            if (selectionChanged)
            {
                lastSelected = selected;
                lastSelectable = selected.GetComponent<Selectable>();
            }

            // 눌리지 않는 버튼(판정 중 잠긴 진열대 등)에는 테두리를 두르지 않게.
            if (lastSelectable == null || !lastSelectable.IsInteractable())
            {
                SetVisible(false);
                return;
            }

            RectTransform target = selected.transform as RectTransform;

            if (target == null)
            {
                SetVisible(false);
                return;
            }

            // 해상도가 바뀌면 Fit 이 나누는 Canvas 배율이 달라지므로 그때도 다시 맞춘다.
            bool resized = Screen.width != lastScreenWidth || Screen.height != lastScreenHeight;

            if (selectionChanged || resized)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                Fit(target);
            }

            SetVisible(true);
        }

        private void Fit(RectTransform target)
        {
            // GetWorldCorners 순서: 0 좌하 · 1 좌상 · 2 우상 · 3 우하
            target.GetWorldCorners(corners);

            rect.position = (corners[0] + corners[2]) * 0.5f;

            // Canvas Scaler 가 캔버스 전체를 확대·축소하므로 월드 길이를 그대로 쓰면
            // 해상도가 바뀔 때 테두리 크기가 어긋난다. 캔버스 배율로 나눠 되돌린다.
            float scale = canvas != null ? canvas.transform.lossyScale.x : 1f;

            if (Mathf.Approximately(scale, 0f))
            {
                scale = 1f;
            }

            float width = Vector3.Distance(corners[0], corners[3]) / scale;
            float height = Vector3.Distance(corners[0], corners[1]) / scale;

            rect.sizeDelta = new Vector2(width + padding * 2f, height + padding * 2f);
        }

        private void SetVisible(bool value)
        {
            if (visible == value)
            {
                return;
            }

            visible = value;

            foreach (Image edge in edges)
            {
                if (edge != null)
                {
                    edge.enabled = value;
                }
            }
        }
    }
}
