// PointD
// Helper class for smoothing
//
// 2021-12-11   PV  from https://github.com/xstos/PolylineSmoothCSharp.git

namespace LSystemTest;

internal class PointD
{
    public float X = 0;
    public float Y = 0;

    public PointD(float nx, float ny) => (X, Y) = (nx, ny);

    public PointD(PointD p) => (X, Y) = (p.X, p.Y);

    public static PointD operator +(PointD p1, PointD p2) => new(p1.X + p2.X, p1.Y + p2.Y);

    public static PointD operator +(PointD p, float d) => new(p.X + d, p.Y + d);

    public static PointD operator +(float d, PointD p) => p + d;

    public static PointD operator -(PointD p1, PointD p2) => new(p1.X - p2.X, p1.Y - p2.Y);

    public static PointD operator -(PointD p, float d) => new(p.X - d, p.Y - d);

    public static PointD operator -(float d, PointD p) => p - d;

    public static PointD operator *(PointD p1, PointD p2) => new(p1.X * p2.X, p1.Y * p2.Y);

    public static PointD operator *(PointD p, float d) => new(p.X * d, p.Y * d);

    public static PointD operator *(float d, PointD p) => p * d;

    public static PointD operator /(PointD p1, PointD p2) => new(p1.X / p2.X, p1.Y / p2.Y);

    public static PointD operator /(PointD p, float d) => new(p.X / d, p.Y / d);

    public static PointD operator /(float d, PointD p) => new(d / p.X, d / p.Y);
}
