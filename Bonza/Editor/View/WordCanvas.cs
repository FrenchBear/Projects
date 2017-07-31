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
            // Just update visual representation and WordPosition.IsVertical (though le latter may be challenged),
            // Caller is responsible for all other tasks (check placment, redraw background grind if layout bounding rectangle has changed, undo support)
            internal void SwapOrientation()
            {
                wp.IsVertical = !wp.IsVertical;
                for (int i = 0; i < wp.Word.Length; i++)
                {
                    TextBlock tb = Children[i] as TextBlock;

                    double top, left;
                    top = wp.IsVertical ? UnitSize * i : 0;
                    left = wp.IsVertical ? 0 : UnitSize * i;
                    tb.SetValue(Canvas.LeftProperty, left);
                    tb.SetValue(Canvas.TopProperty, top);
                }
            }
        }
    }
}