// CharDetailDialog
// Show more information on a character after a click on hyperlink
//
// In UWP version, can't show more than one ContentDialog on screen: drill-down explore
// is handlled by recycling current window and implement a back mechanism
// ToDo: Use commanding for back to enable/disable the button, or do it manually
//
// 2018-09-18   PV


using System;
using System.Threading.Tasks;
using UniDataNS;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace UniSearchUWP
{
    public sealed partial class CharDetailDialog : ContentDialog
    {
        CharDetailViewModel ViewModel;


        internal CharDetailDialog(int codepoint)
        {
            InitializeComponent();

            ViewModel = new CharDetailViewModel(UniData.CharacterRecords[codepoint]);
            DataContext = ViewModel;

            // Esc closes the window automatically, nothing to do
        }

        // Static function for easy opening
        public async static Task ShowDetail(int codepoint)
        {
            var w = new CharDetailDialog(codepoint);
            await w.ShowAsync();
        }

        private void MyBackButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Back();

        }

        private void MyCloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
