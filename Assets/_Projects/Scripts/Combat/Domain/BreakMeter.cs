using System;
using UnityEngine;

namespace Expedition33.Combat
{
    [Serializable]
    public class BreakMeter
    {
        [SerializeField] private int _maxBreak = 100;
        [SerializeField] private int _currentBreak;
        [SerializeField] private bool _isBroken;
        [SerializeField] private int _brokenTurnsRemaining;
        [SerializeField] private float _brokenDamageMultiplier = 1.75f;

        public int MaxBreak => _maxBreak;
        public int CurrentBreak => _currentBreak;
        public bool IsBroken => _isBroken;
        public int BrokenTurnsRemaining => _brokenTurnsRemaining;
        public float BrokenDamageMultiplier => _isBroken ? _brokenDamageMultiplier : 1.0f;

        public event Action<int, int> OnBreakChanged;
        public event Action OnBroken;
        public event Action OnRecovered;

        public BreakMeter(int maxBreak = 100, float brokenDamageMultiplier = 1.75f)
        {
            _maxBreak = Mathf.Max(10, maxBreak);
            _currentBreak = 0;
            _isBroken = false;
            _brokenTurnsRemaining = 0;
            _brokenDamageMultiplier = Mathf.Max(1.0f, brokenDamageMultiplier);
        }

        public bool AddBreak(int amount)
        {
            if (_isBroken || amount <= 0)
                return false;

            _currentBreak = Mathf.Min(_maxBreak, _currentBreak + amount);
            OnBreakChanged?.Invoke(_currentBreak, _maxBreak);

            if (_currentBreak >= _maxBreak)
            {
                TriggerBreak();
                return true;
            }

            return false;
        }

        public void TriggerBreak(int turns = 1)
        {
            _isBroken = true;
            _currentBreak = _maxBreak;
            _brokenTurnsRemaining = Mathf.Max(1, turns);
            OnBreakChanged?.Invoke(_currentBreak, _maxBreak);
            OnBroken?.Invoke();
        }

        public bool ConsumeBrokenTurn()
        {
            if (!_isBroken)
                return false;

            _brokenTurnsRemaining--;
            if (_brokenTurnsRemaining <= 0)
            {
                Recover();
                return false;
            }

            return true;
        }

        public void Recover()
        {
            _isBroken = false;
            _currentBreak = 0;
            _brokenTurnsRemaining = 0;
            OnBreakChanged?.Invoke(_currentBreak, _maxBreak);
            OnRecovered?.Invoke();
        }

        public void ResetBreak()
        {
            _isBroken = false;
            _currentBreak = 0;
            _brokenTurnsRemaining = 0;
            OnBreakChanged?.Invoke(_currentBreak, _maxBreak);
        }
    }
}
