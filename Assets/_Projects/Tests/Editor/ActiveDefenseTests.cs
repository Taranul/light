using NUnit.Framework;
using Expedition33.Combat;

namespace Expedition33.Tests
{
    [TestFixture]
    public class ActiveDefenseTests
    {
        private HitWindowDefinition _standardWindow;
        private HitWindowDefinition _sweepWindow;
        private HitWindowDefinition _unblockableWindow;

        [SetUp]
        public void Setup()
        {
            _standardWindow = new HitWindowDefinition(
                hitTimeOffset: 1.0f,
                parryHalfWindow: 0.08f,
                dodgeHalfWindow: 0.16f,
                jumpHalfWindow: 0.18f,
                damage: 20,
                telegraphType: AttackTelegraphType.Standard
            );

            _sweepWindow = new HitWindowDefinition(
                hitTimeOffset: 1.0f,
                parryHalfWindow: 0.08f,
                dodgeHalfWindow: 0.16f,
                jumpHalfWindow: 0.18f,
                damage: 25,
                telegraphType: AttackTelegraphType.GroundSweep
            );

            _unblockableWindow = new HitWindowDefinition(
                hitTimeOffset: 1.0f,
                parryHalfWindow: 0.08f,
                dodgeHalfWindow: 0.16f,
                jumpHalfWindow: 0.18f,
                damage: 30,
                telegraphType: AttackTelegraphType.HeavyUnblockable
            );
        }

        [Test]
        public void ActiveDefense_ParryInsideWindow_ReturnsParrySuccess()
        {
            // Input exactly at 0.96s (hit is 1.0s, diff 0.04s <= 0.08s)
            DefenseOutcome outcome = ActiveDefenseEvaluator.Evaluate(
                DefenseActionType.Parry,
                inputTime: 0.96f,
                strikeTime: 1.0f,
                window: _standardWindow
            );

            Assert.AreEqual(DefenseOutcome.ParrySuccess, outcome);
        }

        [Test]
        public void ActiveDefense_ParryOutsideWindow_ReturnsMiss()
        {
            // Input at 0.85s (diff 0.15s > 0.08s)
            DefenseOutcome outcome = ActiveDefenseEvaluator.Evaluate(
                DefenseActionType.Parry,
                inputTime: 0.85f,
                strikeTime: 1.0f,
                window: _standardWindow
            );

            Assert.AreEqual(DefenseOutcome.Miss, outcome);
        }

        [Test]
        public void ActiveDefense_DodgeInsideWindow_ReturnsDodgeSuccess()
        {
            // Input at 0.90s (diff 0.10s <= 0.16s)
            DefenseOutcome outcome = ActiveDefenseEvaluator.Evaluate(
                DefenseActionType.Dodge,
                inputTime: 0.90f,
                strikeTime: 1.0f,
                window: _standardWindow
            );

            Assert.AreEqual(DefenseOutcome.DodgeSuccess, outcome);
        }

        [Test]
        public void ActiveDefense_JumpOverGroundSweep_ReturnsJumpSuccess()
        {
            // Input at 0.95s on GroundSweep
            DefenseOutcome outcome = ActiveDefenseEvaluator.Evaluate(
                DefenseActionType.Jump,
                inputTime: 0.95f,
                strikeTime: 1.0f,
                window: _sweepWindow
            );

            Assert.AreEqual(DefenseOutcome.JumpSuccess, outcome);
        }

        [Test]
        public void ActiveDefense_DodgeAgainstGroundSweep_ReturnsInvalidAction()
        {
            // Dodge cannot evade ground sweep
            DefenseOutcome outcome = ActiveDefenseEvaluator.Evaluate(
                DefenseActionType.Dodge,
                inputTime: 1.0f,
                strikeTime: 1.0f,
                window: _sweepWindow
            );

            Assert.AreEqual(DefenseOutcome.InvalidAction, outcome);
        }

        [Test]
        public void ActiveDefense_ParryAgainstUnblockable_ReturnsInvalidAction()
        {
            // Parry cannot deflect unblockable attack
            DefenseOutcome outcome = ActiveDefenseEvaluator.Evaluate(
                DefenseActionType.Parry,
                inputTime: 1.0f,
                strikeTime: 1.0f,
                window: _unblockableWindow
            );

            Assert.AreEqual(DefenseOutcome.InvalidAction, outcome);
        }
    }
}
