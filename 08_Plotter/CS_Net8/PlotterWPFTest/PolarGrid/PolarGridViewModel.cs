// PolarGrid ViewModel
// Parameters of PolarGrid chart bound to PolarGridUserControl
//
// 2021-12-17    PV

using PlotterLibrary;
using System;
using System.Collections.Generic;

namespace PlotterWPFTest;

internal class PolarGridViewModel: BaseViewModel
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
        get => _LargeGrid;
        set => SetProperty(ref _LargeGrid, value);
    }
    private int _LargeGrid = 10;

    public int SmallGrid
    {
        get => _SmallGrid;
        set => SetProperty(ref _SmallGrid, value);
    }
    private int _SmallGrid = 100;

    public List<PolarGridModelBase> Models => _ModelsList;
    internal List<PolarGridModelBase> _ModelsList = [];

    private PolarGridModelBase? _SelectedModel;
    public PolarGridModelBase? SelectedModel { get => _SelectedModel; set => SetProperty(ref _SelectedModel, value); }

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

    /// <summary>
    /// Rotation control
    /// </summary>
    public double K2
    {
        get
        {
            if (_K2 < _K2Min)
                return _K2Min;
            if (_K2 > _K2Max)
                return _K2Max;
            return _K2;
        }
        set => SetProperty(ref _K2, value);
    }
    private double _K2 = 1.0;

    public bool K2Enabled { get => _K2Enabled; set => SetProperty(ref _K2Enabled, value); }
    private bool _K2Enabled = true;

    public double K2Min { get => _K2Min; set => SetProperty(ref _K2Min, value); }
    public double K2Max { get => _K2Max; set => SetProperty(ref _K2Max, value); }
    private double _K2Min, _K2Max;

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
