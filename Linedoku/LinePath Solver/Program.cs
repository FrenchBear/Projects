// LinePath Solver from Adroid app Linedoku
// 2017-10-27   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LinePath_Solver
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Stopwatch sw = Stopwatch.StartNew();
            //var B = new Board(5, new(byte startRow, byte startColumn, byte endRow, byte endColumn)[] { (0, 0, 4, 4), (0, 1, 1, 3), (2, 1, 4, 0), (3, 1, 4, 3) });
            //var B = new Board(7, new(byte startRow, byte startColumn, byte endRow, byte endColumn)[] { (0, 0, 4, 5), (1, 0, 3, 3), (2, 4, 6, 3), (3, 2, 4, 2), (5, 5, 6, 4) });
            var B = new Board(11, new(byte startRow, byte startColumn, byte endRow, byte endColumn)[] { (0, 0, 5, 5), (1, 0, 2, 5), (1, 3, 7, 9), (1, 7, 3, 10), (2, 10, 6, 9), (5, 4, 10, 10), (6, 1, 8, 3), (7, 5, 9, 5), (9, 1, 10, 9) });

            //B.Test2();
            //B.Print();
            B.Solve(false);

            Console.WriteLine("Duration: " + sw.Elapsed);

            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }
    }
}
