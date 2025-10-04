// NativeMethods class
// Regroup P/Invoke declarations, to follow code analysis recommendations
//
// 2023-08-27   PV      https://learn.microsoft.com/en-us/answers/questions/822928/app-icon-windows-app-sdk

using System;
using System.Runtime.InteropServices;

namespace UniView_WinUI3;
internal static class NativeMethods
{
    public const int ICON_SMALL = 0;
    public const int ICON_BIG = 1;
    public const int ICON_SMALL2 = 2;

    public const int WM_GETICON = 0x007F;
    public const int WM_SETICON = 0x0080;

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);
}
