// Bonza class PositionOrientation
// A location and a boolean
//
// 2017-08-05   PV      Extracted

namespace Bonza.Generator
{
    public struct PositionOrientation2
    {
        public Position Start;
        public bool IsVertical;

        public PositionOrientation2(int startRow, int startColumn, bool isVertical) : this()
        {
            Start = new Position(startRow, startColumn);
            IsVertical = isVertical;
        }

        public PositionOrientation2(Position start, bool isVertical) : this()
        {
            Start = start;
            IsVertical = isVertical;
        }

        public PositionOrientation2(PositionOrientation2 copy) : this()
        {
            Start = copy.Start;
            IsVertical = copy.IsVertical;
        }

    }

    public struct PositionOrientation
    {
        public int StartRow { get; set; }
        public int StartColumn { get; set; }
        public bool IsVertical { get; set; }

        public PositionOrientation(PositionOrientation copy)
        {
            StartRow = copy.StartRow;
            StartColumn = copy.StartColumn;
            IsVertical = copy.IsVertical;
        }
    }


}