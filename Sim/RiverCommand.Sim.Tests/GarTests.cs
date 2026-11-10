using NUnit.Framework;

namespace RiverCommand.Sim.Tests
{
    public class GarTests
    {
        [TestCase(23, GarBand.Green)]
        [TestCase(24, GarBand.Amber)]
        [TestCase(44, GarBand.Amber)]
        [TestCase(45, GarBand.Red)]
        public void BandEdges(int total, GarBand expected)
        {
            Assert.That(GarAssessment.BandFor(total), Is.EqualTo(expected));
        }

        [Test]
        public void GoodConditionsAreGreen()
        {
            var gar = GarCalculator.Assess(new GarInputs
            {
                PlanningMinutes = 10, LeastExperiencedCrewHours = 400, MaxCrewHoursAwake = 4,
                BarState = BarState.Open, VisibilityNm = 10,
            });
            Assert.That(gar.Band, Is.EqualTo(GarBand.Green));
            Assert.That(gar.Lines.Count, Is.EqualTo(6));
        }

        [Test]
        public void TiredCrewClosedBarFogAndNightIsRed()
        {
            var gar = GarCalculator.Assess(new GarInputs
            {
                SupervisorOnWatch = false, PlanningMinutes = 1, LeastExperiencedCrewHours = 50,
                MaxCrewHoursAwake = 20, BarState = BarState.Closed, VisibilityNm = 0.5, Night = true,
                InvolvesTow = true, UnitsOnScene = 2,
            });
            Assert.That(gar.Band, Is.EqualTo(GarBand.Red));
        }

        [Test]
        public void EveryHighScoreOffersAMitigation()
        {
            var gar = GarCalculator.Assess(new GarInputs
            {
                SupervisorOnWatch = false, PlanningMinutes = 1, LeastExperiencedCrewHours = 50,
                MaxCrewHoursAwake = 20, BarReportStale = true, VisibilityNm = 0.5, InvolvesTow = true,
            });
            foreach (var line in gar.Lines)
                if (line.Score > 4) Assert.That(line.Mitigation, Is.Not.Null.And.Not.Empty, line.Element.ToString());
        }

        [Test]
        public void KnowingTheBarNeverScoresWorseThanNotKnowing()
        {
            var unknown = GarCalculator.Assess(new GarInputs { BarState = null, BarReportStale = true });
            var restricted = GarCalculator.Assess(new GarInputs { BarState = BarState.Restricted });
            Assert.That(restricted.Total, Is.LessThan(unknown.Total));
        }

        [Test]
        public void StaleBarReportRaisesEnvironmentAndSuggestsBarCheck()
        {
            var fresh = GarCalculator.Assess(new GarInputs { BarState = BarState.Open });
            var stale = GarCalculator.Assess(new GarInputs { BarState = BarState.Open, BarReportStale = true });
            Assert.That(stale.Total, Is.GreaterThan(fresh.Total));
            Assert.That(stale.Lines[(int)GarElement.Environment].Mitigation, Is.EqualTo("Order a bar check"));
        }
    }
}
