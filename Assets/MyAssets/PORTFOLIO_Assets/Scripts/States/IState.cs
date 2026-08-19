using UnityEngine;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.States
{
    /// <summary>
    /// 화면 하나의 진입·이탈·취소 동작을 정의한다.
    /// 패널 활성화와 포커스 지정은 ScreenManager가 전담하므로 여기에 넣지 않는다.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 이 화면이 스택 맨 위가 됐을 때 포커스를 줄 버튼.
        /// Pop으로 되돌아왔을 때도 여기로 포커스가 복구된다.
        /// </summary>
        GameObject FirstSelected { get; }

        /// <summary>스택에 새로 올라올 때 1회. 아래층이 Push로 덮일 때는 호출되지 않는다.</summary>
        void Enter();

        /// <summary>스택에서 빠질 때 1회. Push로 덮이는 것은 이탈이 아니므로 호출되지 않는다.</summary>
        void Exit();

        /// <summary>Esc / 게임패드 B. 화면마다 다르게 동작한다.</summary>
        void OnCancel();
    }
}
