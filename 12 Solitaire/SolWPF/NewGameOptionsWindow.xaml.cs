// Solitaire WPF
// NewGameOptionsWindow
// A quick-and-dirty WPF window to enter options for a new game
//
// 2019-04-21   PV

using System.Windows;


namespace SolWPF
{
    public partial class NewGameOptionsWindow : Window
    {
        public NewGameOptionsWindow(NewGameOptionsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            Loaded += (s, e) =>
            {
                GameSerialTextBox.Focus();
                GameSerialTextBox.SelectAll();
            };
        }

        //private void OKButton_Click(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = true;
        //}

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
