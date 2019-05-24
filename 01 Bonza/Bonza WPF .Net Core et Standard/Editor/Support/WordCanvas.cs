// BonzaEditor - class EditorView.WordCanvas
// A specialization of Canvas to represent visually a WordPosition
//
// 2017-07-22   PV  First version


using Bonza.Generator;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
                TextBlock tb = new TextBlock
                {
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = wp.Word.Substring(i, 1),
                    FontFamily = arial,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(0, 3, 0, 0)
                };

                AdjustTextBlock(i, tb, wp.IsVertical);

                Children.Add(tb);
            }

            SetValue(LeftProperty, UnitSize * wp.StartColumn);
            SetValue(TopProperty, UnitSize * wp.StartRow);
            SetColor(NormalValidForeground, NormalValidBackground);
        }

        private void AdjustTextBlock(int i, TextBlock tb, bool isVertical)
        {
            double top = isVertical ? UnitSize * i : 0;
            double left = isVertical ? 0 : UnitSize * i;
            double width = UnitSize;
            double height = UnitSize;

            if (isVertical)
            {
                left += MarginSize;
                width -= 2 * MarginSize;
                if (i == 0)
                {
                    top += MarginSize;
                    height -= MarginSize;
                }
                else if (i == wp.Word.Length - 1)
                {
                    height -= MarginSize;
                }
            }
            else
            {
                top += MarginSize;
                height -= 2 * MarginSize;
                if (i == 0)
                {
                    left += MarginSize;
                    width -= MarginSize;
                }
                else if (i == wp.Word.Length - 1)
                {
                    width -= MarginSize;
                }
            }

            tb.SetValue(LeftProperty, left);        // Canvas.LeftProperty
            tb.SetValue(TopProperty, top);
            tb.Width = width;
            tb.Height = height;
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
        // Caller is responsible for all other tasks (check placement, redraw background grid if layout bounding rectangle has changed, undo support)
        // ToDo: Redraw squares borders since they are orientation-dependent...
        internal void RebuildCanvasAfterOrientationSwap()
        {
            for (int i = 0; i < wp.Word.Length; i++)
            {
                TextBlock tb = Children[i] as TextBlock;
                AdjustTextBlock(i, tb, wp.IsVertical);

                //double top = wp.IsVertical ? UnitSize * i : 0;
                //double left = wp.IsVertical ? 0 : UnitSize * i;
                //tb?.SetValue(LeftProperty, left);
                //tb?.SetValue(TopProperty, top);
            }
        }
    }
}