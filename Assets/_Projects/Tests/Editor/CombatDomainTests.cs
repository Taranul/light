using NUnit.Framework;
using Expedition33.Combat;
using System.Collections.Generic;

namespace Expedition33.Tests
{
    [TestFixture]
    public class CombatDomainTests
    {
        [Test]
        public void CombatActorStats_ApplyDamage_ReducesHealthProperly()
        {
            var stats = new CombatActorStats("Tester", 100, 20, 5, 10, true);
            stats.ApplyDamage(30);

            Assert.AreEqual(70, stats.CurrentHealth);
            Assert.IsFalse(stats.IsDefeated);
        }

        [Test]
        public void CombatActorStats_FatalDamage_TriggersDefeated()
        {
            var stats = new CombatActorStats("Tester", 100, 20, 5, 10, true);
            bool defeatedTriggered = false;
            stats.OnDefeated += () => defeatedTriggered = true;

            stats.ApplyDamage(150);

            Assert.AreEqual(0, stats.CurrentHealth);
            Assert.IsTrue(stats.IsDefeated);
            Assert.IsTrue(defeatedTriggered);
        }

        [Test]
        public void DamageCalculator_MitigatesDamageWithDefense()
        {
            var attacker = new CombatActorStats("Attacker", 100, 50, 0, 10, true);
            var zeroDefDefender = new CombatActorStats("Defender1", 100, 10, 0, 10, false);
            var highDefDefender = new CombatActorStats("Defender2", 100, 10, 100, 10, false);

            DamageResult unmitigated = DamageCalculator.CalculateDamage(attacker, zeroDefDefender);
            DamageResult mitigated = DamageCalculator.CalculateDamage(attacker, highDefDefender);

            Assert.AreEqual(50, unmitigated.MitigatedDamage);
            Assert.AreEqual(25, mitigated.MitigatedDamage);
        }

        [Test]
        public void DamageCalculator_CriticalHit_MultipliesDamage()
        {
            var attacker = new CombatActorStats("Attacker", 100, 40, 0, 10, true);
            var defender = new CombatActorStats("Defender", 100, 10, 0, 10, false);

            DamageResult normal = DamageCalculator.CalculateDamage(attacker, defender, 1f, isCritical: false);
            DamageResult crit = DamageCalculator.CalculateDamage(attacker, defender, 1f, isCritical: true);

            Assert.AreEqual(40, normal.MitigatedDamage);
            Assert.AreEqual(60, crit.MitigatedDamage);
        }

        [Test]
        public void TurnTimeline_HighAgilityActorTakesMoreTurns()
        {
            var fast = new CombatActorStats("Fast", 100, 10, 5, 30, true);
            var slow = new CombatActorStats("Slow", 100, 10, 5, 10, false);

            var timeline = new TurnTimeline();
            timeline.RegisterActor(fast);
            timeline.RegisterActor(slow);

            List<CombatActorStats> upcoming = timeline.PreviewUpcomingTurns(4);

            int fastCount = upcoming.FindAll(a => a == fast).Count;
            int slowCount = upcoming.FindAll(a => a == slow).Count;

            Assert.Greater(fastCount, slowCount);
        }
    }
}
