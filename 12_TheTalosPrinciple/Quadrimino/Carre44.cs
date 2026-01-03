// Carre44.cs
// Represent a transformation (rotation, symmetry) of a quadrimino over a 4x4 grid
//
// 2026-01-03   PV      First version recycling Pentamino solver code

using System;
using static System.Console;

internal sealed class Carre44
{
    public bool[,] Motif;
    public int Rmax, Cmax;		// Piece bounding dimensions (rows/columns count), each 1..4
    public int OffsetCol;		// Column shift to occupy cell [1, 1]

    private Carre44()
    {
        Motif = new bool[4, 4];
        for (var r = 0; r < 4; r++)
            for (var c = 0; c < 4; c++)
                Motif[r, c] = false;
        Rmax = Cmax = 0;
        OffsetCol = 0;
    }

    public Carre44(bool b00, bool b01, bool b02, bool b03,
                   bool b10, bool b11, bool b12, bool b13)
    {
        Motif = new bool[5, 5];

        Motif[0, 0] = b00;
        Motif[0, 1] = b01;
        Motif[0, 2] = b02;
        Motif[0, 3] = b03;
        Motif[1, 0] = b10;
        Motif[1, 1] = b11;
        Motif[1, 2] = b12;
        Motif[1, 3] = b13;
        Motif[3, 0] = false;
        Motif[3, 1] = false;
        Motif[3, 2] = false;
        Motif[3, 3] = false;
        Motif[4, 0] = false;
        Motif[4, 1] = false;
        Motif[4, 2] = false;
        Motif[4, 3] = false;

        Rmax = !(b10 || b11 || b12 || b13) ? 1 : 2;
        Cmax = !(b01 || b11) ? 1 : !(b02 || b12) ? 2 : !(b03 || b13) ? 3 : 4;

        MkOffset();
    }

    // Determine the iOffsetCol property, i.e. the number of columns it
    // must translate the drawing to the left to occupy the cell [0, 0]
    private void MkOffset()
        => OffsetCol = Motif[0, 0] ? 0 : Motif[0, 1] ? 1 : Motif[0, 2] ? 2 : 3;

    // Comparison operator
    // Simple static method is easier than operator == that requires operator != and override GetHashCode
    // and override Equals(object), none of this needed here
    public static bool SameAs(Carre44 l, Carre44 r)
        => l.Rmax == r.Rmax && l.Cmax == r.Cmax &&
            l.Motif[0, 0] == r.Motif[0, 0] && l.Motif[0, 1] == r.Motif[0, 1] && l.Motif[0, 2] == r.Motif[0, 2] && l.Motif[0, 3] == r.Motif[0, 3] &&
            l.Motif[1, 0] == r.Motif[1, 0] && l.Motif[1, 1] == r.Motif[1, 1] && l.Motif[1, 2] == r.Motif[1, 2] && l.Motif[1, 3] == r.Motif[1, 3] &&
            l.Motif[2, 0] == r.Motif[2, 0] && l.Motif[2, 1] == r.Motif[2, 1] && l.Motif[2, 2] == r.Motif[2, 2] && l.Motif[2, 3] == r.Motif[2, 3] &&
            l.Motif[3, 0] == r.Motif[3, 0] && l.Motif[3, 1] == r.Motif[3, 1] && l.Motif[3, 2] == r.Motif[3, 2] && l.Motif[3, 3] == r.Motif[3, 3];

    // Transformations

    // Line transform
    private int TL(int tr, int r, int c)
        => tr switch
        {
            1 => c,
            2 => Rmax - 1 - r,
            3 => Cmax - 1 - c,
            4 => r,
            5 => Cmax - 1 - c,
            6 => Rmax - 1 - r,
            7 => c,
            _ => r,// cas 0
        };

    // Column transform
    private int TC(int tr, int r, int c)
        => tr switch
        {
            1 => Rmax - 1 - r,
            2 => Cmax - 1 - c,
            3 => r,
            4 => Cmax - 1 - c,
            5 => Rmax - 1 - r,
            6 => c,
            7 => r,
            _ => c,// cas 0
        };

    // Transformations
    // 0: Identity
    // 1: 90°  clockwise
    // 2: 180°
    // 3: 270° clockwise
    // ============ Not used for Quadriminos in Talos Principle
    // 4: mirror Hz
    // 5: mirror Hz + 90°  clockwise
    // 6: mirror Hz + 180°
    // 7: mirror Hz + 270° clockwise

    public Carre44 Transformation(int tr)
    {
        Carre44 ct = new();

        for (int r = 0; r < Rmax; r++)
            for (int c = 0; c < Cmax; c++)
                ct.Motif[TL(tr, r, c), TC(tr, r, c)] = Motif[r, c];

        if ((tr & 1) != 0)
            (ct.Rmax, ct.Cmax) = (Cmax, Rmax);
        else
            (ct.Rmax, ct.Cmax) = (Rmax, Cmax);

        ct.MkOffset();

        return ct;
    }

    // For dev/debug traces
    public void Trace(string name, int p, int tr)
    {
        WriteLine("-------------------------");
        WriteLine($"Piece {name}, transformation {tr}");

        var bak = ForegroundColor;
        ForegroundColor = (ConsoleColor)p;
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
                Write(Motif[r, c] ? "██" : "··");
            WriteLine();
        }
        ForegroundColor = bak;
        WriteLine($"Lmax={Rmax} Cmax={Cmax} Offset={OffsetCol}");
    }
}
