// LinePath Solver, Areas
// Functions to detect closed areas and whether they are OK for a possible solution
// A closed area is defined by a border color, that is a line, from 0 to Lines.Length-1
// If a closed area contains no start/end line cell, it is not valid
// If a closed area contains an odd number of start/end line cell, it is not valid
//
// 2017-10-30   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;


namespace LinePath_Solver
{
    internal partial class Program
    {
        internal static long CheckClosedAreasCalls;


        // Checks if current layout is compatible with a solution by checking closed areas surrounded by line (or grid borders).
        // Returns false if this grid cannot be solved because of no start/end point in a closed area or not both start and end point of same color
        internal static bool CheckClosedAreas(sbyte line)
        {
            CheckClosedAreasCalls++;

            // Start with all cells unpainted
            for (byte row = 0; row < Side; row++)
                for (byte column = 0; column < Side; column++)
                    Grid[Δ(row, column)].Paint = PaintStatus.Unpainted;

            // Then explore all cells, and start coloring a new area each time we find an unpainted cell
            for (byte row = 0; row < Side; row++)
                for (byte column = 0; column < Side; column++)
                    if (Grid[Δ(row, column)].Paint == PaintStatus.Unpainted)
                        if (Grid[Δ(row, column)].Line == line)
                            // If cell line is current line then flag its paint as border and we're done
                            Grid[Δ(row, column)].Paint = PaintStatus.Border;
                        else
                            // Let's flow the paint until we hit borders!
                            return ColorizeArea(row, column, line);

            return true;
        }


        private static byte[] rowStack = new byte[121], columnStack = new byte[121];
        private static byte z;

        // Paints area staring at (row, column) and all adjacent unpainted points recursively, only stopping at cells belonging to line
        // Flags all painted cells with paint=Interior
        // Returns false if this area contains no start or end point, or if there is an odd number of start/end point for a given line
        // which means the puzzle won't have a solution, no need to further explore
        private static bool ColorizeArea(byte row, byte column, sbyte line)
        {
            rowStack[0] = row;
            columnStack[0] = column;
            z = 1;

            int flagsStartEnd = 0;
            bool containsStartOrEnd = false;
            while (z > 0)
            {
                byte r = rowStack[--z];
                byte c = columnStack[z];
                byte δ = Δ(r, c);

                // Check that we can't spread paint over a border
                Debug.Assert(Grid[δ].Paint == PaintStatus.Unpainted || Grid[δ].Paint == PaintStatus.Interior);

                // If Grid[Δ(r, c] == area, it's already been explored, no need to do it again
                if (Grid[δ].Paint == PaintStatus.Unpainted)
                {
                    if (Grid[δ].IsStartLine || Grid[δ].IsEndLine)
                    {
                        flagsStartEnd ^= 1 << Grid[δ].Line;       // To be valid, each line must have 0 or 2 ends in area, so 2 xor on the line nth bit in flags must be 0 at the end
                        containsStartOrEnd = true;
                    }
                    Grid[δ].Paint = PaintStatus.Interior;
                    byte rm1 = (byte)(r - 1);
                    byte cm1 = (byte)(c - 1);
                    byte rp1 = (byte)(r + 1);
                    byte cp1 = (byte)(c + 1);
                    if (r > 0 && Grid[Δ(rm1, c)].Paint == PaintStatus.Unpainted && Grid[Δ(rm1, c)].Line != line) { rowStack[z] = rm1; columnStack[z++] = c; }
                    if (c > 0 && Grid[Δ(r, cm1)].Paint == PaintStatus.Unpainted && Grid[Δ(r, cm1)].Line != line) { rowStack[z] = r; columnStack[z++] = cm1; }
                    if (r < Side - 1 && Grid[Δ(rp1, c)].Paint == PaintStatus.Unpainted && Grid[Δ(rp1, c)].Line != line) { rowStack[z] = rp1; columnStack[z++] = c; }
                    if (c < Side - 1 && Grid[Δ(r, cp1)].Paint == PaintStatus.Unpainted && Grid[Δ(r, cp1)].Line != line) { rowStack[z] = r; columnStack[z++] = cp1; }
                }
            }

            return flagsStartEnd == 0 && containsStartOrEnd;
        }

    }
}
