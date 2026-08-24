using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// 현재 선택된 버튼을 테두리로 감싼다. Canvas 의 마지막 자식에 둔다.
    ///
    /// 버튼마다 테두리 오브젝트를 붙이지 않는 이유: 화면의 Selectable 이 14개인데
    /// 하나만 빠뜨려도 그 버튼에서만 조용히 표시가 사라진다. 선택은 언제나 하나뿐이므로
    /// 테두리도 하나만 두고 옮겨 다니는 편이 어긋날 자리가 없다.
    ///
    /// 선택이 비면(포인터를 쓰면 UIInputRouter 가 지운다) 테두리도 사라진다.
    /// 마우스로 조작할 때는 안 보이고 키보드·게임패드로 옮길 때만 보이는 것이 의도한 동작이다.
    /// 요건 5(키보드·게임패드 Navigate 확인 가능)의 증거가 이 테두리다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class FocusRing : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("UIRoot 의 UIInputRouter. 비워 두면 조작 방식과 무관하게 선택만 있으면 표시한다")]
        [SerializeField] private UIInputRouter inputRouter;

        [Header("모양")]
        [SerializeField] private Color ringColor = new Color(1f, 0.78f, 0.2f);

        [Tooltip("테두리 두께(px)")]
        [SerializeField] private float thickness = 6f;

        [Tooltip("버튼 바깥으로 띄울 간격(px). 0 이면 버튼에 딱 붙는다")]
        [SerializeField] private float padding = 6f;

        private RectTransform rect;
        private Canvas canvas;

        private readonly Image[] edges = new Image[4];
        private readonly Vector3[] corners = new Vector3[4];

        private bool visible = true;

        private void Awake()
        {
            rect = (RectTransform)transform;
            canvas = GetComponentInParent<Canvas>();

            BuildEdges();
            SetVisible(false);
        }

        /// <summary>
        /// 테두리 네 변을 코드로 만든다. 스프라이트 없이 Image 를 그대로 쓰면 단색 사각형이
        /// 되므로 테두리용 에셋을 따로 만들 필요가 없다.
        ///
        /// Stretch 앵커로 붙여 두면 링의 크기만 바꿔도 네 변이 알아서 따라온다.
        /// 매 프레임 변마다 위치를 계산하지 않아도 된다.
        /// </summary>
        private void BuildEdges()
        {
            // 위·아래는 가로로, 좌·우는 세로로 늘어난다.
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

            // 테두리가 버튼 위를 덮으므로 반드시 꺼야 한다. 켜 두면 링이 클릭을 가로챈다.
            image.raycastTarget = false;

            return image;
        }

        /// <summary>
        /// LateUpdate 에서 읽는 이유: EventSystem 이 Update 에서 선택을 확정하므로
        /// 그보다 뒤에서 읽어야 같은 프레임의 결과를 본다.
        ///
        /// UIInputRouter 도 LateUpdate 에서 선택을 지우거나 되살린다. 둘 사이의 실행 순서는
        /// 보장되지 않지만, 매 프레임 다시 계산하므로 어긋나도 다음 프레임에 맞춰진다.
        /// </summary>
        private void LateUpdate()
        {
            // 키보드·게임패드로 조작 중일 때만 두른다. 화면이 열릴 때 첫 버튼이 선택되는
            // 것만으로는 켜지지 않으므로, 마우스로 화면을 넘긴 직후에 테두리가 남지 않는다.
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

            // 눌리지 않는 버튼(판정 중 잠긴 진열대 등)에는 테두리를 두르지 않는다.
            // 조작할 수 있는 것처럼 보이면 안 된다.
            Selectable selectable = selected.GetComponent<Selectable>();

            if (selectable == null || !selectable.IsInteractable())
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

            Fit(target);
            SetVisible(true);
        }

        /// <summary>
        /// 대상의 월드 코너로 맞춘다. 부모가 서로 달라도(화면마다 계층이 다르다)
        /// 월드 좌표를 거치면 같은 방법으로 처리된다.
        /// </summary>
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

        /// <summary>
        /// 자기 자신을 끄지 않고 네 변만 끈다. 오브젝트를 끄면 LateUpdate 가 멈춰
        /// 다음 선택을 감지하지 못한다.
        /// </summary>
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
