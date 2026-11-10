using System;
using System.Collections.Generic;

namespace RiverCommand.Sim
{
    /// <summary>
    /// Standard SAR search patterns as waypoint lists. Headings are degrees clockwise from north.
    /// </summary>
    public static class SearchPatterns
    {
        public static Vec2 FromHeading(double headingDeg, double distance)
        {
            double r = headingDeg * Math.PI / 180.0;
            return new Vec2(Math.Sin(r) * distance, Math.Cos(r) * distance);
        }

        /// <summary>
        /// Expanding square from a datum. Leg lengths run S, S, 2S, 2S, 3S, 3S ... with 90-degree right turns.
        /// </summary>
        public static List<Vec2> ExpandingSquare(Vec2 datum, double trackSpacing, int legs, double firstHeadingDeg = 0)
        {
            if (trackSpacing <= 0) throw new ArgumentOutOfRangeException(nameof(trackSpacing));
            var points = new List<Vec2>(legs + 1) { datum };
            var at = datum;
            for (int i = 0; i < legs; i++)
            {
                double length = trackSpacing * (i / 2 + 1);
                at += FromHeading(firstHeadingDeg + 90 * i, length);
                points.Add(at);
            }
            return points;
        }

        /// <summary>
        /// Creeping line: parallel legs along the major axis, advancing across it by the track spacing.
        /// </summary>
        public static List<Vec2> CreepingLine(Vec2 start, double legHeadingDeg, double legLength,
                                              double trackSpacing, int legCount, bool advanceToRight = true)
        {
            if (trackSpacing <= 0) throw new ArgumentOutOfRangeException(nameof(trackSpacing));
            var points = new List<Vec2> { start };
            var at = start;
            double cross = legHeadingDeg + (advanceToRight ? 90 : -90);
            for (int i = 0; i < legCount; i++)
            {
                double heading = i % 2 == 0 ? legHeadingDeg : legHeadingDeg + 180;
                at += FromHeading(heading, legLength);
                points.Add(at);
                if (i < legCount - 1)
                {
                    at += FromHeading(cross, trackSpacing);
                    points.Add(at);
                }
            }
            return points;
        }

        /// <summary>
        /// Sector search: three equilateral triangles from the datum, each rotated 120 degrees.
        /// Every leg equals the radius; the unit passes back through the datum after each triangle.
        /// </summary>
        public static List<Vec2> Sector(Vec2 datum, double radius, double firstHeadingDeg = 0)
        {
            if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius));
            var points = new List<Vec2> { datum };
            for (int i = 0; i < 3; i++)
            {
                double baseHeading = firstHeadingDeg + 120 * i;
                points.Add(datum + FromHeading(baseHeading, radius));
                points.Add(datum + FromHeading(baseHeading + 60, radius));
                points.Add(datum);
            }
            return points;
        }

        public static List<Order> ToMoveOrders(IEnumerable<Vec2> waypoints)
        {
            var orders = new List<Order>();
            foreach (var p in waypoints) orders.Add(new MoveOrder(p));
            return orders;
        }
    }
}
