namespace RiverCommand.Sim
{
    internal static class Movement
    {
        /// <summary>
        /// Moves the unit toward target at cruise speed for one tick. Returns true on arrival.
        /// Surface units stop short of the bar if breakers exceed their limit.
        /// </summary>
        public static bool StepToward(Unit unit, Vec2 target, SimWorld world, double dt)
        {
            var toTarget = target - unit.Position;
            double dist = toTarget.Length;
            if (dist <= MoveOrder.ArriveRadiusM)
            {
                unit.Velocity = Vec2.Zero;
                return true;
            }

            double step = unit.CruiseSpeedMps * dt;
            var next = step >= dist ? target : unit.Position + toTarget.Normalized * step;

            if (unit.Def.Domain == "surface" && world.Map.Bar.Contains(next) && !world.Map.Bar.Contains(unit.Position))
            {
                var bar = world.Environment.TrueBar(world.Clock.Seconds);
                if (bar.BreakerHeightFt > unit.Def.Limits.MaxBarBreakerFt)
                {
                    if (unit.Status != UnitStatus.WaitingOnBar)
                        world.Emit(new BarBlockedEvent(unit.Id, bar.BreakerHeightFt, unit.Def.Limits.MaxBarBreakerFt));
                    unit.Status = UnitStatus.WaitingOnBar;
                    unit.Velocity = Vec2.Zero;
                    unit.BurnFuel(unit.Def.Stats.IdleBurnFraction, dt);
                    return false;
                }
            }

            unit.Velocity = (next - unit.Position) / dt;
            unit.Position = next;
            unit.Status = UnitStatus.Underway;
            unit.BurnFuel(1.0, dt);
            return step >= dist;
        }
    }
}
