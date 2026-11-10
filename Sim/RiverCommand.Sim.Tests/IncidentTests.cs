using System.Linq;
using NUnit.Framework;

namespace RiverCommand.Sim.Tests
{
    public class IncidentTests
    {
        private static Vec2 Near(SimWorld w, double dx, double dy) => w.Map.StationPosition + new Vec2(dx, dy);

        /// <summary>Plays the case like a player: an expanding square on the drifting datum.</summary>
        private static void SearchThenRespond(SimWorld w, Unit boat, Incident inc, double maxSeconds)
        {
            boat.Orders.Issue(new SearchOrder(inc.Id, SearchPatterns.ExpandingSquare(inc.Grid.Center, 200, 24)), false);
            for (double t = 0; t < maxSeconds && inc.IsOpen; t += 10) w.StepSeconds(10);
        }

        [Test]
        public void SearchPatternDriftsWithTheWater()
        {
            var w = TestContent.NewWorld();
            var inc = w.Incidents.Spawn("capsized_river_boater", Near(w, 2000, 0));
            var boat = w.SpawnUnit("rbs29");
            var order = new SearchOrder(inc.Id, SearchPatterns.ExpandingSquare(inc.Grid.Center, 200, 24));
            boat.Orders.Issue(order, false);
            w.Step();
            var first = order.Waypoint.Value;
            w.StepSeconds(600);
            if (inc.State == IncidentState.Reported)
                Assert.That(inc.Grid.Center.X, Is.LessThan(first.X + 1), "grid drifted west with the river");
            Assert.That(order.NextLeg, Is.GreaterThan(0));
        }

        [Test]
        public void TemplatesLoad()
        {
            Assert.That(TestContent.Db.IncidentList.Select(d => d.Id),
                Is.EquivalentTo(new[] { "capsized_river_boater", "fishing_vessel_taking_water", "hoax_flare", "swimmer_on_rocks" }));
        }

        [Test]
        public void UnansweredCapsizeEndsWithAllLost()
        {
            var w = TestContent.NewWorld();
            var inc = w.Incidents.Spawn("capsized_river_boater", Near(w, 2000, 0));
            Assert.That(inc.Persons.All(p => p.State == PersonState.InWater));
            w.StepSeconds(8 * 3600);
            Assert.That(inc.State, Is.EqualTo(IncidentState.Failed));
            Assert.That(w.Incidents.Stats.LivesLost, Is.EqualTo(inc.Persons.Count));
        }

        [Test]
        public void QuickSearchUsuallySavesEveryone()
        {
            // Detection is a roll, so check the rate over many seeds, not one lucky run.
            int saved = 0, runs = 60;
            for (ulong seed = 1; seed <= (ulong)runs; seed++)
            {
                var w = TestContent.NewWorld(seed);
                var inc = w.Incidents.Spawn("capsized_river_boater", Near(w, 1500, 300));
                SearchThenRespond(w, w.SpawnUnit("rbs29"), inc, 45 * 60);
                if (inc.State == IncidentState.Resolved && inc.Lost == 0) saved++;
            }
            Assert.That(saved, Is.GreaterThanOrEqualTo(runs * 0.85));
        }

        [Test]
        public void SurfRescueNeedsTheLifeboat()
        {
            int warned = 0, resolvedByMlb = 0, runs = 30;
            for (ulong seed = 1; seed <= (ulong)runs; seed++)
            {
                var w = TestContent.NewWorld(seed);
                var inc = w.Incidents.Spawn("swimmer_on_rocks", Near(w, 0, 1500));
                if (inc.IsHoax) { runs--; continue; }
                var rbs = w.SpawnUnit("rbs29");
                SearchThenRespond(w, rbs, inc, 20 * 60);
                Assert.That(inc.State, Is.Not.EqualTo(IncidentState.Resolved), "the RB-S can never finish a surf case");
                if (w.DrainEvents().OfType<IncidentWrongAssetEvent>().Count() == 1) warned++;

                var mlb = w.SpawnUnit("mlb47");
                SearchThenRespond(w, mlb, inc, 40 * 60);
                if (inc.State == IncidentState.Resolved) resolvedByMlb++;
            }
            Assert.That(warned, Is.GreaterThanOrEqualTo(runs * 0.8), "RB-S finds the swimmer and is told it can't do surf work");
            Assert.That(resolvedByMlb, Is.GreaterThanOrEqualTo(runs * 0.8));
        }

        [Test]
        public void RisingTidePutsPeopleInTheWaterIfNobodyComes()
        {
            var w = TestContent.NewWorld();
            var inc = w.Incidents.Spawn("swimmer_on_rocks", Near(w, 0, 1500));
            Assert.That(inc.Persons.All(p => p.State == PersonState.Safe));
            w.StepSeconds(125 * 60);
            Assert.That(w.DrainEvents().OfType<PersonsInWaterEvent>().Count(), Is.EqualTo(1));
            Assert.That(inc.Persons.All(p => p.State != PersonState.Safe));
        }

        [Test]
        public void TowCapableBoatOnSceneSavesTheVessel()
        {
            var w = TestContent.NewWorld();
            var inc = w.Incidents.Spawn("fishing_vessel_taking_water", Near(w, 0, -1500));
            var mlb = w.SpawnUnit("mlb47");
            SearchThenRespond(w, mlb, inc, 90 * 60);
            Assert.That(inc.VesselSaved, Is.True);
            Assert.That(inc.State, Is.EqualTo(IncidentState.Resolved));
            Assert.That(inc.Recovered, Is.EqualTo(inc.Persons.Count));
        }

        [Test]
        public void StandingDownAHoaxIsRewardedButAbandoningARealCaseFails()
        {
            var w = TestContent.NewWorld();
            var hoax = w.Incidents.Spawn("hoax_flare", Near(w, -5000, 0));
            Assert.That(hoax.IsHoax);
            w.Incidents.StandDown(hoax.Id);
            Assert.That(hoax.State, Is.EqualTo(IncidentState.ClosedHoax));
            double afterHoax = w.Incidents.Stats.MissionReadiness;
            Assert.That(afterHoax, Is.GreaterThan(0));

            var real = w.Incidents.Spawn("fishing_vessel_taking_water", Near(w, -5000, 0));
            Assert.That(real.IsHoax, Is.False);
            w.Incidents.StandDown(real.Id);
            Assert.That(real.State, Is.EqualTo(IncidentState.Failed));
            Assert.That(w.Incidents.Stats.MissionReadiness, Is.LessThan(afterHoax));
        }

        [Test]
        public void ScreeningConfirmsGenuineCases()
        {
            var w = TestContent.NewWorld();
            var real = w.Incidents.Spawn("capsized_river_boater", Near(w, 1000, 0));
            if (real.IsHoax) Assert.Ignore("seed produced a hoax");
            Assert.That(real.PersonsKnown, Is.False);
            Assert.That(w.Incidents.Screen(real.Id), Is.EqualTo(ScreenResult.Genuine));
            Assert.That(real.PersonsKnown, Is.True);
            Assert.That(w.Incidents.Screen(real.Id), Is.EqualTo(ScreenResult.AlreadyScreened));
        }

        [Test]
        public void ScreeningExposesSomeHoaxesButNotAll()
        {
            int exposed = 0, runs = 200;
            for (int s = 1; s <= runs; s++)
            {
                var w = TestContent.NewWorld((ulong)s);
                var hoax = w.Incidents.Spawn("hoax_flare", Near(w, -5000, 0));
                if (w.Incidents.Screen(hoax.Id) == ScreenResult.HoaxConfirmed) exposed++;
            }
            Assert.That(exposed, Is.InRange(runs * 0.45, runs * 0.75));
        }

        [Test]
        public void RiverCaseDriftsDownstream()
        {
            var w = TestContent.NewWorld();
            var inc = w.Incidents.Spawn("capsized_river_boater", Near(w, 2000, 0));
            var start = inc.TruePosition;
            w.StepSeconds(600);
            var moved = inc.TruePosition - start;
            Assert.That(moved.X, Is.LessThan(0), "river flows west to the sea");
            Assert.That(moved.Length, Is.EqualTo(Conv.KnotsToMps(1.5) * 600).Within(1));
        }

        [Test]
        public void ColderWaterShortensSurvivalAndPfdExtendsIt()
        {
            var rules = TestContent.Db.SurvivalRules;
            var rng = new SeededRandom(1);
            double maxCold = 0, minWarm = double.MaxValue;
            for (int i = 0; i < 200; i++)
            {
                maxCold = System.Math.Max(maxCold, rules.SampleSurvivalSeconds(2, false, rng));
                minWarm = System.Math.Min(minWarm, rules.SampleSurvivalSeconds(18, false, rng));
            }
            Assert.That(maxCold, Is.LessThan(minWarm));
            var a = rules.SampleSurvivalSeconds(10, false, new SeededRandom(5));
            var b = rules.SampleSurvivalSeconds(10, true, new SeededRandom(5));
            Assert.That(b, Is.EqualTo(a * rules.PfdMultiplier).Within(1e-6));
        }

        [Test]
        public void GridStartsAtOneAndSearchingLowersIt()
        {
            var g = new ProbabilityGrid(Vec2.Zero, 500, 100, 40);
            Assert.That(g.TotalRemaining(), Is.EqualTo(1).Within(1e-9));
            g.Sweep(Vec2.Zero, 600, 0.9);
            // POA inside 600 m of a 500 m-sigma Gaussian is about 0.51; 90% of that is found.
            Assert.That(g.TotalRemaining(), Is.EqualTo(1 - 0.9 * 0.51).Within(0.03));
            g.Drift(new Vec2(100, 0));
            Assert.That(g.Center, Is.EqualTo(new Vec2(100, 0)));
        }

        [Test]
        public void AutoSpawnRateRoughlyMatchesRulesAndStaysDeterministic()
        {
            (int count, ulong hash) Run()
            {
                var w = TestContent.NewWorld(99);
                w.Incidents.AutoSpawn = true;
                w.StepSeconds(24 * 3600);
                return (w.Incidents.All.Count, w.StateHash());
            }
            var a = Run();
            var b = Run();
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.count, Is.InRange(8, 35), "0.8 per hour over 24 h");
        }
    }
}
