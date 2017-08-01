// Bonza.cs
// Test generation of a Bonza-like crossword
// 2017-06  PV

using static System.Console;
using Bonza.Generator;
using System.Diagnostics;

namespace TestGenerator
{
    class TestGeneratorApp
    {
        static void Main()
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int n = 0; n < 3; n++)
            {
                Grille g = new Grille();
                int i;
                for (i = 0; i < 5 && !g.PlaceWords(@"..\Lists\Prénoms.txt"); i++) { }
                Debug.Assert(i < 5);
            }
            WriteLine("Temps 3 générations: " + sw.Elapsed);
            //g.Print();
            //g.Print("C:\\temp\\out.txt");
            //g.SaveLayout("c:\\temp\\fruits.layout");
            //g.BuildPuzzle(5);
            //g.SavePuzzle("c:\\temp\\fruits.chunks");

            WriteLine();
            Write("(Pause)");
            ReadLine();
        }
    }

}
