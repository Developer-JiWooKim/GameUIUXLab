using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level5
{
    /// <summary>
    /// 5-4. 판정 후 0.8초 잠그고 다음 손님으로.  ★★ 세대 토큰
    ///
    /// 규칙
    ///   1. Prepare() 는 CustomerNumber 를 1 로 만들고 잠금을 푼다
    ///   2. Judge() 는 IsJudging=true → 0.8초 대기 → CustomerNumber++ → IsJudging=false
    ///   3. 대기 중 Judge() 가 또 불려도 무시한다
    ///   4. 대기 중에 Prepare() 가 불리면, 그 대기는 아무 일도 하지 않고 조용히 끝난다  ← 전부
    ///
    /// 재현 시나리오 (규칙 4가 없으면)
    ///   실패 판정 → 0.8초 대기 시작 → 0.3초 만에 "다시하기"(Prepare) → 손님 1번
    ///   0.5초 뒤 죽은 줄 알았던 옛 대기가 깨어나 CustomerNumber++ → 손님이 2번으로 건너뜀
    ///
    /// 검증
    ///   Prepare();                       CustomerNumber == 1
    ///   Judge(false); 0.3초 후 Prepare(); CustomerNumber == 1,  IsJudging == false
    ///   1.0초 더 기다린 뒤              CustomerNumber == 1   ← 2면 유령이 깨어난 것
    ///   Judge(true);  1.0초 후          CustomerNumber == 2   ← 정상 경로
    ///
    /// 힌트: CancellationToken 으로도 되지만, 정수 필드 하나로 더 가볍게 풀린다.
    /// </summary>
    public sealed class L5_JudgeRunner : MonoBehaviour
    {
        [SerializeField] private float judgeDelaySeconds = 0.8f;

        public int CustomerNumber { get; private set; }

        public bool IsJudging { get; private set; }

        /// <summary>새 판을 시작한다. (다시하기)</summary>
        public void Prepare()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>판정이 났다. 잠그고 0.8초 뒤 다음 손님으로.</summary>
        public void Judge(bool success)
        {
            throw new System.NotImplementedException();
        }

        // TODO: private int judgeToken;
        // TODO: private async void BeginJudgeDelay()
        //         int token = ++judgeToken;   ← 이번 대기의 세대 번호
        //         ... await (destroyCancellationToken) ... catch (OperationCanceledException) return;
        //         if (token != judgeToken) return;   ← 깨어나 보니 세대가 바뀌었다
    }
}
