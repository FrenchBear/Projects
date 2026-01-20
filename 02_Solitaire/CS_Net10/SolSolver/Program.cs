// Solitaire Solver Test Application
// Quick and dirty simple Solitaire Solver
//
// Game Signature history is reset each time a card is moved from column to base (in fact, only during OneMovementToBase)
// When all cards are visible, game is trivially solvable, no need to continue
//
// 2019-06-04   PV
// 2023-12-20   PV      Net8 C#12
// 2025-03-16   PV      Net9 C#13
// 2026-01-20   PV      Net10 C#14

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Console;
using SolLib;

namespace SolSolver;

#pragma warning disable IDE0051 // Remove unused private members

internal sealed class Program
{
    private static void Main()
    {
        OutputEncoding = new UTF8Encoding();
        TestSolver();
        //SolverStats(100);
    }

    private static void SolverStats(int n)
    {
        int solved = 0;
        int s = 1;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < n; i++)
        {
            SolverDeck d;
            for (; ; )
            {
                d = CreateNewRandomSolverDeck(s++);
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
            var res = d.Solve();
            if (res.HasValue && res.Value)
                solved++;
        }
        sw.Stop();
        WriteLine($"{n} decks analyzed in {sw.ElapsedMilliseconds / 1000.0:F03}s, {solved} solvable = {(double)solved / n:P2}");
    }

    private static void TestSolver()
    {
        var d = CreateNewRandomSolverDeck(4);
        var res = d.Solve(true);
        if (res.HasValue && res.Value)
            WriteLine("Solvable/Solved");
        else
            WriteLine("Stuck");
    }

    private static SolverDeck CreateNewRandomSolverDeck(int seed)
    {
        var s = SolverCard.Set52().Shuffle(seed);

        var Bases = new List<(SolverCard, bool)>[4];
        for (int bi = 0; bi < 4; bi++)
            Bases[bi] = [];
        var Columns = new List<(SolverCard, bool)>[7];
        for (int ci = 0; ci < 7; ci++)
        {
            Columns[ci] = [];
            for (int i = 0; i <= ci; i++)
            {
                var card = s[0];
                s.RemoveAt(0);
                Columns[ci].Add((card, i == 0));
            }
        }
        List<(SolverCard, bool)> TalonFU = [];
        List<(SolverCard, bool)> TalonFD = [];

        foreach (var c in s)
            TalonFD.Add((c, false));

        return new SolverDeck(Bases, Columns, TalonFU, TalonFD);

    }

}