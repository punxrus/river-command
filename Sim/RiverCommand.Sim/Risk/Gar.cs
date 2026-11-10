using System;
using System.Collections.Generic;

namespace RiverCommand.Sim
{
    public enum GarElement { Supervision, Planning, CrewSelection, CrewFitness, Environment, EventComplexity }
    public enum GarBand { Green, Amber, Red }

    public sealed class GarLine
    {
        public GarElement Element { get; }
        public int Score { get; }
        public string Reason { get; }
        /// <summary>Suggested action that would lower this score, or null.</summary>
        public string Mitigation { get; }

        public GarLine(GarElement element, int score, string reason, string mitigation)
        {
            Element = element; Score = Math.Max(1, Math.Min(10, score)); Reason = reason; Mitigation = mitigation;
        }
    }

    /// <summary>Classic six-element GAR: each element 1-10, total 6-60. Green 0-23, Amber 24-44, Red 45-60.</summary>
    public sealed class GarAssessment
    {
        public const int AmberFrom = 24;
        public const int RedFrom = 45;

        public IReadOnlyList<GarLine> Lines { get; }
        public int Total { get; }
        public GarBand Band { get; }

        public GarAssessment(IReadOnlyList<GarLine> lines)
        {
            if (lines.Count != 6) throw new ArgumentException("GAR needs exactly six elements.");
            Lines = lines;
            int total = 0;
            foreach (var l in lines) total += l.Score;
            Total = total;
            Band = BandFor(total);
        }

        public static GarBand BandFor(int total) =>
            total >= RedFrom ? GarBand.Red : total >= AmberFrom ? GarBand.Amber : GarBand.Green;
    }

    /// <summary>Facts about a planned launch that the GAR calculator scores.</summary>
    public sealed class GarInputs
    {
        public bool SupervisorOnWatch { get; set; } = true;
        public double PlanningMinutes { get; set; } = 5;
        /// <summary>Lowest experience among the crew, in qualified hours on this asset.</summary>
        public double LeastExperiencedCrewHours { get; set; } = 200;
        /// <summary>Longest time any crew member has been awake, in hours.</summary>
        public double MaxCrewHoursAwake { get; set; } = 8;
        public BarState? BarState { get; set; }
        /// <summary>True if the bar report is older than the stale limit or missing.</summary>
        public bool BarReportStale { get; set; }
        public double VisibilityNm { get; set; } = 10;
        public bool Night { get; set; }
        public bool InvolvesTow { get; set; }
        public int UnitsOnScene { get; set; } = 1;
    }

    /// <summary>
    /// Turns launch facts into GAR scores with a reason and a mitigation per element.
    /// The scoring curve is a first pass for play-testing, not an official Coast Guard method.
    /// </summary>
    public static class GarCalculator
    {
        public static GarAssessment Assess(GarInputs i)
        {
            var lines = new List<GarLine>(6);

            lines.Add(i.SupervisorOnWatch
                ? new GarLine(GarElement.Supervision, 2, "Officer of the day on watch", null)
                : new GarLine(GarElement.Supervision, 6, "No supervisor on watch", "Recall the officer of the day"));

            lines.Add(i.PlanningMinutes >= 10
                ? new GarLine(GarElement.Planning, 2, $"{i.PlanningMinutes:F0} min of planning", null)
                : i.PlanningMinutes >= 3
                    ? new GarLine(GarElement.Planning, 4, $"{i.PlanningMinutes:F0} min of planning", "Take more time to brief")
                    : new GarLine(GarElement.Planning, 7, "Rushed launch", "Take more time to brief"));

            int sel = i.LeastExperiencedCrewHours >= 300 ? 2 : i.LeastExperiencedCrewHours >= 100 ? 4 : 7;
            lines.Add(new GarLine(GarElement.CrewSelection, sel,
                $"Least experienced crew member: {i.LeastExperiencedCrewHours:F0} h",
                sel > 2 ? "Swap in a more experienced crew member" : null));

            int fit = i.MaxCrewHoursAwake < 10 ? 2 : i.MaxCrewHoursAwake < 14 ? 5 : i.MaxCrewHoursAwake < 18 ? 8 : 10;
            lines.Add(new GarLine(GarElement.CrewFitness, fit,
                $"Crew member awake {i.MaxCrewHoursAwake:F0} h",
                fit > 2 ? "Swap crew" : null));

            int env = 2;
            var envReasons = new List<string>();
            // Unknown bar scores like a restricted one, so checking the bar never raises the score.
            if (i.BarState == null) { env += 3; envReasons.Add("bar conditions unknown"); }
            if (i.BarState == Sim.BarState.Restricted) { env += 3; envReasons.Add("bar restricted"); }
            if (i.BarState == Sim.BarState.Closed) { env += 6; envReasons.Add("bar closed"); }
            if (i.BarReportStale) { env += 2; envReasons.Add("bar report stale"); }
            if (i.VisibilityNm < 1) { env += 3; envReasons.Add($"visibility {i.VisibilityNm:F1} nm"); }
            else if (i.VisibilityNm < 3) { env += 1; envReasons.Add($"visibility {i.VisibilityNm:F1} nm"); }
            if (i.Night) { env += 1; envReasons.Add("night"); }
            lines.Add(new GarLine(GarElement.Environment, env,
                envReasons.Count == 0 ? "Good conditions" : string.Join(", ", envReasons),
                i.BarReportStale ? "Order a bar check" : env > 4 ? "Wait for better conditions" : null));

            int cx = 2 + (i.InvolvesTow ? 3 : 0) + Math.Max(0, i.UnitsOnScene - 1) * 2 + (i.Night ? 1 : 0);
            lines.Add(new GarLine(GarElement.EventComplexity, cx,
                i.InvolvesTow ? "Tow required" : "Single-unit response",
                cx > 4 ? "Send a second unit to share the load" : null));

            return new GarAssessment(lines);
        }
    }
}
