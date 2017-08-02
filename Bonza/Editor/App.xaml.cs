using System.Windows;
using System.Windows.Media;

namespace Bonza.Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static readonly string AppName = "BonzaEditor";

        // Size of the side of a square
        // Background grid is drawn using lines of width 1 (or 3 for origin axes) [hardcoded]
        // Font size is 16 and padding are also hardcoded
        internal static readonly double UnitSize = 25.0;

        // Some colors
        internal static readonly Brush NormalBackgroundBrush = Brushes.Black;
        internal static readonly Brush NormalForegroundBrush = Brushes.White;

        internal static readonly Brush SelectedBackgroundBrush = Brushes.DarkBlue;
        internal static readonly Brush SelectedForegroundBrush = Brushes.White;

        internal static readonly Brush ProblemBackgroundBrush = Brushes.DarkRed;
        internal static readonly Brush ProblemForegroundBrush = Brushes.White;

    }
}
