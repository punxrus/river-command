using System.Collections.Generic;

namespace RiverCommand.Sim
{
    /// <summary>Unit definition loaded from Content/Units/*.json. Field names match the JSON (camelCase).</summary>
    public sealed class UnitDef
    {
        public int SchemaVersion { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        /// <summary>"surface" or "air".</summary>
        public string Domain { get; set; }
        public int Tier { get; set; }
        public UnitStats Stats { get; set; } = new UnitStats();
        public UnitCrew Crew { get; set; } = new UnitCrew();
        public List<string> Capabilities { get; set; } = new List<string>();
        public UnitLimits Limits { get; set; } = new UnitLimits();
        public UnitPresentation Presentation { get; set; } = new UnitPresentation();

        public bool Can(string capability) => Capabilities.Contains(capability);
    }

    public sealed class UnitStats
    {
        public double MaxSpeedKts { get; set; }
        public double CruiseSpeedKts { get; set; }
        public double FuelCapacityGal { get; set; }
        public double BurnGalPerHourCruise { get; set; }
        /// <summary>Fraction of cruise burn used while holding position or searching slowly.</summary>
        public double IdleBurnFraction { get; set; } = 0.2;
    }

    public sealed class UnitCrew
    {
        public List<string> Required { get; set; } = new List<string>();
        public int MaxPassengers { get; set; }
    }

    public sealed class UnitLimits
    {
        /// <summary>Highest breaker height (ft) this unit may cross the bar in.</summary>
        public double MaxBarBreakerFt { get; set; }
        public double MaxWaveFt { get; set; }
        public bool IceOps { get; set; }
    }

    public sealed class UnitPresentation
    {
        public string Prefab { get; set; }
        public string Icon { get; set; }
    }
}
