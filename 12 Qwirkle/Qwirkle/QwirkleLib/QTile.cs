using System;
using System.Diagnostics;

#nullable enable

namespace QwirkleLib
{
    public class QTile
    {
        public int Shape { get; private set; }
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
