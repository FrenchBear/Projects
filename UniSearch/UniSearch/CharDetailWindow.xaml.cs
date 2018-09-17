// CharDetailWindow
// Show more information on a character after a double-click on the list
//
// 2018-09-15   PV


using System.Windows;
using System.Windows.Input;
using UniDataNS;

namespace UniSearchNS
{
    internal sealed partial class CharDetailWindow : Window
    {
        internal CharDetailWindow(int codepoint)
        {
            InitializeComponent();

            DataContext = new CharDetailViewModel(UniData.CharacterRecords[codepoint]);

            // Esc closes the window
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };
        }

        public static void ShowDetail(int codepoint)
        {
            var w = new CharDetailWindow(codepoint);
            w.ShowDialog();
        }
    }
}

