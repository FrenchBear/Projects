using PlotterLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlotterWPFTest;
/// <summary>
/// Interaction logic for SirographUserControl.xaml
/// </summary>
public partial class SpirographUserControl: UserControl
{
    readonly SpirographViewModel vm;

    public SpirographUserControl()
    {
        InitializeComponent();
        vm = new SpirographViewModel(this);
        DataContext = vm;
    }

    public Plotter MyPlotter
    {
        get => (Plotter)GetValue(MyPlotterProperty);
        set
        {
            SetValue(MyPlotterProperty, value);
            foreach (var item in value.ColorsTable)
                vm._PenColors.Add(item.ToString());
        }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MyPlotterProperty =
        DependencyProperty.RegisterAttached("MyPlotter", typeof(Plotter), typeof(SpirographViewModel), new PropertyMetadata(null));

    internal void PlotChart()
    {
        Plotter p = MyPlotter;

        p.Clear();

        int z1 = vm.Z1;
        int z2 = vm.Z2;
        double k2 = vm.K2;      // Position of pen on wheel 2 as a percentage of wheel 2 radius, 0=center, 1=edge, but can be byond [0..1] range
        double r1 = z1 / (2 * Math.PI);
        double r2 = z2 / (2 * Math.PI);
        int zper = PPMC(z1, z2);

        p.ScaleP1P2((float)(-r1 - (1 + Math.Abs(k2)) * r2 - 1), (float)(-r1 - (1 + Math.Abs(k2)) * r2 - 1), (float)(r1 + (1 + Math.Abs(k2)) * r2 + 1), (float)(r1 + (1 + Math.Abs(k2)) * r2 + 1));

        p.PenColor(7);
        p.DrawCircle(0, 0, (float)r1);
        p.PenColor(8);
        p.DrawCircle((float)(r1 + r2), 0, (float)r2);
        p.PenWidth(5);
        p.DrawCircle((float)(r1 + r2 + r2 * k2), 0, 0.5f);

        p.PenColor(0);
        p.Text(0, 0, $"z1={z1} r1={r1:F2}\r\nz2={z2} r2={r2:F2}");

        p.PenColor(vm.PenColorIndex);
        p.PenWidth(vm.PenWidth);

        int kz = 1;
        if (z1 < 20 || z2 < 20)
            kz = 2;
        if (z1 < 10 || z2 < 10)
            kz = 5;
        if (z1 <= 5 || z2 < 5)
            kz = 20;
        for (int z = 0; z <= 2 * kz * zper; z++)
        {
            // a1 = angle of rotation of contact point wheel 1/wheel 2
            double a1 = z / (kz * (double)z1) * Math.PI;

            // a2 = weel 2 proper angle of rotation (real angle of rotation of wheel 2 is a1+a2)
            double a2 = a1 * z1 / z2;

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
