// Bonza class BoundingRectangle
// A simple rectangle with int coordinates to represent layout bounds
//
// 2017-08-05   PV      Extracted
// 2017-08-06   PV      Refactoring


namespace Bonza.Generator
{
    /// <summary>A simple rectangle with int coordinates to represent layout bounds.</summary>
    public struct BoundingRectangle
    {
        public Position Min, Max;

        public BoundingRectangle(Position min, Position max) : this()
        {
            Min = min;
            Max = max;
        }
    }
}