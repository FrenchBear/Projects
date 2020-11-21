// UniSearchUWP
// Unicode Character Search Tool, UWP version
// Learning app for UWP model
//
// 2018-09-18   PV
// 2020-11-11   PV      nullable enable

using System.Linq;
using System.Reflection;
using UniDataNS;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#nullable enable


namespace UniSearchUWPNS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        readonly ViewModel vm;

        public SearchPage()
        {
            this.InitializeComponent();

            vm = new ViewModel(this);
            DataContext = vm;

            Loaded += (s, e) =>
            {
                BlocksTreeView.SelectAll();
                vm.InitialBlocksUnselect();
                vm.RefreshSelectedBlocks(true);
                CharacterFilterTextBox.Focus(FocusState.Programmatic);

                // Main app info
                (string Title, string Description, string Version, string Copyright) = AboutDialog.GetAppVersion(System.Reflection.Assembly.GetExecutingAssembly());
                AssemblyTitle.Text = Title;
                AssemblyDescription.Text = Description;
                AssemblyVersion.Text = "Version " + Version;
                AssemblyCopyright.Text = Copyright;

                // UniData DLL info
                (string DataTitle, string DataDescription, string DataVersion, string DataCopyright) = AboutDialog.GetAppVersion(typeof(UniData).GetTypeInfo().Assembly);
                UniDataTitle.Text = DataTitle;
                UniDataDescription.Text = DataDescription;
                UniDataVersion.Text = "Version " + DataVersion;
                UniDataCopyright.Text = DataCopyright;

            };
        }

        private void CharListOrGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.NotifyPropertyChanged(nameof(vm.SelChars));
        }

        // Some controls such as AutoSuggestBox do not get focused just by defining AccessKey property for some reason.
        // This generic helper launched by AccessKeyInvoked event makes it work.
        // Must remain non-static despite suggestions since it's dynamically called from XAML
        private void SetFocus(UIElement sender, Windows.UI.Xaml.Input.AccessKeyInvokedEventArgs args)
        {
            if (sender is Control c)
                c.Focus(FocusState.Keyboard);
        }

        public bool IsGridViewVisible => CharSemanticView.Visibility == Visibility.Visible;

        public ListViewBase CharCurrentView => IsGridViewVisible ? (ListViewBase)CharGridView : (ListViewBase)CharListView;

        private void ListGrid_Click(object sender, RoutedEventArgs e)
        {
            if (IsGridViewVisible)
            {
                CharSemanticView.Visibility = Visibility.Collapsed;
                CharListView.Visibility = Visibility.Visible;
                CharListView.ScrollIntoView(vm.SelectedChar);
            }
            else
            {
                CharListView.Visibility = Visibility.Collapsed;
                CharSemanticView.Visibility = Visibility.Visible;
                CharGridView.ScrollIntoView(vm.SelectedChar);
            }
        }

        private async void GridViewCell_DoubleTap(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (vm.SelectedChar != null)
                await CharDetailDialog.ShowDetail(vm.SelectedChar.Codepoint, vm);
        }

        // Show details when pressing Enter of Space
        private async void GridViewCell_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space || e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                if (vm.SelectedChar != null)
                    await CharDetailDialog.ShowDetail(vm.SelectedChar.Codepoint, vm);
            }
        }


        // Select character on right-click
        private void CharGridView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is CharacterRecord cr)
            {
                // Select char if it's not in current selection
                if (!CharCurrentView.SelectedItems.Cast<CharacterRecord>().Contains(cr))
                    vm.SelectedChar = cr;
            }
        }


        // Simulate "SelectionChanged" missing event with mouse and key events
        // But this is unreliable, doubletap event gets fired before SelectedNodes has been refreshed...
        private void BlocksTreeView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            vm.RefreshSelectedBlocks(true);
        }
        private void BlocksTreeView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            vm.RefreshSelectedBlocks(true);
        }

        private void BlocksTreeView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
                vm.RefreshSelectedBlocks(true);
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            BlocksTreeView.SelectAll();
            vm.RefreshSelectedBlocks(true);
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            BlocksTreeView.SelectedNodes.Clear();
            vm.RefreshSelectedBlocks(true);
        }


        // On a click on a TreeViewItem text
        private void BlocksTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem is BlockNode bn)
                // Only synchronize for blocks
                if (bn.Level == 0)
                {
                    // Find 1st character of selected block
                    foreach (var cr in vm.CharactersRecordsFilteredList)
                    {
                        if (cr.Block == bn.Block)
                        {
                            if (IsGridViewVisible)
                                CharGridView.ScrollIntoView(cr);
                            else
                                CharListView.ScrollIntoView(cr);
                            return;
                        }
                    }
                }
        }

    }
}

