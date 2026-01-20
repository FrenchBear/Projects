// PointD
// Helper class for smoothing
//
// 2021-12-11   PV  from https://github.com/xstos/PolylineSmoothCSharp.git
// 2026-01-20	PV		Net10 C#14

namespace LSystemTest;

internal sealed class PointD
{
    public double X;
    public double Y;

    public PointD(double nx, double ny) => (X, Y) = (nx, ny);

    public PointD(PointD p) => (X, Y) = (p.X, p.Y);

    public static PointD operator +(PointD p1, PointD p2) => new(p1.X + p2.X, p1.Y + p2.Y);

    public static PointD operator +(PointD p, double d) => new(p.X + d, p.Y + d);

    public static PointD operator +(double d, PointD p) => p + d;

    public static PointD operator -(PointD p1, PointD p2) => new(p1.X - p2.X, p1.Y - p2.Y);

    public static PointD operator -(PointD p, double d) => new(p.X - d, p.Y - d);

    public static PointD operator -(double d, PointD p) => p - d;

    public static PointD operator *(PointD p1, PointD p2) => new(p1.X * p2.X, p1.Y * p2.Y);

    public static PointD operator *(PointD p, double d) => new(p.X * d, p.Y * d);

    public static PointD operator *(double d, PointD p) => p * d;

    public static PointD operator /(PointD p1, PointD p2) => new(p1.X / p2.X, p1.Y / p2.Y);

    public static PointD operator /(PointD p, double d) => new(p.X / d, p.Y / d);

    public static PointD operator /(double d, PointD p) => new(d / p.X, d / p.Y);

    public override string ToString() => $"PointD({X:F3}, {Y:F3})";
}