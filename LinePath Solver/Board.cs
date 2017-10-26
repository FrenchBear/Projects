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
            SolveSE(0, Lines[0].startRow, Lines[0].startColumn, Lines[0].endRow, Lines[0].endColumn);
        }

        private bool SolveSE(int level, int row, int column, int endRow, int endColumn)
        {
            Debug.Assert(level <= Side * Side);
            Print();

            if (row == endRow && column == endColumn)
            {
                Print();
                return true;
            }

            // Right
            if (column < Side - 1 && !GetHzWall(row, column) && ((row == endRow && column + 1 == endColumn) || !IsEndPoint(row, column + 1)))
            {
                SetHzWall(row, column);
                bool res = SolveSE(level + 1, row, column + 1, endRow, endColumn);
                ResetHzWall(row, column);
                if (res) return true;
            }

            // Left
            if (column > 0 && !GetHzWall(row, column - 1))  // && ((row == endRow && column - 1 == endColumn) || !IsEndPoint(row, column - 1)))
            {
                SetHzWall(row, column - 1);
                bool res = SolveSE(level + 1, row, column - 1, endRow, endColumn);
                ResetHzWall(row, column - 1);
                if (res) return true;
            }

            // Up
            if (row > 0 && !GetVtWall(row, column)) // && ((row - 1 == endRow && column == endColumn) || !IsEndPoint(row - 1, column)))
            {
                SetVtWall(row - 1, column);
                bool res = SolveSE(level + 1, row - 1, column, endRow, endColumn);
                ResetVtWall(row - 1, column);
                if (res) return true;
            }

            // Down
            if (row < Side - 1 && !GetVtWall(row, column))  // && ((row + 1 == endRow && column == endColumn) || !IsEndPoint(row + 1, column)))
            {
                SetVtWall(row, column);
                bool res = SolveSE(level + 1, row + 1, column, endRow, endColumn);
                ResetVtWall(row, column);
                if (res) return true;
            }

            return false;
        }

        private ulong Bit(int row, int column) => 1ul << (column + (row << 3));

        private bool GetHzWall(int row, int column)
        {
            Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            return (HzWallBits & Bit(row, column)) != 0;
        }

        private void SetHzWall(int row, int column)
        {
            Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            Debug.Assert(!GetHzWall(row, column));
            HzWallBits |= Bit(row, column);
            Walls++;
        }

        private void ResetHzWall(int row, int column)
        {
            Debug.Assert(row >= 0 && row <= Side && column >= 0 && column < Side);
            Debug.Assert(GetHzWall(row, column));
            HzWallBits &= ~Bit(row, column);
            Walls--;
            Debug.Assert(Walls >= 0);
        }

        private bool GetVtWall(int row, int column)
        {
            Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            return (VtWallBits & Bit(row, column)) != 0;
        }

        private void SetVtWall(int row, int column)
        {
            Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            Debug.Assert(!GetVtWall(row, column));
            VtWallBits |= Bit(row, column);
            Walls++;
        }

        private void ResetVtWall(int row, int column)
        {
            Debug.Assert(row >= 0 && row < Side && column >= 0 && column <= Side);
            Debug.Assert(GetVtWall(row, column));
            VtWallBits &= ~Bit(row, column);
            Walls--;
            Debug.Assert(Walls >= 0);
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
                    Write(IsEndPoint(row, column) ? "■" : "▪");
                    if (column < Side - 1)
                        Write(GetHzWall(row, column) ? "──" : "  ");
                }
                WriteLine();
                if (row < Side - 1)
                {
                    for (int column = 0; column < Side; column++)
                    {
                        Write(GetVtWall(row, column) ? "│" : " ");
                        if (column < Side)
                            Write("  ");
                    }
                    WriteLine();
                }
            }
            WriteLine();
        }
    }
}
