// NativeMethods class
// Regroup P/Invoke declarations, to follow code analysis recommendations
//
// 2023-08-27   PV      https://learn.microsoft.com/en-us/answers/questions/822928/app-icon-windows-app-sdk
// 2023-11-20   PV      Net6 -> Net8, DllImport -> LibraryImport

using System;
using System.Runtime.InteropServices;

namespace UniView_WinUI3;
internal static partial class NativeMethods
{
    public const int ICON_SMALL = 0;
    public const int ICON_BIG = 1;
    public const int ICON_SMALL2 = 2;

    public const int WM_GETICON = 0x007F;
    public const int WM_SETICON = 0x0080;

    [LibraryImport("User32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
    public static partial int SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);
}
