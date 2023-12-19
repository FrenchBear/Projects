﻿// 2023-11-20   PV      Net8 C#12

namespace ColorFlowSolver;

struct Line(byte startRow, byte startColumn, byte endRow, byte endColumn)
{
    public byte startRow = startRow;
    public byte startColumn = startColumn;
    public byte endRow = endRow;
    public byte endColumn = endColumn;
    public byte MaxWalls = 0, MinWalls = 0;
}
