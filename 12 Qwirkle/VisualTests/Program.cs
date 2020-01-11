using System;
using QwirkleLib;

namespace VisualTests
{
    class Program
    {
        static void Main()
        {
            var b = new Board();
            b.AddTile((0, 0), new QTile(0, 2));
            b.AddTile((0, 1), new QTile(0, 3));
            b.UpdatePlayability();
            b.Print();
        }
    }
}
