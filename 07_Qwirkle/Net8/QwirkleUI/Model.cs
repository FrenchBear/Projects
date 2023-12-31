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
    public Bag Bag = new();
    public Player[] Players = [];
    public int PlayerIndex = 0;
    public int PlayersCount = 1;

    public Player CurrentPlayer => Players[PlayerIndex];

    internal void NewBoard(bool withTestInit)
    {
        Board = new Board();
        Bag = new Bag();
        Players = new Player[PlayersCount];

        if (withTestInit)
        {
            Players[0] = new Player();
            Players[0].Name = "Dev Tests Player";
            Players[0].Score = 7;

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

            var h1 = new Tile(Shape.Lozange, Color.Blue, 1);
            var h2 = new Tile(Shape.Square, Color.Blue, 1);
            var h3 = new Tile(Shape.Star, Color.Blue, 1);
            var h4 = new Tile(Shape.Star, Color.Yellow, 1);
            var h5 = new Tile(Shape.Square, Color.Red, 2);
            var h6 = new Tile(Shape.Lozange, Color.Green, 1);

            Players[0].Hand.Add(h1);
            Players[0].Hand.Add(h2);
            Players[0].Hand.Add(h3);
            Players[0].Hand.Add(h4);
            Players[0].Hand.Add(h5);
            Players[0].Hand.Add(h6);

            Bag.RemoveTile(t1);
            Bag.RemoveTile(t2);
            Bag.RemoveTile(t3);
            Bag.RemoveTile(t4);
            Bag.RemoveTile(t5);
            Bag.RemoveTile(t6);

            Bag.RemoveTile(h1);
            Bag.RemoveTile(h2);
            Bag.RemoveTile(h3);
            Bag.RemoveTile(h4);
            Bag.RemoveTile(h5);
            Bag.RemoveTile(h6);

            /*
            var t1 = new Tile(Shape.Circle, Color.Orange, 1);
            var t2 = new Tile(Shape.Star, Color.Orange, 1);
            var t3 = new Tile(Shape.Star, Color.Orange, 2);
            var t4 = new Tile(Shape.Circle, Color.Orange, 2);

            Board.AddMove(new TileRowCol(t1, 50, 50));
            Board.AddMove(new TileRowCol(t2, 50, 51));
            Board.AddMove(new TileRowCol(t3, 50, 53), true);        // Skip tests bacause of a hole
            Board.AddMove(new TileRowCol(t4, 50, 54));

            Bag.RemoveTile(t1);
            Bag.RemoveTile(t2);
            Bag.RemoveTile(t3);
            Bag.RemoveTile(t4);

            var h1 = new Tile(Shape.Square, Color.Orange, 1);
            //var h2 = new Tile(Shape.Square, Color.Blue, 1);
            //var h3 = new Tile(Shape.Star, Color.Blue, 1);
            //var h4 = new Tile(Shape.Star, Color.Yellow, 1);
            //var h5 = new Tile(Shape.Square, Color.Red, 2);
            //var h6 = new Tile(Shape.Lozange, Color.Green, 1);

            Players[0].Hand.Add(h1);
            //Players[0].Hand.Add(h2);
            //Players[0].Hand.Add(h3);
            //Players[0].Hand.Add(h4);
            //Players[0].Hand.Add(h5);
            //Players[0].Hand.Add(h6);

            Bag.RemoveTile(h1);
            //Bag.RemoveTile(h2);
            //Bag.RemoveTile(h3);
            //Bag.RemoveTile(h4);
            //Bag.RemoveTile(h5);
            //Bag.RemoveTile(h6);
            */
        }
        else
        {
            for (int p = 0; p < PlayersCount; p++)
            {
                Players[p] = new Player();
                for (var i = 0; i < 6; i++)
                    Players[p].Hand.Add(Bag.GetTile());
            }
        }
    }

    internal (bool, string) EvaluateMoves(HashSet<TileRowCol> moves)
        => Board.EvaluateMoves(moves);
    internal PointsBonus CountPoints(HashSet<TileRowCol> moves)
        => Board.CountPoints(moves);
    internal void UpdateRanks()
    {
        // No ranking for single player
        if (PlayersCount==1)
        {
            Players[0].Rank = "";
            return;
        }

        var l = Players.Select(p => (p.Index, p.Score)).OrderByDescending(it => it.Score).ToList();
        int r = 1;
        int s = l.First().Score;
        int n = 0;
        foreach (var ixsc in l)
        {
            if (ixsc.Score == s)
            {
                Players[ixsc.Index].Rank = r == 1 ? "1er" : $"{r}è";
                n++;
                continue;
            }
            s = ixsc.Score;
            r += n;
            n = 1;
            Players[ixsc.Index].Rank = $"{r}è";
        }
    }

    public bool IsBagEmpty => Bag.IsEmpty;
}
