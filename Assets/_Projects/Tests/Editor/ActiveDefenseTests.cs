using NUnit.Framework;
using UnityEngine;
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

        [Test]
        public void ActiveDefense_LateDodgeInsidePostHitWindow_ReturnsDodgeSuccess()
        {
            DefenseOutcome outcome = ActiveDefenseEvaluator.Evaluate(
                DefenseActionType.Dodge,
                inputTime: 1.12f,
                strikeTime: 1.0f,
                window: _standardWindow
            );

            Assert.AreEqual(DefenseOutcome.DodgeSuccess, outcome);
        }

        [Test]
        public void InputBuffer_RejectsInputOutsideBufferWindow_AndConsumesWithinWindow()
        {
            var go = new GameObject("TestInputBuffer");
            try
            {
                var buffer = go.AddComponent<InputBuffer>();
                buffer.BufferAction(DefenseActionType.Dodge, 10.0f);

                // Outside default 0.12s buffer window (e.g. 0.50s delta)
                Assert.IsFalse(buffer.TryConsumeInput(10.50f, out _, out _));

                // Within 0.12s buffer window
                Assert.IsTrue(buffer.TryConsumeInput(10.08f, out DefenseActionType action, out float ts));
                Assert.AreEqual(DefenseActionType.Dodge, action);
                Assert.AreEqual(10.0f, ts, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CombatActorView_AnimateApproach_MaintainsSafeSeparationFromTarget()
        {
            var go = new GameObject("TestActor");
            try
            {
                go.transform.position = new Vector3(-2.2f, 0f, 0f);
                go.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                var view = go.AddComponent<CombatActorView>();

                Vector3 enemyPos = new Vector3(2.2f, 0f, 0f);
                bool arrived = false;
                view.AnimateApproach(enemyPos, 0f, () => arrived = true);

                Assert.IsTrue(arrived);
                float dist = Vector3.Distance(go.transform.position, enemyPos);
                Assert.GreaterOrEqual(dist, 1.60f);
                Assert.Less(go.transform.position.x, enemyPos.x);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
