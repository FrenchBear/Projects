// Bonza class PositionOrientation
// Regroups a Position and an orientation flag
//
// 2017-08-05   PV      Extracted
// 2017-08-06   PV      Performance refactoring
// 2017-08-10   PV      Made it immutable

using System;

namespace Bonza.Generator
{
    /// <summary>Regroups a Position and an orientation flag.</summary>
    [Immutable]
    public struct PositionOrientation
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

    }


}
