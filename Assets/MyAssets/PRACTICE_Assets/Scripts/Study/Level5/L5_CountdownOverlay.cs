using System.Threading;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level5
{
    /// <summary>
    /// 5-3. 3 → 2 → 1 → Start! 오버레이. 종료 시엔 Timeout!!
    /// 두 연출을 같은 내부 메서드로 처리한다.
    ///
    /// 규칙
    ///   1. 호출자가 넘긴 토큰과 자기 destroyCancellationToken 을 둘 다 감시한다
    ///   2. 인트로는 끝나면 패널을 끈다
    ///   3. 타임아웃은 끝나도 패널을 켠 채로 둔다 (그 위로 페이드가 덮이므로)
    ///   4. 중간에 취소되어도 패널이 켜진 채 남지 않는다      ← try/finally
    ///
    /// 검증 A : await overlay.PlayIntro(CancellationToken.None);  → IsPanelActive false
    /// 검증 B : 재생 중 cts.Cancel();                             → IsPanelActive false
    /// </summary>
    public sealed class L5_CountdownOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI text;

        [SerializeField] private string[] introSteps = { "3", "2", "1", "Start!" };
        [SerializeField] private float introStepSeconds = 0.7f;

        [SerializeField] private string timeoutMessage = "Timeout!!";
        [SerializeField] private float timeoutSeconds = 1f;

        /// <summary>채점용.</summary>
        public bool IsPanelActive => panel != null && panel.activeSelf;

        public async Awaitable PlayIntro(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public async Awaitable PlayTimeout(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        // TODO: private async Awaitable ShowSteps(string[] steps, float secondsPerStep,
        //                                         bool hideWhenDone, CancellationToken token)
        //         using CancellationTokenSource linked =
        //             CancellationTokenSource.CreateLinkedTokenSource(token, destroyCancellationToken);
        //         SetPanelActive(true);
        //         try   { foreach ... await ... linked.Token }
        //         finally { if (hideWhenDone) SetPanelActive(false); }
    }
}
