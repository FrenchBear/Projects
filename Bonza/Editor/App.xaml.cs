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
        internal static readonly Brush NormalValidBackground = Brushes.Black;
        internal static readonly Brush NormalValidForeground = Brushes.White;

        internal static readonly Brush NormalTooCloseBackground = Brushes.SlateGray;
        internal static readonly Brush NormalTooCloseForeground = Brushes.White;

        internal static readonly Brush NormalInvalidBackground = Brushes.DarkRed;
        internal static readonly Brush NormalInvalidForeground = Brushes.White;

        internal static readonly Brush SelectedValidBackground = Brushes.DarkBlue;
        internal static readonly Brush SelectedValidForeground = Brushes.White;

        internal static readonly Brush SelectedTooCloseBackground = Brushes.MediumVioletRed;
        internal static readonly Brush SelectedTooCloseForeground = Brushes.White;

        internal static readonly Brush SelectedInvalidBackground = Brushes.DarkRed;
        internal static readonly Brush SelectedInvalidForeground = Brushes.White;

    }
}
