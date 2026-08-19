using System.Collections.Generic;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.States;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.UI
{
    /// <summary>
    /// 화면 스택을 소유하고 패널 활성화·키보드 포커스를 전담한다. UIRoot에 붙인다.
    ///
    /// 평면 FSM이 아니라 스택인 이유:
    ///   Play → Pause → Confirm 은 전환이 아니라 겹침이다. Push는 아래층의 Exit()를
    ///   부르지 않으므로, Pause에서 돌아올 때 PlayState.Enter()가 재호출되어 게임이
    ///   다시 시작되는 문제가 구조적으로 생기지 않는다.
    ///
    /// 연산 3개로 전체 흐름이 덮인다.
    ///   Set   — 스택을 비우고 새로 쌓는다 (Title / Play / Result 간 이동)
    ///   Push  — 아래층을 그대로 두고 겹친다 (Play→Pause, Pause→Confirm)
    ///   Pop   — 맨 위 한 겹만 닫는다 (Cancel의 기본 동작)
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        [Header("화면 상태 (각 Screen_ 패널의 컴포넌트)")]
        [SerializeField] private TitleState titleState;
        [SerializeField] private PlayState playState;
        [SerializeField] private PauseState pauseState;
        [SerializeField] private ConfirmState confirmState;
        [SerializeField] private ResultState resultState;

        // 맨 뒤가 스택의 top. Set에서 역순 순회가 필요해 Stack<T> 대신 List를 쓴다.
        private readonly List<UiStateBase> stack = new List<UiStateBase>();

        /// <summary>스택 맨 위. 비어 있으면 null.</summary>
        public UiStateBase Top => stack.Count > 0 ? stack[stack.Count - 1] : null;

        /// <summary>UiInputRouter의 선택 복구가 사용한다. 현재 화면의 첫 버튼.</summary>
        public GameObject CurrentFirstSelected => Top != null ? Top.FirstSelected : null;

        private void Awake()
        {
            // TODO: 5개 상태에 Bind(this) 호출
            // TODO: 5개 패널을 전부 SetActive(false) — 씬에 어떤 게 켜진 채 저장돼 있어도 시작 상태를 일정하게 만든다
        }

        private void Start()
        {
            // TODO: ShowTitle();
            //   Awake가 아니라 Start인 이유: 다른 컴포넌트의 Awake가 끝난 뒤 포커스를 잡아야 안전하다.
        }

        private void OnEnable()
        {
            // TODO: GameState 작성 후 gameState.OnGameOver += ShowResult;
        }

        private void OnDisable()
        {
            // TODO: gameState.OnGameOver -= ShowResult;
            //   구독 해제를 빼먹으면 씬 재로드 시 죽은 핸들러가 호출된다.
        }

        // ── 버튼 onClick에서 부르는 진입점 ──────────────────────────────

        public void ShowTitle()
        {
            // TODO: Set(titleState);
        }

        public void ShowPlay()
        {
            // TODO: Set(playState);
        }

        public void ShowResult()
        {
            // TODO: Set(resultState);
        }

        public void ShowPause()
        {
            // TODO: Push(pauseState);
        }

        public void HidePause()
        {
            // TODO: Pop();
        }

        public void OpenConfirm()
        {
            // TODO: Push(confirmState);
        }

        public void CloseConfirm()
        {
            // TODO: Pop();
        }

        public void Quit()
        {
            // TODO: 에디터에서는 Application.Quit()이 아무 일도 하지 않으므로 분기가 필요하다.
            // #if UNITY_EDITOR
            //     UnityEditor.EditorApplication.isPlaying = false;
            // #else
            //     Application.Quit();
            // #endif
        }

        /// <summary>Esc / 게임패드 B. UiInputRouter가 호출한다.</summary>
        public void HandleCancel()
        {
            // TODO: Top?.OnCancel();
            //   분기를 여기에 두지 말 것. 화면별 동작은 각 상태의 OnCancel이 안다.
            //   그래서 "가장 위 레이어 하나만 닫는다"가 저절로 지켜진다.
        }

        // ── 스택 연산 ──────────────────────────────────────────────────

        /// <summary>스택을 전부 비우고 하나만 쌓는다. 화면 간 이동에 사용.</summary>
        private void Set(UiStateBase next)
        {
            // TODO:
            //   1) 스택이 빌 때까지 Pop (단, 매번 포커스를 잡을 필요는 없으므로 마지막에 한 번만)
            //   2) Push(next)
        }

        /// <summary>아래층을 그대로 두고 겹쳐 올린다. 일시정지·팝업에 사용.</summary>
        private void Push(UiStateBase next)
        {
            // TODO:
            //   1) stack.Add(next)
            //   2) next.gameObject.SetActive(true)
            //   3) next.Enter()
            //   4) ApplyFocus()
            //   순서 주의: 패널을 켜기 전에 포커스를 주면 비활성 오브젝트라 선택이 먹지 않는다.
        }

        /// <summary>맨 위 한 겹만 닫는다.</summary>
        private void Pop()
        {
            // TODO:
            //   1) 스택이 비어 있으면 아무것도 하지 않는다
            //   2) top.Exit()
            //   3) top.gameObject.SetActive(false)
            //   4) stack에서 제거
            //   5) ApplyFocus()  ← 새 top의 첫 버튼으로 포커스를 되돌린다.
            //      이걸 빠뜨리면 확인 팝업을 닫았을 때 포커스가 사라진 채 Pause에 남는다.
        }

        /// <summary>현재 top의 첫 버튼으로 키보드 포커스를 지정한다.</summary>
        private void ApplyFocus()
        {
            // TODO:
            //   EventSystem.current.SetSelectedGameObject(null);
            //   EventSystem.current.SetSelectedGameObject(CurrentFirstSelected);
            //
            //   null로 한 번 비우는 이유: 이전 선택이 남아 있으면 같은 오브젝트를 다시 지정할 때
            //   OnSelect가 발생하지 않아 하이라이트가 안 그려지는 경우가 있다.
            //   화면을 켤 때마다 이 호출이 없으면 전환 후 키보드가 먹통이 된다. 요건 5번 직결.
        }
    }
}
