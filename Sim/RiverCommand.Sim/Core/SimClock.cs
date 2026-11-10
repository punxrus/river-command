namespace RiverCommand.Sim
{
    /// <summary>Fixed-step simulation clock. 10 ticks per simulated second.</summary>
    public sealed class SimClock
    {
        public const int TicksPerSecond = 10;
        public const double TickSeconds = 1.0 / TicksPerSecond;

        public long Tick { get; private set; }

        /// <summary>Seconds since the scenario started.</summary>
        public double Seconds => Tick * TickSeconds;

        /// <summary>Scenario start, as hours after local midnight (e.g. 6.0 = 06:00).</summary>
        public double StartHourOfDay { get; }

        public SimClock(double startHourOfDay = 6.0) { StartHourOfDay = startHourOfDay; }

        public double HourOfDay => (StartHourOfDay + Seconds / 3600.0) % 24.0;

        internal void Advance() => Tick++;
        internal void Restore(long tick) => Tick = tick;
    }
}
