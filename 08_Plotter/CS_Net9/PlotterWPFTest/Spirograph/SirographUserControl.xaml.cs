// SpirographUserControl
// First UC for PlotterWPFTest combining UI and plotting code
//
// 2021-12-17   PV

using PlotterLibrary;
using PlotterWPFTest.Spirograph;
using System.Windows;
using System.Windows.Controls;

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
        MyPlotterCommonUserControl.DataContext = vm.Pcvm;
        vm.PlotChart();
    }

    public Plotter MyPlotter
    {
        get => (Plotter)GetValue(MyPlotterProperty);
        set
        {
            SetValue(MyPlotterProperty, value);
            vm.Pcvm.InitPlotter(value);
        }
    }

    internal void PlotChart() => vm.PlotChart();

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MyPlotterProperty =
        DependencyProperty.RegisterAttached("MyPlotter", typeof(Plotter), typeof(SpirographViewModel), new PropertyMetadata(null));
}
