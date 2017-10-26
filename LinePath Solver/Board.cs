using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace LinePath_Solver
{
    internal class Board
    {
        private int Side;
        private (int startRow, int startColumn, int endRow, int endColumn)[] Lines;

        public Board(int side, (int startRow, int startColumn, int endRow, int endColumn)[] lines)
        {
            this.Side = side;
            this.Lines = lines;
        }

        internal void Print()
        {
            WriteLine($"Size: {Side}");
            WriteLine($"Lines: {Lines.Count()}");
            foreach (var line in Lines)
                WriteLine($"  ({line.startRow}, {line.endRow}) -> ({line.endRow}, {line.endColumn})");
        }
    }
}
