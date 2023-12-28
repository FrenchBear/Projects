// QuirkleUI
// Model
//
// 2023-12-11   PV      First version

using LibQwirkle;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace QwirkleUI;

internal class Model(MainViewModel viewModel)
{
    private readonly MainViewModel viewModel = viewModel;
    public Board Board = new();
    public HashSet<Move> CurrentMoves = [];
    public Hand[] Hands = [new()];      // Just 1 hand for now

    internal void NewBoard() => Board = new Board();

    // Return bounds for Board+CurrentMoves
    public BoundingRectangle Bounds()
    {
        int rowMin = Board.RowMin;
        int colMin = Board.ColMin;
        int rowMax = Board.RowMax;
        int colMax = Board.ColMax;

        Debug.WriteLine($"Board bounds: ({rowMin}, {colMin})-({rowMax}, {colMax})");

        if (CurrentMoves.Count > 0)
        {
            Debug.WriteLine($"CurrentMoves bounds: ({CurrentMoves.Min(m => m.Row)}, {CurrentMoves.Min(m => m.Col)})-({CurrentMoves.Max(m => m.Row)}, {CurrentMoves.Max(m => m.Col)})");
            rowMin = Math.Min(rowMin, CurrentMoves.Min(m => m.Row));
            colMin = Math.Min(colMin, CurrentMoves.Min(m => m.Col));
            rowMax = Math.Max(rowMax, CurrentMoves.Max(m => m.Row));
            colMax = Math.Max(colMax, CurrentMoves.Max(m => m.Col));
        }

        Debug.WriteLine($"Blobal bounds: ({rowMin}, {colMin})-({rowMax}, {colMax})");

        return new(new RowCol(rowMin, colMin), new RowCol(rowMax, colMax));
    }

    // For dev, initialize a simple set of tiles
    internal void InitializeBoard()
    {
        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);
        var t4 = new Tile(Shape.Circle, Color.Blue, 1);
        var t5 = new Tile(Shape.Circle, Color.Green, 1);
        var t6 = new Tile(Shape.Circle, Color.Orange, 1);

        Board.AddMove(new Move(50, 50, t1));
        Board.AddMove(new Move(50, 51, t2));
        Board.AddMove(new Move(50, 52, t3));

        CurrentMoves.Add(new Move(49, 52, t4));
        CurrentMoves.Add(new Move(48, 52, t5));
        CurrentMoves.Add(new Move(47, 52, t6));

        var h1 = new Tile(LibQwirkle.Shape.Lozange, LibQwirkle.Color.Blue, 1);
        var h2 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Blue, 1);
        var h3 = new Tile(LibQwirkle.Shape.Star, LibQwirkle.Color.Blue, 1);
        var h4 = new Tile(LibQwirkle.Shape.Star, LibQwirkle.Color.Yellow, 1);
        var h5 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Purple, 1);
        var h6 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Green, 1);

        Hands[0].Add(h1);
        Hands[0].Add(h2);
        Hands[0].Add(h3);
        Hands[0].Add(h4);
        Hands[0].Add(h5);
        Hands[0].Add(h6);
    }
}
