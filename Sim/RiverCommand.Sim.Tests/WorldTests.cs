using System.Linq;
using NUnit.Framework;

namespace RiverCommand.Sim.Tests
{
    public class WorldTests
    {
        [Test]
        public void BoatReachesTargetAtCruiseSpeedAndBurnsFuel()
        {
            var w = TestContent.NewWorld();
            var start = w.Map.StationPosition;
            var boat = w.SpawnUnit("rbs29", start);
            var target = start + new Vec2(0, Conv.NmToMeters(3)); // 3 nm north, stays inside harbor side
            boat.Orders.Issue(new MoveOrder(target), false);

            w.StepSeconds(5 * 60); // 30 kt => 2.5 nm in 5 min
            Assert.That(Vec2.Distance(boat.Position, start), Is.EqualTo(Conv.NmToMeters(2.5)).Within(5));
            w.StepSeconds(2 * 60);
            Assert.That(Vec2.Distance(boat.Position, target), Is.LessThan(MoveOrder.ArriveRadiusM + 1));
            Assert.That(boat.Orders.Count, Is.EqualTo(0));
            Assert.That(boat.FuelGal, Is.LessThan(boat.Def.Stats.FuelCapacityGal));
        }

        [Test]
        public void SmallBoatWaitsAtBarInBigSurfButLifeboatCrosses()
        {
            var w = TestContent.NewWorld();
            w.Environment.SwellFt = 10;
            var offshore = new Vec2(w.Map.Bar.X - 2000, w.Map.Bar.Y);
            var rbs = w.SpawnUnit("rbs29");
            var mlb = w.SpawnUnit("mlb47");
            rbs.Orders.Issue(new MoveOrder(offshore), false);
            mlb.Orders.Issue(new MoveOrder(offshore), false);

            w.StepSeconds(15 * 60);

            Assert.That(rbs.Status, Is.EqualTo(UnitStatus.WaitingOnBar));
            Assert.That(mlb.Orders.Count, Is.EqualTo(0), "lifeboat arrived offshore");
            Assert.That(w.DrainEvents().OfType<BarBlockedEvent>().Count(e => e.UnitId == rbs.Id), Is.EqualTo(1),
                "blocked event fires once, not every tick");
        }

        [Test]
        public void BarCheckProducesAReportAndClearsStaleness()
        {
            var w = TestContent.NewWorld();
            var boat = w.SpawnUnit("rbs29");
            Assert.That(w.Environment.IsBarReportStale(w.Clock.Seconds), Is.True);
            boat.Orders.Issue(new BarCheckOrder(), false);
            w.StepSeconds(5 * 60);
            Assert.That(w.Environment.LastBarReport, Is.Not.Null);
            Assert.That(w.DrainEvents().OfType<BarReportedEvent>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void LaunchGarUsesLiveConditions()
        {
            var w = TestContent.NewWorld();
            var boat = w.SpawnUnit("mlb47");
            var before = w.AssessLaunch(boat, new GarInputs());
            w.Environment.DoBarCheck(w.Clock.Seconds);
            var after = w.AssessLaunch(boat, new GarInputs());
            Assert.That(after.Total, Is.LessThan(before.Total), "a fresh bar report lowers risk");
        }

        [Test]
        public void SameSeedAndCommandsGiveSameState()
        {
            ulong Run()
            {
                var w = TestContent.NewWorld(seed: 7);
                var a = w.SpawnUnit("mlb47");
                var b = w.SpawnUnit("rbs29");
                a.Orders.IssueAll(SearchPatterns.ToMoveOrders(
                    SearchPatterns.ExpandingSquare(new Vec2(15000, 20000), 300, 8)), false);
                b.Orders.Issue(new BarCheckOrder(), false);
                for (int i = 0; i < 10000; i++)
                {
                    w.Random.NextDouble();
                    w.Step();
                }
                return w.StateHash();
            }
            Assert.That(Run(), Is.EqualTo(Run()));
        }
    }
}
