// DateTymeYMD
// Simple tool to print current date and time in a soratble order without forbidden characters for a filename
// 2019-08-16   PV

using System;

namespace DateTimeYMD_NS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss"));
        }
    }
}
