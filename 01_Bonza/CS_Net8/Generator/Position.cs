// Bonza class Position
// A simple pair of int coordinates to represent cell coordinates on layout
//
// 2017-08-05   PV      Extracted
// 2017-08-07   PV      Performance refactoring, made it immutable
// 2017-08-30   PV      getHashCode, Equals, IEquatable
// 2021-11-13   PV      Net6 C#10
// 2023-05-25   PV      Net7 C#11 Replaced Immutable attribute by readonly structs
// 2023-11-20   PV      Net8 C#12

using System;

namespace Bonza.Generator;

/// <summary>A simple pair of int coordinates to represent cell coordinates on layout.</summary>
/// <remarks>Creates a new Position from two integers.</remarks>
public readonly struct Position(int row, int column): IEquatable<Position>
{
    /// <summary>Row, increasing when going down (similar to text screen coordinates).</summary>
    public int Row { get; } = row;

    /// <summary>Column, increasing when going right.</summary>
    public int Column { get; } = column;

    public override string ToString() => $"Pos({Row}, {Column})";

    public override int GetHashCode() => Row ^ Column;

    public override bool Equals(object obj) => obj is Position position && Equals(position);

    public bool Equals(Position other) => Row == other.Row && Column == other.Column;

    public static bool operator ==(Position position1, Position position2) => position1.Equals(position2);

    public static bool operator !=(Position position1, Position position2) => !position1.Equals(position2);
}
