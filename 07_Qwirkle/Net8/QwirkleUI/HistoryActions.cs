// HistoryActions
// Store game actions to support Undo feature
//
// 2024-01-03       PV

using LibQwirkle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwirkleUI;

internal class HistoryActions: LinkedList<HistoryAction> { }

internal abstract class HistoryAction { }

// Caller must have made a copy of Model.Bag.Tiles
internal class HistoryActionBag(List<Tile> tiles): HistoryAction
{
    public readonly List<Tile> Tiles = tiles;
}

internal class HistoryActionMoves(Moves moves, int points): HistoryAction
{
    public readonly Moves Moves = new(moves);
    public readonly int Points = points;
}
