using System;
using System.Collections.Generic;

namespace Elevator
{
    class Program
    {
        static void Main()
        {
            //int µ = 50;
            //var poi = new PoissonRandom(µ, 0);
            //for (int i = 0; i < 10; i++)
            //    Console.WriteLine(poi.NextValue());

            int µ = 15;
            int[] f = new int[100];
            int xmax = 0;
            var poi = new PoissonRandom(µ, 0);
            for (int i = 0; i < 1000; i++)
            {
                int x = poi.NextValue();
                if (x < 100)
                {
                    f[x] += 1;
                    if (x > xmax) xmax = x;
                }
            }

            for(int i=0 ; i<=xmax ; i++)
                Console.WriteLine("{0}\t{1}", i, f[i]);

            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }
    }
}
