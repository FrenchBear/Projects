// Bonza class PositionOrientation
// Regroups a Position and an orientation flag
//
// 2017-08-05   PV      Extracted
// 2017-08-06   PV      Performance refactoring

using System;

namespace Bonza.Generator
{
    /// <summary>Regroups a Position and an orientation flag.</summary>
    public struct PositionOrientation
    {
        public int StartRow { get; set; }
        public int StartColumn { get; set; }
        public bool IsVertical { get; set; }

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

        internal void SetIsVertical(bool value)
        {
            IsVertical=value;
        }
    }


}
