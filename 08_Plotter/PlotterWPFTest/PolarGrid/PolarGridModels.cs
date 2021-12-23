// Calculation models for PolarGrid
//
// 2021-12-23   PV

using System;

namespace PlotterWPFTest;

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
