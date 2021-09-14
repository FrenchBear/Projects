// Elevator simulation
// random.py - Random numbers generation used by simulation (unifor, Poisson)
// Implemented manually the code can be easily rewitten in other languages 
// and produce same results
//
// 2017-09-02    PV

using System;
using System.Collections.Generic;
using System.Text;

namespace Elevator
{
    /// <summary>
    /// Simple linear congruential generator
    /// </summary>
    internal class UniformIntRandom
    {
        const int m = 0x7FFFFFFF;
        const int a = 1103515245;
        const int c = 12345;
        private int seed;

        // Values fromhttps://en.wikipedia.org/wiki/Linear_congruential_generator
        public UniformIntRandom(int seed = 0)
        {
            this.seed = seed;
        }

        public int MaxValue => m;

        public int NextValue()
        {
            seed = (int)((a * (long)seed + c) % m);
            return seed;
        }
        public double NextDouble() => NextValue() / (double)m;
    }


    /// <summary>
    /// Poisson Law Generator (Knuth)
    /// </summary>
    internal class PoissonRandom
    {
        private readonly double µ;
        private readonly UniformIntRandom rnd;

        public PoissonRandom(double µ, int seed = 0)
        {
            this.µ = µ;
            rnd = new UniformIntRandom(seed);
        }

        public int NextValue()
        {
            double L = Math.Exp(-µ);
            int k = 0;
            double p = 1.0;
            do
            {
                k += 1;
                p *= rnd.NextDouble();
            } while (p > L);
            return k - 1;
        }
    }
}
