// Piece.cs
// Represent a quadrimino over a 2x4 grid
//
// 2026-01-03   PV      First version recycling Pentamino solver code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Console;
using System.Linq;

internal sealed class Piece
{
    public string Name;	                        // Usual letter naming the piece
    public List<Carre44> Transformations = [];	// Up to 8 different transformations (rotations, symmetries)

    public Piece(string name,
                 bool b00, bool b01, bool b02, bool b03,
                 bool b10, bool b11, bool b12, bool b13,
                 int expectedTransformationsCount)
    {
        static int N(bool b) => Convert.ToInt32(b);

        if (N(b00) + N(b01) + N(b02) + N(b03) + N(b10) + N(b11) + N(b12) + N(b13) != 4)
            WriteLine($"Invalid definition of piece {name}");

        Name = name;
        // Start with identity transformation
        var t0 = new Carre44(b00, b01, b02, b03, b10, b11, b12, b13);
        Transformations.Add(t0);

        // Only 3 transformations in The Talos Principle, mirror transformations are not used
        for (var i = 1; i < 4; i++)
        {
            var ct = t0.Transformation(i);

            // Check if a previous transformation is identical to current transformation
            var alreadySeen = Transformations.Any(t => Carre44.SameAs(t, ct));
            if (!alreadySeen)
                Transformations.Add(ct);
        }

        Debug.Assert(expectedTransformationsCount == Transformations.Count);
    }

    // For dev/debug traces
    public void Trace(int p)
    {
        WriteLine("\n==============================");
        WriteLine($"Piece {p}: {Name} {Transformations.Count} transformation(s)");
        for (int i = 0; i < Transformations.Count; i++)
            Transformations[i].Trace(Name, p, i);
    }
}
