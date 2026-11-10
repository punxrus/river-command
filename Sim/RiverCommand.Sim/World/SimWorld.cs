using System;
using System.Collections.Generic;

namespace RiverCommand.Sim
{
    /// <summary>
    /// The whole simulation. Presentation calls Step() at 10 Hz (or not at all while paused)
    /// and issues orders at any time, including while paused.
    /// </summary>
    public sealed class SimWorld
    {
        public MapDef Map { get; }
        public ContentDatabase Content { get; }
        public SimClock Clock { get; }
        public SeededRandom Random { get; }
        public SeaEnvironment Environment { get; }
        public IncidentSystem Incidents { get; }

        private readonly List<Unit> _units = new List<Unit>();
        private readonly List<SimEvent> _events = new List<SimEvent>();
        private int _nextUnitId = 1;

        public IReadOnlyList<Unit> Units => _units;

        public SimWorld(ContentDatabase content, string mapId, ulong seed)
        {
            Content = content;
            Map = content.Map(mapId);
            Clock = new SimClock(Map.StartHourOfDay);
            Random = new SeededRandom(seed);
            Environment = new SeaEnvironment(Map.Tide.Build(), content.BarRules, Map.SwellFt)
            {
                WaterTempC = Map.WaterTempC,
            };
            Incidents = new IncidentSystem(this);
        }

        public Unit SpawnUnit(string defId, Vec2? position = null)
        {
            var unit = new Unit(_nextUnitId++, Content.Unit(defId), position ?? Map.StationPosition);
            _units.Add(unit);
            return unit;
        }

        public Unit GetUnit(int id)
        {
            foreach (var u in _units) if (u.Id == id) return u;
            return null;
        }

        /// <summary>Builds the GAR for launching this unit now. Crew facts come from the caller until the personnel system lands.</summary>
        public GarAssessment AssessLaunch(Unit unit, GarInputs crewFacts)
        {
            double t = Clock.Seconds;
            crewFacts.BarState = Environment.LastBarReport?.State;
            crewFacts.BarReportStale = Environment.IsBarReportStale(t);
            crewFacts.VisibilityNm = Environment.VisibilityNm;
            crewFacts.Night = IsNight;
            return GarCalculator.Assess(crewFacts);
        }

        /// <summary>Records the launch with its GAR (for the post-case review) and starts the orders.</summary>
        public void Launch(Unit unit, GarAssessment gar, IEnumerable<Order> orders)
        {
            unit.Orders.IssueAll(orders, append: false);
            unit.Status = UnitStatus.Underway;
            Emit(new UnitLaunchedEvent(unit.Id, gar));
        }

        public bool IsNight => Clock.HourOfDay < 6 || Clock.HourOfDay >= 20;

        public void Step()
        {
            double dt = SimClock.TickSeconds;
            foreach (var unit in _units) unit.Orders.Tick(unit, this, dt);
            Incidents.Tick(dt);
            Clock.Advance();
        }

        public void StepSeconds(double seconds)
        {
            long ticks = (long)Math.Round(seconds * SimClock.TicksPerSecond);
            for (long i = 0; i < ticks; i++) Step();
        }

        internal void Emit(SimEvent e)
        {
            e.Tick = Clock.Tick;
            _events.Add(e);
        }

        /// <summary>Returns and clears events since the last drain. Presentation calls this each frame.</summary>
        public List<SimEvent> DrainEvents()
        {
            var copy = new List<SimEvent>(_events);
            _events.Clear();
            return copy;
        }

        /// <summary>Hash of the full sim state. Same seed + same commands must give the same hash.</summary>
        public ulong StateHash()
        {
            ulong h = 14695981039346656037UL;
            void Mix(ulong v) { h ^= v; h *= 1099511628211UL; }
            void MixD(double d) => Mix((ulong)BitConverter.DoubleToInt64Bits(d));

            Mix((ulong)Clock.Tick);
            Mix(Random.State);
            MixD(Environment.SwellFt);
            foreach (var u in _units)
            {
                Mix((ulong)u.Id);
                MixD(u.Position.X); MixD(u.Position.Y);
                MixD(u.FuelGal);
                Mix((ulong)u.Status);
                Mix((ulong)u.Orders.Count);
            }
            foreach (var inc in Incidents.All)
            {
                Mix((ulong)inc.Id);
                Mix((ulong)inc.State);
                MixD(inc.TruePosition.X); MixD(inc.TruePosition.Y);
                foreach (var p in inc.Persons) { Mix((ulong)p.State); MixD(p.DiesAt); }
            }
            return h;
        }
    }
}
