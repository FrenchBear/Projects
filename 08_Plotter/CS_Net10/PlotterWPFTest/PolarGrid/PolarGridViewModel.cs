// PolarGrid ViewModel
// Parameters of PolarGrid chart bound to PolarGridUserControl
//
// 2021-12-17    PV
// 2026-01-20	PV		Net10 C#14

using PlotterLibrary;
using PlotterWPFTest.PlotterCommon;
using System.Collections.Generic;

namespace PlotterWPFTest.PolarGrid;

internal sealed class PolarGridViewModel: BaseViewModel
{
    private readonly PolarGridUserControl View;
    public readonly PlotterCommonViewModel Pcvm;

    public PolarGridViewModel(PolarGridUserControl view)
    {
        View = view;
        Pcvm = new(PlotChart);

        Models.Add(new PolarGridModelPowerLinear());
        Models.Add(new PolarGridModel2());
        SelectedModel = Models[0];

        // ToDo: depends on model selected, but since there's only one model for now...
        SetRange("K1", true, 1.0, 0.1, 5.0);
        SetRange("K2", true, 0.0, -3.14, 3.14);
    }

    internal void SetRange(string k, bool isEnabled, double v = 0, double vMin = 0, double vMax = 1)
    {
        switch (k)
        {
            case "K1":
                K1Enabled = isEnabled;
                K1Min = vMin;
                K1Max = vMax;
                K1 = v;
                break;

            case "K2":
                K2Enabled = isEnabled;
                K2Min = vMin;
                K2Max = vMax;
                K2 = v;
                break;
        }
    }

    // ================================================================================

    public int LargeGrid
    {
        get;
        set => SetProperty(ref field, value);
    } = 10;

    public int SmallGrid
    {
        get;
        set => SetProperty(ref field, value);
    } = 100;

    public List<PolarGridModelBase> Models => _ModelsList;
    internal List<PolarGridModelBase> _ModelsList = [];

    public PolarGridModelBase? SelectedModel { get; set => SetProperty(ref field, value); }

    public double K1
    {
        get => field < K1Min ? K1Min : field > K1Max ? K1Max : field;
        set => SetProperty(ref field, value);
    } = 1.0;

    public bool K1Enabled { get; set => SetProperty(ref field, value); } = true;

    public double K1Min { get; set => SetProperty(ref field, value); }
    public double K1Max { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Rotation control
    /// </summary>
    public double K2
    {
        get => field < K2Min ? K2Min : field > K2Max ? K2Max : field;
        set => SetProperty(ref field, value);
    } = 1.0;

    public bool K2Enabled { get; set => SetProperty(ref field, value); } = true;

    public double K2Min { get; set => SetProperty(ref field, value); }
    public double K2Max { get; set => SetProperty(ref field, value); }

    // ================================================================================

    public override void PlotChart()
    {
        Plotter p = View.MyPlotter;
        if (p == null)
            return;
        if (SelectedModel == null)
            return;

        void CalcAndPlot(double x, double y)
        {
            (x, y) = SelectedModel.Calc(x, y, K1, K2);
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
