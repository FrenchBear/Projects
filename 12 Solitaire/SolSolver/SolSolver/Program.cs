// SolSolver
// Quick and dirty simple Solitaire Solver
// 2019-06-04 PV
//
// Base does't need to be a stack, a simple int is enough since base are assigned to a color
// Signature history is reset each time a card is moved from column to base (in fact, only durint OneMovementToBase)
// When all cards are visible, game is solved

using System;
using System.Diagnostics;
using System.Text;
using static System.Console;

using SolLib;


namespace SolSolver
{
    internal class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = new UTF8Encoding();
            //TestSolver();
            SolverStats(100);

            //Console.WriteLine();
            //Console.Write("(Pause)");
            //Console.ReadLine();
        }

        private static void SolverStats(int n)
        {
            int solved = 0;
            int s = 1;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < n; i++)
            {
                SolitaireDeck d;
                for (; ; )
                {
                    d = new SolitaireDeck(s++);
                    break;

                    /*
                    // Check whether an Ace and a King visible at the beginning we're better chances to solve
                    // With this, we shift from ~30% to 37%...
                    bool withAce = false;
                    bool withKing = false;
                    for (int c = 0; c < 7; c++)
                    {
                        if (d.ColumnTopCard(c).Value == 1) withAce = true;
                        if (d.ColumnTopCard(c).Value == 13) withKing = true;
                    }
                    if (withAce && withKing) break;
                    */
                }
                if (d.Solve())
                    solved++;
            }
            sw.Stop();
            WriteLine($"{n} decks analyzed in {sw.ElapsedMilliseconds / 1000.0:F03}s, {solved} solvable = {(double)solved / n:P2}");
        }

        private static void TestSolver()
        {
            var d = new SolitaireDeck(4);
            if (d.Solve(true))
                WriteLine("Solvable/Solved");
            else
            {
                WriteLine("Stuck");
                //d.Print();
            }
        }
    }
}