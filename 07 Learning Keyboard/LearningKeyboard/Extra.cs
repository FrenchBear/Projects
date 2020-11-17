// Learning Keyboard
// Visual Keyboard to learn typing
// Extra class to prepare automatic layout update when keyboard selection changes
//
// 2017-12-22   PV      1.1.1 First tests
// 2020-11-17   PV      C#8, nullable enable
//
// It seems that no event is raised when keyboard layout changes (but Ok for language change), so polling
// GetKbdInfo() regularly should do the trick

using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

#nullable enable


namespace LearningKeyboard
{
    public static class Extra
    {
        internal static void GetKbdInfo()
        {
            IntPtr hKL = GetActiveKeyboard();
            hKL = (IntPtr)(hKL.ToInt32() & 0x0000FFFF);
            InputLanguageManager m = InputLanguageManager.Current;
            m.CurrentInputLanguage = new System.Globalization.CultureInfo(hKL.ToInt32());
            //IntPtr i = LoadKeyboardLayout(hKL.ToString(), 1);

            //string InputLanguage = InputLanguageManager.Current.CurrentInputLanguage.ToString();

            //InputManager im = System.Windows.Input.InputManager.Current;

            //Debugger.Break();

        }
 
        public static IntPtr GetActiveKeyboard()
        {
            IntPtr hActiveWnd = ThreadNativeMethods.GetForegroundWindow(); //handle to focused window
            int hCurrentWnd = ThreadNativeMethods.GetWindowThreadProcessId(hActiveWnd, out int _);
            //thread of focused window
            return ThreadNativeMethods.GetKeyboardLayout(hCurrentWnd); //get the layout identifier for the thread whose window is focused
        }
    }

    internal static class ThreadNativeMethods
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetForegroundWindow();


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetKeyboardLayout(int dwLayout);

    }
}
