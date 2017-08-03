// Bonza Editor - WordAndCanvas class
// Combination of a WordPositon and a WordCanvas, used by View and Selection

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
    }
}
