using System.Windows;

namespace QwirkleUI;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App: Application
{
    internal const string AppName = "QwirkleUI";

    // Size of the side of a square
    // Background grid is drawn using lines of width 1 (or 3 for origin axes) [hardcoded]
    // Font size is 16 and padding are also hardcoded
    internal const double UnitSize = 75.0;

    internal const double MarginSize = 1.0;

    // Player Hand Grid
    internal const int HandRows = 2;
    internal const int HandColumns = 8;
}

