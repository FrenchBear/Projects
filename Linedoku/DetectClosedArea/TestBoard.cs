using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;


namespace DetectClosedArea
{
    class TestBoard
    {
        private readonly int Side;
        private Cell[,] Grid;

        internal TestBoard(int side)
        {
            Side = side;

            Grid = new Cell[Side, Side];
            // All points are white at the beginning
            for (int row = 0; row < Side; row++)
                for (int column = 0; column < Side; column++)
                    Grid[row, column].line = -1;
        }

        internal void Fill()
        {
            Grid[1, 1].line = 0;
            Grid[2, 1].line = 0;
            Grid[2, 0].line = 0;
            Grid[3, 0].line = 0;
            Grid[3, 1].line = 0;
            Grid[3, 2].line = 0;
            Grid[3, 3].line = 0;
            Grid[2, 3].line = 0;
            Grid[1, 3].line = 0;
            Grid[0, 3].line = 0;
            Grid[0, 4].line = 0;
            Grid[0, 5].line = 0;
            Grid[1, 5].line = 0;
            Grid[2, 5].line = 0;
            Grid[3, 5].line = 0;
            Grid[3, 6].line = 0;
            Grid[4, 6].line = 0;
            Grid[5, 6].line = 0;
            Grid[5, 5].line = 0;
            Grid[5, 4].line = 0;
            Grid[5, 3].line = 0;
            Grid[6, 3].line = 0;
        }


        internal void FindAreas(int line)
        {

            // Start with all cells unpainted
            for (int row = 0; row < Side; row++)
                for (int column = 0; column < Side; column++)
                    Grid[row, column].paint = PaintStatus.Unpainted;

            int areaCount = 0;
            // Then explore all cells, and start coloring a new area each time we find an unpainted cell
            for (int row = 0; row < Side; row++)
                for (int column = 0; column < Side; column++)
                    if (Grid[row, column].paint == PaintStatus.Unpainted)
                    {
                        if (Grid[row, column].line == line)
                            // If cell line is current line then flag its paint as border and we're done
                            Grid[row, column].paint = PaintStatus.Border;
                        else
                        {
                            // Let's flow the pait until we hit borders!
                            areaCount++;
                            ColorizeArea(row, column, line, areaCount);
                        }

                    }

            WriteLine($"Areas: {areaCount}");
        }

        private void ColorizeArea(int row, int column, int line, int area)
        {
            Stack<(int row, int column)> paintStack = new Stack<(int row, int column)>();
            WriteLine($"Colorize area {area} from [{row}, {column}]");
            paintStack.Push((row, column));

            while (paintStack.Count > 0)
            {
                (int r, int c) = paintStack.Pop();

                // Check that we can't spread paint over a border
                Debug.Assert(Grid[r, c].paint == PaintStatus.Unpainted || Grid[r, c].paint == PaintStatus.Interior);

                // If Grid[r, c] == area, it's already been explored, no need to do it again
                if (Grid[r, c].paint == PaintStatus.Unpainted)
                {
                    WriteLine($"[{r}, {c}] -> {PaintStatus.Interior}");
                    Grid[r, c].paint = PaintStatus.Interior;
                    if (r > 0 && Grid[r - 1, c].paint == PaintStatus.Unpainted && Grid[r-1, c].line != line) paintStack.Push((r - 1, c));
                    if (c > 0 && Grid[r, c - 1].paint == PaintStatus.Unpainted && Grid[r, c-1].line != line) paintStack.Push((r, c - 1));
                    if (r < Side - 1 && Grid[r + 1, c].paint == PaintStatus.Unpainted && Grid[r+1, c].line != line) paintStack.Push((r + 1, c));
                    if (c < Side - 1 && Grid[r, c + 1].paint == PaintStatus.Unpainted && Grid[r, c + 1].line != line) paintStack.Push((r, c + 1));
                }
            }
        }

        internal void Print()
        {
            for (int row = 0; row < Side; row++)
            {
                for (int column = 0; column < Side; column++)
                {
                    Console.ForegroundColor = GetColor(row, column);
                    Write("■");
                    if (column < Side - 1)
                        Write(" ");
                }
                WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.White;
            WriteLine();
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
            return Grid[row, column].line;
        }
    }
}
