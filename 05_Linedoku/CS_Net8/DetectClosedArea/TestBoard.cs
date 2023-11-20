// 2023-11-20   PV      Net8 C#12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Console;

namespace DetectClosedArea;

class TestBoard
{
    private readonly int Side;
    private readonly int[,] PointLine;
    private readonly int[,] AreaId;

    internal TestBoard(int side)
    {
        Side = side;

        PointLine = new int[Side, Side];
        // All points are white at the beginning
        for (int row = 0; row < Side; row++)
            for (int column = 0; column < Side; column++)
                PointLine[row, column] = -1;

        AreaId = new int[side, side];
    }

    internal void Fill()
    {
        PointLine[1, 1] = 0;
        PointLine[2, 1] = 0;
        PointLine[2, 0] = 0;
        PointLine[3, 0] = 0;
        PointLine[3, 1] = 0;
        PointLine[3, 2] = 0;
        PointLine[3, 3] = 0;
        PointLine[2, 3] = 0;
        PointLine[1, 3] = 0;
        PointLine[0, 3] = 0;
        PointLine[0, 4] = 0;
        PointLine[0, 5] = 0;
        PointLine[1, 5] = 0;
        PointLine[2, 5] = 0;
        PointLine[3, 5] = 0;
        PointLine[3, 6] = 0;
        PointLine[4, 6] = 0;
        PointLine[5, 6] = 0;
        PointLine[5, 5] = 0;
        PointLine[5, 4] = 0;
        PointLine[5, 3] = 0;
        PointLine[6, 3] = 0;
    }

    internal void FindAreas(int line)
    {
        // -1 means no area identified yet
        // 0 this is line
        // 1, 2, ... Areas
        for (int row = 0; row < Side; row++)
            for (int column = 0; column < Side; column++)
                AreaId[row, column] = -1;
        int areaCount = 0;
        for (int row = 0; row < Side; row++)
            for (int column = 0; column < Side; column++)
                if (AreaId[row, column] == -1)
                {
                    if (PointLine[row, column] == line)
                        AreaId[row, column] = 0;
                    else
                    {
                        areaCount++;
                        ColorizeArea(row, column, line, areaCount);
                    }

                }

        WriteLine($"Areas: {areaCount}");
    }

    private void ColorizeArea(int row, int column, int line, int area)
    {
        var paintStack = new Stack<(int row, int column)>();
        WriteLine($"Colorize area {area} from [{row}, {column}]");
        paintStack.Push((row, column));

        while (paintStack.Count > 0)
        {
            (int r, int c) = paintStack.Pop();

            Debug.Assert(AreaId[r, c] == -1 || AreaId[r, c] == area);
            // If AreaId[r, c] == area, it's already been explored, no need to do it again
            if (AreaId[r, c] == -1)
            {
                WriteLine($"[{r}, {c}] -> {area}");
                AreaId[r, c] = area;
                if (r > 0 && AreaId[r - 1, c] == -1 && PointLine[r-1, c] != line) paintStack.Push((r - 1, c));
                if (c > 0 && AreaId[r, c - 1] == -1 && PointLine[r, c-1] != line) paintStack.Push((r, c - 1));
                if (r < Side - 1 && AreaId[r + 1, c] == -1 && PointLine[r+1, c] != line) paintStack.Push((r + 1, c));
                if (c < Side - 1 && AreaId[r, c + 1] == -1 && PointLine[r, c + 1] != line) paintStack.Push((r, c + 1));
            }
        }
    }

    internal void Print()
    {
        for (int row = 0; row < Side; row++)
        {
            for (int column = 0; column < Side; column++)
            {
                ForegroundColor = GetColor(row, column);
                Write("■");
                if (column < Side - 1)
                    Write(" ");
            }
            WriteLine();
        }
        ForegroundColor = ConsoleColor.White;
        WriteLine();
    }

    private ConsoleColor GetColor(int row, int column)
    {
        int i = GetColorIndex(row, column);
        return i switch
        {
            0 => ConsoleColor.Green,
            1 => ConsoleColor.Red,
            2 => ConsoleColor.Magenta,
            3 => ConsoleColor.Cyan,
            4 => ConsoleColor.Yellow,
            5 => ConsoleColor.Gray,
            6 => ConsoleColor.DarkGreen,
            7 => ConsoleColor.DarkYellow,
            8 => ConsoleColor.DarkRed,
            9 => ConsoleColor.DarkMagenta,
            _ => ConsoleColor.White,
        };
    }

    private int GetColorIndex(int row, int column)  // int lastRow = -1, int lastColumn = -1)
        => PointLine[row, column];
}
