// PlotterWPFTest MainWindow
// Example of WPF window containing a WindowsForms host control itself containing a picture box to use as plotting surface for PlotterLibrary
//
// 2021-12-17   PV

using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using PlotterLibrary;

namespace PlotterWPFTest;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow: Window
{
    readonly Plotter p;

    public MainWindow()
    {
        InitializeComponent();
        p = new Plotter();
        p.Output(MyPic);
        SizeChanged += MainWindow_SizeChanged;
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        => p?.Refresh();

    private void SpirographButton_Click(object sender, RoutedEventArgs e)
    {
        var ctl = new SpirographUserControl();
        DrawControlBorder.Child = ctl;
        WFHost.Dispatcher.Invoke(DispatcherPriority.Render, () => { });     // Force layout recalc and resize WFHost.pic
        ctl.MyPlotter = p;
    }

    private void PolarGridButton_Click(object sender, RoutedEventArgs e)
    {
        var ctl = new PolarGridUserControl();
        DrawControlBorder.Child = ctl;
        WFHost.Dispatcher.Invoke(DispatcherPriority.Render, () => { });     // Force layout recalc and resize WFHost.pic
        ctl.MyPlotter = p;
    }

    // For dev
    private void DebugButton_Click(object sender, RoutedEventArgs e) 
        => Debug.WriteLine($"WFHost: W={WFHost.ActualWidth} H={WFHost.ActualHeight}");

    private void PythagorasTreeButton_Click(object sender, RoutedEventArgs e)
    {
        var ctl = new PythagorasTreeUserControl();
        DrawControlBorder.Child = ctl;
        WFHost.Dispatcher.Invoke(DispatcherPriority.Render, () => { });     // Force layout recalc and resize WFHost.pic
        ctl.MyPlotter = p;
    }
}
