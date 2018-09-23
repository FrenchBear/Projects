// UniSearchUWP
// Unicode Character Search Tool, UWP version
// Learning app for UWP model
//
// 2018-09-18   PV


using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace UniSearchUWPNS
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

            Loaded += (s, e) =>
            {
                BlocksTreeView.SelectAll();
                vm.FilterBlockTree(true);
            };
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


        // Simulate "SelectionChanged" missing event with mouse and key events
        // But this is unreliable, doubletap event gets fired before SelectedNodes has been refreshed...
        private void BlocksTreeView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("BlocksTreeView_Tapped");
            vm.FilterBlockTree();
        }
        private void BlocksTreeView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Debug.WriteLine("BlocksTreeView_DoubleTapped");
            vm.FilterBlockTree();
        }

        private void BlocksTreeView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
                vm.FilterBlockTree();
        }


        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            BlocksTreeView.SelectAll();
            vm.FilterBlockTree();
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Not working for now
        }
    }




    public class BlockGroupItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BlockTemplate { get; set; } 
        public DataTemplate GroupL1Template { get; set; }
        public DataTemplate GroupTemplate { get; set; }

        // Used by binding
        protected override DataTemplate SelectTemplateCore(object item)
        {
            var blockItem = (BlockNode)item;
            if (blockItem.Level == 0) return BlockTemplate;
            if (blockItem.Level == 1) return GroupL1Template;
            return GroupTemplate;
        }
    }

}
