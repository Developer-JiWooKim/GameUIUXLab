using System;
using System.Collections.Generic;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level3;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level4
{
    /// <summary>
    /// 4-4. OrderSession 을 감싸서 "사건" 과 "상태" 두 성격의 이벤트를 발행한다.
    ///
    /// 규칙
    ///   1. Begin 은 OnOrderChanged 와 OnTrayChanged 를 각각 한 번 쏜다
    ///   2. Pick 은 결과와 무관하게 OnPicked 를 한 번 쏜다 (사건은 일어났으니까)
    ///   3. OnTrayChanged 는 쟁반이 실제로 바뀌었을 때만. Rejected 면 안 쏜다
    ///   4. IsLocked 가 true 면 Pick 은 아무것도 하지 않는다 (이벤트도 안 쏨)
    ///   5. SetLocked 는 값이 바뀔 때만 OnLockChanged 를 쏜다
    ///
    /// 검증
    ///   runner.Begin(new[]{ 초코, 초코, 레몬 });   →  orderChanged 1, trayChanged 1
    ///   runner.Pick(키위);   // Rejected            →  picked 1,  trayChanged 증가분 0
    ///   runner.Pick(초코);   // Accepted            →  picked 2,  trayChanged 증가분 1
    ///   runner.SetLocked(true); runner.Pick(초코);  →  picked 증가분 0
    /// </summary>
    public sealed class L4_OrderRunner : MonoBehaviour
    {
        private readonly OrderSession session = new();   // 3-3 재사용

        public IReadOnlyList<DessertType> Order => session.Order;

        public IReadOnlyList<DessertType> Tray => session.Tray;

        private bool _isLocked;

        public bool IsLocked => _isLocked;

        // TODO: 이벤트 4개
        //   【사건】 OnPicked(PickOutcome)                     — 토스트가 듣는다
        //   【상태】 OnTrayChanged(IReadOnlyList<DessertType>) — 쟁반 아이콘이 듣는다
        //   【상태】 OnOrderChanged(IReadOnlyList<DessertType>)
        //   【상태】 OnLockChanged(bool)                       — 4-3 ShelfView 가 듣는다

        public event Action<PickOutcome> OnPicked;
        public event Action<IReadOnlyList<DessertType>> OnTrayChanged;
        public event Action<IReadOnlyList<DessertType>> OnOrderChanged;
        public event Action<bool> OnLockChanged;
        public void Begin(IReadOnlyList<DessertType> newOrder)
        {
            session.Begin(newOrder);
            OnTrayChanged?.Invoke(Tray);
            OnOrderChanged?.Invoke(Order);
        }

        public void Pick(DessertType type)
        {
            if (_isLocked)
            {
                return;
            }

            PickOutcome result = session.Pick(type);

            OnPicked?.Invoke(result);

            if (result != PickOutcome.Rejected)
            {
                OnTrayChanged?.Invoke(Tray);
            }
        }

        public void SetLocked(bool locked)
        {
            if (_isLocked == locked)
            {
                return;
            }

            _isLocked = locked;
            OnLockChanged?.Invoke(_isLocked);
        }
    }
}
