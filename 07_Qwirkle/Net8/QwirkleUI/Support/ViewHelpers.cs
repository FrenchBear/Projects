﻿// VisuelHelpers
// Helpers for vews/windows
//
// 2023-12-13   PV

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace QwirkleUI;

internal static class ViewHelpers
{
    // Return UITile hit by mouse on canvas c, or null if no UITile has been hit
    public static UITile? GetHitHile(MouseButtonEventArgs e, Canvas c)
    {
        if (c.InputHitTest(e.GetPosition(c)) is not DependencyObject h)
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
}