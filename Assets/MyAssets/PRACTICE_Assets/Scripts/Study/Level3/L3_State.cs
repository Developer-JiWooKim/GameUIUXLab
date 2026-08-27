using System;
using System.Collections.Generic;
using System.Linq;
using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level3
{
    // ─────────────────────────────────────────────────────────────
    // 3-1. ChangeTracker — 값이 바뀌었을 때만 알려주는 클래스
    //
    //   var t = new ChangeTracker();
    //   t.TryUpdate(0);   // true    ← 첫 호출은 무조건 true
    //   t.TryUpdate(0);   // false
    //   t.TryUpdate(5);   // true
    //   t.TryUpdate(5);   // false
    //   t.Reset();
    //   t.TryUpdate(0);   // true    ← Reset 직후 첫 호출도 무조건 true
    // ─────────────────────────────────────────────────────────────
    public sealed class ChangeTracker
    {
        private int previousValue = 0;
        private bool hasValue = false;

        /// <summary>직전 값과 다르면 저장하고 true. 같으면 아무것도 안 하고 false.</summary>
        public bool TryUpdate(int value)
        {
            if (hasValue && previousValue == value)
            {
                return false;
            }
            else
            {
                hasValue = true;
                previousValue = value;
                return true;
            }
        }

        /// <summary>"아직 아무 값도 안 받았다" 상태로 되돌린다.</summary>
        public void Reset()
        {
            hasValue = false;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 3-2. Countdown — 멈출 수 있는 카운트다운
    //
    //   c.Reset(1f);        // Remaining=1,   IsRunning=false
    //   c.Tick(0.5f);       // false          ← 멈춰 있어 시간이 안 간다 (Remaining 그대로 1)
    //   c.SetRunning(true);
    //   c.Tick(0.5f);       // false          Remaining=0.5
    //   c.Tick(0.5f);       // true   ←       이번 호출로 0이 됐다
    //   c.Tick(0.5f);       // false  ←       이미 0이었다
    // ─────────────────────────────────────────────────────────────
    public sealed class Countdown
    {
        private float _remaining;
        private bool _isRunning;
        public float Remaining => _remaining;
        public bool IsRunning => _isRunning;


        /// <summary>시간을 설정하고 되감는다. 멈춘 상태(IsRunning=false)가 된다. 음수를 넣어도 0 밑으로 안 간다.</summary>
        public void Reset(float seconds)
        {
            _remaining = Math.Max(0f, seconds);
            _isRunning = false;
        }

        public void SetRunning(bool running) => _isRunning = running;

        /// <summary>시간을 흘린다. 이번 호출로 0이 되면 true. 멈춰 있거나 이미 0이면 false.</summary>
        public bool Tick(float deltaTime)
        {
            if (!_isRunning || _remaining <= 0f)
            {
                return false;
            }

            _remaining = Math.Max(0f, _remaining - deltaTime);

            return _remaining <= 0;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 3-3. OrderSession — 주문 판정  ★ 이 단계의 핵심
    //
    //   s.Begin(new[]{ 초코, 초코, 레몬 });
    //   s.Pick(초코);   // Accepted
    //   s.Pick(레몬);   // Accepted
    //   s.Pick(레몬);   // Rejected  ← 레몬은 1개뿐이었다
    //   s.Pick(키위);   // Rejected  ← 주문에 아예 없다
    //   s.Pick(초코);   // Completed
    //   s.Tray.Count;   // 3         ← Rejected 는 담기지 않는다
    //
    //   s.Begin(new[]{ 키위 });   // 이전 주문의 흔적이 남으면 안 된다
    //   s.Begin(null);            // 예외가 나지 않는다. IsComplete 는 true
    // ─────────────────────────────────────────────────────────────
    public enum PickOutcome
    {
        /// <summary>맞게 담았고, 아직 남았다.</summary>
        Accepted,

        /// <summary>맞게 담았고, 이걸로 주문이 완성됐다.</summary>
        Completed,

        /// <summary>주문에 없거나, 이미 다 담았다.</summary>
        Rejected,
    }

    public sealed class OrderSession
    {
        private List<DessertType> _order = new();
        private List<DessertType> _tray = new();

        public IReadOnlyList<DessertType> Order => _order;
        public IReadOnlyList<DessertType> Tray => _tray;

        private Dictionary<DessertType, int> _remaining = new();

        /// <summary>남은 개수가 전부 0 이하인가. 필드가 아니라 매번 계산해서 돌려준다.</summary>
        public bool IsComplete => _remaining.Values.All(v => v <= 0);

        /// <summary>새 주문을 시작한다. 이전 주문의 흔적(3가지)을 전부 지운다.</summary>
        public void Begin(IReadOnlyList<DessertType> newOrder)
        {
            _remaining.Clear();
            _order.Clear();
            _tray.Clear();

            if (newOrder is null)
            {
                return;
            }

            foreach (var menu in newOrder)
            {
                _order.Add(menu);

                _remaining.TryGetValue(menu, out int count);
                _remaining[menu] = count + 1;
            }
        }

        /// <summary>하나 담는다. 깎기 전에 검사한다.</summary>
        public PickOutcome Pick(DessertType type)
        {
            if (!_remaining.TryGetValue(type, out int count) || count <= 0)
            {
                return PickOutcome.Rejected;
            }

            _remaining[type] = count - 1;
            _tray.Add(type);

            return IsComplete ? PickOutcome.Completed : PickOutcome.Accepted;
        }
    }
}
