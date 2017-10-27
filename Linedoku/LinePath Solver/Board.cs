using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace LinePath_Solver
{
    internal class Board
    {
        private readonly int Side;
        private (int startRow, int startColumn, int endRow, int endColumn)[] Lines;

        private ulong HzWallBits, VtWallBits;
        private int Walls;

        public Board(int side, (int startRow, int startColumn, int endRow, int endColumn)[] lines)
        {
            this.Side = side;
            this.Lines = lines;
            Walls = 0;
            HzWallBits = 0;
            VtWallBits = 0;
        }

        public void Test()
        {
            SetHzWall(0, 1); SetHzWall(0, 2); SetHzWall(0, 3);
            SetHzWall(1, 0); SetHzWall(1, 1); SetHzWall(1, 3);
            SetHzWall(2, 0); SetHzWall(2, 2); SetHzWall(2, 3);
            SetHzWall(3, 2);
            SetHzWall(4, 1);

            SetVtWall(0, 0); SetVtWall(2, 0); SetVtWall(3, 0);
            SetVtWall(3, 1);
            SetVtWall(1, 2); SetVtWall(3, 2);
            SetVtWall(3, 3);
            SetVtWall(0, 4); SetVtWall(2, 4); SetVtWall(3, 4);
        }

        public void Solve0()
        {
            SolveLine(0, 0);
        }

        private bool SolveLine(int line, int level)
        {
            if (line == Lines.Length)
            {
                // Consider it a solution if all point have a connectivity of 2 (except the end of lines)
                //if (level == Side * Side - Lines.Length)
                //{
                    Print();
                    return true;        // Stop at 1st solution returning true
                //}
                //return false;
            }

            return SolveSE(line, level, Lines[line].startRow, Lines[line].startColumn, Lines[line].endRow, Lines[line].endColumn);
        }

        private bool SolveSE(int line, int level, int row, int column, int endRow, int endColumn)
        {
            // Debug.Assert(level <= Side * Side);

            // Reached target for current line?
            if (row == endRow && column == endColumn)
                return SolveLine(line + 1, level);

            // Right
            if (column < Side - 1 && !GetHzWall(row, column))
                if ((row == endRow && column + 1 == endColumn) || !IsEndPoint(row, column + 1))
                    if (Connectivity(row, column + 1) == 0)
                    {
                        SetHzWall(row, column);
                        bool res = SolveSE(line, level + 1, row, column + 1, endRow, endColumn);
                        ResetHzWall(row, column);
                        if (res) return true;
                    }

            // Left
            if (column > 0 && !GetHzWall(row, column - 1))
                if ((row == endRow && column - 1 == endColumn) || !IsEndPoint(row, column - 1))
                    if (Connectivity(row, column - 1) == 0)
                    {
                        SetHzWall(row, column - 1);
                        bool res = SolveSE(line, level + 1, row, column - 1, endRow, endColumn);
                        ResetHzWall(row, column - 1);
                        if (res) return true;
                    }

            // Up
            if (row > 0 && !GetVtWall(row, column))
                if ((row - 1 == endRow && column == endColumn) || !IsEndPoint(row - 1, column))
                    if (Connectivity(row - 1, column) == 0)
                    {
                        SetVtWall(row - 1, column);
                        bool res = SolveSE(line, level + 1, row - 1, column, endRow, endColumn);
                        ResetVtWall(row - 1, column);
                        if (res) return true;
                    }

            // Down
            if (row < Side - 1 && !GetVtWall(row, column))
                if ((row + 1 == endRow && column == endColumn) || !IsEndPoint(row + 1, column))
                    if (Connectivity(row + 1, column) == 0)
                    {
                        SetVtWall(row, column);
                        bool res = SolveSE(line, level + 1, row + 1, column, endRow, endColumn);
                        ResetVtWall(row, column);
                        if (res) return true;
                    }

            return false;
        }

        private ulong Bit(int row, int column) => 1ul << (column + (row << 3));

        private bool GetHzWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            return (HzWallBits & Bit(row, column)) != 0;
        }

        private void SetHzWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            // Debug.Assert(!GetHzWall(row, column));
            HzWallBits |= Bit(row, column);
            Walls++;
        }

        private void ResetHzWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            // Debug.Assert(GetHzWall(row, column));
            HzWallBits &= ~Bit(row, column);
            Walls--;
            // Debug.Assert(Walls >= 0);
        }

        private bool GetVtWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            return (VtWallBits & Bit(row, column)) != 0;
        }

        private void SetVtWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            // Debug.Assert(!GetVtWall(row, column));
            VtWallBits |= Bit(row, column);
            Walls++;
        }

        private void ResetVtWall(int row, int column)
        {
            // Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            // Debug.Assert(GetVtWall(row, column));
            VtWallBits &= ~Bit(row, column);
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

        private bool IsEndPoint(int row, int column)
        {
            //Lines.Any(l => (l.startRow == row && l.startColumn == column) || (l.endRow == row && l.endColumn == column));
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].startRow == row && Lines[i].startColumn == column) return true;
                if (Lines[i].endRow == row && Lines[i].endColumn == column) return true;
            }
            return false;
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
                    Write(IsEndPoint(row, column) ? "■" : "▪");
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
        }

        private ConsoleColor GetColor(int row, int column)
        {
            int lastRow = -1, lastColumn = -1;
            for (; ; )
            {
                // If we have reach an end, we can return color
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].startRow == row && Lines[i].startColumn == column) return GetColor(i);
                    if (Lines[i].endRow == row && Lines[i].endColumn == column) return GetColor(i);
                }

                // Go to next connected cell avoiding previous one
                if (row > 0 && GetVtWall(row - 1, column) && (row - 1 != lastRow || column != lastColumn))
                {
                    lastRow = row;
                    lastColumn = column;
                    row = row - 1;
                }
                else if (row < Side - 1 && GetVtWall(row, column) && (row + 1 != lastRow || column != lastColumn))
                {
                    lastRow = row;
                    lastColumn = column;
                    row = row + 1;
                }
                else if (column > 0 && GetHzWall(row, column - 1) && (row != lastRow || column - 1 != lastColumn))
                {
                    lastRow = row;
                    lastColumn = column;
                    column = column - 1;
                }
                else if (column < Side - 1 && GetHzWall(row, column) && (row != lastRow || column + 1 != lastColumn))
                {
                    lastRow = row;
                    lastColumn = column;
                    column = column + 1;
                }
                else
                    Debugger.Break();
            }
        }

        private ConsoleColor GetColor(int i)
        {
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
    }
}
