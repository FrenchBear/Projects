// LinePath Solver

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinePath_Solver
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            //var B = new Board(5, new(int startRow, int startColumn, int endRow, int endColumn)[] { (0, 0, 4, 4), (0, 1, 1, 3), (2, 1, 4, 0), (3, 1, 4, 3) });
            var B = new Board(7, new(int startRow, int startColumn, int endRow, int endColumn)[] { (0, 0, 4, 5), (1, 0, 3, 3), (2, 4, 6, 3), (3, 2, 4, 2), (5, 5, 6, 4) });
            //B.Test();
            //B.Print();
            B.Solve0();

            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }
    }
}
