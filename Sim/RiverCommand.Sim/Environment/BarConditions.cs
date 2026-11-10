using System;

namespace RiverCommand.Sim
{
    public enum BarState { Open, Restricted, Closed }

    /// <summary>Tuning for the bar model. Loaded from Content/Rules/bar.json.</summary>
    public sealed class BarRules
    {
        /// <summary>Breakers below this height (ft): bar open.</summary>
        public double OpenBelowFt { get; set; } = 4;
        /// <summary>Breakers below this height (ft): restricted. At or above: closed.</summary>
        public double ClosedAtFt { get; set; } = 8;
        /// <summary>Fractional breaker growth per knot of ebb current opposing the swell.</summary>
        public double EbbSteepeningPerKnot { get; set; } = 0.15;
        /// <summary>How long (s) a bar report stays "current" before the HUD marks it stale.</summary>
        public double ReportStaleAfterSeconds { get; set; } = 4 * 3600;
    }

    public readonly struct BarReport
    {
        public readonly BarState State;
        public readonly double BreakerHeightFt;
        public readonly double TidalCurrentKnots;
        public readonly double TimeSeconds;

        public BarReport(BarState state, double breakerFt, double currentKts, double time)
        {
            State = state; BreakerHeightFt = breakerFt; TidalCurrentKnots = currentKts; TimeSeconds = time;
        }
    }

    public static class BarConditions
    {
        /// <summary>
        /// Breaker height at the bar: offshore swell steepened by an opposing ebb current.
        /// A flood current does not reduce it below the swell height.
        /// </summary>
        public static double BreakerHeightFt(double swellFt, double tidalCurrentKnots, BarRules rules)
        {
            double ebbKnots = Math.Max(0, -tidalCurrentKnots);
            return swellFt * (1 + rules.EbbSteepeningPerKnot * ebbKnots);
        }

        public static BarState Classify(double breakerFt, BarRules rules)
        {
            if (breakerFt < rules.OpenBelowFt) return BarState.Open;
            if (breakerFt < rules.ClosedAtFt) return BarState.Restricted;
            return BarState.Closed;
        }

        public static BarReport Evaluate(double swellFt, TideModel tide, double timeSeconds, BarRules rules)
        {
            double currentKts = Conv.MpsToKnots(tide.CurrentMps(timeSeconds));
            double breaker = BreakerHeightFt(swellFt, currentKts, rules);
            return new BarReport(Classify(breaker, rules), breaker, currentKts, timeSeconds);
        }
    }
}
