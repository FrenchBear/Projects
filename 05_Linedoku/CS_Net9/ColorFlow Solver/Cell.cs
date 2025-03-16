// Cell
// Represent a ColorFlow cell in the grid
//
// 2017-10-27   PV
// 2023-11-20   PV      Net8 C#12

namespace ColorFlowSolver;

// Paint:
// Unpainted = No area identified yet
// Border = Cell of line color
// Interior = Cell of any other color than line color
enum PaintStatus: byte
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
    public byte Line;
    public PaintStatus Paint;
}
