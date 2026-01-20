// PlotterCommonUserControl
// UC for common plotter properties such as pen width, pen color or printer
//
// 2021-12-17   PV
// 2026-01-20	PV		Net10 C#14

using PlotterWPFTest.PlotterCommon;
using System.Windows.Controls;

namespace PlotterWPFTest;

public partial class PlotterCommonUserControl: UserControl
{
    public PlotterCommonUserControl()
        => InitializeComponent();

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is PlotterCommonViewModel pcvm && pcvm.Plotter is not null && !string.IsNullOrEmpty(pcvm.SelectedPrinter))
        {
            //Debug.WriteLine($"Print to <{pcvm.SelectedPrinter}>");
            pcvm.Plotter.Print(pcvm.SelectedPrinter);
        }
    }
}
