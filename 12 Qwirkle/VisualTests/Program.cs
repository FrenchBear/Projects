using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            b.PlayTile((0, 0), "F1");
            b.PlayTile((1, 0), "D1");
            b.PlayTile((2, 0), "A1");
            Debug.Assert(b.PlayPoints() == 3);
            b.CommitPlay();

            var hand = new List<QTile>
            {
                new QTile("C1"),
                new QTile("E3"),
                new QTile("C5"),
                new QTile("C6"),
                new QTile("D4"),
                new QTile("F6")
            };

            var s = b.Solve(hand);
            if (s == null)
                WriteLine("Pas de solution");
            else
            {
                WriteLine($"Solution à {s.Points} points:");
                foreach (var move in s.Moves)
                {
                    WriteLine($"  {move.ToString()}");
                }
            }
        }

        private static void TracePrint(Board b)
        {
            b.Print("// ");
            //for (int row = b.RowMin; row <= b.RowMax; row++)
            //    for (int col = b.ColMin; col <= b.ColMax; col++)
            //        if (!string.IsNullOrEmpty(b[(row, col)].ToString()))
            //            WriteLine($"Assert.AreEqual(\"{b[(row, col)].ToString()}\", b[({row},{col})].ToString());");
            WriteLine();
        }
    }
}
