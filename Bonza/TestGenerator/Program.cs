// Bonza.cs
// Test generation of a Bonza-like crossword
// 2017-06  PV

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using static System.Console;
using Bonza.Generator;
using System.Diagnostics;

namespace TestGenerator
{
    class TestGeneratorApp
    {
        static void Main()
        {
            Grille g = new Grille();
            int i;
            for (i = 0; i < 5 && !g.PlaceWords(@"..\Lists\Jours.txt"); i++) { }
            Debug.Assert(i < 5);
            g.Print();
            //g.Print("C:\\temp\\fruits.txt");
            //g.SaveLayout("c:\\temp\\fruits.layout");
            //g.BuildPuzzle(5);
            //g.SavePuzzle("c:\\temp\\fruits.chunks");

            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }
    }

}
