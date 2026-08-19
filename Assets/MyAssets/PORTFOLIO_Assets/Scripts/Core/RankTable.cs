using System;
using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core
{
    /// <summary>
    /// 성공·실패 건수로 최종 등급을 판정하는 조회 테이블. UIRoot에 붙인다.
    ///
    /// ResultView가 아니라 별도 컴포넌트인 이유:
    ///   랭크는 표시가 아니라 판단이다. "View는 스스로 판단하지 않는다"는 원칙에 따라
    ///   규칙을 데이터로 분리한다. GameState에 넣지 않는 이유는 게임이 끝날 때 한 번만
    ///   필요한 파생값이라 가장 큰 파일을 더 키울 이유가 없기 때문이다.
    ///
    /// 점수(score)를 입력으로 받지 않는 이유:
    ///   현재 설계에서 score는 successCount * 100 과 완전히 같은 값이다.
    ///   같은 정보를 두 번 세지 않도록 처리량(success)과 정확도(fail) 두 축만 쓴다.
    /// </summary>
    public class RankTable : MonoBehaviour
    {
        [Serializable]
        public struct RankRule
        {
            [Tooltip("RankText에 표시할 등급 문자")]
            public string label;

            [Tooltip("이 값 이상이면 이 등급. 마지막 항목(F)은 int.MinValue로 둘 것")]
            public int minRankScore;

            [Tooltip("등급별 RankText 색. 알파를 255로 올리는 것을 잊지 말 것")]
            public Color color;
        }

        [Header("가중치")]
        [Tooltip("성공 1건당 가산점")]
        [SerializeField] private int successWeight = 100;

        [Tooltip("실패 1건당 감점. 실패는 1탭이면 끝나 시간을 거의 쓰지 않으므로 감점이 없으면 막 눌러보기가 무비용이 된다")]
        [SerializeField] private int failPenalty = 50;

        [Header("등급 규칙 (높은 등급부터 순서대로)")]
        [SerializeField] private RankRule[] rules;

        /// <summary>랭크 계산용 점수. HUD에 표시되는 score와는 다른 값이다.</summary>
        public int CalculateRankScore(int successCount, int failCount)
        {
            // TODO: successCount * successWeight - failCount * failPenalty
            //   음수가 나올 수 있다(0성공 20실패 = -1000). 정상이며 F로 떨어진다.
            return 0;
        }

        /// <summary>등급 규칙을 돌려준다. ResultView가 label과 color를 함께 쓴다.</summary>
        public RankRule Evaluate(int successCount, int failCount)
        {
            // TODO:
            //   1) rules가 비어 있으면 label "-" + Color.white 를 돌려준다.
            //      인스펙터를 안 채운 채 실행해도 예외가 나지 않게 한다.
            //   2) CalculateRankScore 로 점수를 구한다.
            //   3) rules를 위에서부터 훑어 minRankScore 이하로 처음 걸리는 항목을 반환 (>= 비교)
            //   4) 어디에도 안 걸리면 마지막 항목을 반환.
            //      마지막 항목의 minRankScore를 int.MinValue로 두면 이 경우는 생기지 않는다.
            //   5) 반환 직전에 color.a <= 0 이면 Color.white 로 보정한다.
            //      Serializable struct의 Color 기본값은 알파 0이라, 인스펙터에서 색을 안 만지면
            //      글자가 투명해져 아무것도 안 보인다. 조용한 버그가 되기 쉬운 자리다.
            return default;
        }
    }
}
