using System;
using QwirkleLib;
using static System.Console;

#nullable enable

namespace VisualTests
{
    class Program
    {
        static void Main()
        {
            var b = new Board();
            b.AddTile((0, 0), "A1"); b.AddTile((0, 1), "A2"); b.AddTile((0, 2), "A3"); b.AddTile((0, 3), "A4"); b.AddTile((0, 4), "A5"); b.AddTile((0, 5), "A6");
            b.AddTile((1, 0), "B1"); b.AddTile((1, 1), "B2"); b.AddTile((1, 2), "B3"); b.AddTile((1, 3), "B4"); b.AddTile((1, 4), "B5"); b.AddTile((1, 5), "B6");
            b.AddTile((2, 0), "C1"); b.AddTile((2, 1), "C2"); b.AddTile((2, 2), "C3"); b.AddTile((2, 3), "C4"); b.AddTile((2, 4), "C5"); b.AddTile((2, 5), "C6");
            b.AddTile((3, 0), "D1"); b.AddTile((3, 1), "D2"); b.AddTile((3, 2), "D3"); b.AddTile((3, 3), "D4"); b.AddTile((3, 4), "D5"); b.AddTile((3, 5), "D6");
            b.AddTile((4, 0), "E1"); b.AddTile((4, 1), "E2"); b.AddTile((4, 2), "E3"); b.AddTile((4, 3), "E4"); b.AddTile((4, 4), "E5"); b.AddTile((4, 5), "E6");

            b.PlayTile((5, 0), "F1"); b.PlayTile((5, 1), "F2"); b.PlayTile((5, 2), "F3"); b.PlayTile((5, 3), "F4"); b.PlayTile((5, 4), "F5"); b.PlayTile((5, 5), "F6");

            b.UpdatePlayedPlayability();
            TracePrint(b);
            WriteLine($"Points = {b.PlayPoints()}");
        }

        private static void TracePrint(Board b)
        {
            b.Print("// ");
            for (int row = b.RowMin; row <= b.RowMax; row++)
                for (int col = b.ColMin; col <= b.ColMax; col++)
                    if (!string.IsNullOrEmpty(b[(row, col)].ToString()))
                        WriteLine($"Assert.AreEqual(\"{b[(row, col)].ToString()}\", b[({row},{col})].ToString());");
            WriteLine();
        }
    }
}
