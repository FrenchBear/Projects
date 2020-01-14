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
            var l = new List<int> { 1, 2, 3, 4, 5, 6 };
            var sw = Stopwatch.StartNew();
            var sols = GetLRPerm2(l);
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.WriteLine(sols.Count());
            //foreach (var oneList in sols)
            //{
            //    foreach (IntPos item in oneList)
            //        Console.Write($"{item.value}{item.pos} ");
            //    Console.WriteLine();
            //}
        }


        static IEnumerable<List<IntPos>> GetLRPerm1(List<int> l)
            => GetLRPerm1Sub(0, l);

        static IEnumerable<List<IntPos>> GetLRPerm1Sub(int depth, List<int> l)
        {
            var ret = new List<List<IntPos>>();
            if (l.Count == 1)
            {
                var r1 = new List<IntPos>();
                r1.Add(new IntPos { value = l[0], pos = depth == 0 ? '.' : 'L' });
                ret.Add(r1);
                if (depth > 0)
                {
                    r1 = new List<IntPos>();
                    r1.Add(new IntPos { value = l[0], pos = 'R' });
                    ret.Add(r1);
                }
                return ret;
            }

            foreach (var item in l)
            {
                List<int> l2 = new List<int>(l);
                l2.Remove(item);
                foreach (List<IntPos> onePerm in GetLRPerm1Sub(depth + 1, l2))
                {
                    var l3 = new List<IntPos>();
                    l3.Add(new IntPos { value = item, pos = depth == 0 ? '.' : 'L' });
                    l3.AddRange(onePerm);
                    ret.Add(l3);

                    if (depth > 0)
                    {
                        l3 = new List<IntPos>();
                        l3.Add(new IntPos { value = item, pos = 'R' });
                        l3.AddRange(onePerm);
                        ret.Add(l3);
                    }
                }
            }
            return ret;
        }


        // ==================================================================================
        // Classical implementation of a permutator

        private static IEnumerable<List<IntPos>> GetLRPerm2(List<int> l)
        {
            var l2 = new List<IntPos>();
            foreach (var item in l)
                l2.Add(new IntPos { value = item, pos = '.' });

            var sol = new List<List<IntPos>>();
            ClassicalPermutatorSub(sol, l2, 0);
            return sol;
        }

        private static void ClassicalPermutatorSub(IList<List<IntPos>> res, List<IntPos> l, int from)
        {
            if (from + 1 == l.Count)
            {
                var l2 = new List<IntPos>(l);
                var zz = l[from];
                zz.pos = 'L';
                l2[from] = zz;
                res.Add(l2);
                l2 = new List<IntPos>(l);
                zz.pos = 'R';
                l2[from] = zz;
                res.Add(l2);
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

                    var zz = l2[from];
                    zz.pos = from == 0 ? '.' : 'L';
                    l2[from] = zz;

                    ClassicalPermutatorSub(res, l2, from + 1);

                    if (from > 0)
                    {
                        l2 = new List<IntPos>(l);
                        if (i != from)
                        {
                            var temp = l2[from];
                            l2[from] = l2[i];
                            l2[i] = temp;
                        }

                        zz = l2[from];
                        zz.pos = 'R';
                        l2[from] = zz;

                        ClassicalPermutatorSub(res, l2, from + 1);
                    }
                }
        }
    }

    struct IntPos
    {
        public int value;
        public char pos;
    }
}
