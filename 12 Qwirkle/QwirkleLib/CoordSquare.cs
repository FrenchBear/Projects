// CoordSquare.cs - Convenient class grouping coordinates and a Square for solution analysis
// Qwirkle simulation project
// 2019-01-12   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#nullable enable

namespace QwirkleLib
{
    public class CoordSquare
    {
        public (int row, int col) Coord { get; }
        public Square Square { get; }

        public CoordSquare((int row, int col) coord, Square square)
        {
            Debug.Assert(square.Tile != null);
            Coord = coord;
            Square = square;
        }


        public override string ToString() => $"({Coord.row}, {Coord.col}) {Square.Tile!.ToString()}";
    }
}
