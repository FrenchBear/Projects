using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DetectClosedArea
{
    enum PaintStatus : byte
    {
        Unpainted, Interior, Border
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Cell
    {
        [FieldOffset(0)] public bool HzWall;
        [FieldOffset(1)] public bool VtWall;
        [FieldOffset(2)] public sbyte line;
        [FieldOffset(3)] public PaintStatus paint;
    }
}
