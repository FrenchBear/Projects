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
    internal class Board
    {
        private readonly int Side;
        private (int startRow, int startColumn, int endRow, int endColumn)[] Lines;

        private ulong HzWallBitsHi, HzWallBitsLo;
        private ulong VtWallBitsHi, VtWallBitsLo;
        private int Walls;

        private int[,] PointLine;

        public Board(int side, (int startRow, int startColumn, int endRow, int endColumn)[] lines)
        {
            Side = side;
            Lines = lines;

            PointLine = new int[Side, Side];
            // All points are white at the beginning
            for (int row = 0; row < Side; row++)
                for (int column = 0; column < Side; column++)
                    PointLine[row, column] = -1;
            // Except start of lines that are colored
            for (int i = 0; i < Lines.Length; i++)
                PointLine[Lines[i].startRow, Lines[i].startColumn] = i;

            // The rest is initialized at 0
        }


        /// <summary>
        /// Just a simple test building a solution manually to test Print()
        /// </summary>
        public void TestWalls()
        {
            Debug.Assert(Walls == 0);
            for (int row = 0; row < Side; row++)
                for (int column = 0; column < Side - 1; column++)
                    SetHzWall(row, column);
            Debug.Assert(Walls == Side * (Side - 1));
            for (int row = 0; row < Side - 1; row++)
                for (int column = 0; column < Side; column++)
                    SetVtWall(row, column);
            Debug.Assert(Walls == 2 * Side * (Side - 1));
            for (int row = 0; row < Side; row++)
                for (int column = 0; column < Side - 1; column++)
                    ResetHzWall(row, column);
            Debug.Assert(Walls == Side * (Side - 1));
            for (int row = 0; row < Side - 1; row++)
                for (int column = 0; column < Side; column++)
                    ResetVtWall(row, column);
            Debug.Assert(Walls == 0);

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
        public void Solve(bool firstSolutionOnly)
        {
            FirstSolutionOnly = firstSolutionOnly;
            SolveLine(0, 0);
        }

        private bool FullConnectivity = true;       // If true, all cells must be connected
        private bool FirstSolutionOnly;             // If true, stops after 1st solution

        /// <summary>
        /// Solver starting placement of lines from a given index and next ones (recursively) until
        /// a solution is found
        /// </summary>
        /// <param name="line">Index of line to place</param>
        /// <param name="level">Nomber of strokes already used, used to check if a solution implements full connectivity</param>
        /// <returns>True if a solution has been found and we should terminate, false to indicate a dead end, unwind and continue exploration</returns>
        private bool SolveLine(int line, int level)
        {
            // All lines must be placed for a solution
            if (line == Lines.Length)
            {
                // Consider it a solution if all points have a connectivity of 2 (except the end of lines)
                // Unless FullConnectivity is false, in which case all lines placed is just enough
                if (!FullConnectivity || level == Side * Side - Lines.Length)
                {
                    Print();
                    return FirstSolutionOnly;        // Stop at 1st solution returning true
                }
                return false;
            }

            // Place next line
            return SolveSE(line, level, Lines[line].startRow, Lines[line].startColumn, Lines[line].endRow, Lines[line].endColumn);
        }

        /// <summary>
        /// Place a line knowing starting point (actually, current exploration point during recursive call) and end point 
        /// </summary>
        /// <param name="line">Index of line being placed</param>
        /// <param name="level">Number of segments already draw</param>
        /// <param name="row">Start or current row</param>
        /// <param name="column">Start or current column</param>
        /// <param name="endRow">Target row to reach</param>
        /// <param name="endColumn">Target column to reach</param>
        /// <returns>true if a solution has been found, and no need to continue, false means no solution found, 
        /// unwind recursive call stack and continue</returns>
        private bool SolveSE(int line, int level, int row, int column, int endRow, int endColumn)
        {
            // // Debug.Assert(level <= Side * Side);

            if (level == 17) Print();
            if (IsShiftPressed()) Print();

            // Reached target for current line?
            if (row == endRow && column == endColumn)
                return SolveLine(line + 1, level);

            bool res;

            // Right
            if (column < Side - 1 && !GetHzWall(row, column))
                if ((row == endRow && column + 1 == endColumn) || EndPointOfLine(row, column + 1) < 0)
                    if (Connectivity(row, column + 1) == 0)
                    {
                        SetHzWall(row, column);
                        SetPointLine(row, column + 1, line);
                        res = SolveSE(line, level + 1, row, column + 1, endRow, endColumn);
                        ResetHzWall(row, column);
                        ResetPointLine(row, column + 1, line);
                        if (res) return true;
                    }

            // Left
            if (column > 0 && !GetHzWall(row, column - 1))
                if ((row == endRow && column - 1 == endColumn) || EndPointOfLine(row, column - 1) < 0)
                    if (Connectivity(row, column - 1) == 0)
                    {
                        SetHzWall(row, column - 1);
                        SetPointLine(row, column - 1, line);
                        res = SolveSE(line, level + 1, row, column - 1, endRow, endColumn);
                        ResetPointLine(row, column - 1, line);
                        ResetHzWall(row, column - 1);
                        if (res) return true;
                    }

            // Up
            if (row > 0 && !GetVtWall(row, column))
                if ((row - 1 == endRow && column == endColumn) || EndPointOfLine(row - 1, column) < 0)
                    if (Connectivity(row - 1, column) == 0)
                    {
                        SetVtWall(row - 1, column);
                        SetPointLine(row - 1, column, line);
                        res = SolveSE(line, level + 1, row - 1, column, endRow, endColumn);
                        ResetVtWall(row - 1, column);
                        ResetPointLine(row - 1, column, line);
                        if (res) return true;
                    }

            // Down
            if (row < Side - 1 && !GetVtWall(row, column))
                if ((row + 1 == endRow && column == endColumn) || EndPointOfLine(row + 1, column) < 0)
                    if (Connectivity(row + 1, column) == 0)
                    {
                        SetVtWall(row, column);
                        SetPointLine(row + 1, column, line);
                        res = SolveSE(line, level + 1, row + 1, column, endRow, endColumn);
                        ResetVtWall(row, column);
                        ResetPointLine(row + 1, column, line);
                        if (res) return true;
                    }

            return false;
        }


        /// <summary>
        /// Returns bit index of a cell from its coordinates = 2^(Side*row+column)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool isHigh, ulong mask) WallMask(int row, int column)
        {
            int bit = Side * row + column;
            // Debug.Assert(bit >= 0 && bit < 128);
            if (bit < 64)
                return (false, 1ul << bit);
            else
                return (true, 1ul << (bit - 64));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetHzWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            (bool isHigh, ulong mask) = WallMask(row, column);
            if (isHigh)
                return (HzWallBitsHi & mask) != 0;
            else
                return (HzWallBitsLo & mask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetHzWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            // Debug.Assert(!GetHzWall(row, column));
            (bool isHigh, ulong mask) = WallMask(row, column);
            if (isHigh)
                HzWallBitsHi |= mask;
            else
                HzWallBitsLo |= mask;
            Walls++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetHzWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            // Debug.Assert(GetHzWall(row, column));
            (bool isHigh, ulong mask) = WallMask(row, column);
            if (isHigh)
                HzWallBitsHi &= ~mask;
            else
                HzWallBitsLo &= ~mask;
            Walls--;
            // Debug.Assert(Walls >= 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetVtWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            (bool isHigh, ulong mask) = WallMask(row, column);
            if (isHigh)
                return (VtWallBitsHi & mask) != 0;
            else
                return (VtWallBitsLo & mask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetVtWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            // Debug.Assert(!GetVtWall(row, column));
            (bool isHigh, ulong mask) = WallMask(row, column);
            if (isHigh)
                VtWallBitsHi |= mask;
            else
                VtWallBitsLo |= mask;
            Walls++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetVtWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            // Debug.Assert(GetVtWall(row, column));
            (bool isHigh, ulong mask) = WallMask(row, column);
            if (isHigh)
                VtWallBitsHi &= ~mask;
            else
                VtWallBitsLo &= ~mask;
            Walls--;
            // Debug.Assert(Walls >= 0);
        }

        // Number of walls linked to a point
        private int Connectivity(int row, int column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column < Side);
            int c = 0;
            if (row > 0 && GetVtWall(row - 1, column)) c++;         // Top
            if (column > 0 && GetHzWall(row, column - 1)) c++;      // Left
            if (row < Side - 1 && GetVtWall(row, column)) c++;      // Bottom
            if (column < Side - 1 && GetHzWall(row, column)) c++;   // Right
            return c;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetPointLine(int row, int column, int line)
        {
            Debug.Assert(PointLine[row, column] == -1);
            PointLine[row, column] = line;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetPointLine(int row, int column, int line)
        {
            Debug.Assert(PointLine[row, column] == line);
            PointLine[row, column] = -1;
        }


        //private bool IsEndPoint(int row, int column)
        //{
        //    //Lines.Any(l => (l.startRow == row && l.startColumn == column) || (l.endRow == row && l.endColumn == column));
        //    for (int i = 0; i < Lines.Length; i++)
        //    {
        //        if (Lines[i].startRow == row && Lines[i].startColumn == column) return true;
        //        if (Lines[i].endRow == row && Lines[i].endColumn == column) return true;
        //    }
        //    return false;
        //}

        private int EndPointOfLine(int row, int column)
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].startRow == row && Lines[i].startColumn == column) return i;
                if (Lines[i].endRow == row && Lines[i].endColumn == column) return i;
            }
            return -1;
        }

        internal void Print()
        {
            /*
            WriteLine($"Size: {Side}");
            WriteLine($"Lines: {Lines.Count()}");
            foreach (var line in Lines)
                WriteLine($"  ({line.startRow}, {line.startColumn}) -> ({line.endRow}, {line.endColumn})");
            */

            for (int row = 0; row < Side; row++)
            {
                for (int column = 0; column < Side; column++)
                {
                    Console.ForegroundColor = GetColor(row, column);
                    Write(EndPointOfLine(row, column) >= 0 ? "■" : "▪");
                    if (column < Side - 1)
                        Write(GetHzWall(row, column) ? "──" : "  ");
                }
                WriteLine();

                if (row < Side - 1)
                {
                    for (int column = 0; column < Side; column++)
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

            /*
            for (int row = 0; row < Side; row++)
                for (int column = 0; column < Side - 1; column++)
                    if (GetHzWall(row, column))
                        Write($"SetHzWall({row}, {column});");
            WriteLine();
            for (int row = 0; row < Side - 1; row++)
                for (int column = 0; column < Side; column++)
                    if (GetVtWall(row, column))
                        Write($"SetVtWall({row}, {column});");
            WriteLine();
            */
        }

        private ConsoleColor GetColor(int row, int column)
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

        private int GetColorIndex(int row, int column)  // int lastRow = -1, int lastColumn = -1)
        {
            int ix = PointLine[row, column];
            if (ix >= 0) return ix;
            return EndPointOfLine(row, column);

            //for (; ; )
            //{
            //    // If we have reach an end, we can return color
            //    for (int i = 0; i < Lines.Length; i++)
            //    {
            //        if (Lines[i].startRow == row && Lines[i].startColumn == column) return i;
            //        if (Lines[i].endRow == row && Lines[i].endColumn == column) return i;
            //    }

            //    if (Connectivity(row, column) == 0)
            //        return -1;

            //    // Go to next connected cell avoiding previous one
            //    if (row > 0 && GetVtWall(row - 1, column) && (row - 1 != lastRow || column != lastColumn))
            //    {
            //        int c = GetColorIndex(row - 1, column, row, column);
            //        if (c >= 0) return c;
            //    }
            //    if (row < Side - 1 && GetVtWall(row, column) && (row + 1 != lastRow || column != lastColumn))
            //    {
            //        int c = GetColorIndex(row + 1, column, row, column);
            //        if (c >= 0) return c;
            //    }
            //    if (column > 0 && GetHzWall(row, column - 1) && (row != lastRow || column - 1 != lastColumn))
            //    {
            //        int c = GetColorIndex(row, column - 1, row, column);
            //        if (c >= 0) return c;
            //    }
            //    if (column < Side - 1 && GetHzWall(row, column) && (row != lastRow || column + 1 != lastColumn))
            //    {
            //        int c = GetColorIndex(row, column + 1, row, column);
            //        if (c >= 0) return c;
            //    }
            //    return -1;
            //}
        }


        internal bool IsShiftPressed()
        {
            return System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) ||
                   System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
        }


    }
}
