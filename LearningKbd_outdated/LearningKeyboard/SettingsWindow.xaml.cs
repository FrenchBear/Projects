using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LearningKeyboard
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // ToDo: Optimize if current color scheme has not changed
            // Apply settings
            if (BrownOption.IsChecked.Value)
                Properties.Settings.Default["ColorScheme"] = "Brown";
            else if (RainbowOption.IsChecked.Value)
            {
                Properties.Settings.Default["ColorScheme"] = "Rainbow";
            }

            Properties.Settings.Default.Save();

            // ToDo: Refresh display

            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Topmost = true;
        }
    }
}
