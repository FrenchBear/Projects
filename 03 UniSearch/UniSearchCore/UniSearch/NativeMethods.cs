// NativeMethods class
// Regroup P/Invoke declarations, to follow code analysis recommendations
// 2016-11-12   PV

using System;
using System.Runtime.InteropServices;

#nullable enable


namespace UniSearchNS
{
    internal static class NativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int StrCmpLogicalW(string x, string y);

        // Win32 API for menus
        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool InsertMenu(IntPtr hMenu, int wPosition, int wFlags, IntPtr wIDNewItem, string lpNewItem);
    }
}