namespace RiverCommand.Sim
{
    /// <summary>
    /// Live environment state. The sim knows the truth; the player only sees the last bar report,
    /// which ages until a crew does a new bar check.
    /// </summary>
    public sealed class SeaEnvironment
    {
        public TideModel Tide { get; }
        public BarRules BarRules { get; }

        /// <summary>Offshore swell height (ft). Scenario scripts change this over time.</summary>
        public double SwellFt { get; set; }
        /// <summary>Visibility in nautical miles.</summary>
        public double VisibilityNm { get; set; } = 10;
        /// <summary>Sea water temperature (C). Drives survival time.</summary>
        public double WaterTempC { get; set; } = 10;

        /// <summary>The player's latest bar report, or null if no check has been made.</summary>
        public BarReport? LastBarReport { get; private set; }

        public SeaEnvironment(TideModel tide, BarRules barRules, double swellFt)
        {
            Tide = tide; BarRules = barRules; SwellFt = swellFt;
        }

        public BarReport TrueBar(double timeSeconds) => BarConditions.Evaluate(SwellFt, Tide, timeSeconds, BarRules);

        /// <summary>A crew observes the bar and reports. Returns the new report.</summary>
        public BarReport DoBarCheck(double timeSeconds)
        {
            var report = TrueBar(timeSeconds);
            LastBarReport = report;
            return report;
        }

        public bool IsBarReportStale(double timeSeconds) =>
            LastBarReport == null || timeSeconds - LastBarReport.Value.TimeSeconds > BarRules.ReportStaleAfterSeconds;

        internal void RestoreBarReport(BarReport? report) => LastBarReport = report;
    }
}
