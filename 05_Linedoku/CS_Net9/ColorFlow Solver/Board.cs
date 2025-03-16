// Board
// Represent a ColorFlow grid and associated method
//
// 2017-10-27   PV
// 2023-11-20   PV      Net8 C#12

using System;
using System.Runtime.CompilerServices;
using static System.Console;

namespace ColorFlowSolver;

partial class Program
{
    static byte Side, Sidem1;
    static Line[] Lines = [];       // Init to keep variable non-null
    static Cell[] Grid = [];
    static byte FinalWallsCount;
    static int Solutions;
    static long CheckClosedAreasCalls;
    static long SolveSECalls;

    private static byte Δ(byte row, byte column) => (byte)((row << 4) + column);

    static void Board(byte side, Line[] lines)
    {
        Side = side;
        Sidem1 = (byte)(Side - 1);
        Lines = lines;
        FinalWallsCount = (byte)(Side * Side - Lines.Length);        // Number of walls to draw

        Solutions = 0;
        CheckClosedAreasCalls = 0;
        SolveSECalls = 0;

        Grid = new Cell[Side * 16];
        // All points are white at the beginning
        for (byte row = 0; row < Side; row++)
            for (byte column = 0; column < Side; column++)
                Grid[Δ(row, column)].Line = byte.MaxValue;

        // Except start and end of lines that are colored
        for (byte line = (byte)Lines.Length; --line != byte.MaxValue;)
        {
            byte δStart = Δ(Lines[line].startRow, Lines[line].startColumn);
            byte δEnd = Δ(Lines[line].endRow, Lines[line].endColumn);
            Grid[δStart].Line = line;
            Grid[δStart].IsStartLine = true;
            Grid[δEnd].Line = line;
            Grid[δEnd].IsEndLine = true;
            Lines[line].MinWalls = (byte)(Math.Abs(Lines[line].endRow - Lines[line].startRow) + Math.Abs(Lines[line].endColumn - Lines[line].startColumn));
            Lines[line].MaxWalls = line == (sbyte)(Lines.Length - 1)
                ? (byte)(Side * Side - Lines.Length)
                : (byte)(Lines[line + 1].MaxWalls - Lines[line + 1].MinWalls);
        }

        // The rest is initialized at 0 or false
    }

    /// <summary>
    /// Starting point of solving current puzzle for convenience
    /// </summary>
    /// <param name="firstSolutionOnly">If true, limits output to first solution</param>
    public static void Solve(bool firstSolutionOnly)
    {
        FirstSolutionOnly = firstSolutionOnly;
        SolveLine(0, 0);
    }

    private static readonly bool FullConnectivity = true;       // If true, all cells must be connected
    private static bool FirstSolutionOnly;             // If true, stops after 1st solution

    private static int Placements;
    private static bool AreasChecked;

    /// <summary>
    /// Solver starting placement of lines from a given index and next ones (recursively) until
    /// a solution is found
    /// </summary>
    /// <param name="line">Index of line to place</param>
    /// <param name="walls">Number of strokes already used, used to check if a solution implements full connectivity</param>
    /// <returns>True if a solution has been found and we should terminate, false to indicate a dead end, unwind and continue exploration</returns>
    static bool SolveLine(byte line, byte walls)
    {
        if ((++Placements & 0xffff) == 0)
        {
            if ((Placements >> 16) == 100)
            {
                Write("\n");
                Placements = 0;
            }
            else
                Write(".");
        }
        // All lines must be placed for a solution
        if (line == Lines.Length)
        {
            // Consider it a solution if all points have a connectivity of 2 (except the end of lines)
            // Unless FullConnectivity is false, in which case all lines placed is just enough
            if (!FullConnectivity || walls == FinalWallsCount)
            {
                Solutions++;
                PrintBoard($"Solution #{Solutions}, elapsed={stw!.Elapsed}, SolveSE={SolveSECalls:N0}, CheckClosedAreas={CheckClosedAreasCalls:N0}");
                return FirstSolutionOnly;        // Stop at 1st solution returning true
            }
            return false;
        }

        // After finishing a line, check closed areas, even if line doesn't touch borders
        if (!CheckClosedAreas((byte)(line - 1)))
            return false;

        PrintBoard($"Partial snapshot, elapsed={stw!.Elapsed}, SolveSE={SolveSECalls:N0}, CheckClosedAreas={CheckClosedAreasCalls:N0}");

        AreasChecked = false;

        // Place next line
        return SolveSE(line, walls, Lines[line].startRow, Lines[line].startColumn, Lines[line].endRow, Lines[line].endColumn);

    }

    /// <summary>
    /// Place a line knowing starting point (actually, current exploration point during recursive call) and end point 
    /// </summary>
    /// <param name="line">Index of line being placed</param>
    /// <param name="walls">Number of segments already draw</param>
    /// <param name="row">Start or current row</param>
    /// <param name="column">Start or current column</param>
    /// <param name="endRow">Target row to reach</param>
    /// <param name="endColumn">Target column to reach</param>
    /// <returns>true if a solution has been found, and no need to continue, false means no solution found, 
    /// unwind recursive call stack and continue</returns>
    private static bool SolveSE(byte line, byte walls, byte row, byte column, int endRow, int endColumn)
    {
        SolveSECalls++;

        // Reached target for current line?
        if (row == endRow && column == endColumn)
            return SolveLine(++line, walls);

        // If we've already drawn maximum number of walls permitted for a line, no need to continue
        if (walls >= Lines[line].MaxWalls)
            return false;

        bool res;
        if (row == 0 || row == Sidem1 || column == 0 || column == Sidem1)
        {
            if (!AreasChecked)
            {
                res = CheckClosedAreas(line);
                //PrintBoard($"CheckClosedAreas({line}) -> {res}");
                if (!res)
                    return false;
                AreasChecked = true;
            }
        }
        else
            AreasChecked = false;

        byte wp1 = (byte)(walls + 1);

        // Right
        if (column < Sidem1 && !GetHzWall(row, column))
        {
            byte cp1 = (byte)(column + 1);
            if ((row == endRow && cp1 == endColumn) || ExtremityOfLine(row, cp1) == byte.MaxValue)
                if (IsUnconnected(row, cp1))
                {
                    SetHzWall(row, column);
                    SetPointLine(row, cp1, line);
                    bool ac = AreasChecked;
                    res = SolveSE(line, wp1, row, cp1, endRow, endColumn);
                    AreasChecked = ac;
                    ResetHzWall(row, column);
                    ResetPointLine(row, cp1);
                    if (res)
                        return true;
                }
        }

        // Left
        byte cm1 = (byte)(column - 1);
        if (column > 0 && !GetHzWall(row, cm1))
            if ((row == endRow && cm1 == endColumn) || ExtremityOfLine(row, cm1) == byte.MaxValue)
                if (IsUnconnected(row, cm1))
                {
                    SetHzWall(row, cm1);
                    SetPointLine(row, cm1, line);
                    bool ac = AreasChecked;
                    res = SolveSE(line, wp1, row, cm1, endRow, endColumn);
                    AreasChecked = ac;
                    ResetPointLine(row, cm1);
                    ResetHzWall(row, cm1);
                    if (res)
                        return true;
                }

        // Up
        byte rm1 = (byte)(row - 1);
        if (row > 0 && !GetVtWall(rm1, column))
            if ((rm1 == endRow && column == endColumn) || ExtremityOfLine(rm1, column) == byte.MaxValue)
                if (IsUnconnected(rm1, column))
                {
                    SetVtWall(rm1, column);
                    SetPointLine(rm1, column, line);
                    bool ac = AreasChecked;
                    res = SolveSE(line, wp1, rm1, column, endRow, endColumn);
                    AreasChecked = ac;
                    ResetVtWall(rm1, column);
                    ResetPointLine(rm1, column);
                    if (res)
                        return true;
                }

        // Down
        if (row < Sidem1 && !GetVtWall(row, column))
        {
            byte rp1 = (byte)(row + 1);
            if ((rp1 == endRow && column == endColumn) || ExtremityOfLine(rp1, column) == byte.MaxValue)
                if (IsUnconnected(rp1, column))
                {
                    SetVtWall(row, column);
                    SetPointLine(rp1, column, line);
                    bool ac = AreasChecked;
                    res = SolveSE(line, wp1, rp1, column, endRow, endColumn);
                    AreasChecked = ac;
                    ResetVtWall(row, column);
                    ResetPointLine(rp1, column);
                    if (res)
                        return true;
                }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetHzWall(byte row, byte column) =>
        // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
        Grid[Δ(row, column)].HzWall;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetHzWall(byte row, byte column) =>
        // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
        // Debug.Assert(!GetHzWall(row, column));
        Grid[Δ(row, column)].HzWall = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ResetHzWall(byte row, byte column) =>
        // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
        // Debug.Assert(GetHzWall(row, column));
        Grid[Δ(row, column)].HzWall = false;// Debug.Assert(Walls >= 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetVtWall(byte row, byte column) =>
        // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
        Grid[Δ(row, column)].VtWall;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetVtWall(byte row, byte column) =>
        // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
        // Debug.Assert(!GetVtWall(row, column));
        Grid[Δ(row, column)].VtWall = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ResetVtWall(byte row, byte column) =>
        // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
        // Debug.Assert(GetVtWall(row, column));
        Grid[Δ(row, column)].VtWall = false;// Debug.Assert(Walls >= 0);

    // Number of walls linked to a point
    private static bool IsUnconnected(byte row, byte column)
    {
        // Debug.Assert(row >= 0 && row < Side && column >= 0 && column < Side);
        if (row > 0 && GetVtWall((byte)(row - 1), column))
            return false;         // Top
        if (column > 0 && GetHzWall(row, (byte)(column - 1)))
            return false;      // Left
        if (row < Sidem1 && GetVtWall(row, column))
            return false;      // Bottom
        if (column < Sidem1 && GetHzWall(row, column))
            return false;   // Right
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetPointLine(byte row, byte column, byte line) => Grid[Δ(row, column)].Line = line;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ResetPointLine(byte row, byte column)
    {
        if (!Grid[Δ(row, column)].IsEndLine)
            Grid[Δ(row, column)].Line = byte.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ExtremityOfLine(byte row, byte column)
    {
        //Cell c = Grid[Δ(row, column)];
        //return (c.IsStartLine || c.IsEndLine) ? c.Line : byte.MaxValue;
        byte δ = Δ(row, column);
        return (Grid[δ].IsStartLine || Grid[δ].IsEndLine) ? Grid[δ].Line : byte.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte EndOfLine(byte row, byte column)
    {
        //Cell c = Grid[Δ(row, column)];
        //return (c.IsEndLine) ? c.Line : byte.MaxValue;
        byte δ = Δ(row, column);
        return Grid[δ].IsEndLine ? Grid[δ].Line : byte.MaxValue;
    }

    internal static void PrintBoard(string header)
    {
        LogWriteLine();
        LogWriteLine(header);
        for (byte row = 0; row < Side; row++)
        {
            for (byte column = 0; column < Side; column++)
            {
                ForegroundColor = GetColor(row, column);
                LogWrite(ExtremityOfLine(row, column) < byte.MaxValue ? "■" : "▪");
                if (column < Sidem1)
                    LogWrite(GetHzWall(row, column) ? "──" : "  ");
            }
            LogWriteLine();

            if (row < Sidem1)
            {
                for (byte column = 0; column < Side; column++)
                {
                    ForegroundColor = GetColor(row, column);
                    LogWrite(GetVtWall(row, column) ? "│" : " ");
                    if (column < Side)
                        LogWrite("  ");
                }
                LogWriteLine();
            }
        }
        ForegroundColor = ConsoleColor.Gray;
        LogWriteLine();
    }

    private static ConsoleColor GetColor(byte row, byte column)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetColorIndex(byte row, byte column) => Grid[Δ(row, column)].Line;
}
