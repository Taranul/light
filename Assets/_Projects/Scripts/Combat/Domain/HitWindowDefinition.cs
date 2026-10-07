using System;
using UnityEngine;

namespace Expedition33.Combat
{
    [Serializable]
    public class HitWindowDefinition
    {
        [SerializeField] private float _hitTimeOffset = 1.0f;
        [SerializeField] private float _parryHalfWindow = 0.08f;
        [SerializeField] private float _dodgeHalfWindow = 0.16f;
        [SerializeField] private float _jumpHalfWindow = 0.18f;
        [SerializeField] private int _damage = 20;
        [SerializeField] private AttackTelegraphType _telegraphType = AttackTelegraphType.Standard;

        public float HitTimeOffset => _hitTimeOffset;
        public float ParryHalfWindow => _parryHalfWindow;
        public float DodgeHalfWindow => _dodgeHalfWindow;
        public float JumpHalfWindow => _jumpHalfWindow;
        public int Damage => _damage;
        public AttackTelegraphType TelegraphType => _telegraphType;

        public HitWindowDefinition()
        {
        }

        public HitWindowDefinition(
            float hitTimeOffset,
            float parryHalfWindow,
            float dodgeHalfWindow,
            float jumpHalfWindow,
            int damage,
            AttackTelegraphType telegraphType)
        {
            _hitTimeOffset = hitTimeOffset;
            _parryHalfWindow = parryHalfWindow;
            _dodgeHalfWindow = dodgeHalfWindow;
            _jumpHalfWindow = jumpHalfWindow;
            _damage = damage;
            _telegraphType = telegraphType;
        }
    }
}
