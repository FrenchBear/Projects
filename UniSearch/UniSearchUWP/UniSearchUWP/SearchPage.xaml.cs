// UniSearchUWP
// Unicode Character Search Tool, UWP version
// Learning app for UWP model
//
// 2018-09-18   PV


using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace UniSearchUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        ViewModel vm;

        public SearchPage()
        {
            this.InitializeComponent();

            vm = new ViewModel(this);
            DataContext = vm;

            CharacterFilterTextBox.Focus(FocusState.Programmatic);
        }

        private void CheckBox_Flip(object sender, RoutedEventArgs e)
        {
            vm.AfterCheckboxFlip();
        }

        private void CharListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.NotifyPropertyChanged(nameof(vm.SelChars));
        }

        // Some controls such as AutoSuggestBox do not get focused just by defining AccessKey property for some reason.
        // This generic helper launched by AccessKeyInvoked event makes it work.
        private void SetFocus(UIElement sender, Windows.UI.Xaml.Input.AccessKeyInvokedEventArgs args)
        {
            if (sender is Control c)
                c.Focus(FocusState.Keyboard);
        }

        public bool IsGridViewVisible => CharGridView.Visibility == Visibility.Visible;

        public ListViewBase CharCurrentView => IsGridViewVisible ? (ListViewBase)CharGridView : (ListViewBase)CharListView;

        private void ListGrid_Click(object sender, RoutedEventArgs e)
        {
            if (IsGridViewVisible)
            {
                CharGridView.Visibility = Visibility.Collapsed;
                CharListView.Visibility = Visibility.Visible;
                CharListView.ScrollIntoView(vm.SelectedChar);
            }
            else
            {
                CharListView.Visibility = Visibility.Collapsed;
                CharGridView.Visibility = Visibility.Visible;
                CharGridView.ScrollIntoView(vm.SelectedChar);
            }
        }

        private async void GridViewCell_DoubleTap(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            await CharDetailDialog.ShowDetail(vm.SelectedChar.Codepoint);
        }
    }
}
