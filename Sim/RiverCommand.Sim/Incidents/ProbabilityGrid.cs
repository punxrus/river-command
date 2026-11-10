using System;

namespace RiverCommand.Sim
{
    /// <summary>
    /// Where the player thinks the target is. Starts as a Gaussian around the reported position
    /// (probability of area, POA) and drifts with the current. Searching a cell raises its cumulative
    /// probability of detection (POD). Remaining probability per cell = POA x (1 - POD).
    /// This is the heat map the HUD draws; actual detection is rolled against the hidden true position.
    /// </summary>
    public sealed class ProbabilityGrid
    {
        public int Size { get; }
        public double CellM { get; }
        /// <summary>World position of the grid center. Moves with drift.</summary>
        public Vec2 Center { get; private set; }

        private readonly double[] _poa;
        private readonly double[] _pod;

        public ProbabilityGrid(Vec2 center, double sigmaM, double cellM, int halfSizeCells)
        {
            Size = halfSizeCells * 2 + 1;
            CellM = cellM;
            Center = center;
            _poa = new double[Size * Size];
            _pod = new double[Size * Size];
            double sum = 0, s2 = 2 * sigmaM * sigmaM;
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                double dx = (x - halfSizeCells) * cellM, dy = (y - halfSizeCells) * cellM;
                double v = Math.Exp(-(dx * dx + dy * dy) / s2);
                _poa[y * Size + x] = v;
                sum += v;
            }
            for (int i = 0; i < _poa.Length; i++) _poa[i] /= sum;
        }

        public void Drift(Vec2 delta) => Center += delta;

        public Vec2 CellCenter(int x, int y)
        {
            int h = Size / 2;
            return Center + new Vec2((x - h) * CellM, (y - h) * CellM);
        }

        public double Poa(int x, int y) => _poa[y * Size + x];
        public double Pod(int x, int y) => _pod[y * Size + x];
        public double Remaining(int x, int y) => _poa[y * Size + x] * (1 - _pod[y * Size + x]);

        /// <summary>Sum of remaining probability. 1.0 = nothing searched; near 0 = area well covered.</summary>
        public double TotalRemaining()
        {
            double t = 0;
            for (int i = 0; i < _poa.Length; i++) t += _poa[i] * (1 - _pod[i]);
            return t;
        }

        /// <summary>
        /// A searcher at `position` covers cells within `radiusM` with per-tick detection chance `p`.
        /// POD accumulates: pod' = 1 - (1 - pod)(1 - p).
        /// </summary>
        public void Sweep(Vec2 position, double radiusM, double p)
        {
            int h = Size / 2;
            var local = position - Center;
            int cx = (int)Math.Round(local.X / CellM) + h, cy = (int)Math.Round(local.Y / CellM) + h;
            int r = (int)Math.Ceiling(radiusM / CellM);
            for (int y = Math.Max(0, cy - r); y <= Math.Min(Size - 1, cy + r); y++)
            for (int x = Math.Max(0, cx - r); x <= Math.Min(Size - 1, cx + r); x++)
            {
                if (Vec2.Distance(CellCenter(x, y), position) > radiusM) continue;
                int i = y * Size + x;
                _pod[i] = 1 - (1 - _pod[i]) * (1 - p);
            }
        }
    }
}
