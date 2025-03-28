﻿// HistoryActions
// Store game actions to support Undo feature
//
// 2024-01-03       PV

using LibQwirkle;
using System.Collections.Generic;
using System.Text;

namespace QwirkleUI;

internal sealed class HistoryActions: LinkedList<HistoryAction>
{
    //public HistoryActions() : base()
    //{
    //    Debug.WriteLine("HistoryActions.ctor");
    //}

    //public new void Clear()
    //{
    //    base.Clear();
    //    Debug.WriteLine("HistoryActions.Clear");
    //}

    //public new void AddLast(HistoryAction h)
    //{
    //    base.AddLast(h);
    //    Debug.WriteLine("HistoryActions.AddLast "+h.ToString());
    //}

    //public new void RemoveLast()
    //{
    //    base.RemoveLast();
    //    Debug.WriteLine("HistoryActions.RemoveLast");
    //}
}

internal abstract class HistoryAction { }

// Caller must have made a copy of Model.Bag.Tiles
internal sealed class HistoryActionBag(List<Tile> tiles): HistoryAction
{
    public readonly List<Tile> Tiles = tiles;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("Tiles: ");
        foreach (var tile in Tiles)
        {
            sb.Append(tile.AsString(null, true));
            if (sb.Length > 50)
            {
                sb.Append('…');
                break;
            }
            sb.Append(' ');
        }
        return sb.ToString();
    }
}

internal sealed class HistoryActionMoves(Moves moves, int points): HistoryAction
{
    public readonly Moves Moves = [.. moves];
    public readonly int Points = points;

    public override string ToString() => moves.ToString();
}
