using System;
using UnityEngine;

namespace Expedition33.Combat
{
    public enum CombatStance
    {
        Balanced = 0,
        Offensive = 1,
        Defensive = 2,
        Virtuoso = 3
    }

    [Serializable]
    public class StanceController
    {
        [SerializeField] private CombatStance _currentStance = CombatStance.Balanced;

        public CombatStance CurrentStance => _currentStance;

        public event Action<CombatStance> OnStanceChanged;

        public StanceController(CombatStance initialStance = CombatStance.Balanced)
        {
            _currentStance = initialStance;
        }

        public void SetStance(CombatStance stance)
        {
            if (_currentStance == stance)
                return;

            _currentStance = stance;
            OnStanceChanged?.Invoke(_currentStance);
        }

        public CombatStance CycleNextStance()
        {
            CombatStance next = _currentStance switch
            {
                CombatStance.Balanced => CombatStance.Offensive,
                CombatStance.Offensive => CombatStance.Defensive,
                CombatStance.Defensive => CombatStance.Virtuoso,
                CombatStance.Virtuoso => CombatStance.Balanced,
                _ => CombatStance.Balanced
            };

            SetStance(next);
            return next;
        }

        public float GetAttackMultiplier()
        {
            return _currentStance switch
            {
                CombatStance.Offensive => 1.30f,
                CombatStance.Defensive => 0.85f,
                _ => 1.0f
            };
        }

        public float GetDefenseMultiplier()
        {
            return _currentStance switch
            {
                CombatStance.Defensive => 1.25f,
                CombatStance.Offensive => 0.80f,
                _ => 1.0f
            };
        }

        public float GetParryWindowMultiplier()
        {
            return _currentStance switch
            {
                CombatStance.Defensive => 1.30f,
                CombatStance.Offensive => 0.80f,
                _ => 1.0f
            };
        }

        public int GetBonusAPGeneration()
        {
            return _currentStance == CombatStance.Virtuoso ? 1 : 0;
        }
    }
}
