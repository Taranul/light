using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expedition33.Combat
{
    public enum StatusEffectType
    {
        None = 0,
        Burn = 1,
        Haste = 2,
        Weaken = 3,
        Vulnerable = 4
    }

    [Serializable]
    public class ActiveStatusEffect
    {
        [SerializeField] private StatusEffectType _type;
        [SerializeField] private int _remainingTurns;
        [SerializeField] private int _potency;

        public StatusEffectType Type => _type;
        public int RemainingTurns { get => _remainingTurns; set => _remainingTurns = value; }
        public int Potency => _potency;

        public ActiveStatusEffect(StatusEffectType type, int remainingTurns, int potency = 0)
        {
            _type = type;
            _remainingTurns = remainingTurns;
            _potency = potency;
        }
    }

    public class StatusEffectController
    {
        private readonly List<ActiveStatusEffect> _activeEffects = new();

        public IReadOnlyList<ActiveStatusEffect> ActiveEffects => _activeEffects;

        public event Action<StatusEffectType, int> OnStatusApplied;
        public event Action<StatusEffectType> OnStatusExpired;
        public event Action<StatusEffectType, int> OnStatusTicked;

        public void ApplyStatus(StatusEffectType type, int duration, int potency = 0)
        {
            if (type == StatusEffectType.None || duration <= 0)
                return;

            var existing = _activeEffects.Find(e => e.Type == type);
            if (existing != null)
            {
                existing.RemainingTurns = Mathf.Max(existing.RemainingTurns, duration);
            }
            else
            {
                _activeEffects.Add(new ActiveStatusEffect(type, duration, potency));
            }

            OnStatusApplied?.Invoke(type, duration);
        }

        public void ProcessTurnStart(CombatActorStats target)
        {
            if (target == null || target.IsDefeated)
                return;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];

                if (effect.Type == StatusEffectType.Burn)
                {
                    int burnDamage = Mathf.Max(5, effect.Potency);
                    target.ApplyDamage(burnDamage);
                    OnStatusTicked?.Invoke(StatusEffectType.Burn, burnDamage);
                }

                effect.RemainingTurns--;
                if (effect.RemainingTurns <= 0)
                {
                    StatusEffectType expiredType = effect.Type;
                    _activeEffects.RemoveAt(i);
                    OnStatusExpired?.Invoke(expiredType);
                }
            }
        }

        public bool HasStatus(StatusEffectType type)
        {
            return _activeEffects.Exists(e => e.Type == type);
        }

        public float GetSpeedMultiplier()
        {
            return HasStatus(StatusEffectType.Haste) ? 1.30f : 1.0f;
        }

        public float GetAttackMultiplier()
        {
            return HasStatus(StatusEffectType.Weaken) ? 0.75f : 1.0f;
        }

        public float GetIncomingDamageMultiplier()
        {
            return HasStatus(StatusEffectType.Vulnerable) ? 1.25f : 1.0f;
        }

        public void ClearAll()
        {
            _activeEffects.Clear();
        }
    }
}
