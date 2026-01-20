// NativeMethods class
// Regroup P/Invoke declarations, to follow code analysis recommendations
//
// 2016-11-12   PV
// 2026-01-20	PV		Net10 C#14

using System;
using System.Runtime.InteropServices;

namespace UniSearch;

internal static partial class NativeMethods
{
    [LibraryImport("shlwapi.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int StrCmpLogicalW(string x, string y);

    // Win32 API for menus
    [LibraryImport("user32.dll")]
    public static partial IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

    [LibraryImport("user32.dll", EntryPoint = "InsertMenuW", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool InsertMenu(IntPtr hMenu, int wPosition, int wFlags, IntPtr wIdNewItem, string lpNewItem);
}