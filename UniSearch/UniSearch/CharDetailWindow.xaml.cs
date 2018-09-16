// CharDetailWindow
// Show more information on a character after a double-click on the list
//
// 2018-09-15   PV


using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UniData;
using DirectDrawWrite;


namespace UniSearchNS
{
    public partial class CharDetailWindow : Window
    {
        internal CharDetailWindow(int codepoint)
        {
            InitializeComponent();

            DataContext = new CharDetailViewModel { SelectedChar = UnicodeData.CharacterRecords[codepoint] };

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

    internal class CharDetailViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public CharacterRecord SelectedChar { get; set; }

        public BitmapSource SelectedCharImage
        {
            get
            {
                if (SelectedChar == null)
                    return null;
                else
                {
                    string s = SelectedChar.Character;
                    if (s.StartsWith("U+", StringComparison.Ordinal))
                        s = "U+ " + s.Substring(2);
                    return D2DDrawText.GetBitmapSource(s);
                }
            }
        }

    }
}
