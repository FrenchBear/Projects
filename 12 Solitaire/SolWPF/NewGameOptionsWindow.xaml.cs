// NewGameOotionsWindow
// A quick-and-dirty WPF window to enter options for a new game
//
// 2019-04-21   PV
//
// ToDo: Validate numeric input, input range, better visual

using System.Windows;


namespace SolWPF
{
    public partial class NewGameOptionsWindow : Window
    {
        public NewGameOptionsWindow(NewGameOptionsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            GameSerialTextBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
