// Solver.cs - Solution analyzer
// Qwirkle simulation project
//
// 2019-01-12   PV
// 2023-11-20   PV      Net8 C#12

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QwirkleLib;

/// <summary>
/// Qwirkle game board, a grid of BoardSquare
/// </summary>
public partial class Board
{
    public Solution? Solve(List<QTile> hand)
    {
        Debug.Assert(Played.dict.Count == 0);
        UpdateBoardPlayability();

        var solutions = new List<Solution>();
        var (handPermutations, skipTable) = QTileSidePermutations(hand);

        var startingPoints = Base.dict.Where(kv => kv.Value.State == SquareState.Playable).ToList();
        // If there is no playable tile...
        if (startingPoints.Count == 0)
            return null;
        foreach (var startingPoint in startingPoints)
            ExploreSolutions(solutions, startingPoint, handPermutations, skipTable, hand.Count);
        // No solution has been found
        if (startingPoints.Count == 0)
            return null;

        var maxPoints = solutions.Max(s => s.Points);
        var bestSol = solutions.Where(s => s.Points == maxPoints).OrderBy(s => (s.ColMax - s.ColMin) * (s.RowMax - s.RowMin)).First();
        return bestSol;
    }

    private static void ExploreSolutions(List<Solution> solutions, KeyValuePair<(int, int), Square> startingPoint, List<List<QTileSide>> handPermutations, int[] skipTable, int handCount)
    {
        /*
        var (coord, square) = startingPoint;

        var playedDictStack = new Dictionary<(int, int), Square>[handCount];

        foreach (var permutation in handPermutations)
        {

        }

        PlayTile(coord, handPermutations[0][0].tile);
        if ()
        */

        var oneSol = new Solution();
        solutions.Add(oneSol);
    }

    /// <summary>
    /// Classical implementation of a permutator with a twist, each element but the first can be on - or + side
    /// </summary>
    private static (List<List<QTileSide>>, int[]) QTileSidePermutations(List<QTile> l)
    {
        var l2 = new List<QTileSide>();
        foreach (var item in l)
            l2.Add(new QTileSide { tile = item, side = '.' });

        var sol = new List<List<QTileSide>>();
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

    private static void PermutatorSub(IList<List<QTileSide>> res, List<QTileSide> l, int from)
    {
        if (from + 1 == l.Count)
        {
            var l2 = new List<QTileSide>(l);
            l2[from] = l2[from].SetSide(from == 0 ? '.' : '+');
            res.Add(l2);
            if (from > 0)
            {
                l2 = new List<QTileSide>(l);
                l2[from] = l2[from].SetSide('-');
                res.Add(l2);
            }
        }
        else
            for (int i = from; i < l.Count; i++)
            {
                var l2 = new List<QTileSide>(l);
                if (i != from)
                {
                    (l2[i], l2[from]) = (l2[from], l2[i]);
                }
                l2[from] = l2[from].SetSide(from == 0 ? '.' : '+');
                PermutatorSub(res, l2, from + 1);
                if (from > 0)
                {
                    l2 = new List<QTileSide>(l);
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
