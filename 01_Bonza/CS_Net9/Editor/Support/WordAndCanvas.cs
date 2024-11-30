// Bonza Editor - WordAndCanvas class
// Combination of a WordPositon and a WordCanvas, used by View and Selection
//
// 2021-11-13   PV      Net6 C#10
// 2024-11-15	PV		Net9 C#13

using Bonza.Generator;
using System.Windows.Media;

namespace Bonza.Editor.Support;

internal sealed class WordAndCanvas
{
    internal WordPosition WordPosition { get; }

    internal WordCanvas WordCanvas { get; }

    internal WordAndCanvas(WordPosition wp, WordCanvas wc)
    {
        WordPosition = wp;
        WordCanvas = wc;
    }

    internal void SetColor(Brush foreground, Brush background) => WordCanvas.SetColor(foreground, background);

    internal void RebuildCanvasAfterOrientationSwap() => WordCanvas.RebuildCanvasAfterOrientationSwap();
}
