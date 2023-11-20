// TileSidePermutator
// Sidework for Qwirkle solver, generates all permutations of a hand
//
// 2020-01-14   PV      Tests 
// 2023-12-20   PV      Net8 C#12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApp1;

class Program
{
    static void Main()
    {
        var l = new List<QTile> {
            new("A1"),
            new("B2"),
            new("C3"),
            //new ("D3"),
            //new ("E5"),
            //new ("F6"),
        };
        var sw = Stopwatch.StartNew();
        var (perms, skipTab) = GetLRPerm2(l);
        Console.WriteLine($"Elapsed (ms): {sw.ElapsedMilliseconds}");
        Console.WriteLine($"Perms count: {perms.Count()}");
        Console.WriteLine();
        int n = 0;
        foreach (var oneList in perms)
        {
            if (++n > 20)
                break;
            foreach (QtileSide item in oneList)
                Console.Write($"{item.tile}{item.side} ");
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
    // can be on - or + side

    private static (IEnumerable<List<QtileSide>>, int[]) GetLRPerm2(List<QTile> l)
    {
        var l2 = new List<QtileSide>();
        foreach (var item in l)
            l2.Add(new QtileSide { tile = item, side = '.' });

        var sol = new List<List<QtileSide>>();
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

    private static void PermutatorSub(IList<List<QtileSide>> res, List<QtileSide> l, int from)
    {
        if (from + 1 == l.Count)
        {
            var l2 = new List<QtileSide>(l);
            l2[from] = l2[from].SetSide(from == 0 ? '.' : '+');
            res.Add(l2);
            if (from > 0)
            {
                l2 = new List<QtileSide>(l);
                l2[from] = l2[from].SetSide('-');
                res.Add(l2);
            }
        }
        else
            for (int i = from; i < l.Count; i++)
            {
                var l2 = new List<QtileSide>(l);
                if (i != from)
                {
                    (l2[i], l2[from]) = (l2[from], l2[i]);
                }
                l2[from] = l2[from].SetSide(from == 0 ? '.' : '+');
                PermutatorSub(res, l2, from + 1);
                if (from > 0)
                {
                    l2 = new List<QtileSide>(l);
                    if (i != from)
                    {
                        (l2[i], l2[from]) = (l2[from], l2[i]);
                    }
                    l2[from] = l2[from].SetSide('-');
                    PermutatorSub(res, l2, from + 1);
                }
            }
    }
}

struct QtileSide
{
    public QTile tile;
    public char side;

    internal QtileSide SetSide(char newSide) => new() { tile = tile, side = newSide };
}
