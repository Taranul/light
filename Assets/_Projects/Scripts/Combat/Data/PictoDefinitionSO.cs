using UnityEngine;

namespace Expedition33.Combat
{
    /// <summary>
    /// Defines a passive ability granted to a character when this Picto is equipped.
    /// Designers can create new Pictos as ScriptableObject assets without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "Expedition33/Picto Definition", fileName = "Picto_New")]
    public class PictoDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _pictoName;
        [SerializeField] [TextArea(2, 4)] private string _description;
        [SerializeField] private Color _accentColor = Color.white;

        [Header("Lumina Cost")]
        [SerializeField] private int _luminaCost = 1;

        [Header("Stat Modifiers (additive multipliers; 0 = no change)")]
        [SerializeField] private float _attackBonus;
        [SerializeField] private float _defenseBonus;
        [SerializeField] private float _speedBonus;
        [SerializeField] private float _maxHpBonus;

        [Header("Special Passive")]
        [SerializeField] private PictoPassiveType _passiveType = PictoPassiveType.None;
        [SerializeField] private float _passiveValue;

        public string PictoName => _pictoName;
        public string Description => _description;
        public Color AccentColor => _accentColor;
        public int LuminaCost => _luminaCost;

        public float AttackBonus => _attackBonus;
        public float DefenseBonus => _defenseBonus;
        public float SpeedBonus => _speedBonus;
        public float MaxHpBonus => _maxHpBonus;

        public PictoPassiveType PassiveType => _passiveType;
        public float PassiveValue => _passiveValue;

        public static PictoDefinitionSO Create(
            string name,
            string desc,
            int luminaCost,
            float atkBonus = 0f,
            float defBonus = 0f,
            float spdBonus = 0f,
            float hpBonus = 0f,
            PictoPassiveType passive = PictoPassiveType.None,
            float passiveValue = 0f,
            Color accentColor = default)
        {
            var so = CreateInstance<PictoDefinitionSO>();
            so._pictoName = name;
            so._description = desc;
            so._luminaCost = luminaCost;
            so._attackBonus = atkBonus;
            so._defenseBonus = defBonus;
            so._speedBonus = spdBonus;
            so._maxHpBonus = hpBonus;
            so._passiveType = passive;
            so._passiveValue = passiveValue;
            so._accentColor = accentColor == default ? Color.white : accentColor;
            so.name = name;
            return so;
        }
    }

    public enum PictoPassiveType
    {
        None,
        RegenerateAPOnKill,         // +N AP when enemy is defeated
        LifeSteal,                  // Heal % of damage dealt
        ParryWindowExtension,       // Extends parry window by value seconds
        GradientOnParry,            // Extra gradient charge on successful parry
        CriticalStrikeChance,       // Flat % bonus to crit chance
        ReduceSkillAPCost,          // Reduce all skill costs by N AP
        OverchargeStart,            // Start battle with N overcharge pips
    }
}
