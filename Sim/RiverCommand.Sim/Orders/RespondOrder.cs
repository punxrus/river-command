namespace RiverCommand.Sim
{
    /// <summary>
    /// Respond to an incident: head for the best known position (the drifting reported position until
    /// the target is located, then the target itself) and stay with it until the case closes.
    /// </summary>
    public sealed class RespondOrder : Order
    {
        public int IncidentId { get; }
        private Vec2? _lastTarget;

        public RespondOrder(int incidentId) { IncidentId = incidentId; }
        public override string Kind => "respond";
        public override Vec2? Waypoint => _lastTarget;

        internal override void Execute(Unit unit, SimWorld world, double dt)
        {
            var inc = world.Incidents.Get(IncidentId);
            if (inc == null || !inc.IsOpen) { IsComplete = true; return; }

            bool located = inc.State != IncidentState.Reported;
            var target = located ? inc.TruePosition : inc.Grid.Center;
            _lastTarget = target;

            if (Movement.StepToward(unit, target, world, dt))
            {
                unit.Status = UnitStatus.Holding;
                unit.BurnFuel(unit.Def.Stats.IdleBurnFraction, dt);
                // Not located yet: arriving at the datum does not finish the order; the player
                // should queue a search pattern. Located: stay on scene until the case closes.
            }
        }
    }
}
