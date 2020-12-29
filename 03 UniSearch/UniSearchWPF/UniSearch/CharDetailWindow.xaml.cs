// CharDetailWindow
// Show more information on a character after a click on hyperlink
//
// 2018-09-15   PV
// 2020-11-11   PV      1.3 Hyperlinks to block and subheader
// 2020-12-29   PV      Break large main grid in 6 smaller grids; Scripts

using System.Windows;
using System.Windows.Input;
using UniDataNS;

#nullable enable


namespace UniSearchNS
{
    internal sealed partial class CharDetailWindow : Window
    {
        internal CharDetailWindow(int codepoint, ViewModel mainViewModel)
        {
            InitializeComponent();

            DataContext = new CharDetailViewModel(this, UniData.CharacterRecords[codepoint], mainViewModel);

            // Esc closes the window
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };
        }

        public static void ShowDetail(int codepoint, ViewModel mainViewModel)
        {
            var w = new CharDetailWindow(codepoint, mainViewModel);
            w.ShowDialog();
        }
    }
}

