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

        public string SkillName => _skillName;
        public int APCost => _apCost;
        public float DamageMultiplier => _damageMultiplier;
        public string Description => _description;
        public QTEWindowDefinition QTEConfig => _qteConfig;
        public int HitCount => _hitCount;

        public static SkillDefinitionSO CreateSkill(string name, int apCost, float multiplier, string desc)
        {
            var skill = CreateInstance<SkillDefinitionSO>();
            skill._skillName = name;
            skill._apCost = apCost;
            skill._damageMultiplier = multiplier;
            skill._description = desc;
            skill._qteConfig = new QTEWindowDefinition(0.85f, 0.58f, 0.08f, 0.16f);
            return skill;
        }
    }
}
