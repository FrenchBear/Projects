// Spirograph ViewModel
// Parameters of Spirograph chart bound to SpirographUserControl
//
//2021-12-17    PV

using PlotterLibrary;
using System;

namespace PlotterWPFTest;

internal class SpirographViewModel: BaseViewModel
{
    private readonly SpirographUserControl View;
    public readonly PlotterCommonViewModel Pcvm;

    public SpirographViewModel(SpirographUserControl view)
    {
        View = view;
        Pcvm = new(PlotChart);
    }

    // ================================================================================

        /// <summary>
        /// Fixed wheel teeth count
        /// </summary>
    public int Z1
    {
        get => _Z1;
        set => SetProperty(ref _Z1, value);
    }
    private int _Z1 = 60;

    /// <summary>
    /// Mobile wheel teeth count
    /// </summary>
    public int Z2
    {
        get => _Z2;
        set => SetProperty(ref _Z2, value);
    }
    private int _Z2 = 45;

    /// <summary>
    /// Position of pen on mobile wheel radius, 0=center, 1=edge
    /// </summary>
    public double K2 { get => _K2; set => SetProperty(ref _K2, value); }
    private double _K2 = 0.8;

    // ================================================================================

    public override void PlotChart()
    {
        Plotter p = View.MyPlotter;
        if (p== null)
            return;

        p.Clear();

        double k2 = K2;      // Position of pen on wheel 2 as a percentage of wheel 2 radius, 0=center, 1=edge, but can be byond [0..1] range
        double r1 = Z1 / (2 * Math.PI);
        double r2 = Z2 / (2 * Math.PI);
        int zper = PPMC(Z1, Z2);

        p.ScaleP1P2((float)(-r1 - (1 + Math.Abs(k2)) * r2 - 1), (float)(-r1 - (1 + Math.Abs(k2)) * r2 - 1), (float)(r1 + (1 + Math.Abs(k2)) * r2 + 1), (float)(r1 + (1 + Math.Abs(k2)) * r2 + 1));

        p.PenColor(7);
        p.DrawCircle(0, 0, (float)r1);
        p.PenColor(8);
        p.DrawCircle((float)(r1 + r2), 0, (float)r2);
        p.PenWidth(5);
        p.DrawCircle((float)(r1 + r2 + r2 * k2), 0, 0.5f);

        p.PenColor(0);
        p.Text(0, 0, $"z1={Z1} r1={r1:F2}\r\nz2={Z2} r2={r2:F2}");

        p.PenColor(Pcvm.PenColorIndex);
        p.PenWidth(Pcvm.PenWidth);

        int kz = 1;
        if (Z1 < 20 || Z2 < 20)
            kz = 2;
        if (Z1 < 10 || Z2 < 10)
            kz = 5;
        if (Z1 <= 5 || Z2 < 5)
            kz = 20;
        for (int z = 0; z <= 2 * kz * zper; z++)
        {
            // a1 = angle of rotation of contact point wheel 1/wheel 2
            double a1 = z / (kz * (double)Z1) * Math.PI;

            // a2 = weel 2 proper angle of rotation (real angle of rotation of wheel 2 is a1+a2)
            double a2 = a1 * Z1 / Z2;

            // Center of wheel 2
            double xc2 = (r1 + r2) * Math.Cos(a1);
            double yc2 = (r1 + r2) * Math.Sin(a1);

            // Pen position
            double xp = xc2 + r2 * k2 * Math.Cos(a1 + a2);
            double yp = yc2 + r2 * k2 * Math.Sin(a1 + a2);

            p.Plot((float)xp, (float)yp);
        }

        p.Refresh();
    }

    /// <summary>
    /// Return Greatest Common Divisor using Euclidean Algorithm (Plus Grand Diviseur Commun)
    /// </summary>
    private static int PGDC(int a, int b)
    {
        if (a <= 0 || b <= 0)
            throw new ArgumentException("Negative or zero argument not supported");
        while (b != 0)
            (a, b) = (b, a % b);
        return a;
    }

    /// <summary>
    /// Smallest Common Multiple (Plus Petit Multiple Commun)
    /// </summary>
    private static int PPMC(int a, int b)
        => a * b / PGDC(a, b);
}
