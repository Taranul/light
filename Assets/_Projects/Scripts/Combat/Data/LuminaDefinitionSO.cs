using UnityEngine;

namespace Expedition33.Combat
{
    /// <summary>
    /// Defines a Lumina passive ability: simple stat or behavior tweaks that a character
    /// can equip within their total Lumina point budget (set on PictoLoadout).
    /// </summary>
    [CreateAssetMenu(menuName = "Expedition33/Lumina Definition", fileName = "Lumina_New")]
    public class LuminaDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _luminaName;
        [SerializeField] [TextArea(2, 4)] private string _description;

        [Header("Cost")]
        [SerializeField] private int _luminaCost = 1;

        [Header("Effect")]
        [SerializeField] private LuminaEffectType _effectType = LuminaEffectType.None;
        [SerializeField] private float _effectValue;

        public string LuminaName => _luminaName;
        public string Description => _description;
        public int LuminaCost => _luminaCost;
        public LuminaEffectType EffectType => _effectType;
        public float EffectValue => _effectValue;

        public static LuminaDefinitionSO Create(
            string name,
            string desc,
            int cost,
            LuminaEffectType effect,
            float value)
        {
            var so = CreateInstance<LuminaDefinitionSO>();
            so._luminaName = name;
            so._description = desc;
            so._luminaCost = cost;
            so._effectType = effect;
            so._effectValue = value;
            so.name = name;
            return so;
        }
    }

    public enum LuminaEffectType
    {
        None,
        BonusAttack,        // +% attack multiplier
        BonusDefense,       // +% defense multiplier
        BonusSpeed,         // +% speed
        BonusMaxHP,         // +% max HP
        APPerTurn,          // Gain N bonus AP per player turn start
        GradientOnHit,      // +N gradient per successful attack
        GradientOnDefense,  // +N gradient per successful parry/dodge
        ReduceBreakThreshold, // Enemy needs less break to stagger
    }
}
