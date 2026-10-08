using System.Collections.Generic;
using UnityEngine;

namespace Expedition33.Combat
{
    /// <summary>
    /// Runtime loadout: a set of equipped Pictos and Luminas for one character, tracked
    /// against a total Lumina point budget. Aggregates all stat/passive bonuses for query.
    /// </summary>
    public class PictoLoadout
    {
        private readonly List<PictoDefinitionSO> _pictos = new();
        private readonly List<LuminaDefinitionSO> _luminas = new();
        private readonly int _maxLuminaPoints;

        public IReadOnlyList<PictoDefinitionSO> Pictos => _pictos;
        public IReadOnlyList<LuminaDefinitionSO> Luminas => _luminas;
        public int MaxLuminaPoints => _maxLuminaPoints;

        public int UsedLuminaPoints
        {
            get
            {
                int total = 0;
                foreach (var p in _pictos) total += p.LuminaCost;
                foreach (var l in _luminas) total += l.LuminaCost;
                return total;
            }
        }

        public int RemainingLuminaPoints => _maxLuminaPoints - UsedLuminaPoints;

        public PictoLoadout(int maxLuminaPoints = 10)
        {
            _maxLuminaPoints = maxLuminaPoints;
        }

        /// <summary>Equips a Picto if there's enough Lumina budget. Returns true on success.</summary>
        public bool TryEquipPicto(PictoDefinitionSO picto)
        {
            if (picto == null) return false;
            if (_pictos.Contains(picto)) return false;
            if (RemainingLuminaPoints < picto.LuminaCost) return false;
            _pictos.Add(picto);
            return true;
        }

        /// <summary>Equips a Lumina if there's enough budget. Returns true on success.</summary>
        public bool TryEquipLumina(LuminaDefinitionSO lumina)
        {
            if (lumina == null) return false;
            if (_luminas.Contains(lumina)) return false;
            if (RemainingLuminaPoints < lumina.LuminaCost) return false;
            _luminas.Add(lumina);
            return true;
        }

        // ── Aggregated stat queries ──────────────────────────────────────────────

        /// <summary>Combined attack multiplier bonus from all Pictos and Luminas (additive).</summary>
        public float GetTotalAttackBonus()
        {
            float bonus = 0f;
            foreach (var p in _pictos) bonus += p.AttackBonus;
            foreach (var l in _luminas)
                if (l.EffectType == LuminaEffectType.BonusAttack) bonus += l.EffectValue;
            return bonus;
        }

        /// <summary>Combined defense multiplier bonus.</summary>
        public float GetTotalDefenseBonus()
        {
            float bonus = 0f;
            foreach (var p in _pictos) bonus += p.DefenseBonus;
            foreach (var l in _luminas)
                if (l.EffectType == LuminaEffectType.BonusDefense) bonus += l.EffectValue;
            return bonus;
        }

        /// <summary>Bonus gradient charge per successful hit.</summary>
        public int GetGradientOnHitBonus()
        {
            int bonus = 0;
            foreach (var l in _luminas)
                if (l.EffectType == LuminaEffectType.GradientOnHit) bonus += Mathf.RoundToInt(l.EffectValue);
            return bonus;
        }

        /// <summary>Bonus gradient charge per successful parry/dodge.</summary>
        public int GetGradientOnDefenseBonus()
        {
            int bonus = 0;
            foreach (var l in _luminas)
                if (l.EffectType == LuminaEffectType.GradientOnDefense) bonus += Mathf.RoundToInt(l.EffectValue);
            foreach (var p in _pictos)
                if (p.PassiveType == PictoPassiveType.GradientOnParry) bonus += Mathf.RoundToInt(p.PassiveValue);
            return bonus;
        }

        /// <summary>Bonus AP to generate at the start of each player turn.</summary>
        public int GetBonusAPPerTurn()
        {
            int bonus = 0;
            foreach (var l in _luminas)
                if (l.EffectType == LuminaEffectType.APPerTurn) bonus += Mathf.RoundToInt(l.EffectValue);
            return bonus;
        }

        /// <summary>Parry window extension in seconds from Picto passives.</summary>
        public float GetParryWindowExtension()
        {
            float ext = 0f;
            foreach (var p in _pictos)
                if (p.PassiveType == PictoPassiveType.ParryWindowExtension) ext += p.PassiveValue;
            return ext;
        }

        /// <summary>Starting Overcharge pips from Picto passives.</summary>
        public int GetStartingOvercharge()
        {
            int bonus = 0;
            foreach (var p in _pictos)
                if (p.PassiveType == PictoPassiveType.OverchargeStart) bonus += Mathf.RoundToInt(p.PassiveValue);
            return bonus;
        }
    }
}
