using NUnit.Framework;
using UnityEngine;
using Expedition33.Combat;

namespace Expedition33.Tests
{
    [TestFixture]
    public class Milestone5Tests
    {
        // -------------------------------------------------------------------------
        // 1. Break System Tests
        // -------------------------------------------------------------------------
        [Test]
        public void BreakMeter_AccumulatesBreak_AndTriggersBrokenAtMax()
        {
            var breakMeter = new BreakMeter(maxBreak: 100, brokenDamageMultiplier: 1.75f);
            Assert.IsFalse(breakMeter.IsBroken);
            Assert.AreEqual(0, breakMeter.CurrentBreak);

            bool broke = breakMeter.AddBreak(60);
            Assert.IsFalse(broke);
            Assert.IsFalse(breakMeter.IsBroken);
            Assert.AreEqual(60, breakMeter.CurrentBreak);

            broke = breakMeter.AddBreak(40);
            Assert.IsTrue(broke);
            Assert.IsTrue(breakMeter.IsBroken);
            Assert.AreEqual(100, breakMeter.CurrentBreak);
            Assert.AreEqual(1.75f, breakMeter.BrokenDamageMultiplier);
        }

        [Test]
        public void BreakMeter_ConsumeBrokenTurn_ClearsBrokenAndResetsPosture()
        {
            var breakMeter = new BreakMeter(maxBreak: 100, brokenDamageMultiplier: 1.75f);
            breakMeter.AddBreak(100);
            Assert.IsTrue(breakMeter.IsBroken);

            breakMeter.ConsumeBrokenTurn();
            Assert.IsFalse(breakMeter.IsBroken);
            Assert.AreEqual(0, breakMeter.CurrentBreak);
            Assert.AreEqual(1.0f, breakMeter.BrokenDamageMultiplier);
        }

        // -------------------------------------------------------------------------
        // 2. Status Effects Tests
        // -------------------------------------------------------------------------
        [Test]
        public void StatusEffectController_Burn_DealsDamageAndExpires()
        {
            var controller = new StatusEffectController();
            var stats = new CombatActorStats("Dummy", maxHealth: 100, attackPower: 10, defense: 0, agility: 10, isPlayer: false);

            controller.ApplyStatus(StatusEffectType.Burn, duration: 2, potency: 15);
            Assert.IsTrue(controller.HasStatus(StatusEffectType.Burn));

            // Turn 1 start
            controller.ProcessTurnStart(stats);
            Assert.AreEqual(85, stats.CurrentHealth);
            Assert.IsTrue(controller.HasStatus(StatusEffectType.Burn));

            // Turn 2 start
            controller.ProcessTurnStart(stats);
            Assert.AreEqual(70, stats.CurrentHealth);
            Assert.IsFalse(controller.HasStatus(StatusEffectType.Burn));
        }

        [Test]
        public void StatusEffectController_Multipliers_ApplyCorrectly()
        {
            var controller = new StatusEffectController();
            controller.ApplyStatus(StatusEffectType.Haste, duration: 2);
            controller.ApplyStatus(StatusEffectType.Weaken, duration: 2);
            controller.ApplyStatus(StatusEffectType.Vulnerable, duration: 2);

            Assert.AreEqual(1.30f, controller.GetSpeedMultiplier(), 0.001f);
            Assert.AreEqual(0.75f, controller.GetAttackMultiplier(), 0.001f);
            Assert.AreEqual(1.25f, controller.GetIncomingDamageMultiplier(), 0.001f);
        }

        // -------------------------------------------------------------------------
        // 3. Overcharge Mechanics Tests
        // -------------------------------------------------------------------------
        [Test]
        public void OverchargeMeter_AccumulatesAndDischargesAllCharges()
        {
            var overcharge = new OverchargeMeter(maxCharges: 3);
            Assert.AreEqual(0, overcharge.CurrentCharges);

            overcharge.AddCharge(2);
            Assert.AreEqual(2, overcharge.CurrentCharges);

            // Cannot exceed 3
            overcharge.AddCharge(5);
            Assert.AreEqual(3, overcharge.CurrentCharges);

            int discharged = overcharge.ConsumeAllCharges();
            Assert.AreEqual(3, discharged);
            Assert.AreEqual(0, overcharge.CurrentCharges);
        }

        // -------------------------------------------------------------------------
        // 4. Stance System Tests
        // -------------------------------------------------------------------------
        [Test]
        public void StanceController_CyclesThroughStances_WithCorrectModifiers()
        {
            var controller = new StanceController(CombatStance.Balanced);
            Assert.AreEqual(CombatStance.Balanced, controller.CurrentStance);
            Assert.AreEqual(1.0f, controller.GetAttackMultiplier());
            Assert.AreEqual(1.0f, controller.GetDefenseMultiplier());
            Assert.AreEqual(1.0f, controller.GetParryWindowMultiplier());
            Assert.AreEqual(0, controller.GetBonusAPGeneration());

            // Cycle -> Offensive
            Assert.AreEqual(CombatStance.Offensive, controller.CycleNextStance());
            Assert.AreEqual(1.30f, controller.GetAttackMultiplier(), 0.001f);
            Assert.AreEqual(0.80f, controller.GetDefenseMultiplier(), 0.001f);
            Assert.AreEqual(0.80f, controller.GetParryWindowMultiplier(), 0.001f);

            // Cycle -> Defensive
            Assert.AreEqual(CombatStance.Defensive, controller.CycleNextStance());
            Assert.AreEqual(0.85f, controller.GetAttackMultiplier(), 0.001f);
            Assert.AreEqual(1.25f, controller.GetDefenseMultiplier(), 0.001f);
            Assert.AreEqual(1.30f, controller.GetParryWindowMultiplier(), 0.001f);

            // Cycle -> Virtuoso
            Assert.AreEqual(CombatStance.Virtuoso, controller.CycleNextStance());
            Assert.AreEqual(1, controller.GetBonusAPGeneration());

            // Cycle -> Balanced
            Assert.AreEqual(CombatStance.Balanced, controller.CycleNextStance());
        }
    }
}
