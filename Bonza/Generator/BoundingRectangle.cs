// Bonza class BoundingRectangle
// A simple rectangle with int coordinates to represent layout bounds
//
// 2017-08-05   PV      Extracted
// 2017-08-06   PV      performance efactoring; Immutable; Constructor BoundingRectangle(Position min, Position max) is too slow


using System;

namespace Bonza.Generator
{
    /// <summary>A simple rectangle with int coordinates to represent layout bounds.</summary>
    [Immutable]
    public struct BoundingRectangle : IEquatable<BoundingRectangle>
    {
        public Position Min { get; }
        public Position Max { get; }

        public BoundingRectangle(int minRow, int maxRow, int minColumn, int maxColumn) : this()
        {
            Min = new Position(minRow, minColumn);
            Max = new Position(maxRow, maxColumn);
        }


        public override string ToString()
        {
            return $"Bounds[{Min}-{Max}]";
        }

        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BoundingRectangle))
                return false;

            return Equals((BoundingRectangle)obj);
        }

        public bool Equals(BoundingRectangle other)
        {
            return Min == other.Min && Max == other.Max;
        }

        public static bool operator ==(BoundingRectangle br1, BoundingRectangle br2)
        {
            return br1.Equals(br2);
        }

        public static bool operator !=(BoundingRectangle br1, BoundingRectangle br2)
        {
            return !br1.Equals(br2);
        }

    }

}