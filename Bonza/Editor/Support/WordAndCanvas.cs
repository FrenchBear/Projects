// Bonza Editor - WordAndCanvas class
// Combination of a WordPositon and a WordCanvas, used by View and Selection

using System.Windows.Media;
using Bonza.Generator;


namespace Bonza.Editor.Support
{
    internal class WordAndCanvas
    {
        internal WordPosition WordPosition { get; }

        internal WordCanvas WordCanvas { get; }


        internal WordAndCanvas(WordPosition wp, WordCanvas wc)
        {
            WordPosition = wp;
            WordCanvas = wc;
        }

        internal void SetColor(Brush foreground, Brush background)
        {
            WordCanvas.SetColor(foreground, background);
        }

        internal void RebuildCanvasAfterOrientationSwap()
        {
            WordCanvas.RebuildCanvasAfterOrientationSwap();
        }
    }
}
