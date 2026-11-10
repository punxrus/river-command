using System;
using System.Collections.Generic;

namespace RiverCommand.Sim
{
    /// <summary>
    /// Search pattern tied to an incident. Waypoints are laid out on the moving water: they shift with
    /// the incident's drift, as real SAR patterns are planned around a drifting datum. When the target is
    /// located the unit breaks off the pattern and goes straight to it, then stays until the case closes.
    /// </summary>
    public sealed class SearchOrder : Order
    {
        public int IncidentId { get; }
        private readonly List<Vec2> _pattern;
        private Vec2 _anchorAtIssue;
        private bool _anchored;
        private int _next;
        private readonly RespondOrder _respond;

        public SearchOrder(int incidentId, IEnumerable<Vec2> pattern)
        {
            IncidentId = incidentId;
            _pattern = new List<Vec2>(pattern ?? throw new ArgumentNullException(nameof(pattern)));
            _respond = new RespondOrder(incidentId);
        }

        public override string Kind => "search";
        public IReadOnlyList<Vec2> Pattern => _pattern;
        public int NextLeg => _next;
        public override Vec2? Waypoint => _next < _pattern.Count ? _pattern[_next] + _driftSinceIssue : (Vec2?)null;
        private Vec2 _driftSinceIssue;

        internal override void Execute(Unit unit, SimWorld world, double dt)
        {
            var inc = world.Incidents.Get(IncidentId);
            if (inc == null || !inc.IsOpen) { IsComplete = true; return; }
            if (!_anchored) { _anchorAtIssue = inc.Grid.Center; _anchored = true; }
            _driftSinceIssue = inc.Grid.Center - _anchorAtIssue;

            if (inc.State != IncidentState.Reported || _next >= _pattern.Count)
            {
                _respond.Execute(unit, world, dt);
                if (_respond.IsComplete) IsComplete = true;
                return;
            }

            if (Movement.StepToward(unit, _pattern[_next] + _driftSinceIssue, world, dt)) _next++;
        }
    }
}
