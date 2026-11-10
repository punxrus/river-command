using System;

namespace RiverCommand.Sim
{
    /// <summary>2D position or vector in meters. X = east, Y = north.</summary>
    public readonly struct Vec2 : IEquatable<Vec2>
    {
        public readonly double X;
        public readonly double Y;

        public Vec2(double x, double y) { X = x; Y = y; }

        public static readonly Vec2 Zero = new Vec2(0, 0);

        public double Length => Math.Sqrt(X * X + Y * Y);
        public Vec2 Normalized => Length < 1e-9 ? Zero : this / Length;

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, double s) => new Vec2(a.X * s, a.Y * s);
        public static Vec2 operator /(Vec2 a, double s) => new Vec2(a.X / s, a.Y / s);

        public static double Distance(Vec2 a, Vec2 b) => (a - b).Length;

        /// <summary>Rotate counter-clockwise by angle in radians.</summary>
        public Vec2 Rotate(double radians)
        {
            double c = Math.Cos(radians), s = Math.Sin(radians);
            return new Vec2(X * c - Y * s, X * s + Y * c);
        }

        public bool Equals(Vec2 other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object obj) => obj is Vec2 v && Equals(v);
        public override int GetHashCode() => X.GetHashCode() * 397 ^ Y.GetHashCode();
        public override string ToString() => $"({X:F1}, {Y:F1})";
    }
}
