// Board
// Represent a LinePath grid and associated method
// 2017-10-27   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace LinePath_Solver
{
    partial class Program
    {
        private static byte Side, Sidem1;
        private static Line[] Lines;
        private static byte FinalWallsCount;
        private static Cell[] Grid;
        private static long SolveSECalls;

        private static byte Δ(byte row, byte column) => (byte)((row << 4) + column);

        static void Board(byte side, Line[] lines)
        {
            Side = side;
            Sidem1 = (byte)(Side - 1);
            Lines = lines;
            FinalWallsCount = (byte)(Side * Side - Lines.Length);        // Number of walls to draw

            Grid = new Cell[Side * 16];
            // All points are white at the beginning
            for (byte row = 0; row < Side; row++)
                for (byte column = 0; column < Side; column++)
                {
                    Grid[Δ(row, column)].Line = -1;
                    //Grid[Δ(row, column)].EndPointOfLine = -1;
                }
            // Except start and end of lines that are colored
            for (sbyte line = (sbyte)Lines.Length; --line >=0 ; )
            {
                byte δStart = Δ(Lines[line].startRow, Lines[line].startColumn);
                byte δEnd = Δ(Lines[line].endRow, Lines[line].endColumn);
                Grid[δStart].Line = line;
                Grid[δStart].IsStartLine = true;
                //Grid[δStart].EndPointOfLine = line;
                Grid[δEnd].Line = line;
                Grid[δEnd].IsEndLine = true;
                //Grid[δEnd].EndPointOfLine = line;
                Lines[line].MinWalls = (byte)(Math.Abs(Lines[line].endRow - Lines[line].startRow) + Math.Abs(Lines[line].endColumn - Lines[line].startColumn));
                if (line == (sbyte)(Lines.Length - 1))
                    Lines[line].MaxWalls = (byte)(Side * Side - Lines.Length);
                else
                    Lines[line].MaxWalls = (byte)(Lines[line + 1].MaxWalls - Lines[line + 1].MinWalls);
            }

            // The rest is initialized at 0 or false
        }


        /// <summary>
        /// Just a simple test building a solution manually to test Print()
        /// </summary>
        public void TestWalls()
        {
            for (byte row = 0; row < Side; row++)
                for (byte column = 0; column < Sidem1; column++)
                    SetHzWall(row, column);
            for (byte row = 0; row < Sidem1; row++)
                for (byte column = 0; column < Side; column++)
                    SetVtWall(row, column);
            for (byte row = 0; row < Side; row++)
                for (byte column = 0; column < Sidem1; column++)
                    ResetHzWall(row, column);
            for (byte row = 0; row < Sidem1; row++)
                for (byte column = 0; column < Side; column++)
                    ResetVtWall(row, column);

            //    SetHzWall(0, 1); SetHzWall(0, 2); SetHzWall(0, 3);
            //    SetHzWall(1, 0); SetHzWall(1, 1); SetHzWall(1, 3);
            //    SetHzWall(2, 0); SetHzWall(2, 2); SetHzWall(2, 3);
            //    SetHzWall(3, 2);
            //    SetHzWall(4, 1);

            //    SetVtWall(0, 0); SetVtWall(2, 0); SetVtWall(3, 0);
            //    SetVtWall(3, 1);
            //    SetVtWall(1, 2); SetVtWall(3, 2);
            //    SetVtWall(3, 3);
            //    SetVtWall(0, 4); SetVtWall(2, 4); SetVtWall(3, 4);
        }

        internal void Test2()
        {
            SetHzWall(0, 0); SetHzWall(0, 1); SetHzWall(0, 2); SetHzWall(0, 3); SetHzWall(0, 4); SetHzWall(0, 5); SetHzWall(0, 6); SetHzWall(0, 7); SetHzWall(0, 8); SetHzWall(0, 9); SetHzWall(1, 0); SetHzWall(1, 1); SetHzWall(1, 2); SetHzWall(1, 3); SetHzWall(1, 4); SetHzWall(1, 5); SetHzWall(1, 7); SetHzWall(1, 8); SetHzWall(1, 9); SetHzWall(2, 6); SetHzWall(2, 7); SetHzWall(2, 8); SetHzWall(3, 0); SetHzWall(3, 1); SetHzWall(3, 2); SetHzWall(3, 3); SetHzWall(3, 4); SetHzWall(3, 5); SetHzWall(3, 6); SetHzWall(3, 7); SetHzWall(3, 8); SetHzWall(4, 0); SetHzWall(4, 1); SetHzWall(4, 2); SetHzWall(4, 3); SetHzWall(4, 4); SetHzWall(4, 5); SetHzWall(4, 6); SetHzWall(4, 7); SetHzWall(4, 8); SetHzWall(4, 9); SetHzWall(5, 0); SetHzWall(5, 1); SetHzWall(5, 2); SetHzWall(5, 3); SetHzWall(5, 4); SetHzWall(5, 5); SetHzWall(5, 6); SetHzWall(5, 7); SetHzWall(5, 8); SetHzWall(5, 9); SetHzWall(6, 0); SetHzWall(6, 1); SetHzWall(6, 2); SetHzWall(6, 3); SetHzWall(6, 4); SetHzWall(6, 5); SetHzWall(6, 6); SetHzWall(6, 7); SetHzWall(7, 1); SetHzWall(7, 2); SetHzWall(7, 3); SetHzWall(7, 5); SetHzWall(7, 8); SetHzWall(7, 9); SetHzWall(8, 1); SetHzWall(8, 4); SetHzWall(8, 6); SetHzWall(8, 7); SetHzWall(8, 8); SetHzWall(8, 9); SetHzWall(9, 2); SetHzWall(9, 4); SetHzWall(9, 5); SetHzWall(9, 6); SetHzWall(9, 7); SetHzWall(10, 3); SetHzWall(10, 4); SetHzWall(10, 5); SetHzWall(10, 6); SetHzWall(10, 7);
            SetVtWall(0, 0); SetVtWall(0, 10); SetVtWall(1, 6); SetVtWall(2, 9); SetVtWall(3, 0); SetVtWall(4, 10); SetVtWall(5, 0); SetVtWall(6, 8); SetVtWall(7, 1); SetVtWall(7, 4); SetVtWall(7, 5); SetVtWall(7, 6); SetVtWall(7, 10); SetVtWall(8, 2); SetVtWall(9, 3); SetVtWall(9, 8);
            Print();
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

        private static bool FullConnectivity = true;       // If true, all cells must be connected
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
        static bool SolveLine(sbyte line, byte walls)
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
            if (IsShiftPressed()) Print();

            // All lines must be placed for a solution
            if (line == Lines.Length)
            {
                // Consider it a solution if all points have a connectivity of 2 (except the end of lines)
                // Unless FullConnectivity is false, in which case all lines placed is just enough
                if (!FullConnectivity || walls == FinalWallsCount)
                {
                    Print();
                    return FirstSolutionOnly;        // Stop at 1st solution returning true
                }
                return false;
            }

            // Check if placed line closed areas are OK
            //if (line > 0 && !CheckClosedAreas((sbyte)(line - 1))) return false;
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
        private static bool SolveSE(sbyte line, byte walls, byte row, byte column, int endRow, int endColumn)
        {
            SolveSECalls++;

            // Reached target for current line?
            if (row == endRow && column == endColumn)
                return SolveLine(++line, walls);

            // If we've already drawn maximum number of walls permitted for a line, no need to continue
            if (walls >= Lines[line].MaxWalls)
                return false;

            if (row == 0 || row == Sidem1 || column == 0 || column == Sidem1)
            {
                if (!AreasChecked)
                {
                    if (!CheckClosedAreas(line)) return false;
                    AreasChecked = true;
                }
            }
            else
                AreasChecked = false;

            bool res;
            byte wp1 = (byte)(walls + 1);

            // Right
            if (column < Sidem1 && !GetHzWall(row, column))
            {
                byte cp1 = (byte)(column + 1);
                if ((row == endRow && cp1 == endColumn) || EndPointOfLine(row, cp1) < 0)
                    if (IsUnconnected(row, cp1))
                    {
                        SetHzWall(row, column);
                        SetPointLine(row, cp1, line);
                        bool ac = AreasChecked;
                        res = SolveSE(line, wp1, row, cp1, endRow, endColumn);
                        AreasChecked = ac;
                        ResetHzWall(row, column);
                        ResetPointLine(row, cp1);
                        if (res) return true;
                    }
            }

            // Left
            byte cm1 = (byte)(column - 1);
            if (column > 0 && !GetHzWall(row, cm1))
                if ((row == endRow && cm1 == endColumn) || EndPointOfLine(row, cm1) < 0)
                    if (IsUnconnected(row, cm1))
                    {
                        SetHzWall(row, cm1);
                        SetPointLine(row, cm1, line);
                        bool ac = AreasChecked;
                        res = SolveSE(line, wp1, row, cm1, endRow, endColumn);
                        AreasChecked = ac;
                        ResetPointLine(row, cm1);
                        ResetHzWall(row, cm1);
                        if (res) return true;
                    }

            // Up
            byte rm1 = (byte)(row - 1);
            if (row > 0 && !GetVtWall(rm1, column))
                if ((rm1 == endRow && column == endColumn) || EndPointOfLine(rm1, column) < 0)
                    if (IsUnconnected(rm1, column))
                    {
                        SetVtWall(rm1, column);
                        SetPointLine(rm1, column, line);
                        bool ac = AreasChecked;
                        res = SolveSE(line, wp1, rm1, column, endRow, endColumn);
                        AreasChecked = ac;
                        ResetVtWall(rm1, column);
                        ResetPointLine(rm1, column);
                        if (res) return true;
                    }

            // Down
            if (row < Sidem1 && !GetVtWall(row, column))
            {
                byte rp1 = (byte)(row + 1);
                if ((rp1 == endRow && column == endColumn) || EndPointOfLine(rp1, column) < 0)
                    if (IsUnconnected(rp1, column))
                    {
                        SetVtWall(row, column);
                        SetPointLine(rp1, column, line);
                        bool ac = AreasChecked;
                        res = SolveSE(line, wp1, rp1, column, endRow, endColumn);
                        AreasChecked = ac;
                        ResetVtWall(row, column);
                        ResetPointLine(rp1, column);
                        if (res) return true;
                    }
            }

            return false;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetHzWall(byte row, byte column)
        {
            // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            return Grid[Δ(row, column)].HzWall;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetHzWall(byte row, byte column)
        {
            // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            // Debug.Assert(!GetHzWall(row, column));
            Grid[Δ(row, column)].HzWall = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResetHzWall(byte row, byte column)
        {
            // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            // Debug.Assert(GetHzWall(row, column));
            Grid[Δ(row, column)].HzWall = false;
            // Debug.Assert(Walls >= 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetVtWall(byte row, byte column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            return Grid[Δ(row, column)].VtWall;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetVtWall(byte row, byte column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            // Debug.Assert(!GetVtWall(row, column));
            Grid[Δ(row, column)].VtWall = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResetVtWall(byte row, byte column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            // Debug.Assert(GetVtWall(row, column));
            Grid[Δ(row, column)].VtWall = false;
            // Debug.Assert(Walls >= 0);
        }

        // Number of walls linked to a point
        private static bool IsUnconnected(byte row, byte column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column < Side);
            if (row > 0 && GetVtWall((byte)(row - 1), column)) return false;         // Top
            if (column > 0 && GetHzWall(row, (byte)(column - 1))) return false;      // Left
            if (row < Sidem1 && GetVtWall(row, column)) return false;      // Bottom
            if (column < Sidem1 && GetHzWall(row, column)) return false;   // Right
            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetPointLine(byte row, byte column, sbyte line)
        {
            //if (!Grid[Δ(row, column].IsEndLine)
            Grid[Δ(row, column)].Line = line;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResetPointLine(byte row, byte column)
        {
            if (!Grid[Δ(row, column)].IsEndLine)
                Grid[Δ(row, column)].Line = -1;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static sbyte EndPointOfLine(byte row, byte column)
        {
            //return Grid[Δ(row, column)].EndPointOfLine;
            Cell c = Grid[Δ(row, column)];
            return (c.IsStartLine || c.IsEndLine) ? c.Line : (sbyte)-1;
        }
 


        internal static void Print()
        {
            WriteLine();
            for (byte row = 0; row < Side; row++)
            {
                for (byte column = 0; column < Side; column++)
                {
                    Console.ForegroundColor = GetColor(row, column);
                    Write(EndPointOfLine(row, column) >= 0 ? "■" : "▪");
                    if (column < Sidem1)
                        Write(GetHzWall(row, column) ? "──" : "  ");
                }
                WriteLine();

                if (row < Sidem1)
                {
                    for (byte column = 0; column < Side; column++)
                    {
                        Console.ForegroundColor = GetColor(row, column);
                        Write(GetVtWall(row, column) ? "│" : " ");
                        if (column < Side)
                            Write("  ");
                    }
                    WriteLine();
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
            WriteLine();
        }

        private static ConsoleColor GetColor(byte row, byte column)
        {
            int i = GetColorIndex(row, column);
            switch (i)
            {
                case 0: return ConsoleColor.Green;
                case 1: return ConsoleColor.Red;
                case 2: return ConsoleColor.Magenta;
                case 3: return ConsoleColor.Cyan;
                case 4: return ConsoleColor.Yellow;
                case 5: return ConsoleColor.Gray;
                case 6: return ConsoleColor.DarkGreen;
                case 7: return ConsoleColor.DarkYellow;
                case 8: return ConsoleColor.DarkRed;
                case 9: return ConsoleColor.DarkMagenta;
                default: return ConsoleColor.White;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static sbyte GetColorIndex(byte row, byte column) => Grid[Δ(row, column)].Line;


        internal static bool IsShiftPressed()
        {
            return System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) ||
                   System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
        }


    }
}
