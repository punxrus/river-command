namespace RiverCommand.Sim
{
    /// <summary>
    /// One queued order. Orders run in sequence; when one completes the queue advances.
    /// Orders with a Waypoint draw as a node on the map that the player can drag.
    /// </summary>
    public abstract class Order
    {
        public bool IsComplete { get; protected set; }
        /// <summary>Map position this order goes to, if any (used for order lines and drag editing).</summary>
        public virtual Vec2? Waypoint => null;
        public abstract string Kind { get; }
        internal abstract void Execute(Unit unit, SimWorld world, double dt);
    }

    public sealed class MoveOrder : Order
    {
        public Vec2 Target { get; internal set; }
        public const double ArriveRadiusM = 15;

        public MoveOrder(Vec2 target) { Target = target; }
        public override Vec2? Waypoint => Target;
        public override string Kind => "move";

        internal override void Execute(Unit unit, SimWorld world, double dt)
        {
            if (Movement.StepToward(unit, Target, world, dt)) IsComplete = true;
        }
    }

    /// <summary>Hold position for a time, or until another order replaces it (durationSeconds &lt;= 0).</summary>
    public sealed class HoldOrder : Order
    {
        private readonly double _duration;
        private double _elapsed;

        public HoldOrder(double durationSeconds = 0) { _duration = durationSeconds; }
        public override string Kind => "hold";

        internal override void Execute(Unit unit, SimWorld world, double dt)
        {
            unit.Velocity = Vec2.Zero;
            unit.Status = UnitStatus.Holding;
            unit.BurnFuel(unit.Def.Stats.IdleBurnFraction, dt);
            _elapsed += dt;
            if (_duration > 0 && _elapsed >= _duration) IsComplete = true;
        }
    }

    /// <summary>Go to the bar observation point and report bar conditions.</summary>
    public sealed class BarCheckOrder : Order
    {
        public override string Kind => "bar_check";
        public override Vec2? Waypoint => _point;
        private Vec2? _point;

        internal override void Execute(Unit unit, SimWorld world, double dt)
        {
            _point ??= world.Map.BarObservationPoint;
            if (!Movement.StepToward(unit, _point.Value, world, dt)) return;
            var report = world.Environment.DoBarCheck(world.Clock.Seconds);
            world.Emit(new BarReportedEvent(unit.Id, report));
            IsComplete = true;
        }
    }
}
