// Bonza class Position
// A simple pair of int coordinates to represent cell coordinates on layout
//
// 2017-08-05   PV      Extracted
// 2017-08-07   PV      Performance refactoring, made it immutable
// 2017-08-30   PV      getHashCode, Equals, IEquatable
// 2021-11-13   PV      Net6 C#10
// 2023-05-25   PV      Net7 C#11 Replaced Immutable attribute by readonly structs

using System;

namespace Bonza.Generator;

/// <summary>A simple pair of int coordinates to represent cell coordinates on layout.</summary>
public readonly struct Position: IEquatable<Position>
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

    public override string ToString() => $"Pos({Row}, {Column})";

    public override int GetHashCode() => Row ^ Column;

    public override bool Equals(object obj)
    {
        if (obj is not Position)
            return false;

        return Equals((Position)obj);
    }

    public bool Equals(Position other) => Row == other.Row && Column == other.Column;

    public static bool operator ==(Position position1, Position position2) => position1.Equals(position2);

    public static bool operator !=(Position position1, Position position2) => !position1.Equals(position2);
}
