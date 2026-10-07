using System;
using UnityEngine;

namespace Expedition33.Combat
{
    public enum HitboxType
    {
        Body,
        WeakPoint,
        Armored
    }

    public readonly struct FreeAimShotResult
    {
        public bool DidHit { get; }
        public HitboxType HitboxType { get; }
        public int Damage { get; }
        public bool IsCritical { get; }
        public Vector3 HitPoint { get; }

        public FreeAimShotResult(bool didHit, HitboxType hitboxType, int damage, bool isCritical, Vector3 hitPoint)
        {
            DidHit = didHit;
            HitboxType = hitboxType;
            Damage = damage;
            IsCritical = isCritical;
            HitPoint = hitPoint;
        }
    }

    public static class FreeAimDamageCalculator
    {
        public static (int damage, bool isCritical) Calculate(int baseAttack, HitboxType hitboxType, int defenderDefense = 0)
        {
            float multiplier;
            bool isCrit;

            switch (hitboxType)
            {
                case HitboxType.WeakPoint:
                    multiplier = 2.2f;
                    isCrit = true;
                    break;
                case HitboxType.Armored:
                    multiplier = 0.5f;
                    isCrit = false;
                    break;
                default:
                    multiplier = 1.0f;
                    isCrit = false;
                    break;
            }

            float baseDamage = baseAttack * multiplier;
            float mitigation = 100f / (100f + Mathf.Max(0, defenderDefense));
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * mitigation));

            return (finalDamage, isCrit);
        }
    }
}
