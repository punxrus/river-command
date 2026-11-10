using NUnit.Framework;

namespace RiverCommand.Sim.Tests
{
    public class EnvironmentTests
    {
        private static TideModel Tide() => new TideModel(1.2, 1.0, 12.42, 0, 3.0);

        [Test]
        public void HighWaterAtStartAndAfterOnePeriod()
        {
            var tide = Tide();
            Assert.That(tide.HeightM(0), Is.EqualTo(2.2).Within(1e-9));
            Assert.That(tide.HeightM(12.42 * 3600), Is.EqualTo(2.2).Within(1e-9));
            Assert.That(tide.HeightM(6.21 * 3600), Is.EqualTo(0.2).Within(1e-9));
        }

        [Test]
        public void CurrentEbbsAfterHighWaterAndFloodsAfterLowWater()
        {
            var tide = Tide();
            Assert.That(tide.IsEbb(3 * 3600), Is.True, "falling tide = ebb");
            Assert.That(tide.IsEbb(9 * 3600), Is.False, "rising tide = flood");
            Assert.That(Conv.MpsToKnots(tide.CurrentMps(12.42 / 4 * 3600)), Is.EqualTo(-3.0).Within(1e-6), "peak ebb mid-fall");
        }

        [Test]
        public void EbbSteepensBreakersButFloodDoesNotFlattenThem()
        {
            var rules = new BarRules();
            Assert.That(BarConditions.BreakerHeightFt(5, 0, rules), Is.EqualTo(5));
            Assert.That(BarConditions.BreakerHeightFt(5, -3, rules), Is.EqualTo(5 * 1.45).Within(1e-9));
            Assert.That(BarConditions.BreakerHeightFt(5, 3, rules), Is.EqualTo(5));
        }

        [TestCase(3.99, BarState.Open)]
        [TestCase(4.0, BarState.Restricted)]
        [TestCase(7.99, BarState.Restricted)]
        [TestCase(8.0, BarState.Closed)]
        public void BarStateThresholds(double breakerFt, BarState expected)
        {
            Assert.That(BarConditions.Classify(breakerFt, new BarRules()), Is.EqualTo(expected));
        }

        [Test]
        public void SameSwellCanBeOpenOnFloodAndRestrictedOnEbb()
        {
            var tide = Tide();
            var rules = new BarRules();
            var flood = BarConditions.Evaluate(3.5, tide, 9 * 3600, rules);
            var ebb = BarConditions.Evaluate(3.5, tide, 3 * 3600, rules);
            Assert.That(flood.State, Is.EqualTo(BarState.Open));
            Assert.That(ebb.State, Is.EqualTo(BarState.Restricted));
        }

        [Test]
        public void BarReportGoesStale()
        {
            var env = new SeaEnvironment(Tide(), new BarRules(), 3);
            Assert.That(env.IsBarReportStale(0), Is.True, "no report yet");
            env.DoBarCheck(0);
            Assert.That(env.IsBarReportStale(3600), Is.False);
            Assert.That(env.IsBarReportStale(5 * 3600), Is.True);
        }
    }
}
