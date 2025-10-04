// SearchWindow
// Unicode Character Search Tool, WinUI3 version
//
// 2018-09-18   PV
// 2020-11-11   PV      nullable enable
// 2024-09-27   PV      Converted from UWP version, not simple...
// 2024-10-09   PV      Fixed contect menu commands to copy information, not working
// 2024-12-08   PV      Net9 C#13
// 2024-12-08   PV      Support for font switching
// 2024-12-10   PV      Updated LastResortHE font to Unicode 16.0 (was previously Unicode 13.0)
// 2024-12-11   PV      Fixed SelectAll not working

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Linq;
using System.Reflection;
using UniDataWinUI3;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0051 // Remove unused private members

namespace UniSearchWinUI3;

public sealed partial class SearchWindow: Window
{
    readonly ViewModel VM;

    public SearchWindow()
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

        // Control initial window size
        // https://stackoverflow.com/questions/67169712/winui-3-0-reunion-0-5-window-size
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1600, Height = 1200 });
        appWindow.Title = "UniSearch WinUI3";

        AppWindow.SetIcon(@"Assets\Icons\DarkUnicorn2.ico");

        InitializeComponent();

        VM = new ViewModel(this);
        MainGrid.DataContext = VM;

        MainGrid.Loaded += (s, e) =>
        {
            BlocksTreeView.SelectAll();
            VM.InitialBlocksUnselect();
            VM.RefreshSelectedBlocks(true);
            CharacterFilterTextBox.Focus(FocusState.Programmatic);

            // Main app info
            (string Title, string Description, string Version, string Copyright) = AboutDialog.GetAppVersion(Assembly.GetExecutingAssembly());
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

    private void CharListOrGridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => VM.NotifyPropertyChanged(nameof(VM.SelChars));

    // Some controls such as AutoSuggestBox do not get focused just by defining AccessKey property for some reason.
    // This generic helper launched by AccessKeyInvoked event makes it work.
    // Must remain non-static despite suggestions since it's dynamically called from XAML
#pragma warning disable CA1822 // Mark members as static
    private void SetFocus(UIElement sender, AccessKeyInvokedEventArgs args)
#pragma warning restore IDE0079 // Remove unnecessary suppression
    {
        if (sender is Control c)
            c.Focus(FocusState.Keyboard);
    }

    public bool IsGridViewVisible => CharSemanticView.Visibility == Visibility.Visible;

    public ListViewBase CharCurrentView => IsGridViewVisible ? CharGridView : CharListView;

    private void ListGrid_Click(object sender, RoutedEventArgs e)
    {
        if (IsGridViewVisible)
        {
            CharSemanticView.Visibility = Visibility.Collapsed;
            CharListView.Visibility = Visibility.Visible;
            CharListView.ScrollIntoView(VM.SelectedChar);
        }
        else
        {
            CharListView.Visibility = Visibility.Collapsed;
            CharSemanticView.Visibility = Visibility.Visible;
            CharGridView.ScrollIntoView(VM.SelectedChar);
        }
    }

    private async void GridViewCell_DoubleTap(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (VM.SelectedChar != null)
            await CharDetailDialog.ShowDetail(VM.SelectedChar.Codepoint, VM, MainGrid.XamlRoot);
    }

    // Show details when pressing Enter of Space
    private async void GridViewCell_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is Windows.System.VirtualKey.Space or Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            if (VM.SelectedChar != null)
                await CharDetailDialog.ShowDetail(VM.SelectedChar.Codepoint, VM, MainGrid.XamlRoot);
        }
    }

    // Select character on right-click
    private void CharGridView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is CharacterRecord cr)
        {
            // Select char if it's not in current selection
            if (!CharCurrentView.SelectedItems.Cast<CharacterRecord>().Contains(cr))
                VM.SelectedChar = cr;
        }
    }

    // Simulate "SelectionChanged" missing event with mouse and key events
    // But this is unreliable, doubletap event gets fired before SelectedNodes has been refreshed...
    private void BlocksTreeView_Tapped(object sender, TappedRoutedEventArgs e) => VM.RefreshSelectedBlocks(true);
    private void BlocksTreeView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) => VM.RefreshSelectedBlocks(true);

    private void BlocksTreeView_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Space)
            VM.RefreshSelectedBlocks(true);
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        //BlocksTreeView.SelectAll();       // Does not work...
        MySelectAll(VM.BlocksRoot);
        VM.RefreshSelectedBlocks(true);
    }

    void MySelectAll(BlockNode bn)
    {
        if (bn.Level == 0)
        {
            if (!BlocksTreeView.SelectedNodes.Contains(bn))
                BlocksTreeView.SelectedNodes.Add(bn);
        }
        else
        {
            foreach (var child in bn.Children)
                MySelectAll((child as BlockNode)!);
        }
    }

    private void UnselectAllButton_Click(object sender, RoutedEventArgs e)
    {
        BlocksTreeView.SelectedNodes.Clear();
        VM.RefreshSelectedBlocks(true);
    }

    // On a click on a TreeViewItem text
    private void BlocksTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is BlockNode bn)
            // Only synchronize for blocks
            if (bn.Level == 0)
            {
                // Find 1st character of selected block
                foreach (var cr in VM.CharactersRecordsFilteredList)
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

    private void ZommableTB_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border b)
            return;
        if (b.Resources["ZoomIn"] is not Storyboard sb)
            return;
        sb.Begin();
    }

    private void ZommableTB_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border b)
            return;
        if (b.Resources["ZoomOut"] is not Storyboard sb)
            return;
        sb.Begin();
    }

    private void SettingsToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (SettingsToggleButton.IsChecked.HasValue)
            MainSplitView.IsPaneOpen = true;
    }

    private void MainSplitView_PaneClosed(SplitView sender, object args)
        => SettingsToggleButton.IsChecked = false;

    private void BlocksToggleButton_Click(object sender, RoutedEventArgs e)
        => BlocksSplitView.IsPaneOpen = BlocksToggleButton.IsChecked!.Value;
}
