using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level4
{
    /// <summary>
    /// 4-2. GameSession 의 이벤트를 구독해 남은 시간을 표시한다.  ★ 이 단계의 핵심
    /// Update() 를 만들지 않는다. 이벤트가 올 때만 일한다.
    ///
    /// 규칙
    ///   1. OnEnable 에서 OnTimeChanged 구독, OnDisable 에서 반드시 해제
    ///   2. 구독 직후 현재 값으로 한 번 즉시 갱신
    ///   3. 갱신할 때마다 RefreshCount 를 1 올린다
    ///   4. session / label 이 null 이면 경고 한 번 남기고 조용히 넘어간다
    ///
    /// 검증 A — 껐다 켜도 구독이 쌓이지 않는가
    ///   for (int i = 0; i &lt; 3; i++) { hud.gameObject.SetActive(false); hud.gameObject.SetActive(true); }
    ///   int before = hud.RefreshCount;
    ///   session.Tick(0.1f);                     // OnTimeChanged 를 딱 1번 발행
    ///   hud.RefreshCount - before == 1          // 4 가 나오면 구독이 4겹 쌓인 것
    ///
    /// 검증 B — 켜지는 순간 즉시 동기화하는가
    ///   hud.gameObject.SetActive(false);
    ///   int before2 = hud.RefreshCount;
    ///   hud.gameObject.SetActive(true);         // 이벤트는 아직 안 왔다
    ///   hud.RefreshCount - before2 == 1
    /// </summary>
    public sealed class L4_TimerHud : MonoBehaviour
    {
        [SerializeField] private L4_GameSession session;
        [SerializeField] private TextMeshProUGUI label;

        /// <summary>채점용. 화면을 실제로 갱신한 횟수.</summary>
        public int RefreshCount { get; private set; }

        private void OnEnable()
        {
            if (session == null || label == null)
            {
                Debug.LogWarning(name + ": 인스펙터 연결 안됨", this);
                return;
            }
            session.OnTimeChanged += Refresh;
            Refresh(session.Remaining, session.PlayTime);
        }
        private void OnDisable()
        {
            if (session == null)
            {
                return;
            }

            session.OnTimeChanged -= Refresh;
        }

        private void Refresh(float remaining, float total)
        {
            label.text = remaining.ToString() + "/" + total.ToString();
            RefreshCount++;
        }
    }
}
