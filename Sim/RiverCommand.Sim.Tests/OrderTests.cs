using System;
using System.Linq;
using NUnit.Framework;

namespace RiverCommand.Sim.Tests
{
    public class OrderTests
    {
        [Test]
        public void PlainIssueReplacesShiftIssueAppends()
        {
            var q = new OrderQueue();
            q.Issue(new MoveOrder(new Vec2(1, 0)), append: false);
            q.Issue(new MoveOrder(new Vec2(2, 0)), append: true);
            Assert.That(q.Count, Is.EqualTo(2));
            q.Issue(new MoveOrder(new Vec2(3, 0)), append: false);
            Assert.That(q.Count, Is.EqualTo(1));
            Assert.That(q.Current.Waypoint, Is.EqualTo(new Vec2(3, 0)));
        }

        [Test]
        public void DraggingAWaypointChangesThePath()
        {
            var q = new OrderQueue();
            q.Issue(new MoveOrder(new Vec2(100, 0)), false);
            q.Issue(new MoveOrder(new Vec2(200, 0)), true);
            Assert.That(q.PathLengthFrom(Vec2.Zero), Is.EqualTo(200).Within(1e-9));
            Assert.That(q.MoveWaypoint(0, new Vec2(100, 100)), Is.True);
            Assert.That(q.PathLengthFrom(Vec2.Zero), Is.EqualTo(Math.Sqrt(2) * 100 * 2).Within(1e-9));
        }

        [Test]
        public void ExpandingSquareLegsGrowEveryTwoTurns()
        {
            var pts = SearchPatterns.ExpandingSquare(Vec2.Zero, 100, 6);
            var legs = Enumerable.Range(0, 6).Select(i => Vec2.Distance(pts[i], pts[i + 1])).ToArray();
            Assert.That(legs, Is.EqualTo(new[] { 100.0, 100, 200, 200, 300, 300 }).Within(1e-6));
        }

        [Test]
        public void ExpandingSquareTracksAreSpacedEvenly()
        {
            // After legs N(S), E(S), S(2S), W(2S) the unit sits one spacing SW of the datum.
            var pts = SearchPatterns.ExpandingSquare(Vec2.Zero, 100, 4);
            Assert.That(pts[4].X, Is.EqualTo(-100).Within(1e-6));
            Assert.That(pts[4].Y, Is.EqualTo(-100).Within(1e-6));
        }

        [Test]
        public void CreepingLineAdvancesBySpacing()
        {
            var pts = SearchPatterns.CreepingLine(Vec2.Zero, 0, 1000, 200, 4);
            // legs north/south of 1000 m, each step east 200 m
            Assert.That(pts.Last().X, Is.EqualTo(600).Within(1e-6));
            Assert.That(pts.Count, Is.EqualTo(1 + 4 + 3));
        }

        [Test]
        public void SectorSearchLegsAllEqualRadiusAndReturnToDatum()
        {
            var datum = new Vec2(500, 500);
            var pts = SearchPatterns.Sector(datum, 300);
            for (int i = 0; i < pts.Count - 1; i++)
                Assert.That(Vec2.Distance(pts[i], pts[i + 1]), Is.EqualTo(300).Within(1e-6), $"leg {i}");
            Assert.That(Vec2.Distance(pts.Last(), datum), Is.LessThan(1e-6));
        }
    }
}
