namespace RiverCommand.Sim
{
    public sealed class IncidentReportedEvent : SimEvent
    { public Incident Incident { get; } public IncidentReportedEvent(Incident i) { Incident = i; } }

    public sealed class IncidentLocatedEvent : SimEvent
    {
        public Incident Incident { get; } public int UnitId { get; }
        public IncidentLocatedEvent(Incident i, int unitId) { Incident = i; UnitId = unitId; }
    }

    public sealed class IncidentOnSceneEvent : SimEvent
    {
        public Incident Incident { get; } public int UnitId { get; }
        public IncidentOnSceneEvent(Incident i, int unitId) { Incident = i; UnitId = unitId; }
    }

    /// <summary>A unit reached the scene but lacks the capability needed (e.g. surf, tow).</summary>
    public sealed class IncidentWrongAssetEvent : SimEvent
    {
        public Incident Incident { get; } public int UnitId { get; } public string Needs { get; }
        public IncidentWrongAssetEvent(Incident i, int unitId, string needs) { Incident = i; UnitId = unitId; Needs = needs; }
    }

    public sealed class PersonsInWaterEvent : SimEvent
    { public Incident Incident { get; } public PersonsInWaterEvent(Incident i) { Incident = i; } }

    public sealed class PersonLostEvent : SimEvent
    { public Incident Incident { get; } public PersonLostEvent(Incident i) { Incident = i; } }

    public sealed class IncidentClosedEvent : SimEvent
    { public Incident Incident { get; } public IncidentClosedEvent(Incident i) { Incident = i; } }

    public enum ScreenResult { AlreadyScreened, HoaxConfirmed, Inconclusive, Genuine }
}
