using System;

namespace Expedition33.Combat
{
    /// <summary>
    /// Shared team Gradient charge bar. Fills from landing attacks, taking hits, and successful
    /// parries/dodges. When full, the team can unleash a cinematic Gradient Super Attack.
    /// Range: 0..MaxCharge (default 100). OnGradientChanged fires on every change.
    /// </summary>
    public class GradientMeter
    {
        private int _currentCharge;
        private readonly int _maxCharge;

        public int CurrentCharge => _currentCharge;
        public int MaxCharge => _maxCharge;
        public bool IsReady => _currentCharge >= _maxCharge;

        public event Action<int, int> OnGradientChanged;
        public event Action OnGradientReady;

        public GradientMeter(int maxCharge = 100)
        {
            _maxCharge = maxCharge;
            _currentCharge = 0;
        }

        /// <summary>Adds charge (clamped). Returns true if meter just became ready.</summary>
        public bool AddCharge(int amount)
        {
            if (amount <= 0) return false;
            bool wasReady = IsReady;
            _currentCharge = Math.Min(_currentCharge + amount, _maxCharge);
            OnGradientChanged?.Invoke(_currentCharge, _maxCharge);
            if (!wasReady && IsReady)
            {
                OnGradientReady?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>Consumes the full bar. Returns true if it was ready.</summary>
        public bool ConsumeForSuperAttack()
        {
            if (!IsReady) return false;
            _currentCharge = 0;
            OnGradientChanged?.Invoke(_currentCharge, _maxCharge);
            return true;
        }
    }
}
