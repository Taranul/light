using UnityEngine;

namespace Expedition33.Combat
{
    public static class DamageCalculator
    {
        public static DamageResult CalculateDamage(
            CombatActorStats attacker,
            CombatActorStats target,
            float skillMultiplier = 1f,
            bool isCritical = false)
        {
            float baseDamage = attacker.AttackPower * skillMultiplier;
            if (isCritical)
            {
                baseDamage *= 1.5f;
            }

            // Standard defense mitigation curve: Damage = Attack * (100 / (100 + Defense))
            float mitigationFactor = 100f / (100f + target.Defense);
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * mitigationFactor));

            bool willDefeat = (target.CurrentHealth - finalDamage) <= 0;

            return new DamageResult(
                rawDamage: Mathf.RoundToInt(baseDamage),
                mitigatedDamage: finalDamage,
                isCritical: isCritical,
                isDefeated: willDefeat
            );
        }
    }
}
