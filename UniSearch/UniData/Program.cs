using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;


namespace UniData
{
    class Program
    {
        static void Main()
        {

            var cr = UnicodeData.CharacterRecords[65];
            WriteLine(cr);
            WriteLine(cr.BlockRecord);


            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }
    }
}
