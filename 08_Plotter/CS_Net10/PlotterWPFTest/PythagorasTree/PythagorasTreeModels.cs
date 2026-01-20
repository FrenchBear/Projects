// Calculation models for PythagorasTree
//
// 2021-12-23   PV
// 2026-01-20	PV		Net10 C#14

using System;
using System.Diagnostics;
using System.Numerics;

namespace PlotterWPFTest.PythagorasTree;

public static class PythagorasTreeModel
{
    public static Vector2 RotateVector2(Vector2 v, float angle)
        => Vector2.Transform(v, Matrix3x2.CreateRotation(angle));

    public static Vector2 RotateVector2HalfPi(Vector2 v)
        => RotateVector2(v, (float)Math.PI / 2);

    public static Vector2 GetPTNewPoint(Vector2 A, Vector2 B, float x)
    {
        var p = A + (B - A) * x;
        var P = p + RotateVector2(Vector2.Multiply(B - A, (float)Math.Sqrt(x * (1 - x))), (float)Math.PI / 2);
        return P;
    }

    public static (Vector2, Vector2) GetSquareCD(Vector2 A, Vector2 B)
    {
        Vector2 bottomSide = B - A;
        Vector2 C = B + RotateVector2HalfPi(bottomSide);
        Vector2 D = C - bottomSide;
        return (C, D);
    }

    internal static void Test()
    {
        var A = new Vector2(0, 0);
        var B = new Vector2(2, 1);
        float x = 0.5f;
        var P = GetPTNewPoint(A, B, x);

        Debug.WriteLine(A);
        Debug.WriteLine(B);
        Debug.WriteLine(P);
        Debugger.Break();
    }
}
