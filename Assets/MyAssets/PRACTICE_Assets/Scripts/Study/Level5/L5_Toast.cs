using System;
using System.Threading;
using Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level4;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level5
{
    /// <summary>
    /// 5-2. 판정 결과를 0.5초 띄웠다 지운다.  ★ CTS 재생성 패턴
    ///
    /// 규칙
    ///   1. OnEnable 에서 runner.OnPicked 구독, OnDisable 에서 해제 (4단계 그대로)
    ///   2. Rejected 면 "실패..", 아니면 "성공!"
    ///   3. 0.5초 뒤 자동으로 꺼지고 HideCount 가 1 오른다
    ///   4. 떠 있는 동안 새 토스트가 오면, 새 토스트가 0.5초를 온전히 채운다   ← 핵심
    ///   5. OnDisable / OnDestroy 에서 대기를 취소한다
    ///
    /// 검증 (0.3초 간격으로 두 번 띄운 뒤)
    ///   toast.IsVisible  == true    ← 첫 타이머가 껐으면 false
    ///   toast.HideCount  == 0       ← 1이면 첫 타이머가 살아 있었다는 뜻
    ///   다시 0.3초 뒤 → IsVisible false, HideCount 1
    /// </summary>
    public sealed class L5_Toast : MonoBehaviour
    {
        [SerializeField] private L4_OrderRunner runner;
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float showSeconds = 0.5f;

        [SerializeField] private string successMessage = "성공!";
        [SerializeField] private string failMessage = "실패..";

        // TODO: private CancellationTokenSource hideCts;

        /// <summary>채점용.</summary>
        public bool IsVisible => root != null && root.activeSelf;

        public int HideCount { get; private set; }

        // TODO: OnEnable  — SetVisible(false) + 구독
        // TODO: OnDisable — 해제 + 취소 + SetVisible(false)
        // TODO: OnDestroy — 취소
        // TODO: HandlePicked(PickOutcome) — 문구 정하고 SetVisible(true) 후 RunHide()
        // TODO: async void RunHide()
        //         ① CancelHide() 로 이전 대기를 끊는다
        //         ② CreateLinkedTokenSource(destroyCancellationToken) 로 파괴도 함께 감시
        //         ③ try { await ... } catch (OperationCanceledException) { return; }
        //         ④ SetVisible(false); HideCount++;
        // TODO: CancelHide() — Cancel() → Dispose() → null  세 줄이 한 세트
    }
}
