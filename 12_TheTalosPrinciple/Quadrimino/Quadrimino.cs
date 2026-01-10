// Quadrimino.cs
// Quadrimino paving paving solver for The Talos Principle
//
// 2026-01-03   PV      First version recycling Pentamino solver code
// 2026-01-10   PV      Code cleanup and use named pieces shared by all puzzles

using System;
using System.Diagnostics;
using static System.Console;

internal static class Quadrimino
{
    private static int ROWS;                 // Number of lines of the surface to pave
    private static int COLS;                 // Number of columns of the surface to pave
    private static int PIECES;

    private const int MAXSOLUTION = 1;       // Limit search time if needed

    private static int NbSolutions;
    private static int NbPavingCalls;

    // Table of pentaminos to use for the problem
    private static Piece[] Lp;

    private static void Main(string[] args)
    {
        WriteLine("Quadrimino rectangle paving in C# - The Talos Principle");

        const bool X = true;
        const bool o = false;

        Piece O = new("O", X, X, o, o, X, X, o, o, 1);
        Piece T = new("T", X, X, X, o, o, X, o, o, 4);
        Piece I = new("I", X, X, X, X, o, o, o, o, 2);
        Piece L = new("L", X, X, X, o, X, o, o, o, 4);
        Piece Lm = new("L'", X, o, o, o, X, X, X, o, 4);
        Piece S = new("S", o, X, X, o, X, X, o, o, 2);
        Piece Sm = new("S'", X, X, o, o, o, X, X, o, 2);

        //Solve("A-Special 1", 6, 6, [O, O, T, T, L, L, Lm, I, S]);
        //Solve("A-Special 2", 6, 6, [Sm, Sm, Sm, L, L, Lm, Lm, T, T]);
        //Solve("A-Special", 10, 4, [S, T, T, T, T, O, O, I, L, L]);
        //Solve("B-Special 1", 10, 4, [S, T, T, T, T, Sm, Sm, I, I, L]);
        //Solve("B-Special 2", 4, 7, [S, Sm, I, I, T, T, Lm]);
        //Solve("B-Special 3", 6, 8, [T, T, O, O, I, L, L, Lm, Lm, Lm, S, S]);
        //Solve("B-Special 4", 6, 8, [T, T, O, O, I, S, S, S, S, Lm, Lm, Lm]);
        //Solve("Tower level 2", 6, 6, [O, T, T, T, T, L, L, L, L]);
        //Solve("Tower level 3", 5, 8, [I, I, I, I, L, L, Lm, Lm, S, Sm]);
        Solve("Tower level 4", 6, 8, [O, O, T, T, T, T, L, Lm, S, S, Sm, Sm]);
    }

    static void Solve(string name, int rows, int cols, Piece[] lp)
    {
        Lp = lp;
        ROWS = rows;
        COLS = cols;
        PIECES = Lp.Length;
        Debug.Assert(4 * PIECES == ROWS * COLS);

        // Rectangle for paving, zero-initialized by default (https://stackoverflow.com/questions/8679052/initialization-of-memory-allocated-with-stackalloc)
        Span<byte> rect = stackalloc byte[ROWS * COLS];

        // Paving
        WriteLine("\n" + name);
        var sw = Stopwatch.StartNew();
        NbSolutions = 0;
        Paving(0, 0, rect, (1 << PIECES) - 1);

        //WriteLine($"Duration {sw.ElapsedMilliseconds / 1000.0:f3}s");
        //WriteLine($"{NbSolutions} solutions");
        //WriteLine($"{NbPavingCalls} calls to Paving()");
    }

    private static void Paving(int rstart, int cstart, Span<byte> rect, int piecesMask)
    {
        if (NbSolutions >= MAXSOLUTION)
            return;

        NbPavingCalls++;

        // We are looking for an empty square to cover, from left to right, from top to bottom
        int r, c = 0;       // Declare loop index outside of the loop to make it persistent once loop is ended
        var found = false;
        for (r = 0; r < ROWS; r++)
        {
            for (c = 0; c < COLS; c++)
            {
                if (r == 0 && c == 0)	  // Optimization, slart from last empty square found
                {
                    r = rstart;
                    c = cstart;
                }

                if (rect[r * COLS + c] == 0)
                {
                    found = true;
                    break;
                }
            }
            if (found)
                break;
        }

        // Not supposed to happen...
        if (!found)
            Debugger.Break();

        // Allocate one rect buffer for recursive call outside the loop, otherwise it will
        // allocate tons of memory, only released when the function exits. Here we have 1 alloc per depth level,
        // with a max of 12 levels, it's acceptable
        // It may do useless allocations if current call is a dead end, but it's supposed to be lightweight anyway
        Span<byte> nextRect = stackalloc byte[ROWS * COLS];

        // We search among all the pieces that remain for a piece to cover the empty square
        for (int i = 0; i < PIECES; i++)
            if ((piecesMask & (1 << i)) != 0)
                foreach (var ca in Lp[i].Transformations)
                {
                    if (c + ca.Cmax - ca.OffsetCol > COLS ||	// Too wide
                        r + ca.Rmax > ROWS ||				    // Too high
                        c < ca.OffsetCol)					    // Must be shifted too much to the left
                        continue;

                    int r2, c2;
                    var collision = false;
                    for (r2 = 0; r2 < ca.Rmax; r2++)
                    {
                        for (c2 = 0; c2 < ca.Cmax; c2++)
                            if (ca.Motif[r2, c2] && rect[(r + r2) * COLS + c + c2 - ca.OffsetCol] != 0)  // Square already occupied
                            {
                                collision = true;
                                break;
                            }
                        if (collision)
                            break;
                    }

                    // If there is a collision for current piece current transformation, no need to proceed in depth calling Pavage again
                    // Just continue to next piece/transformation, that's it
                    if (!collision)
                    {
                        // Piece is Ok! Let's place it
                        rect.CopyTo(nextRect);

                        for (r2 = 0; r2 < ca.Rmax; r2++)
                            for (c2 = 0; c2 < ca.Cmax; c2++)
                                if (ca.Motif[r2, c2])
                                    nextRect[(r + r2) * COLS + c + c2 - ca.OffsetCol] = (byte)(i + 1);

                        //PrintSolution(nextRect, 0);
                        //Debugger.Break();

                        // If there are no more pieces left, we found a solution!
                        var nextMask = piecesMask & ~(1 << i);
                        if (nextMask == 0)
                        {
                            if (NbSolutions == 0)
                                PrintSolution(nextRect, NbSolutions);
                            NbSolutions++;
                            return;
                        }

                        // Continue recursively with remaining pieces up to a solution or a dead end
                        Paving(r, c, nextRect, nextMask);
                    }
                }
    }

    private static void PrintSolution(Span<byte> rect, int sol)
    {
        var bak = ForegroundColor;

        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLS; c++)
            {
                var p = rect[r * COLS + c];         // Bug of original pentamino code fixed: we actually place mirror places (swapped row/col), so we swar again during print to compensate
                if (p == 0)
                    Write("··");
                else
                {
                    Debug.Assert(p >= 1 && p <= PIECES);
                    ForegroundColor = (ConsoleColor)p;
                    Write("██");
                }
            }
            WriteLine();
        }

        ForegroundColor = bak;
    }
}
