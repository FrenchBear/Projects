// Bonza class Position
// A simple pair of int coordinates to represent cell coordinates on layout
//
// 2017-08-05   PV      Extracted
// 2017-08-07   PV      Performance refactoring, made it immutable


namespace Bonza.Generator
{
    /// <summary>A simple pair of int coordinates to represent cell coordinates on layout.</summary>
    [Immutable]
    public struct Position
    {
        /// <summary>Row, increasing when going down (similar to text screen coordinates).</summary>
        public int Row { get; }

        /// <summary>Column, increasing when going right.</summary>
        public int Column { get; }

        /// <summary>Creates a new Position from two integers.</summary>
        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }


        public override string ToString()
        {
            return $"Pos({Row}, {Column})";
        }
    }
}
