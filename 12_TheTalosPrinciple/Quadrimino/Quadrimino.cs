// Quadrimino.cs
// Quadrimino paving paving solver for The Talos Principle
//
// 2026-01-03   PV      First version recycling Pentamino solver code

using System;
using System.Diagnostics;
using static System.Console;

internal static class Quadrimino
{
    private static int ROWS = 6;                 // Number of lines of the surface to pave
    private static int COLS = 6;                 // Number of columns of the surface to pave
    private static int PIECES = 9;

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

        // Puzzle 1 (A-Special)
        //Piece P1 = new("O1", X, X, o, o, X, X, o, o, 1);
        //Piece P2 = new("O2", X, X, o, o, X, X, o, o, 1);
        //Piece P3 = new("T1", X, X, X, o, o, X, o, o, 4);
        //Piece P4 = new("T2", X, X, X, o, o, X, o, o, 4);
        //Piece P5 = new("L1", X, X, X, o, X, o, o, o, 4);
        //Piece P6 = new("L2", X, X, X, o, X, o, o, o, 4);
        //Piece P7 = new("L'", X, o, o, o, X, X, X, o, 4);
        //Piece P8 = new("I", X, X, X, X, o, o, o, o, 2);
        //Piece P9 = new("S", o, X, X, o, X, X, o, o, 2);

        // Puzzle 2 (A-Special)
        //Piece P1 = new("S'1", X, X, o, o, o, X, X, o, 2);
        //Piece P2 = new("S'2", X, X, o, o, o, X, X, o, 2);
        //Piece P3 = new("S'3", X, X, o, o, o, X, X, o, 2);
        //Piece P4 = new("L1", X, X, X, o, X, o, o, o, 4);
        //Piece P5 = new("L2", X, X, X, o, X, o, o, o, 4);
        //Piece P6 = new("L'1", X, o, o, o, X, X, X, o, 4);
        //Piece P7 = new("L'2", X, o, o, o, X, X, X, o, 4);
        //Piece P8 = new("T1", X, X, X, o, o, X, o, o, 4);
        //Piece P9 = new("T2", X, X, X, o, o, X, o, o, 4);

        // Puzzle 3 (A-Special)
        //ROWS = 10;
        //COLS = 4;
        //PIECES = 10;
        //Piece P1 = new("S", o, X, X, o, X, X, o, o, 2);
        //Piece P2 = new("T1", X, X, X, o, o, X, o, o, 4);
        //Piece P3 = new("T2", X, X, X, o, o, X, o, o, 4);
        //Piece P4 = new("T3", X, X, X, o, o, X, o, o, 4);
        //Piece P5 = new("T4", X, X, X, o, o, X, o, o, 4);
        //Piece P6 = new("O1", X, X, o, o, X, X, o, o, 1);
        //Piece P7 = new("O2", X, X, o, o, X, X, o, o, 1);
        //Piece P8 = new("I", X, X, X, X, o, o, o, o, 2);
        //Piece P9 = new("L1", X, X, X, o, X, o, o, o, 4);
        //Piece P10 = new("L2", X, X, X, o, X, o, o, o, 4);

        // Puzzle B-Special 1
        //ROWS = 10;
        //COLS = 4;
        //PIECES = 10;
        //Piece P1 = new("S", o, X, X, o, X, X, o, o, 2);
        //Piece P2 = new("T1", X, X, X, o, o, X, o, o, 4);
        //Piece P3 = new("T2", X, X, X, o, o, X, o, o, 4);
        //Piece P4 = new("T3", X, X, X, o, o, X, o, o, 4);
        //Piece P5 = new("T4", X, X, X, o, o, X, o, o, 4);
        //Piece P6 = new("S'1", X, X, o, o, o, X, X, o, 2);
        //Piece P7 = new("S'2", X, X, o, o, o, X, X, o, 2);
        //Piece P8 = new("I1", X, X, X, X, o, o, o, o, 2);
        //Piece P9 = new("I2", X, X, X, X, o, o, o, o, 2);
        //Piece P10 = new("L'", X, o, o, o, X, X, X, o, 4);

        // Puzzle B-Special 2
        //ROWS = 4;
        //COLS = 7;
        //PIECES = 7;
        //Piece P1 = new("S", o, X, X, o, X, X, o, o, 2);
        //Piece P2 = new("S'", X, X, o, o, o, X, X, o, 2);
        //Piece P3 = new("I1", X, X, X, X, o, o, o, o, 2);
        //Piece P4 = new("I2", X, X, X, X, o, o, o, o, 2);
        //Piece P5 = new("T1", X, X, X, o, o, X, o, o, 4);
        //Piece P6 = new("T2", X, X, X, o, o, X, o, o, 4);
        //Piece P7 = new("L'", X, o, o, o, X, X, X, o, 4);

        // Puzzle B-Special 3
        //ROWS = 6;
        //COLS = 8;
        //PIECES = 12;
        //Piece P1 = new("T1", X, X, X, o, o, X, o, o, 4);
        //Piece P2 = new("T2", X, X, X, o, o, X, o, o, 4);
        //Piece P3 = new("O1", X, X, o, o, X, X, o, o, 1);
        //Piece P4 = new("O2", X, X, o, o, X, X, o, o, 1);
        //Piece P5 = new("I", X, X, X, X, o, o, o, o, 2);
        //Piece P6 = new("L1", X, X, X, o, X, o, o, o, 4);
        //Piece P7 = new("L2", X, X, X, o, X, o, o, o, 4);
        //Piece P8 = new("L'1", X, o, o, o, X, X, X, o, 4);
        //Piece P9 = new("L'2", X, o, o, o, X, X, X, o, 4);
        //Piece P10 = new("L'3", X, o, o, o, X, X, X, o, 4);
        //Piece P11 = new("S1", o, X, X, o, X, X, o, o, 2);
        //Piece P12 = new("S2", o, X, X, o, X, X, o, o, 2);

        // Puzzle B-Special 4
        ROWS = 6;
        COLS = 8;
        PIECES = 12;
        Piece P1 = new("T1", X, X, X, o, o, X, o, o, 4);
        Piece P2 = new("T2", X, X, X, o, o, X, o, o, 4);
        Piece P3 = new("O1", X, X, o, o, X, X, o, o, 1);
        Piece P4 = new("O2", X, X, o, o, X, X, o, o, 1);
        Piece P5 = new("I", X, X, X, X, o, o, o, o, 2);
        Piece P6 = new("S", o, X, X, o, X, X, o, o, 2);
        Piece P7 = new("S1'", X, X, o, o, o, X, X, o, 2);
        Piece P8 = new("S2'", X, X, o, o, o, X, X, o, 2);
        Piece P9 = new("S3", X, X, o, o, o, X, X, o, 2);
        Piece P10 = new("L'1", X, o, o, o, X, X, X, o, 4);
        Piece P11 = new("L'2", X, o, o, o, X, X, X, o, 4);
        Piece P12 = new("L'3", X, o, o, o, X, X, X, o, 4);

        //P1.Trace(1);
        //P2.Trace(2);
        //P3.Trace(3);
        //P4.Trace(4);
        //P5.Trace(5);
        //P6.Trace(6);
        //P7.Trace(7);
        //P8.Trace(8);
        //P9.Trace(9);
        //P10.Trace(10);
        //WriteLine("\n\n*********************************************");

        // Pieces to use, allowing easy indexed access (order is not meaningful)
        Lp = [P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12];

        Debug.Assert(Lp.Length == PIECES);
        Debug.Assert(4 * PIECES == ROWS * COLS);


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
                        //ca.Trace(Lp[i].Name, 1, 0);

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

        WriteLine($"--- Solution {sol}");
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
