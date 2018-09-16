// CharDetailWindow
// Show more information on a character after a double-click on the list
//
// 2018-09-15   PV


using System.Windows;
using System.Windows.Input;

namespace UniSearchNS
{
    /// <summary>
    /// Interaction logic for CharDetailWindow.xaml
    /// </summary>
    public partial class CharDetailWindow : Window
    {
        internal CharDetailWindow(ViewModel vm)
        {
            InitializeComponent();

            // Use same DataContext than main window to keep things simple
            DataContext = vm;

            // Esc closes the window
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };
        }
    }
}
