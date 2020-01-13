﻿using System;
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
            // for test
            if (solutions.Count > 0) return;

            RollbackPlay();

            var oneSol = new Solution();
            PlayTile((3, 0), "C1");
            PlayTile((3, 1), "C5");
            PlayTile((3, 2), "C6");
            oneSol.RowMin = RowMin;
            oneSol.RowMin = RowMax;
            oneSol.ColMin = ColMin;
            oneSol.ColMax = ColMax;
            oneSol.Points = PlayPoints();
            foreach (var kv in PlayedDict.Where(kv => kv.Value.Tile != null))
                oneSol.Moves.Add(new CoordSquare(kv.Key, kv.Value));

            solutions.Add(oneSol);
        }
    }
}
