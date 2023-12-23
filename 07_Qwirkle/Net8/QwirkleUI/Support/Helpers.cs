// Helpers
// Helpers for views/windows
//
// 2023-12-13   PV

using System.Runtime.CompilerServices;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;

namespace QwirkleUI;

internal static class Helpers
{
    public static bool HandOverInProgress;

    // Return UITile hit by mouse on BoardDrawingCanvas dc, or null if no UITile has been hit
    public static UITile? GetHitHile(Point p, Canvas dc)
    {
        if (dc.InputHitTest(p) is not DependencyObject h)
            return null;
        for (; ; )
        {
            h = VisualTreeHelper.GetParent(h);
            if (h == null || h is Canvas)       // UITile doesn't contain a Canvas in its visual tree, so if we hit a canvas, there's no UITile here
                return null;
            if (h is UITile ut)
                return ut;
        }
    }

    public static void TraceCall(string prefix = "", [CallerMemberName] string callerName = "")
    {
        string l2 = new StackFrame(2, true).GetMethod()?.Name ?? "";
        Debug.WriteLine($"Enter: {prefix}{callerName}     from {l2}");
    }
}
