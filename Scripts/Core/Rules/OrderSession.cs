using System.Collections.Generic;

namespace Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core.Rules
{
    /// <summary>
    /// 손님 한 명의 주문 판정 규칙.
    /// </summary>
    public sealed class OrderSession
    {
        private readonly List<DessertType> _order = new();
        private readonly List<DessertType> _tray = new();

        /// <summary>주문에서 아직 안 담은 개수.</summary>
        private readonly Dictionary<DessertType, int> _remaining = new();

        public IReadOnlyList<DessertType> Order => _order;

        public IReadOnlyList<DessertType> Tray => _tray;

        public bool IsComplete
        {
            get
            {
                foreach (int left in _remaining.Values)
                {
                    if (left > 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void Begin(IReadOnlyList<DessertType> newOrder)
        {
            _order.Clear();
            _remaining.Clear();
            _tray.Clear();

            if (newOrder == null)
            {
                return;
            }

            foreach (DessertType type in newOrder)
            {
                _order.Add(type);
                _remaining.TryGetValue(type, out int count);
                _remaining[type] = count + 1;
            }
        }

        public PickResult Pick(DessertType type)
        {
            if (!_remaining.TryGetValue(type, out int left) || left <= 0)
            {
                return PickResult.Rejected;
            }

            _remaining[type] = left - 1;
            _tray.Add(type);

            return IsComplete ? PickResult.Completed : PickResult.Accepted;
        }
    }
}
