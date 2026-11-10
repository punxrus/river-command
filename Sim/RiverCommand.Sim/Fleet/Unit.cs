namespace RiverCommand.Sim
{
    public enum UnitStatus { Moored, Underway, Holding, WaitingOnBar, OutOfFuel }

    /// <summary>A live unit in the simulation.</summary>
    public sealed class Unit
    {
        public int Id { get; }
        public UnitDef Def { get; }
        public Vec2 Position { get; internal set; }
        public Vec2 Velocity { get; internal set; }
        public double FuelGal { get; internal set; }
        public UnitStatus Status { get; internal set; } = UnitStatus.Moored;
        public OrderQueue Orders { get; } = new OrderQueue();

        public Unit(int id, UnitDef def, Vec2 position)
        {
            Id = id; Def = def; Position = position; FuelGal = def.Stats.FuelCapacityGal;
        }

        public double CruiseSpeedMps => Conv.KnotsToMps(Def.Stats.CruiseSpeedKts);
        public double FuelFraction => Def.Stats.FuelCapacityGal <= 0 ? 0 : FuelGal / Def.Stats.FuelCapacityGal;

        /// <summary>Hours of cruising left on current fuel.</summary>
        public double EnduranceHours => Def.Stats.BurnGalPerHourCruise <= 0 ? double.PositiveInfinity
            : FuelGal / Def.Stats.BurnGalPerHourCruise;

        internal void BurnFuel(double fractionOfCruise, double dtSeconds)
        {
            FuelGal -= Def.Stats.BurnGalPerHourCruise * fractionOfCruise * dtSeconds / 3600.0;
            if (FuelGal <= 0) { FuelGal = 0; Status = UnitStatus.OutOfFuel; Velocity = Vec2.Zero; }
        }
    }
}
