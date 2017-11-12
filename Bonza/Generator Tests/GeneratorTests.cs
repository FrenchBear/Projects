// Generator Tests
// Simple text-based tests of generation and performances of a Bonza-like crossword
//
// 2017-06      PV
// 2017-08-05   PV      testGeneration and TestPerformances; Output forced to UTF-8


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Console;

namespace Bonza.Generator.Tests
{
    public class GeneratorTestsApp
    {
        public static void Main()
        {
            OutputEncoding = Encoding.UTF8;         // For Dot Net Core

            //TestGeneration();

            var t1 = TestPerformances();
            var t2 = TestPerformances();
            var t3 = TestPerformances();
            var median = (new List<TimeSpan> { t1, t2, t3 }).OrderBy(x => x).Skip(1).First();
            WriteLine();
            WriteLine("Median: " + median);

            WriteLine();
            Write("(Pause)");
            ReadLine();
        }

        public static void TestGeneration()
        {
            const string file = "Fruits";
            Stopwatch sw = Stopwatch.StartNew();
            Grille g = new Grille();
            int i;
            for (i = 0; i < 5; i++)
            {
                if (g.AddWordsFromFile(@"..\Lists\" + file + ".txt")) break;
            }
            Debug.Assert(i < 5);
            //}
            WriteLine("Generation time: " + sw.Elapsed);
            g.Print();
            //g.Print("C:\\temp\\" + file + ".txt");
            //g.Layout.SaveLayoutAsCode("C:\\temp\\" + file + ".cs");
        }

        public static TimeSpan TestPerformances()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var g = new Bonza.Generator.Grille(123);
            g.AddWordsFromFile(@"..\Lists\Prénoms.txt");
            sw.Stop();
            WriteLine("Time: " + sw.Elapsed);
            return sw.Elapsed;
        }
    }
}