﻿// BonzaEditor - class BonzaView.WordCanvas
// A specialization of Canvas to represent visually a WordPosition
//
// 2017-07-22   PV  First version


using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Bonza.Generator;
using static Bonza.Editor.App;


namespace Bonza.Editor.Support
{
    internal class WordCanvas : Canvas
    {
        private readonly FontFamily arial = new FontFamily("Arial");
        private readonly WordPosition wp;

        // Builds all visual letters of a WordPosition, include them as children TextBlocks
        internal WordCanvas(WordPosition wp)
        {
            // Keep a link to WordPosition, we need it when swapping orientation
            this.wp = wp;

            for (int i = 0; i < wp.Word.Length; i++)
            {
                TextBlock tb = new TextBlock()
                {
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = wp.Word.Substring(i, 1),
                    FontFamily = arial,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(0, 3, 0, 0)
                };

                double top = wp.IsVertical ? UnitSize * i : 0;
                double left = wp.IsVertical ? 0 : UnitSize * i;
                double width = UnitSize;
                double height = UnitSize;

                tb.SetValue(LeftProperty, left);        // Canvas.LeftProperty
                tb.SetValue(TopProperty, top);
                tb.Width = width;
                tb.Height = height;

                Children.Add(tb);
            }

            SetValue(LeftProperty, UnitSize * wp.StartColumn);
            SetValue(TopProperty, UnitSize * wp.StartRow);
            SetColor(NormalForegroundBrush, NormalBackgroundBrush);
        }


        // Helper to set foreground/background on all TextBlock of a wordCanvas 
        internal void SetColor(Brush foreground, Brush background)
        {
            foreach (TextBlock tb in Children)
            {
                tb.Foreground = foreground;
                tb.Background = background;
            }
        }

        // Update children (letter TextBlocks) placement after changing orientation
        // Just update visual representation
        // Caller is responsible for all other tasks (check placment, redraw background grind if layout bounding rectangle has changed, undo support)
        internal void RebuildCanvasAfterOrientationSwap()
        {
            for (int i = 0; i < wp.Word.Length; i++)
            {
                TextBlock tb = Children[i] as TextBlock;

                double top = wp.IsVertical ? UnitSize * i : 0;
                double left = wp.IsVertical ? 0 : UnitSize * i;
                tb?.SetValue(LeftProperty, left);
                tb?.SetValue(TopProperty, top);
            }
        }
    }
}
 