// Generator Tests
// Simple text-based tests of generation and performances of a Bonza-like crossword
//
// 2017-06      PV
// 2017-08-05   PV      testGeneration and TestPerformances; Output forced to UTF-8


using System;
using static System.Console;
using System.Diagnostics;
using System.Text;

namespace Bonza.Generator.Tests
{
    public class GeneratorTestsApp
    {
        public static void Main()
        {
            OutputEncoding = Encoding.UTF8;         // For Dot Net Core

            //TestGeneration();
            TestPerformances();

            WriteLine();
            Write("(Pause)");
            ReadLine();
        }

        public static void TestGeneration()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Grille g = new Grille();
            int i;
            for (i = 0; i < 5; i++)
            {
                if (g.AddWordsFromFile(@"..\Lists\Fruits.txt")) break;
            }
            Debug.Assert(i < 5);
            //}
            WriteLine("Generation time: " + sw.Elapsed);
            g.Print();
            //g.Print("C:\\temp\\out.txt");
            //g.SaveLayout("c:\\temp\\fruits.layout");
            //g.BuildPuzzle(5);
            //g.SavePuzzle("c:\\temp\\fruits.chunks");
        }

        public static void TestPerformances()
        {
            const int loops = 1;

            Stopwatch sw = Stopwatch.StartNew();
            for (int n = 0; n < loops; n++)
            {
                Grille g = new Grille();
                int i;
                for (i = 0; i < 5 && !g.AddWordsFromFile(@"..\Lists\Prénoms.txt"); i++) { }
                Debug.Assert(i < 5);
            }
            WriteLine($"Total time for {loops} generations: " + sw.Elapsed);

            Environment.Exit(0);
        }
    }

}
