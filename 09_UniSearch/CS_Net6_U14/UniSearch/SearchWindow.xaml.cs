// UniSearch
// Unicode Character Search Tool
//
// 2018-09-10   PV
// 2019-08-09   PV      1.1 .Net Core 3.0, C#8, Nullable
// 2020-11-11   PV      1.2 .Net 5; New rule for 1-letter filters
// 2020-11-12   PV      1.3 Copy Full Info; #Nullable Enable generalized; Synonyms, Cross-Refs and Comments; 
//                      Swapped Block and Character grids to have a similar layout than UWP version

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using static UniSearch.NativeMethods;

#nullable enable

namespace UniSearch;

internal sealed partial class SearchWindow
{
    readonly ViewModel VM;

    public SearchWindow()
    {
        InitializeComponent();

        VM = new ViewModel(this);
        DataContext = VM;

        Loaded += (s, e) =>
        {
            // Get the Handle for the Forms System Menu
            var systemMenuHandle = GetSystemMenu(Handle, false);

            // Create our new System Menu items just before the Close menu item
            InsertMenu(systemMenuHandle, 5, MfByposition | MfSeparator, (IntPtr)0, string.Empty);
            InsertMenu(systemMenuHandle, 6, MfByposition, (IntPtr)SettingsSysMenuId, "&About UniSearch...");

            // Attach our WindowCommandHandler handler to this Window
            var source = HwndSource.FromHwnd(Handle);
            source?.AddHook(WindowCommandHandler);

            CharacterFilterTextBox.Focus();
        };
    }

    /// Define Constants we will use
    private const int WmSyscommand = 0x112;

    private const int MfSeparator = 0x800;
    private const int MfByposition = 0x400;

    // The constant we'll use to identify our custom system menu items
    private const int SettingsSysMenuId = 1000;

    /// <summary>
    /// This is the Win32 interop Handle for this Window
    /// </summary>
    public IntPtr Handle => new WindowInteropHelper(this).Handle;

    private IntPtr WindowCommandHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // Check if a System Command has been executed
        if (msg == WmSyscommand && wParam.ToInt32() == SettingsSysMenuId)
        {
            var aw = new AboutWindow();
            aw.ShowDialog();

            handled = true;
        }

        return IntPtr.Zero;
    }

    private void CheckBox_Flip(object sender, RoutedEventArgs e) => VM.AfterCheckboxFlip();

    private void TreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not TreeView {SelectedItem: CheckableNode cn}) return;
        CharacterFilterTextBox.Text = "b:\"" + cn.Name + "\"";
    }
}