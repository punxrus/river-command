using System.Collections.Generic;

namespace RiverCommand.Sim
{
    /// <summary>Map definition from Content/Maps/*.json. Positions are meters from the map origin.</summary>
    public sealed class MapDef
    {
        public int SchemaVersion { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public double WidthM { get; set; }
        public double HeightM { get; set; }
        public PointDef Station { get; set; }
        public CircleZone Bar { get; set; }
        public PointDef BarObservation { get; set; }
        public TideDef Tide { get; set; }
        public double SwellFt { get; set; }
        public double WaterTempC { get; set; }
        public double StartHourOfDay { get; set; } = 6;
        public List<Landmark> Landmarks { get; set; } = new List<Landmark>();
        public FlowDef RiverFlow { get; set; }
        public FlowDef OffshoreDrift { get; set; }
        /// <summary>Heading (deg) from the bar out to sea; offshore incidents spawn around it.</summary>
        public double SeawardHeadingDeg { get; set; } = 270;

        public Vec2 StationPosition => Station.ToVec2();
        public Vec2 BarObservationPoint => BarObservation.ToVec2();
    }

    public sealed class PointDef
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Vec2 ToVec2() => new Vec2(X, Y);
    }

    public sealed class CircleZone
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double RadiusM { get; set; }
        public bool Contains(Vec2 p) => Vec2.Distance(p, new Vec2(X, Y)) <= RadiusM;
    }

    public sealed class TideDef
    {
        public double MeanHeightM { get; set; }
        public double AmplitudeM { get; set; }
        public double PeriodHours { get; set; } = 12.42;
        public double FirstHighWaterHours { get; set; }
        public double PeakCurrentKts { get; set; }

        public TideModel Build() => new TideModel(MeanHeightM, AmplitudeM, PeriodHours, FirstHighWaterHours, PeakCurrentKts);
    }

    /// <summary>Surface drift: direction (normalized on use) and speed in knots.</summary>
    public sealed class FlowDef
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double SpeedKts { get; set; }
        public Vec2 Velocity => new Vec2(X, Y).Normalized * Conv.KnotsToMps(SpeedKts);
    }

    public sealed class Landmark
    {
        public string Id { get; set; }
        public string Name { get; set; }
        /// <summary>e.g. "island", "sea_stack", "jetty", "harbor", "river", "town", "beach".</summary>
        public string Kind { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}
