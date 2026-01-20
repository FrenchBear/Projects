// PythagorasTree ViewModel
// Parameters of PythagorasTree chart bound to PythagorasTreeUserControl
//
// 2022-06-24    PV
// 2026-01-20	PV		Net10 C#14

using PlotterLibrary;
using PlotterWPFTest.PlotterCommon;
using System.Numerics;

namespace PlotterWPFTest.PythagorasTree;

internal sealed class PythagorasTreeViewModel: BaseViewModel
{
    private readonly PythagorasTreeUserControl View;
    public readonly PlotterCommonViewModel Pcvm;

    public PythagorasTreeViewModel(PythagorasTreeUserControl view)
    {
        View = view;
        Pcvm = new(PlotChart);
    }

    // ================================================================================

    public double X
    {
        get;
        set => SetProperty(ref field, value);
    } = 0.75;

    public int Depth
    {
        get;
        set => SetProperty(ref field, value);
    } = 4;

    // ================================================================================

    public override void PlotChart()
    {
        Plotter p = View.MyPlotter;
        if (p == null)
            return;

        p.Clear();
        //p.PenColor(Pcvm.PenColorIndex);
        p.PenWidth(Pcvm.PenWidth);

        Vector2 A = new(0, 0);
        Vector2 B = new(1, 0);
        DrawPythagorasSquare(A, B, Depth, Pcvm.PenColorIndex);

        p.AutoScale();
        p.Refresh();

        void DrawPythagorasSquare(Vector2 A, Vector2 B, int Depth, int color)
        {
            p.PenColor(color);

            var (C, D) = PythagorasTreeModel.GetSquareCD(A, B);
            p.PenUp();
            p.Plot(A);
            p.Plot(B);
            p.Plot(C);
            p.Plot(D);
            p.Plot(A);
            p.PenUp();

            if (Depth > 1)
            {
                var P = PythagorasTreeModel.GetPTNewPoint(D, C, (float)X);
                var c2 = (color + 1) % p.ColorsTable.Length;
                DrawPythagorasSquare(D, P, Depth - 1, c2);
                DrawPythagorasSquare(P, C, Depth - 1, c2);
            }
        }
    }
}
