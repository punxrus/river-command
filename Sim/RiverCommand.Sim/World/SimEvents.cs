namespace RiverCommand.Sim
{
    /// <summary>Events the sim emits for the presentation layer (HUD, radio log, audio).</summary>
    public abstract class SimEvent
    {
        public long Tick { get; internal set; }
    }

    public sealed class BarReportedEvent : SimEvent
    {
        public int UnitId { get; }
        public BarReport Report { get; }
        public BarReportedEvent(int unitId, BarReport report) { UnitId = unitId; Report = report; }
    }

    public sealed class BarBlockedEvent : SimEvent
    {
        public int UnitId { get; }
        public double BreakerFt { get; }
        public double UnitLimitFt { get; }
        public BarBlockedEvent(int unitId, double breakerFt, double limitFt) { UnitId = unitId; BreakerFt = breakerFt; UnitLimitFt = limitFt; }
    }

    public sealed class UnitLaunchedEvent : SimEvent
    {
        public int UnitId { get; }
        public GarAssessment Gar { get; }
        public UnitLaunchedEvent(int unitId, GarAssessment gar) { UnitId = unitId; Gar = gar; }
    }
}
