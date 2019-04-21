// GameSerialWindow
// A quick-and-dirty WPF window to enter game number
//
// 2019-04-21   PV
//
// ToDo: Validate numeric input, input range, better visual

using System.Windows;


namespace SolWPF
{
    public partial class GameSerialWindow : Window
    {
        public GameSerialWindow(GameSerialViewModel vm)
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
