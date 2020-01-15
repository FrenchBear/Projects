// IntPosPermutator
// 2020-01-14       PV      Test for Qwirkle solver
//
// Next step: during solution iteration, at some point "skip" all solutions that begin with the same permutation
// Useful if at some point of the play of an iteration a tile is not playable, there is no point in looking at
// all following permutations that will attempt to place the same combination

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            var l = new List<int> { 1, 2, 3 };
            var sw = Stopwatch.StartNew();
            var (perms, skipTab) = GetLRPerm2(l);
            Console.WriteLine($"Elapsed (ms): {sw.ElapsedMilliseconds}");
            Console.WriteLine($"Perms count: {perms.Count()}");
            Console.WriteLine();
            foreach (var oneList in perms)
            {
                foreach (IntPos item in oneList)
                    Console.Write($"{item.value}{item.pos} ");
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.Write("SkipTab: ");
            foreach (var s in skipTab)
                Console.Write($"{s} ");
            Console.WriteLine();
        }


        // ==================================================================================
        // Classical implementation of a permutator with a twist, each element but the first
        // can be in Left and Right position

        private static (IEnumerable<List<IntPos>>, int[]) GetLRPerm2(List<int> l)
        {
            var l2 = new List<IntPos>();
            foreach (var item in l)
                l2.Add(new IntPos { value = item, pos = '.' });

            var sol = new List<List<IntPos>>();
            PermutatorSub(sol, l2, 0);

            // Prepare skipping table: if item[n] of a permutation doesn't fit, skip skipTab[n]
            // permutations to get the next one with item[n] different
            var skipTab = new int[l.Count];
            int div = l.Count;
            skipTab[0] = sol.Count / div;
            for (int i = 1; --div >= 1; i++)
                skipTab[i] = skipTab[i - 1] / (2 * div);

            return (sol, skipTab);
        }

        private static void PermutatorSub(IList<List<IntPos>> res, List<IntPos> l, int from)
        {
            if (from + 1 == l.Count)
            {
                var l2 = new List<IntPos>(l);
                l2[from] = l2[from].SetPos(from == 0 ? '.' : 'L');
                res.Add(l2);
                if (from > 0)
                {
                    l2 = new List<IntPos>(l);
                    l2[from] = l2[from].SetPos('R');
                    res.Add(l2);
                }
            }
            else
                for (int i = from; i < l.Count; i++)
                {
                    var l2 = new List<IntPos>(l);
                    if (i != from)
                    {
                        var temp = l2[from];
                        l2[from] = l2[i];
                        l2[i] = temp;
                    }
                    l2[from] = l2[from].SetPos(from == 0 ? '.' : 'L');
                    PermutatorSub(res, l2, from + 1);
                    if (from > 0)
                    {
                        l2 = new List<IntPos>(l);
                        if (i != from)
                        {
                            var temp = l2[from];
                            l2[from] = l2[i];
                            l2[i] = temp;
                        }
                        l2[from] = l2[from].SetPos('R');
                        PermutatorSub(res, l2, from + 1);
                    }
                }
        }
    }

    struct IntPos
    {
        public int value;
        public char pos;

        internal IntPos SetPos(char newPos) => new IntPos { value = this.value, pos = newPos };
    }
}
