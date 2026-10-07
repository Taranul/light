using UnityEngine;

namespace Expedition33.Combat
{
    [CreateAssetMenu(fileName = "NewEnemyAttackPattern", menuName = "Expedition33/Combat/Enemy Attack Pattern")]
    public class EnemyAttackPatternSO : ScriptableObject
    {
        [SerializeField] private string _attackName = "Basic Strike";
        [SerializeField] private float _windupDuration = 1.0f;
        [SerializeField] private AttackTelegraphType _telegraphType = AttackTelegraphType.Standard;
        [SerializeField] private HitWindowDefinition[] _strikes;

        public string AttackName => _attackName;
        public float WindupDuration => _windupDuration;
        public AttackTelegraphType TelegraphType => _telegraphType;
        public HitWindowDefinition[] Strikes => _strikes;

        public static EnemyAttackPatternSO CreateDefaultPattern(string name, AttackTelegraphType type, float strikeOffset, int damage)
        {
            var pattern = CreateInstance<EnemyAttackPatternSO>();
            pattern._attackName = name;
            pattern._windupDuration = 0.8f;
            pattern._telegraphType = type;
            pattern._strikes = new[]
            {
                new HitWindowDefinition(strikeOffset, 0.10f, 0.20f, 0.22f, damage, type)
            };
            return pattern;
        }
    }
}
