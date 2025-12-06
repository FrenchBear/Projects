// MainWindow
// Test all displays
//
// 2025-12-06   PV

namespace Displays;

public partial class MainWindow: Window
{
    public MainWindow()
    {
        InitializeComponent();
        //Loaded += (s, e) => Matrix5x7Display.Variant = 1;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e) => throw new System.NotImplementedException();
}
