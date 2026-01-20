// Calculation models for PolarGrid
//
// 2021-12-23   PV
// 2026-01-20	PV		Net10 C#14

using System;

namespace PlotterWPFTest.PolarGrid;

public abstract class PolarGridModelBase
{
    public abstract string Name { get; }

    public abstract (double, double) Calc(double x, double y, double k1, double k2);
}

public class PolarGridModelPowerLinear: PolarGridModelBase
{
    public override string Name => "r'=r^K1; α'=α+(1-r)K2";

    public override (double, double) Calc(double x, double y, double k1, double k2)
    {
        double r = Math.Sqrt(x * x + y * y);
        double a = Math.Atan2(y, x);
        if (r <= 1)
        {
            r = Math.Pow(r, k1);
            a += (1 - r) * k2;

            x = r * Math.Cos(a);
            y = r * Math.Sin(a);
        }
        return (x, y);
    }
}

public class PolarGridModel2: PolarGridModelBase
{
    public override string Name => "Model 2";

    public override (double, double) Calc(double x, double y, double k1, double k2)
    {
        double r = Math.Sqrt(x * x + y * y);
        double a = Math.Atan2(y, x);
        if (a < 0)
            a += 2 * Math.PI;

        if (r <= 1)
        {
            r = Math.Pow(r, k1);
            a += (1 - Math.Cos(2 * (1 - r) * Math.PI)) * k2 / 4;

            x = r * Math.Cos(a);
            y = r * Math.Sin(a);
        }
        return (x, y);
    }
}
