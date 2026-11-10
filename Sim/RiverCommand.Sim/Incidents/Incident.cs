using System.Collections.Generic;

namespace RiverCommand.Sim
{
    public enum IncidentState { Reported, Located, OnScene, Resolved, Failed, ClosedHoax }
    public enum PersonState { Safe, InWater, Recovered, Lost }

    public sealed class Person
    {
        public PersonState State { get; internal set; } = PersonState.Safe;
        public bool HasPfd { get; internal set; }
        /// <summary>Sim time (s) the person dies if still in the water.</summary>
        public double DiesAt { get; internal set; } = double.PositiveInfinity;
    }

    /// <summary>
    /// A live incident. Fields marked "truth" are hidden from the player; the HUD shows only the
    /// reported position, the probability grid, and what screening and searching have revealed.
    /// </summary>
    public sealed class Incident
    {
        public int Id { get; }
        public IncidentDef Def { get; }
        public IncidentState State { get; internal set; } = IncidentState.Reported;

        // Truth
        public Vec2 TruePosition { get; internal set; }
        public bool IsHoax { get; }
        public List<Person> Persons { get; } = new List<Person>();
        /// <summary>Sim time (s) persons go into the water (rising tide, vessel sinks). Infinity if never.</summary>
        public double DangerAt { get; internal set; } = double.PositiveInfinity;

        // Known picture
        public Vec2 ReportedPosition { get; }
        public double ReportedErrorM { get; }
        public bool PersonsKnown { get; internal set; }
        public bool Screened { get; internal set; }
        public double ReportedAt { get; }
        public ProbabilityGrid Grid { get; }

        // Response
        public int? OnSceneUnitId { get; internal set; }
        public double OnSceneSince { get; internal set; }
        public double? FirstOnSceneAt { get; internal set; }
        public bool VesselSaved { get; internal set; }

        internal Incident(int id, IncidentDef def, Vec2 truePos, Vec2 reportedPos, double errorM,
                          bool hoax, double reportedAt, ProbabilityGrid grid)
        {
            Id = id; Def = def; TruePosition = truePos; ReportedPosition = reportedPos; ReportedErrorM = errorM;
            IsHoax = hoax; ReportedAt = reportedAt; Grid = grid; PersonsKnown = def.Report.PersonsKnown;
        }

        public bool IsOpen => State == IncidentState.Reported || State == IncidentState.Located || State == IncidentState.OnScene;
        public int Recovered => Persons.FindAll(p => p.State == PersonState.Recovered).Count;
        public int Lost => Persons.FindAll(p => p.State == PersonState.Lost).Count;
    }
}
