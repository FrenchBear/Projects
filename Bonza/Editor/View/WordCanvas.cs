// BonzaEditor - class BonzaView.WordCanvas
// MVVM View
// 2017-07-22   PV  First version

// ToDo: Esc cancel a move operation, clean properly
// ToDo: Let user change orientation of a word
// ToDo: Delete selection command
// ToDo: Add a word or a group of words

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Bonza.Generator;


namespace Bonza.Editor
{
    public partial class BonzaView
    {
        // --------------------------------------------------------------------
        // WordCanvas class
        // Virual representation of a word as a Canvas

        internal class WordCanvas : Canvas
        {
            private readonly FontFamily arial = new FontFamily("Arial");

            // Builds all visual letters of a WordPosition, include them as children TextBlocks
            internal WordCanvas(WordPosition wp)
            {
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
                    double top, left, width, height;
                    top = wp.IsVertical ? UnitSize * i : 0;
                    left = wp.IsVertical ? 0 : UnitSize * i;
                    width = UnitSize;
                    height = UnitSize;

                    tb.SetValue(Canvas.LeftProperty, left);
                    tb.SetValue(Canvas.TopProperty, top);
                    tb.Width = width;
                    tb.Height = height;

                    this.Children.Add(tb);
                }

                this.SetValue(Canvas.LeftProperty, UnitSize * wp.StartColumn);
                this.SetValue(Canvas.TopProperty, UnitSize * wp.StartRow);
                SetWordCanvasColor(NormalForegroundBrush, NormalBackgroundBrush);
            }


            // Helper to set foreground/background on all TextBlock of a wordCanvas 
            internal void SetWordCanvasColor(Brush foreground, Brush background)
            {
                foreach (TextBlock tb in Children)
                {
                    tb.Foreground = foreground;
                    tb.Background = background;
                }
            }

        }
    }
}
