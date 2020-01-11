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
    }
}
