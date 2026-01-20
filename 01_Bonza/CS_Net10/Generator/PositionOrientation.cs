// Bonza class PositionOrientation
// Regroups a Position and an orientation flag
//
// 2017-08-05   PV      Extracted
// 2017-08-06   PV      Performance refactoring
// 2017-08-10   PV      Made it immutable
// 2017-08-31   PV      getHashCode, Equals, IEquatable
// 2021-11-13   PV      Net6 C#10
// 2023-05-25   PV      Net7 C#11 Replaced Immutable attribute by readonly structs
// 2024-11-15	PV		Net9 C#13
// 2026-01-20	PV		Net10 C#14

using System;

namespace Bonza.Generator;

/// <summary>Regroups a Position and an orientation flag.</summary>
public readonly struct PositionOrientation: IEquatable<PositionOrientation>
{
    public int StartRow { get; }
    public int StartColumn { get; }
    public bool IsVertical { get; }

    public PositionOrientation(int startRow, int startColumn, bool isVertical)
    {
        StartRow = startRow;
        StartColumn = startColumn;
        IsVertical = isVertical;
    }

    public PositionOrientation(PositionOrientation copy)
    {
        StartRow = copy.StartRow;
        StartColumn = copy.StartColumn;
        IsVertical = copy.IsVertical;
    }

    public override string ToString() => $"PosOrient({StartRow}, {StartColumn}, {(IsVertical ? "V" : "H")})";

    public override int GetHashCode() => StartRow ^ StartColumn;

    public override bool Equals(object obj)
        => obj is PositionOrientation orientation && Equals(orientation);

    public bool Equals(PositionOrientation other) => StartRow == other.StartRow && StartColumn == other.StartColumn && IsVertical == other.IsVertical;

    public static bool operator ==(PositionOrientation po1, PositionOrientation po2) => po1.Equals(po2);

    public static bool operator !=(PositionOrientation po1, PositionOrientation po2) => !po1.Equals(po2);
}
