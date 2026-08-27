using System;
using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level3
{
    /// <summary>
    /// 3-5. 남은 시간을 초 단위로 표시. SetTime 은 매 프레임 불리지만
    /// 글자는 1초에 한 번만 바뀌어야 한다.
    ///
    /// 요구사항
    ///   1. TextMeshProUGUI 를 [SerializeField] private 로 받는다
    ///   2. 올림한 정수 초. 음수는 "0" (2단계 2-3 규칙 그대로)
    ///   3. 표시할 초가 직전과 같으면 text 에 대입하지 않는다
    ///   4. OnEnable 에서 캐시를 초기화 → 재활성화 직후 첫 호출은 반드시 대입
    ///   5. 참조가 null 이면 예외 대신 경고를 "한 번만" 남기고 조용히 무시
    ///      Debug.LogWarning(name + ": ...", this)   ← 두 번째 인자 필수
    ///
    /// 3-1 의 ChangeTracker 를 가져다 쓰면 3번과 4번이 거의 공짜로 풀린다.
    ///
    /// 검증
    ///   float[] steps = { 3.0f, 2.5f, 2.0f, 1.5f, 1.0f, 0.5f, 0.0f };
    ///   //  올림 →       3     3     2     2     1     1     0
    ///   //  대입 →       O     X     O     X     O     X     O   = 4회
    ///   foreach (float t in steps) label.SetTime(t);
    ///   Debug.Log(label.WriteCount);   // 4
    ///
    ///   int before = label.WriteCount;
    ///   label.gameObject.SetActive(false);
    ///   label.gameObject.SetActive(true);
    ///   label.SetTime(0.0f);                     // 직전과 같은 0초여도 대입돼야 한다
    ///   Debug.Log(label.WriteCount - before);    // 1
    /// </summary>
    public sealed class L3_TimerLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        private readonly ChangeTracker changeTracker = new();
        private bool warned;
        private void OnEnable()
        {
            changeTracker.Reset();
        }

        /// <summary>채점용. text 에 실제로 대입한 횟수.</summary>
        public int WriteCount { get; private set; }

        /// <summary>외부에서 매 프레임 호출한다.</summary>
        public void SetTime(float remainingSeconds)
        {
            if (label == null)
            {
                if (!warned)
                {
                    warned = true;
                    Debug.LogWarning(name + ": label is null", this);
                }
                return;
            }
            int seconds = (int)Math.Max(0f, Math.Ceiling(remainingSeconds));

            if (changeTracker.TryUpdate(seconds))
            {
                label.text = seconds.ToString();
                WriteCount++;
            }
        }
    }
}
