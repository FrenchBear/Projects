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
    private readonly MainViewModel viewModel = viewModel;               // Useful?
    public Board Board = new();
    //public HashSet<TileRowCol> CurrentMoves = [];
    public Player[] Players = [new()];          // Just 1 player now

    internal void NewBoard() => Board = new Board();

    // For dev, initialize Board with few tiles
    internal void InitializeBoard()
    {
        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);
        var t4 = new Tile(Shape.Circle, Color.Blue, 1);
        var t5 = new Tile(Shape.Circle, Color.Green, 1);
        var t6 = new Tile(Shape.Circle, Color.Orange, 1);

        Board.AddMove(new TileRowCol(t1, 50, 50));
        Board.AddMove(new TileRowCol(t2, 50, 51));
        Board.AddMove(new TileRowCol(t3, 50, 52));

        Board.AddMove(new TileRowCol(t4, 49, 52));
        Board.AddMove(new TileRowCol(t5, 48, 52));
        Board.AddMove(new TileRowCol(t6, 47, 52));

        var h1 = new Tile(LibQwirkle.Shape.Lozange, LibQwirkle.Color.Blue, 1);
        var h2 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Blue, 1);
        var h3 = new Tile(LibQwirkle.Shape.Star, LibQwirkle.Color.Blue, 1);
        var h4 = new Tile(LibQwirkle.Shape.Star, LibQwirkle.Color.Yellow, 1);
        var h5 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Red, 1);
        var h6 = new Tile(LibQwirkle.Shape.Square, LibQwirkle.Color.Green, 1);

        Players[0].HandZZ.Add(h1);
        Players[0].HandZZ.Add(h2);
        Players[0].HandZZ.Add(h3);
        Players[0].HandZZ.Add(h4);
        Players[0].HandZZ.Add(h5);
        Players[0].HandZZ.Add(h6);
    }

    internal (bool, string) EvaluateMoves(HashSet<TileRowCol> moves)
        => Board.EvaluateMoves(moves);
    internal PointsBonus CountPoints(HashSet<TileRowCol> moves) 
        => Board.CountPoints(moves);
}
