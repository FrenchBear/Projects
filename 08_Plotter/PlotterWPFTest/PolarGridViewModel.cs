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
    public double K1 { get => _K1; set => SetProperty(ref _K1, value); }
    private double _K1 = 1.0;

    // ================================================================================

    public override void PlotChart()
    {
        Plotter p = View.MyPlotter;

        p.Clear();

        p.DrawCircle(0, 0, 5);
        p.DrawCircle(0, 0, 6);
        p.DrawCircle(0, 0, 6.2f);

        p.Refresh();
    }
}
