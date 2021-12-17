using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        MyPage.MyPlotter = p;
        Loaded += MainWindow_Loaded;
        SizeChanged += MainWindow_SizeChanged;
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) 
        => p?.Refresh();

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        => MyPage.PlotChart();
}
