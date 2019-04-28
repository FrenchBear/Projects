// UniSearch
// Unicode Character Search Tool
//
// 2018-09-10   PV


using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Interop;
using static UniSearchNS.NativeMethods;


namespace UniSearchNS
{
    internal sealed partial class SearchWindow : Window
    {
        readonly ViewModel vm;


        public SearchWindow()
        {
            InitializeComponent();

            vm = new ViewModel(this);
            DataContext = vm;

            Loaded += (s, e) =>
            {
                // Get the Handle for the Forms System Menu
                var systemMenuHandle = GetSystemMenu(Handle, false);

                // Create our new System Menu items just before the Close menu item
                InsertMenu(systemMenuHandle, 5, MfByposition | MfSeparator, (IntPtr)0, string.Empty);
                InsertMenu(systemMenuHandle, 6, MfByposition, (IntPtr)SettingsSysMenuId, "&About UniSearch...");

                // Attach our WindowCommandHandler handler to this Window
                var source = HwndSource.FromHwnd(Handle);
                source.AddHook(WindowCommandHandler);

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



        // ToDo: Move this to VM
        private void CheckBox_Flip(object sender, RoutedEventArgs e)
        {
            vm.AfterCheckboxFlip();
        }
    }
}
