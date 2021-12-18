// PolarGrid ViewModel
// Parameters of PolarGrid chart bound to PolarGridUserControl
//
// 2021-12-17    PV

using PlotterLibrary;
using System;

namespace PlotterWPFTest;

internal class PolarGridViewModel: BaseViewModel
{
    private readonly PolarGridUserControl View;
    public readonly PlotterCommonViewModel Pcvm;

    public PolarGridViewModel(PolarGridUserControl view)
    {
        View = view;
        Pcvm = new(PlotChart);
        SetRange("K1", true, 1.0, 0.1, 5.0);
    }

    internal void SetRange(string k, bool isEnabled, double v, double vMin, double vMax)
    {
        switch (k)
        {
            case "K1":
                K1Enabled = isEnabled;
                K1Min = vMin;
                K1Max = vMax;
                K1 = v;
                break;
        }
    }

    // ================================================================================

    public int LargeGrid
    {
        get => _LargeGrid;
        set => SetProperty(ref _LargeGrid, value);
    }
    private int _LargeGrid = 10;

    /// <summary>
    /// Mobile wheel teeth count
    /// </summary>
    public int SmallGrid
    {
        get => _SmallGrid;
        set => SetProperty(ref _SmallGrid, value);
    }
    private int _SmallGrid = 100;

    /// <summary>
    /// Position of pen on mobile wheel radius, 0=center, 1=edge
    /// </summary>
    public double K1
    {
        get
        {
            if (_K1 < _K1Min)
                return _K1Min;
            if (_K1 > _K1Max)
                return _K1Max;
            return _K1;
        }
        set => SetProperty(ref _K1, value);
    }
    private double _K1 = 1.0;

    public bool K1Enabled { get => _K1Enabled; set => SetProperty(ref _K1Enabled, value); }
    private bool _K1Enabled = true;

    public double K1Min { get => _K1Min; set => SetProperty(ref _K1Min, value); }
    public double K1Max { get => _K1Max; set => SetProperty(ref _K1Max, value); }
    private double _K1Min, _K1Max;

    // ================================================================================

    public override void PlotChart()
    {
        Plotter p = View.MyPlotter;
        if (p == null)
            return;

        void CalcAndPlot(double x, double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double a = Math.Atan2(y, x);
            if (r <= 1)
            {
                r = Math.Pow(r, K1);
                x = r * Math.Cos(a);
                y = r * Math.Sin(a);
            }
            p.Plot((float)x, (float)y);
        }

        p.Clear();
        p.ScaleP1P2(-1.1f, -1.1f, 1.1f, 1.1f);
        p.PenColor(Pcvm.PenColorIndex);
        p.PenWidth(Pcvm.PenWidth);

        for (int i = 0; i <= LargeGrid; i++)
        {
            double x = (2.0 * i - LargeGrid) / LargeGrid;
            p.PenUp();
            for (int j = 0; j <= SmallGrid; j++)
            {
                double y = (2.0 * j - SmallGrid) / SmallGrid;
                CalcAndPlot(x, y);
            }
        }

        for (int i = 0; i <= LargeGrid; i++)
        {
            double y = (2.0 * i - LargeGrid) / LargeGrid;
            p.PenUp();
            for (int j = 0; j <= SmallGrid; j++)
            {
                double x = (2.0 * j - SmallGrid) / SmallGrid;
                CalcAndPlot(x, y);
            }
        }

        p.Refresh();
    }
}
