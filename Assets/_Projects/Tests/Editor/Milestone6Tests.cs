using NUnit.Framework;
using UnityEngine;
using Expedition33.Combat;

namespace Expedition33.Tests
{
    public class Milestone6Tests
    {
        [Test]
        public void GradientMeter_StartsAtZero_AccumulatesAndClamps()
        {
            var meter = new GradientMeter(maxCharge: 100);
            Assert.AreEqual(0, meter.CurrentCharge);
            Assert.IsFalse(meter.IsReady);

            bool becameReadyOnFirstAdd = meter.AddCharge(40);
            Assert.AreEqual(40, meter.CurrentCharge);
            Assert.IsFalse(meter.IsReady);
            Assert.IsFalse(becameReadyOnFirstAdd);

            bool becameReadyOnSecondAdd = meter.AddCharge(70);
            Assert.AreEqual(100, meter.CurrentCharge);
            Assert.IsTrue(meter.IsReady);
            Assert.IsTrue(becameReadyOnSecondAdd);

            // Adding more should remain clamped at max
            meter.AddCharge(25);
            Assert.AreEqual(100, meter.CurrentCharge);
        }

        [Test]
        public void GradientMeter_ConsumeForSuperAttack_OnlyWhenReady()
        {
            var meter = new GradientMeter(maxCharge: 100);
            meter.AddCharge(50);
            Assert.IsFalse(meter.ConsumeForSuperAttack());
            Assert.AreEqual(50, meter.CurrentCharge);

            meter.AddCharge(50);
            Assert.IsTrue(meter.IsReady);
            Assert.IsTrue(meter.ConsumeForSuperAttack());
            Assert.AreEqual(0, meter.CurrentCharge);
            Assert.IsFalse(meter.IsReady);
        }

        [Test]
        public void PictoLoadout_EnforcesLuminaBudget()
        {
            var loadout = new PictoLoadout(maxLuminaPoints: 5);
            var p1 = PictoDefinitionSO.Create("Picto A", "Desc", luminaCost: 2);
            var p2 = PictoDefinitionSO.Create("Picto B", "Desc", luminaCost: 3);
            var p3 = PictoDefinitionSO.Create("Picto C", "Desc", luminaCost: 1);

            Assert.IsTrue(loadout.TryEquipPicto(p1));
            Assert.AreEqual(2, loadout.UsedLuminaPoints);
            Assert.AreEqual(3, loadout.RemainingLuminaPoints);

            // Duplicate cannot be equipped
            Assert.IsFalse(loadout.TryEquipPicto(p1));

            // Equip p2 (cost 3) -> 5 used, 0 remaining
            Assert.IsTrue(loadout.TryEquipPicto(p2));
            Assert.AreEqual(5, loadout.UsedLuminaPoints);
            Assert.AreEqual(0, loadout.RemainingLuminaPoints);

            // p3 exceeds budget
            Assert.IsFalse(loadout.TryEquipPicto(p3));
            Assert.AreEqual(2, loadout.Pictos.Count);
        }

        [Test]
        public void PictoLoadout_AggregatesAttackAndDefenseBonuses()
        {
            var loadout = new PictoLoadout(maxLuminaPoints: 10);
            var picto = PictoDefinitionSO.Create(
                "Striker Picto",
                "Desc",
                luminaCost: 2,
                atkBonus: 0.15f,
                defBonus: 0.10f
            );
            var luminaAtk = LuminaDefinitionSO.Create(
                "Lumina Strength",
                "Desc",
                cost: 1,
                effect: LuminaEffectType.BonusAttack,
                value: 0.08f
            );
            var luminaDef = LuminaDefinitionSO.Create(
                "Lumina Guard",
                "Desc",
                cost: 1,
                effect: LuminaEffectType.BonusDefense,
                value: 0.05f
            );

            loadout.TryEquipPicto(picto);
            loadout.TryEquipLumina(luminaAtk);
            loadout.TryEquipLumina(luminaDef);

            Assert.AreEqual(0.23f, loadout.GetTotalAttackBonus(), 0.001f);
            Assert.AreEqual(0.15f, loadout.GetTotalDefenseBonus(), 0.001f);
        }

        [Test]
        public void PictoLoadout_AggregatesGradientAndPassives()
        {
            var loadout = new PictoLoadout(maxLuminaPoints: 10);
            var picto = PictoDefinitionSO.Create(
                "Lumiere Cadence",
                "Desc",
                luminaCost: 3,
                passive: PictoPassiveType.GradientOnParry,
                passiveValue: 15f
            );
            var pictoParry = PictoDefinitionSO.Create(
                "Reflex Picto",
                "Desc",
                luminaCost: 2,
                passive: PictoPassiveType.ParryWindowExtension,
                passiveValue: 0.05f
            );
            var luminaHit = LuminaDefinitionSO.Create(
                "Lumina Hit",
                "Desc",
                cost: 1,
                effect: LuminaEffectType.GradientOnHit,
                value: 6f
            );
            var luminaFlow = LuminaDefinitionSO.Create(
                "Lumina Flow",
                "Desc",
                cost: 2,
                effect: LuminaEffectType.APPerTurn,
                value: 1f
            );

            loadout.TryEquipPicto(picto);
            loadout.TryEquipPicto(pictoParry);
            loadout.TryEquipLumina(luminaHit);
            loadout.TryEquipLumina(luminaFlow);

            Assert.AreEqual(6, loadout.GetGradientOnHitBonus());
            Assert.AreEqual(15, loadout.GetGradientOnDefenseBonus());
            Assert.AreEqual(1, loadout.GetBonusAPPerTurn());
            Assert.AreEqual(0.05f, loadout.GetParryWindowExtension(), 0.001f);
        }

        [Test]
        public void PictoLoadout_StartingOverchargePassive()
        {
            var loadout = new PictoLoadout(maxLuminaPoints: 5);
            var picto = PictoDefinitionSO.Create(
                "Charged Picto",
                "Desc",
                luminaCost: 2,
                passive: PictoPassiveType.OverchargeStart,
                passiveValue: 2f
            );
            loadout.TryEquipPicto(picto);

            Assert.AreEqual(2, loadout.GetStartingOvercharge());
        }

        [Test]
        public void LuminaDefinitionSO_ValuesMatchConstruction()
        {
            var lumina = LuminaDefinitionSO.Create(
                "Lumina of Flow",
                "Generates AP",
                cost: 2,
                effect: LuminaEffectType.APPerTurn,
                value: 1.5f
            );

            Assert.AreEqual("Lumina of Flow", lumina.LuminaName);
            Assert.AreEqual("Generates AP", lumina.Description);
            Assert.AreEqual(2, lumina.LuminaCost);
            Assert.AreEqual(LuminaEffectType.APPerTurn, lumina.EffectType);
            Assert.AreEqual(1.5f, lumina.EffectValue, 0.001f);
        }
    }
}
