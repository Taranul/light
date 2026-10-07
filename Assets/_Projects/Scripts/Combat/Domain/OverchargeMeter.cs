using System;
using UnityEngine;

namespace Expedition33.Combat
{
    [Serializable]
    public class OverchargeMeter
    {
        [SerializeField] private int _maxCharges = 3;
        [SerializeField] private int _currentCharges;

        public int MaxCharges => _maxCharges;
        public int CurrentCharges => _currentCharges;
        public bool IsFullyCharged => _currentCharges >= _maxCharges;

        public event Action<int, int> OnOverchargeChanged;

        public OverchargeMeter(int maxCharges = 3)
        {
            _maxCharges = Mathf.Max(1, maxCharges);
            _currentCharges = 0;
        }

        public void AddCharge(int amount = 1)
        {
            if (amount <= 0)
                return;

            _currentCharges = Mathf.Min(_maxCharges, _currentCharges + amount);
            OnOverchargeChanged?.Invoke(_currentCharges, _maxCharges);
        }

        public int ConsumeAllCharges()
        {
            int consumed = _currentCharges;
            _currentCharges = 0;
            OnOverchargeChanged?.Invoke(_currentCharges, _maxCharges);
            return consumed;
        }

        public bool TryConsume(int amount)
        {
            if (amount <= 0 || _currentCharges < amount)
                return false;

            _currentCharges -= amount;
            OnOverchargeChanged?.Invoke(_currentCharges, _maxCharges);
            return true;
        }

        public void ResetCharges()
        {
            _currentCharges = 0;
            OnOverchargeChanged?.Invoke(_currentCharges, _maxCharges);
        }
    }
}
