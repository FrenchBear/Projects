using System;
using static System.Console;


namespace CSFormat
{
    class CSFormatApp
    {
        static void Main(string[] args)
        {
            string s = "Hello";
            T1(s, "{0}");
            T1(s, "{0,10}");
            T1(s, "{0,-10}");

            double p = 0.031416;
            T1(p, "{0:P2}");
            T1(p, "{0,10:P2}");
            T1(p, "{0:+0.0%;-0.0%}");

            T2(1234567, "N2");

            WriteLine();

            CitiesExample();

            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }

        private static void T1<T>(T v, string fmt)
        {
            Write("{0,-15}", fmt);
            WriteLine("[" + String.Format(fmt, v) + "]");
        }

        private static void T2<T>(T v, string fmt)
        {
            Write("{0,-15}", fmt);
            WriteLine("[" + ((dynamic)v).ToString(fmt) + "]");
        }


        private static void CitiesExample()
        {
            // Create array of 5-tuples with population data for three U.S. cities, 1940-1950.
            Tuple<string, DateTime, int, DateTime, int>[] cities =
            {   Tuple.Create("Los Angeles", new DateTime(1940, 1, 1), 1504277, new DateTime(1950, 1, 1), 1970358),
                Tuple.Create("New York", new DateTime(1940, 1, 1), 7454995, new DateTime(1950, 1, 1), 7891957),
                Tuple.Create("Chicago", new DateTime(1940, 1, 1), 3396808, new DateTime(1950, 1, 1), 3620962),
                Tuple.Create("Detroit", new DateTime(1940, 1, 1), 1623452, new DateTime(1950, 1, 1), 1849568)
            };

            // Display header
            string header = String.Format("{0,-9}{1,8}{2,12}{1,8}{2,12}{3,14}\n",
                                          "City", "Year", "Population", "Change (%)");
            Console.WriteLine(header);
            string output;
            foreach (var city in cities)
            {
                output = String.Format("{0,-9}{1,8:yyyy}{2,12:N0}{3,8:yyyy}{4,12:N0}{5,14:+0.0%}",
                                       city.Item1, city.Item2, city.Item3, city.Item4, city.Item5,
                                       (city.Item5 - city.Item3) / (double)city.Item3);
                Console.WriteLine(output);
            }
            foreach (var city in cities)
            {
                WriteLine();
                string bar1 = new string('▓', (int)(50 * city.Item3 / 8000000.0));
                string bar2 = new string('█', (int)(50 * city.Item5 / 8000000.0));
                double p = (city.Item5 - city.Item3) / (double)city.Item3;
                WriteLine($"{city.Item1,-12} {city.Item2:yyyy} {city.Item3,12:N0}        {bar1,50}");
                WriteLine($"             {city.Item4:yyyy} {city.Item5,12:N0} {p,6:+0.0%} {bar2,50}");
            }
        }
    }
}
