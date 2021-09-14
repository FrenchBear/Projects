// QTile.cs - A game tile
// Qwirkle simulation project
// 2019-01-12   PV

using System;
using System.Diagnostics;

#nullable enable

namespace QwirkleLib
{
    /// <summary>
    /// A game tile with a specific color and shape.  Immutable
    /// </summary>
    public class QTile
    {
        /// <summary>
        /// Tile shape on a scale 0..5, represented as a letter A..F
        /// </summary>
        public int Shape { get; private set; }

        /// <summary>
        /// Tile color on a scale 0..5, represented as a digit 1..6
        /// </summary>
        public int Color { get; private set; }

        public QTile(int shape, int color)
        {
            Debug.Assert(shape >= 0 && shape <= 5);
            Debug.Assert(color >= 0 && color <= 5);

            Shape = shape;
            Color = color;
        }

        [DebuggerStepThrough]
        public QTile(string s)
        {
            Debug.Assert(s.Length == 2);
            var shape = Char.ToUpper(s[0]);
            var color = s[1];
            Debug.Assert(shape >= 'A' && shape <= 'F');
            Debug.Assert(color >= '1' && color <= '6');

            Shape = shape - 'A';
            Color = color - '1';
        }

        public override string ToString() => $"{(char)(65+Shape)}{(char)(49+Color)}";
    }
}
