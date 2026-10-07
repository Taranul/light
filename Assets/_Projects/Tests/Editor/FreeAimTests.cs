using NUnit.Framework;
using Expedition33.Combat;

namespace Expedition33.Tests
{
    [TestFixture]
    public class FreeAimTests
    {
        [Test]
        public void FreeAim_WeakPointHit_AppliesCriticalAnd22xMultiplier()
        {
            int baseAttack = 30;
            var (damage, isCrit) = FreeAimDamageCalculator.Calculate(baseAttack, HitboxType.WeakPoint, defenderDefense: 0);

            Assert.IsTrue(isCrit);
            Assert.AreEqual(66, damage); // 30 * 2.2 = 66
        }

        [Test]
        public void FreeAim_BodyHit_AppliesStandardDamage()
        {
            int baseAttack = 30;
            var (damage, isCrit) = FreeAimDamageCalculator.Calculate(baseAttack, HitboxType.Body, defenderDefense: 0);

            Assert.IsFalse(isCrit);
            Assert.AreEqual(30, damage);
        }

        [Test]
        public void FreeAim_ArmoredHit_ReducesDamage()
        {
            int baseAttack = 30;
            var (damage, isCrit) = FreeAimDamageCalculator.Calculate(baseAttack, HitboxType.Armored, defenderDefense: 0);

            Assert.IsFalse(isCrit);
            Assert.AreEqual(15, damage); // 30 * 0.5 = 15
        }

        [Test]
        public void FreeAim_MitigatesWithDefenseProperly()
        {
            int baseAttack = 100;
            // Defense = 100 -> mitigationFactor = 100 / (100 + 100) = 0.5
            var (damage, isCrit) = FreeAimDamageCalculator.Calculate(baseAttack, HitboxType.WeakPoint, defenderDefense: 100);

            Assert.IsTrue(isCrit);
            Assert.AreEqual(110, damage); // (100 * 2.2) * 0.5 = 110
        }
    }
}
