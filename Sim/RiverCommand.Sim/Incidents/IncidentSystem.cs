using System;
using System.Collections.Generic;

namespace RiverCommand.Sim
{
    public sealed class SimStats
    {
        public int LivesSaved { get; internal set; }
        public int LivesLost { get; internal set; }
        public int IncidentsResolved { get; internal set; }
        public int IncidentsFailed { get; internal set; }
        public int HoaxesClosed { get; internal set; }
        public double MissionReadiness { get; internal set; }
    }

    /// <summary>Spawns, updates, and resolves incidents. Owned by SimWorld.</summary>
    public sealed class IncidentSystem
    {
        private readonly SimWorld _w;
        private readonly List<Incident> _incidents = new List<Incident>();
        private readonly HashSet<long> _wrongAssetWarned = new HashSet<long>();
        private int _nextId = 1;

        /// <summary>Random incidents on/off. Missions and endless mode turn this on.</summary>
        public bool AutoSpawn { get; set; }
        public IReadOnlyList<Incident> All => _incidents;
        public SimStats Stats { get; } = new SimStats();

        internal IncidentSystem(SimWorld world) { _w = world; }

        private SearchRules Rules => _w.Content.SearchRules;

        public Incident Get(int id)
        {
            foreach (var i in _incidents) if (i.Id == id) return i;
            return null;
        }

        /// <summary>Creates an incident now. Position is random per the template unless given.</summary>
        public Incident Spawn(string defId, Vec2? truePosition = null)
        {
            var def = _w.Content.Incident(defId);
            var rng = _w.Random;
            double now = _w.Clock.Seconds;

            bool hoax = rng.Chance(def.Truth.HoaxChance);
            var truth = truePosition ?? PickPosition(def);
            double error = rng.Range(def.Report.PositionErrorM[0], def.Report.PositionErrorM[1]);
            var reported = truth + SearchPatterns.FromHeading(rng.Range(0.0, 360.0), rng.Range(0.0, error));
            var grid = new ProbabilityGrid(reported, error, Rules.GridCellM, Rules.GridHalfSizeCells);

            var inc = new Incident(_nextId++, def, truth, reported, error, hoax, now, grid);
            double happenedAt = now - rng.Range(def.Report.DelayMinutes[0], def.Report.DelayMinutes[1]) * 60;

            int persons = hoax ? 0 : rng.Range(def.Truth.Persons[0], def.Truth.Persons[1] + 1);
            for (int p = 0; p < persons; p++)
                inc.Persons.Add(new Person { HasPfd = rng.Chance(def.Truth.PfdChance) });

            switch (def.Clock.Kind)
            {
                case "cold_water":
                    PutInWater(inc, happenedAt);
                    break;
                case "rising_tide":
                case "vessel_flooding":
                    inc.DangerAt = happenedAt + rng.Range(def.Clock.SafeMinutes[0], def.Clock.SafeMinutes[1]) * 60;
                    break;
            }

            _incidents.Add(inc);
            _w.Emit(new IncidentReportedEvent(inc));
            return inc;
        }

        /// <summary>Callback to the reporting party. May expose a hoax; confirms details if genuine.</summary>
        public ScreenResult Screen(int incidentId)
        {
            var inc = Get(incidentId);
            if (inc == null || !inc.IsOpen) throw new InvalidOperationException("No open incident " + incidentId);
            if (inc.Screened) return ScreenResult.AlreadyScreened;
            inc.Screened = true;
            if (!inc.IsHoax) { inc.PersonsKnown = true; return ScreenResult.Genuine; }
            if (_w.Random.Chance(Rules.ScreenRevealsHoaxChance))
            {
                Close(inc, IncidentState.ClosedHoax);
                Stats.HoaxesClosed++;
                Stats.MissionReadiness += 15;
                return ScreenResult.HoaxConfirmed;
            }
            return ScreenResult.Inconclusive;
        }

        /// <summary>
        /// The player stands down the case. Right call on a hoax; abandoning a real case fails it.
        /// </summary>
        public void StandDown(int incidentId)
        {
            var inc = Get(incidentId);
            if (inc == null || !inc.IsOpen) return;
            if (inc.IsHoax) { Stats.HoaxesClosed++; Stats.MissionReadiness += 10; Close(inc, IncidentState.ClosedHoax); return; }
            foreach (var p in inc.Persons)
                if (p.State == PersonState.Safe || p.State == PersonState.InWater) { p.State = PersonState.Lost; Stats.LivesLost++; }
            Stats.IncidentsFailed++;
            Stats.MissionReadiness -= 100;
            Close(inc, IncidentState.Failed);
        }

        internal void Tick(double dt)
        {
            double now = _w.Clock.Seconds;

            if (AutoSpawn && _w.Random.Chance(Rules.IncidentsPerHour * dt / 3600.0))
            {
                var def = PickTemplate();
                if (def != null) Spawn(def.Id);
            }

            foreach (var inc in _incidents)
            {
                if (!inc.IsOpen) continue;
                ApplyDrift(inc, dt);
                UpdateDanger(inc, now);
                UpdateDeaths(inc, now);
                if (!inc.IsOpen) continue;
                UpdateSearch(inc, dt);
                UpdateOnScene(inc, now);
            }
        }

        private void ApplyDrift(Incident inc, double dt)
        {
            FlowDef flow = inc.Def.Drift == "river" ? _w.Map.RiverFlow
                         : inc.Def.Drift == "offshore" ? _w.Map.OffshoreDrift : null;
            if (flow == null || inc.State == IncidentState.OnScene) return;
            var delta = flow.Velocity * dt;
            inc.TruePosition += delta;
            inc.Grid.Drift(delta);
        }

        private void UpdateDanger(Incident inc, double now)
        {
            if (now < inc.DangerAt) return;
            inc.DangerAt = double.PositiveInfinity;
            PutInWater(inc, now);
            _w.Emit(new PersonsInWaterEvent(inc));
        }

        private void PutInWater(Incident inc, double at)
        {
            foreach (var p in inc.Persons)
            {
                if (p.State != PersonState.Safe) continue;
                p.State = PersonState.InWater;
                p.DiesAt = at + _w.Content.SurvivalRules.SampleSurvivalSeconds(_w.Environment.WaterTempC, p.HasPfd, _w.Random);
            }
        }

        private void UpdateDeaths(Incident inc, double now)
        {
            foreach (var p in inc.Persons)
            {
                if (p.State != PersonState.InWater || now < p.DiesAt) continue;
                p.State = PersonState.Lost;
                Stats.LivesLost++;
                _w.Emit(new PersonLostEvent(inc));
            }
            if (inc.Persons.Count > 0 && inc.Lost == inc.Persons.Count)
            {
                Stats.IncidentsFailed++;
                Close(inc, IncidentState.Failed);
            }
        }

        private double VisibilityFactor =>
            Math.Max(Rules.MinVisibilityFactor, Math.Min(1.0, _w.Environment.VisibilityNm / 2.0));

        private void UpdateSearch(Incident inc, double dt)
        {
            if (inc.State != IncidentState.Reported) return;
            double radius = Rules.DetectRadiusM * VisibilityFactor;
            double p = Rules.DetectChancePerSecond * dt;
            foreach (var u in _w.Units)
            {
                if (u.Status == UnitStatus.Moored) continue;
                inc.Grid.Sweep(u.Position, radius, p);
                if (inc.IsHoax) continue;
                if (Vec2.Distance(u.Position, inc.TruePosition) <= radius && _w.Random.Chance(p))
                {
                    inc.State = IncidentState.Located;
                    _w.Emit(new IncidentLocatedEvent(inc, u.Id));
                    return;
                }
            }
        }

        private void UpdateOnScene(Incident inc, double now)
        {
            if (inc.State == IncidentState.OnScene)
            {
                var unit = _w.GetUnit(inc.OnSceneUnitId.Value);
                if (unit == null || Vec2.Distance(unit.Position, inc.TruePosition) > Rules.OnSceneRadiusM)
                {
                    inc.State = IncidentState.Located;
                    inc.OnSceneUnitId = null;
                    return;
                }
                if (now - inc.OnSceneSince >= inc.Def.Resolution.OnSceneMinutes * 60) Resolve(inc, now);
                return;
            }
            if (inc.State != IncidentState.Located) return;

            string needs = inc.Def.Resolution.NeedsCapability;
            foreach (var u in _w.Units)
            {
                if (Vec2.Distance(u.Position, inc.TruePosition) > Rules.OnSceneRadiusM) continue;
                if (needs != null && !u.Def.Can(needs))
                {
                    if (_wrongAssetWarned.Add(((long)inc.Id << 32) | (uint)u.Id))
                        _w.Emit(new IncidentWrongAssetEvent(inc, u.Id, needs));
                    continue;
                }
                inc.State = IncidentState.OnScene;
                inc.OnSceneUnitId = u.Id;
                inc.OnSceneSince = now;
                inc.FirstOnSceneAt ??= now;
                if (inc.Def.Clock.Kind == "vessel_flooding" && !double.IsPositiveInfinity(inc.DangerAt))
                {
                    inc.VesselSaved = true;
                    inc.DangerAt = double.PositiveInfinity;
                }
                _w.Emit(new IncidentOnSceneEvent(inc, u.Id));
                return;
            }
        }

        private void Resolve(Incident inc, double now)
        {
            int saved = 0;
            foreach (var p in inc.Persons)
                if (p.State == PersonState.Safe || p.State == PersonState.InWater) { p.State = PersonState.Recovered; saved++; }
            Stats.LivesSaved += saved;
            Stats.IncidentsResolved++;
            Stats.MissionReadiness += saved * inc.Def.Rewards.ReadinessPerLife;
            double responseMin = ((inc.FirstOnSceneAt ?? now) - inc.ReportedAt) / 60.0;
            if (responseMin <= inc.Def.Rewards.TimeBonusUnderMin) Stats.MissionReadiness += 20;
            Close(inc, IncidentState.Resolved);
        }

        private void Close(Incident inc, IncidentState state)
        {
            inc.State = state;
            _w.Emit(new IncidentClosedEvent(inc));
        }

        private IncidentDef PickTemplate()
        {
            bool night = _w.IsNight;
            double total = 0;
            var candidates = new List<IncidentDef>();
            foreach (var d in _w.Content.IncidentList)
            {
                string t = d.Spawn.TimeOfDay;
                if ((t == "day" && night) || (t == "night" && !night)) continue;
                candidates.Add(d);
                total += d.Spawn.Weight;
            }
            if (candidates.Count == 0) return null;
            double roll = _w.Random.Range(0.0, total);
            foreach (var d in candidates) { roll -= d.Spawn.Weight; if (roll <= 0) return d; }
            return candidates[candidates.Count - 1];
        }

        private Vec2 PickPosition(IncidentDef def)
        {
            var rng = _w.Random;
            if (def.Spawn.Zone == "offshore")
            {
                double heading = _w.Map.SeawardHeadingDeg + rng.Range(-50.0, 50.0);
                double dist = Conv.NmToMeters(rng.Range(def.Spawn.DistanceNm[0], def.Spawn.DistanceNm[1]));
                return Clamp(new Vec2(_w.Map.Bar.X, _w.Map.Bar.Y) + SearchPatterns.FromHeading(heading, dist));
            }
            var matches = new List<Landmark>();
            foreach (var l in _w.Map.Landmarks) if (def.Spawn.LandmarkKinds.Contains(l.Kind)) matches.Add(l);
            if (matches.Count == 0) return _w.Map.StationPosition;
            var lm = matches[rng.Range(0, matches.Count)];
            var off = SearchPatterns.FromHeading(rng.Range(0.0, 360.0), rng.Range(def.Spawn.OffsetM[0], def.Spawn.OffsetM[1]));
            return Clamp(new Vec2(lm.X, lm.Y) + off);
        }

        private Vec2 Clamp(Vec2 p) =>
            new Vec2(Math.Max(0, Math.Min(_w.Map.WidthM, p.X)), Math.Max(0, Math.Min(_w.Map.HeightM, p.Y)));
    }
}
