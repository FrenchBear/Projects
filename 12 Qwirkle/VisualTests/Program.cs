using System;
using QwirkleLib;
using static System.Console;


namespace VisualTests
{
    class Program
    {
        static void Main()
        {
            var b = new Board();
            b.AddTile((0, 0), "A3");
            b.UpdateBoardPlayability();
            b.PlayTile((0, 1), "A4");
            b.UpdatePlayedPlayability();
            TracePrint(b);
            b.PlayTile((1, 0), "B3");
            b.UpdatePlayedPlayability();
            TracePrint(b);
            //b.CommitPlay();
            //TracePrint(b);
        }

        private static void TracePrint(Board b)
        {
            b.Print("// ");
            //for (int row = b.RowMin; row <= b.RowMax; row++)
            //    for (int col = b.ColMin; col <= b.ColMax; col++)
            //        if (!string.IsNullOrEmpty(b[(row, col)].ToString()))
            //            WriteLine($"Assert.AreEqual(b[({row},{col})].ToString(), \"{b[(row, col)].ToString()}\");");
            //WriteLine();
        }
    }
}
