namespace RiverCommand.Sim
{
    /// <summary>
    /// Deterministic PRNG (xorshift64*). The only source of randomness allowed in the sim.
    /// Its state is part of the save file.
    /// </summary>
    public sealed class SeededRandom
    {
        private ulong _state;

        public SeededRandom(ulong seed) { _state = seed == 0 ? 0x9E3779B97F4A7C15UL : seed; }

        public ulong State => _state;

        public ulong NextULong()
        {
            _state ^= _state >> 12;
            _state ^= _state << 25;
            _state ^= _state >> 27;
            return _state * 0x2545F4914F6CDD1DUL;
        }

        /// <summary>Uniform double in [0, 1).</summary>
        public double NextDouble() => (NextULong() >> 11) * (1.0 / (1UL << 53));

        /// <summary>Uniform int in [minInclusive, maxExclusive).</summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            return minInclusive + (int)(NextULong() % (ulong)(maxExclusive - minInclusive));
        }

        public double Range(double min, double max) => min + (max - min) * NextDouble();

        public bool Chance(double probability) => NextDouble() < probability;
    }
}
