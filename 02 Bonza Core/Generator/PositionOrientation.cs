// Bonza class PositionOrientation
// Regroups a Position and an orientation flag
//
// 2017-08-05   PV      Extracted
// 2017-08-06   PV      Performance refactoring
// 2017-08-10   PV      Made it immutable
// 2017-08-31   PV      getHashCode, Equals, IEquatable


using System;

namespace Bonza.Generator
{
    /// <summary>Regroups a Position and an orientation flag.</summary>
    [Immutable]
    public struct PositionOrientation : IEquatable<PositionOrientation>
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


        public override string ToString()
        {
            return $"PosOrient({StartRow}, {StartColumn}, {(IsVertical ? "V" : "H")})";
        }



        public override int GetHashCode()
        {
            return StartRow ^ StartColumn;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PositionOrientation))
                return false;

            return Equals((PositionOrientation)obj);
        }

        public bool Equals(PositionOrientation other)
        {
            return StartRow == other.StartRow && StartColumn == other.StartColumn && IsVertical == other.IsVertical;
        }

        public static bool operator ==(PositionOrientation po1, PositionOrientation po2)
        {
            return po1.Equals(po2);
        }

        public static bool operator !=(PositionOrientation po1, PositionOrientation po2)
        {
            return !po1.Equals(po2);
        }
    }
}