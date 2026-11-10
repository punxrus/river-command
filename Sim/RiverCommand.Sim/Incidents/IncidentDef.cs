using System.Collections.Generic;
using Newtonsoft.Json;

namespace RiverCommand.Sim
{
    /// <summary>Incident template from Content/Incidents/*.json.</summary>
    public sealed class IncidentDef
    {
        public int SchemaVersion { get; set; }
        public string Id { get; set; }
        /// <summary>"sar" or "screening".</summary>
        public string Category { get; set; }
        public string Summary { get; set; }
        public SpawnDef Spawn { get; set; } = new SpawnDef();
        public TruthDef Truth { get; set; } = new TruthDef();
        public ReportDef Report { get; set; } = new ReportDef();
        public ClockDef Clock { get; set; } = new ClockDef();
        /// <summary>"none", "river", or "offshore".</summary>
        public string Drift { get; set; } = "none";
        public ResolutionDef Resolution { get; set; } = new ResolutionDef();
        public RewardsDef Rewards { get; set; } = new RewardsDef();
        [JsonProperty("_note")] public string Note { get; set; }
    }

    public sealed class SpawnDef
    {
        public double Weight { get; set; } = 1;
        /// <summary>"any", "day", or "night".</summary>
        public string TimeOfDay { get; set; } = "any";
        /// <summary>"landmark" or "offshore".</summary>
        public string Zone { get; set; } = "landmark";
        public List<string> LandmarkKinds { get; set; } = new List<string>();
        public double[] OffsetM { get; set; } = { 0, 100 };
        public double[] DistanceNm { get; set; } = { 2, 8 };
    }

    public sealed class TruthDef
    {
        public int[] Persons { get; set; } = { 1, 1 };
        public double PfdChance { get; set; }
        public double HoaxChance { get; set; }
    }

    public sealed class ReportDef
    {
        public double[] PositionErrorM { get; set; } = { 100, 500 };
        public bool PersonsKnown { get; set; }
        /// <summary>Minutes between the event happening and the call reaching the station.</summary>
        public double[] DelayMinutes { get; set; } = { 0, 0 };
    }

    public sealed class ClockDef
    {
        /// <summary>"none", "cold_water", "rising_tide", or "vessel_flooding".</summary>
        public string Kind { get; set; } = "cold_water";
        /// <summary>Minutes before persons enter the water (rising_tide, vessel_flooding).</summary>
        public double[] SafeMinutes { get; set; } = { 0, 0 };
        [JsonProperty("_note")] public string Note { get; set; }
    }

    public sealed class ResolutionDef
    {
        /// <summary>Capability the on-scene unit must have, or null for any unit.</summary>
        public string NeedsCapability { get; set; }
        public double OnSceneMinutes { get; set; } = 5;
    }

    public sealed class RewardsDef
    {
        public double ReadinessPerLife { get; set; }
        public double TimeBonusUnderMin { get; set; }
    }

    public sealed class SurvivalRules
    {
        public List<SurvivalBand> Bands { get; set; } = new List<SurvivalBand>();
        public double PfdMultiplier { get; set; } = 1.5;
        [JsonProperty("_note")] public string Note { get; set; }

        /// <summary>Survival time in water (s), sampled within the band for this temperature.</summary>
        public double SampleSurvivalSeconds(double waterTempC, bool pfd, SeededRandom rng)
        {
            var band = Bands[Bands.Count - 1];
            foreach (var b in Bands) { if (waterTempC <= b.MaxWaterTempC) { band = b; break; } }
            double minutes = rng.Range(band.Minutes[0], band.Minutes[1]);
            if (pfd) minutes *= PfdMultiplier;
            return minutes * 60;
        }
    }

    public sealed class SurvivalBand
    {
        public double MaxWaterTempC { get; set; }
        public double[] Minutes { get; set; }
    }

    public sealed class SearchRules
    {
        public double DetectRadiusM { get; set; } = 150;
        public double DetectChancePerSecond { get; set; } = 0.15;
        public double MinVisibilityFactor { get; set; } = 0.2;
        public double OnSceneRadiusM { get; set; } = 60;
        public double IncidentsPerHour { get; set; } = 0.8;
        public double GridCellM { get; set; } = 100;
        public int GridHalfSizeCells { get; set; } = 40;
        /// <summary>Chance a callback to the reporting party exposes a hoax.</summary>
        public double ScreenRevealsHoaxChance { get; set; } = 0.6;
        [JsonProperty("_note")] public string Note { get; set; }
    }
}
