// Quadrimino.cs
// Quadrimino paving paving solver for The Talos Principle
//
// 2026-01-03   PV      First version recycling Pentamino solver code

using System;
using System.Diagnostics;
using static System.Console;

internal static class Quadrimino
{
    private const int ROWS = 6;                 // Number of lines of the surface to pave
    private const int COLS = 6;                 // Number of columns of the surface to pave
    private const int PIECES = 9;

    private const int MAXSOLUTION = 5000;       // Limit search time if needed

    private static int NbSolutions;
    private static int NbPavingCalls;

    // Table of pentaminos to use for the problem
    private static Piece[] Lp;

    private static void Main(string[] args)
    {
        WriteLine("Quadrimino rectangle paving in C#");

        const bool X = true;
        const bool o = false;

        // Prepare pieces
        Piece P1 = new("Carré 1", X, X, o, o, X, X, o, o, 1);
        Piece P2 = new("Carré 2", X, X, o, o, X, X, o, o, 1);
        Piece P3 = new("T", X, X, X, o, o, X, o, o, 4);
        Piece P4 = new("T", X, X, X, o, o, X, o, o, 4);
        Piece P5 = new("L", X, X, X, o, X, o, o, o, 4);
        Piece P6 = new("L", X, X, X, o, X, o, o, o, 4);
        Piece P7 = new("L miroir", X, o, o, o, X, X, X, o, 4);
        Piece P8 = new("Barre", X, X, X, X, o, o, o, o, 2);
        Piece P9 = new("Serpent", o, X, X, o, X, X, o, o, 2);

        P1.Trace();
        P2.Trace();
        P3.Trace();
        P4.Trace();
        P5.Trace();
        P6.Trace();
        P7.Trace();
        P8.Trace();
        P9.Trace();
        //P10.Trace();
        //P11.Trace();
        //P12.Trace();

        // Pieces to use, allowing easy indexed access (order is not meaningful)
        Lp = [P1, P2, P3, P4, P5, P6, P7, P8, P9];

        // Rectangle for paving, zero-initialized by default (https://stackoverflow.com/questions/8679052/initialization-of-memory-allocated-with-stackalloc)
        Span<byte> rect = stackalloc byte[ROWS * COLS];

        // Paving
        var sw = Stopwatch.StartNew();
        Paving(0, 0, rect, (1 << PIECES) - 1);

        WriteLine($"Duration {sw.ElapsedMilliseconds / 1000.0:f3}s");
        WriteLine($"{NbSolutions} solutions");
        WriteLine($"{NbPavingCalls} calls to Paving()");
    }

    private static void Paving(int rstart, int cstart, Span<byte> rect, int piecesMask)
    {
        if (NbSolutions > MAXSOLUTION)
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

        WriteLine($"--- Solution {sol}");
        for (int c = 0; c < COLS; c++)
        {
            for (int r = 0; r < ROWS; r++)
            {
                var p = rect[r * COLS + c];
                Debug.Assert(p is >= 1 and <= PIECES);
                ForegroundColor = (ConsoleColor)p;
                Write("██");
            }
            WriteLine();
        }

        ForegroundColor = bak;
    }
}
