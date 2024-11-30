// Bonza class BoundingRectangle
// A simple rectangle with int coordinates to represent layout bounds
//
// 2017-08-05   PV      Extracted
// 2017-08-06   PV      Performance refactoring; Immutable; Constructor BoundingRectangle(Position min, Position max) is too slow
// 2017-08-30   PV      getHashCode, Equals, IEquatable
// 2021-11-13   PV      Net6 C#10
// 2023-05-25   PV      Net7 C#11 Replaced Immutable attribute by readonly structs
// 2024-11-15	PV		Net9 C#13

using System;

namespace Bonza.Generator;

/// <summary>A simple rectangle with int coordinates to represent layout bounds.</summary>
public readonly struct BoundingRectangle: IEquatable<BoundingRectangle>
{
    public Position Min { get; }
    public Position Max { get; }

    public BoundingRectangle(int minRow, int maxRow, int minColumn, int maxColumn) : this()
    {
        Min = new Position(minRow, minColumn);
        Max = new Position(maxRow, maxColumn);
    }

    public override string ToString() => $"Bounds[{Min}-{Max}]";

    public override int GetHashCode() => Min.GetHashCode() ^ Max.GetHashCode();

    public override bool Equals(object obj) => obj is BoundingRectangle rectangle && Equals(rectangle);

    public bool Equals(BoundingRectangle other) => Min == other.Min && Max == other.Max;

    public static bool operator ==(BoundingRectangle br1, BoundingRectangle br2) => br1.Equals(br2);

    public static bool operator !=(BoundingRectangle br1, BoundingRectangle br2) => !br1.Equals(br2);
}
