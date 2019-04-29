// CharDetailDialog
// Show more information on a character after a click on hyperlink
//
// In UWP version, can't show more than one ContentDialog on screen: drill-down explore
// is handled by recycling current window and implement a back navigation mechanism
//
// 2018-09-18   PV


using System;
using System.Threading.Tasks;
using UniDataNS;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace UniSearchUWPNS
{
    public sealed partial class CharDetailDialog : ContentDialog
    {
        readonly CharDetailViewModel ViewModel;


        internal CharDetailDialog(int codepoint)
        {
            InitializeComponent();

            ViewModel = new CharDetailViewModel(this, UniData.CharacterRecords[codepoint]);
            DataContext = ViewModel;

            Loaded += (s, e) =>
            {
                DefaultButton = ContentDialogButton.None;
                CloseButton.Focus(FocusState.Programmatic);
            };

            // Esc closes the window automatically, nothing to do
        }

        // Static function for easy opening
        public async static Task ShowDetail(int codepoint)
        {
            var w = new CharDetailDialog(codepoint);
            await w.ShowAsync();
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Back();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CharacterRecord cr = ViewModel.SelectedChar;
            string s = $"Character\t{cr.Character}\r\nCodepoint\t{cr.CodepointHex}\r\nName\t{cr.Name}\r\nCategories\t{cr.CategoryRecord.Categories}\r\nAge\t{cr.Age}\r\nBlock\t{cr.Block.BlockNameAndRange}\r\nSubheader\t{cr.Subheader}\r\nUTF-16\t{cr.UTF16}\r\nUTF-8\t{cr.UTF8}\r\n";
            UniSearchUWPNS.ViewModel.ClipboardSetData(s);
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
