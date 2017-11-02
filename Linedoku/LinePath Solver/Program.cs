// LinePath Solver from Android app Linedoku
// 2017-10-27   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace LinePath_Solver
{
    partial class Program
    {
        [STAThread]
        static void Main()
        {
            OutputEncoding = Encoding.UTF8;

            Stopwatch sw = Stopwatch.StartNew();
            //Board(5, new Line[] { new Line(0, 0, 4, 4), new Line(0, 1, 1, 3), new Line(2, 1, 4, 0), new Line(3, 1, 4, 3) });
            //Board(7, new Line[] { new Line(0, 0, 4, 5), new Line(1, 0, 3, 3), new Line(2, 4, 6, 3), new Line(3, 2, 4, 2), new Line(5, 5, 6, 4) });
            Board(11, new Line[] { new Line(0, 0, 5, 5), new Line(1, 0, 2, 5), new Line(1, 3, 7, 9), new Line(1, 7, 3, 10), new Line(2, 10, 6, 9), new Line(5, 4, 10, 10), new Line(6, 1, 8, 3), new Line(7, 5, 9, 5), new Line(9, 1, 10, 9) });

            //B.Test2();
            //B.Print();
            Solve(false);

            WriteLine();
            WriteLine($"Total duration:            {sw.Elapsed}");
            WriteLine($"Calls to SolveSE:          {SolveSECalls:N0}");
            WriteLine($"Calls to CheckClosedAreas: {CheckClosedAreasCalls:N0}");

            WriteLine();
            Write("(Pause)");
            ReadLine();
        }
    }
}
