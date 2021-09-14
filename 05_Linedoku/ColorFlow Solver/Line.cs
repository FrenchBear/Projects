using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorFlow_Solver
{
    struct Line
    {
        public byte startRow;
        public byte startColumn;
        public byte endRow;
        public byte endColumn;
        public byte MaxWalls, MinWalls;

        public Line(byte startRow, byte startColumn, byte endRow, byte endColumn)
        {
            this.startRow = startRow;
            this.startColumn = startColumn;
            this.endRow = endRow;
            this.endColumn = endColumn;
            MinWalls = 0;
            MaxWalls = 0;
        }
    }
}
