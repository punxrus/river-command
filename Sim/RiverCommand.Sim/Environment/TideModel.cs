using System;

namespace RiverCommand.Sim
{
    /// <summary>
    /// Single-constituent (M2-like) tide. Good enough for gameplay timing: players learn that
    /// the ebb runs out against the swell and steepens the bar. Real Pacific tides are mixed
    /// semidiurnal; add constituents later if it matters for play.
    /// </summary>
    public sealed class TideModel
    {
        public double MeanHeightM { get; }
        public double AmplitudeM { get; }
        public double PeriodSeconds { get; }
        /// <summary>Seconds after scenario start of the first high water.</summary>
        public double FirstHighWaterSeconds { get; }
        /// <summary>Peak tidal current at the bar in m/s (reached mid-flood and mid-ebb).</summary>
        public double PeakCurrentMps { get; }

        public TideModel(double meanHeightM, double amplitudeM, double periodHours,
                         double firstHighWaterHours, double peakCurrentKnots)
        {
            if (periodHours <= 0) throw new ArgumentOutOfRangeException(nameof(periodHours));
            MeanHeightM = meanHeightM;
            AmplitudeM = amplitudeM;
            PeriodSeconds = periodHours * 3600.0;
            FirstHighWaterSeconds = firstHighWaterHours * 3600.0;
            PeakCurrentMps = Conv.KnotsToMps(peakCurrentKnots);
        }

        private double Phase(double t) => 2 * Math.PI * (t - FirstHighWaterSeconds) / PeriodSeconds;

        public double HeightM(double t) => MeanHeightM + AmplitudeM * Math.Cos(Phase(t));

        /// <summary>Rate of change of height in m/s. Positive = rising (flood).</summary>
        public double RateMps(double t) => -AmplitudeM * Math.Sin(Phase(t)) * 2 * Math.PI / PeriodSeconds;

        /// <summary>Tidal current at the bar in m/s. Positive = flood (inbound), negative = ebb (outbound).</summary>
        public double CurrentMps(double t) => -PeakCurrentMps * Math.Sin(Phase(t));

        public bool IsEbb(double t) => CurrentMps(t) < 0;
    }
}
