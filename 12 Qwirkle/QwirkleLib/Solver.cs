// Solver.cs - Solution analyzer
// Qwirkle simulation project
// 2019-01-12   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Console;

#nullable enable

namespace QwirkleLib
{
    /// <summary>
    /// Qwirkle game board, a grid of BoardSquare
    /// </summary>
    public partial class Board
    {
        public Solution? Solve(List<QTile> hand)
        {
            Debug.Assert(PlayedDict.Count == 0);
            UpdateBoardPlayability();
            var solutions = new List<Solution>();
            var myHand = new List<QTile>(hand);
            var startingPoints = BoardDict.Where(kv => kv.Value.State == SquareState.Playable).ToList();
            // If there is no playable tile...
            if (startingPoints.Count == 0)
                return null;
            foreach (var startingPoint in startingPoints)
                ExploreSolutions(solutions, startingPoint, myHand);
            // No solution has been found
            if (startingPoints.Count == 0)
                return null;

            var maxPoints = solutions.Max(s => s.Points);
            var bestSol = solutions.Where(s => s.Points == maxPoints).OrderBy(s => (s.ColMax - s.ColMin) * (s.RowMax - s.RowMin)).First();
            return bestSol;
        }

        private void ExploreSolutions(List<Solution> solutions, KeyValuePair<(int, int), Square> startingPoint, List<QTile> hand)
        {
            var oneSol = new Solution();
            solutions.Add(oneSol);
        }
    }
}
