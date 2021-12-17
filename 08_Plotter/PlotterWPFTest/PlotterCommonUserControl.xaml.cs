// PlotterCommonUserControl
// UC for common plotter properties such as pen width, pen color or printer
//
// 2021-12-17   PV

using System.Diagnostics;
using System.Windows.Controls;

namespace PlotterWPFTest;
/// <summary>
/// Interaction logic for PlotterCommonUserControl.xaml
/// </summary>
public partial class PlotterCommonUserControl: UserControl
{
    public PlotterCommonUserControl()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is PlotterCommonViewModel pcvm && pcvm.Plotter is not null && !string.IsNullOrEmpty(pcvm.SelectedPrinter))
            Debug.WriteLine($"Print to <{pcvm.SelectedPrinter}>");
    }
}
