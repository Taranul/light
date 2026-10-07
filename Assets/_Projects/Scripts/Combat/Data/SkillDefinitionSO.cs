using UnityEngine;

namespace Expedition33.Combat
{
    [CreateAssetMenu(fileName = "NewSkillDefinition", menuName = "Expedition33/Combat/Skill Definition")]
    public class SkillDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _skillName = "Heavy Strike";
        [SerializeField] private int _apCost = 2;
        [SerializeField] private float _damageMultiplier = 1.5f;
        [SerializeField] private string _description = "Strikes with concentrated force.";
        [SerializeField] private QTEWindowDefinition _qteConfig = new QTEWindowDefinition(0.85f, 0.60f, 0.08f, 0.16f);
        [SerializeField] private int _hitCount = 1;
        [SerializeField] private int _breakDamage = 25;
        [SerializeField] private StatusEffectType _inflictedStatus = StatusEffectType.None;
        [SerializeField] private int _statusDuration = 0;
        [SerializeField] private bool _consumesOvercharge = false;

        public string SkillName => _skillName;
        public int APCost => _apCost;
        public float DamageMultiplier => _damageMultiplier;
        public string Description => _description;
        public QTEWindowDefinition QTEConfig => _qteConfig;
        public int HitCount => _hitCount;
        public int BreakDamage => _breakDamage;
        public StatusEffectType InflictedStatus => _inflictedStatus;
        public int StatusDuration => _statusDuration;
        public bool ConsumesOvercharge => _consumesOvercharge;

        public static SkillDefinitionSO CreateSkill(
            string name,
            int apCost,
            float multiplier,
            string desc,
            int breakDamage = 25,
            StatusEffectType status = StatusEffectType.None,
            int statusDuration = 0,
            bool consumesOvercharge = false)
        {
            var skill = CreateInstance<SkillDefinitionSO>();
            skill._skillName = name;
            skill._apCost = apCost;
            skill._damageMultiplier = multiplier;
            skill._description = desc;
            skill._qteConfig = new QTEWindowDefinition(0.85f, 0.58f, 0.08f, 0.16f);
            skill._breakDamage = breakDamage;
            skill._inflictedStatus = status;
            skill._statusDuration = statusDuration;
            skill._consumesOvercharge = consumesOvercharge;
            return skill;
        }
    }
}
