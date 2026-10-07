using System;
using UnityEngine;

namespace Expedition33.Combat
{
    public class ActionPointPool
    {
        private readonly int _maxAP;
        private int _currentAP;

        public int MaxAP => _maxAP;
        public int CurrentAP => _currentAP;

        public event Action<int, int> OnAPChanged;

        public ActionPointPool(int maxAP = 6, int startingAP = 2)
        {
            _maxAP = Mathf.Max(1, maxAP);
            _currentAP = Mathf.Clamp(startingAP, 0, _maxAP);
        }

        public bool CanSpend(int amount)
        {
            return amount >= 0 && _currentAP >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount))
                return false;

            _currentAP -= amount;
            OnAPChanged?.Invoke(_currentAP, _maxAP);
            return true;
        }

        public void Generate(int amount)
        {
            if (amount <= 0)
                return;

            _currentAP = Mathf.Min(_maxAP, _currentAP + amount);
            OnAPChanged?.Invoke(_currentAP, _maxAP);
        }
    }
}
