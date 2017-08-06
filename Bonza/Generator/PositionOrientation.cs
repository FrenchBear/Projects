// Bonza class PositionOrientation
// Regroups a Position and an orientation flag
//
// 2017-08-05   PV      Extracted
// 2017-08-06   PV      refactoring


namespace Bonza.Generator
{
    /// <summary>Regroups a Position and an orientation flag.</summary>
    public struct PositionOrientation
    {
        public Position Start;
        public bool IsVertical;


        public PositionOrientation(int startRow, int startColumn, bool isVertical) : this()
        {
            Start = new Position(startRow, startColumn);
            IsVertical = isVertical;
        }

        public PositionOrientation(Position start, bool isVertical) : this()
        {
            Start = start;
            IsVertical = isVertical;
        }

        public PositionOrientation(PositionOrientation copy) : this()
        {
            Start = copy.Start;
            IsVertical = copy.IsVertical;
        }

    }

}