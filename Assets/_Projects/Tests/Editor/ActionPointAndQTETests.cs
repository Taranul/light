using NUnit.Framework;
using Expedition33.Combat;

namespace Expedition33.Tests
{
    [TestFixture]
    public class ActionPointAndQTETests
    {
        [Test]
        public void ActionPointPool_InitialState_MatchesParameters()
        {
            var pool = new ActionPointPool(maxAP: 6, startingAP: 3);
            Assert.AreEqual(6, pool.MaxAP);
            Assert.AreEqual(3, pool.CurrentAP);
        }

        [Test]
        public void ActionPointPool_TrySpend_DeductsWhenAffordable()
        {
            var pool = new ActionPointPool(maxAP: 6, startingAP: 4);
            bool eventFired = false;
            pool.OnAPChanged += (cur, max) => eventFired = true;

            bool success = pool.TrySpend(3);

            Assert.IsTrue(success);
            Assert.AreEqual(1, pool.CurrentAP);
            Assert.IsTrue(eventFired);
        }

        [Test]
        public void ActionPointPool_TrySpend_FailsWhenInsufficient()
        {
            var pool = new ActionPointPool(maxAP: 6, startingAP: 2);
            bool success = pool.TrySpend(3);

            Assert.IsFalse(success);
            Assert.AreEqual(2, pool.CurrentAP);
        }

        [Test]
        public void ActionPointPool_Generate_ClampsToMaxAP()
        {
            var pool = new ActionPointPool(maxAP: 6, startingAP: 5);
            pool.Generate(4);

            Assert.AreEqual(6, pool.CurrentAP);
        }

        [Test]
        public void QTEEvaluator_InputInSweetSpot_ReturnsPerfect()
        {
            var qteDef = new QTEWindowDefinition(
                duration: 0.85f,
                sweetSpotTime: 0.60f,
                perfectHalfWindow: 0.08f,
                goodHalfWindow: 0.16f
            );

            // Input at 0.62s (diff = 0.02 <= 0.08)
            QTEOutcome outcome = QTEEvaluator.Evaluate(0.62f, qteDef);
            Assert.AreEqual(QTEOutcome.Perfect, outcome);
        }

        [Test]
        public void QTEEvaluator_InputInGoodWindow_ReturnsGood()
        {
            var qteDef = new QTEWindowDefinition(
                duration: 0.85f,
                sweetSpotTime: 0.60f,
                perfectHalfWindow: 0.08f,
                goodHalfWindow: 0.16f
            );

            // Input at 0.72s (diff = 0.12 <= 0.16, but > 0.08)
            QTEOutcome outcome = QTEEvaluator.Evaluate(0.72f, qteDef);
            Assert.AreEqual(QTEOutcome.Good, outcome);
        }

        [Test]
        public void QTEEvaluator_InputOutside_ReturnsMiss()
        {
            var qteDef = new QTEWindowDefinition(
                duration: 0.85f,
                sweetSpotTime: 0.60f,
                perfectHalfWindow: 0.08f,
                goodHalfWindow: 0.16f
            );

            // Input at 0.35s (diff = 0.25 > 0.16)
            QTEOutcome outcome = QTEEvaluator.Evaluate(0.35f, qteDef);
            Assert.AreEqual(QTEOutcome.Miss, outcome);
        }
    }
}
