using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectClosedArea
{
    class Program
    {
        static void Main()
        {
            var t = new TestBoard(7);
            t.Fill();
            t.Print();
            t.FindAreas(0);


            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }
    }
}
