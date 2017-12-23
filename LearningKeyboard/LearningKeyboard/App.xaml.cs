// LearningKeyboard App.xaml.cs
// SUpport for single-instance app
// 2017-12-23   PV

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace LearningKeyboard
{
    public partial class App : Application
    {
        static System.Threading.Mutex InstanceMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            InstanceMutex = new System.Threading.Mutex(true, "{CB5BF4E5-1361-41AD-9B8E-2B0D84CAEAFC}", out bool isNewMutex);
            if (!isNewMutex)
            {
                ShowExistingWindow();

                InstanceMutex = null;
                Application.Current.Shutdown();
            }
            base.OnStartup(e);
        }

        // Release mutex on exit
        protected override void OnExit(ExitEventArgs e)
        {
            if (InstanceMutex != null)
                InstanceMutex.ReleaseMutex();
            base.OnExit(e);
        }



        // signals to restore the window to its normal state
        private const int SW_SHOWNORMAL = 1;

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // shows the window of the single-instance that is already open
        private void ShowExistingWindow()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (var process in processes)
            {
                // the single-instance already open should have a MainWindowHandle
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    // restores the window in case it was minimized
                    ShowWindow(process.MainWindowHandle, SW_SHOWNORMAL);

                    // brings the window to the foreground
                    SetForegroundWindow(process.MainWindowHandle);

                    return;
                }
            }
        }
    }
}