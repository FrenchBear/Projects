// Cell
// Represent a LinePath cell in the grid
// 2017-10-27   PV

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LinePath_Solver
{
    // Paint:
    // Unpainted = No area identified yet
    // Border = Cell of line color
    // Interior = Cell of any other color than line color
    enum PaintStatus : byte
    {
        Unpainted, Interior, Border
    }

    //[StructLayout(LayoutKind.Explicit)]
    struct Cell
    {
        //[FieldOffset(0)]
        public bool HzWall;
        public bool VtWall;
        public bool IsStartLine;
        public bool IsEndLine;
        public sbyte Line;
        //public PaintStatus Paint;
    }
}
