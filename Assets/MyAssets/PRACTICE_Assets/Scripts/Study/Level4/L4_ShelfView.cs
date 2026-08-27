using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level4
{
    /// <summary>
    /// 4-3. 버튼 배열에 런타임으로 리스너를 걸고, 두 군데서 오는 신호를 조합해 잠근다.
    ///
    /// 규칙
    ///   1. 각 버튼의 onClick 에 런타임으로 리스너를 건다. 눌리면 LastPicked 에 그 버튼의 Type
    ///   2. 리스너 등록은 Start 에서 한 번만 (OnEnable 에 두면 켤 때마다 중복 등록)
    ///   3. 잠금 조건: session.IsRunning &amp;&amp; !runner.IsLocked 일 때만 눌린다
    ///      session.OnRunningChanged 와 runner.OnLockChanged 를 둘 다 구독한다
    ///   4. 배열에 null 칸이 있어도 터지지 않는다
    ///
    /// 검증 A — 각 버튼이 자기 디저트를 알리는가
    ///   shelfButtons[2].Button.onClick.Invoke();   →  LastPicked == shelfButtons[2].Type
    ///   shelfButtons[0].Button.onClick.Invoke();   →  LastPicked == shelfButtons[0].Type
    ///
    /// 검증 B — 두 조건의 조합
    ///   session.SetRunning(true);  runner.SetLocked(false);  →  interactable true
    ///   runner.SetLocked(true);                              →  false
    ///   runner.SetLocked(false);  session.SetRunning(false); →  false
    /// </summary>
    public sealed class L4_ShelfView : MonoBehaviour
    {
        [SerializeField] private L4_GameSession session;
        [SerializeField] private L4_OrderRunner runner;
        [SerializeField] private L3_ShelfButton[] shelfButtons;   // 3-4 재사용

        /// <summary>마지막으로 눌린 디저트. 채점용.</summary>
        public DessertType LastPicked { get; private set; }

        private void Start()
        {
            if (shelfButtons == null)
            {
                return;
            }

            foreach (L3_ShelfButton button in shelfButtons)
            {
                if (button == null)
                {
                    continue;
                }
                button.Button.onClick.AddListener(() => LastPicked = button.Type);
            }
        }
        private void OnEnable()
        {
            if (session == null || runner == null)
            {
                Debug.LogWarning(name + ": 인스펙터 연결 안됨", this);
                return;
            }

            session.OnRunningChanged += HandleChanged;
            runner.OnLockChanged += HandleChanged;

            ApplyLock();
        }

        private void OnDisable()
        {
            if (session == null || runner == null)
            {
                Debug.LogWarning(name + ": 인스펙터 연결 안됨", this);
                return;
            }
            runner.OnLockChanged -= HandleChanged;
            session.OnRunningChanged -= HandleChanged;
        }

        private void HandleChanged(bool _) => ApplyLock();

        private void ApplyLock()
        {
            if (shelfButtons == null)
            {
                return;
            }
            // 게임이 진행 중, 판정 잠금이 아닐 때만 버튼 누르기 가능
            bool interactable = session.IsRunning && !runner.IsLocked;

            foreach (L3_ShelfButton button in shelfButtons)
            {
                if (button == null)
                {
                    continue;
                }
                button.Button.interactable = interactable;
            }
        }
    }
}
