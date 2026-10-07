using UnityEngine;

namespace Expedition33.Combat
{
    [RequireComponent(typeof(Collider))]
    public class CombatHitbox : MonoBehaviour
    {
        [SerializeField] private HitboxType _hitboxType = HitboxType.Body;
        [SerializeField] private CombatActorView _ownerActorView;

        public HitboxType HitboxType => _hitboxType;
        public CombatActorView OwnerActorView => _ownerActorView;

        public void Initialize(HitboxType type, CombatActorView owner)
        {
            _hitboxType = type;
            _ownerActorView = owner;
        }
    }
}
