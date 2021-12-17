// PlotterWPFTest MainWindow
// Example of WPF window containing a WindowsForms host control itself containing a picture box to use as plotting surface for PlotterLibrary
//
// 2021-12-17   PV

using System.Windows;
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
        //MyPage.MyPlotter = p;
        //Loaded += MainWindow_Loaded;
        SizeChanged += MainWindow_SizeChanged;
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) 
        => p?.Refresh();

    //private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    //    => MyPage.PlotChart();

    private void SpirographButton_Click(object sender, RoutedEventArgs e)
    {
        var ctl = new SpirographUserControl();
        DrawControlBorder.Child = ctl;
        ctl.MyPlotter = p;
        ctl.PlotChart();
    }

    private void PolarGridButton_Click(object sender, RoutedEventArgs e)
    {
        var ctl = new PolarGridUserControl();
        DrawControlBorder.Child = ctl;
        ctl.MyPlotter = p;
        ctl.PlotChart();
    }
}
