﻿// Q2 -- Simple test program for development (actual tests should be stored in QwirkleTestProject)
// Qwirkle reboot
//
// 2023-11-23   PV

using LibQwirkle;
using System.Diagnostics;

namespace Q2;

internal class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        //Tests_Base();
        //Tests_Play();
        //Tests_EmptyBoard();
        Tests_FullPlay();
    }

    private static void Tests_FullPlay()
    {
        var b = new Board();
        var d = new Dock();
        var hand = new Hand();

        int nMoves = 0;
        int totPoints = 0;
        int totBonus = 0;
        int minPoints = 20;
        int maxPoints = 0;
        for (; ; )
        {
            while (!d.IsEmpty && hand.Count < 6)
                hand.Add(d.GetTile());
            if (hand.Count == 0)
            {
                // Normal end, full dock played
                Debug.Assert(b.TilesCount == 6 * 6 * 3);
                Console.WriteLine("Normal endgame, all tiles played");
                break;
            }

            Console.WriteLine($"Hand: {hand.AsString(true)}");
            var play = b.Play(hand);
            if (play.Points == 0)
            {
                Console.WriteLine("No solution");
                // If dock is empty, no possibility to return tiles
                if (d.IsEmpty)
                {
                    Console.WriteLine("Dock is empty, no possible shuffle, endgame");
                    break;
                }
                Console.WriteLine("Returning hand to dock and take 6 random tiles again");
                d.ReturnTiles(hand);
                hand.Clear();
                continue;
            }
            Console.WriteLine($"Play: {play.AsString(true)}");
            b.AddMoves(play.Moves);
            b.Print();
            hand = play.NewHand;

            nMoves++;
            totPoints += play.Points;
            totBonus += play.Bonus;
            minPoints = Math.Min(play.Points, minPoints);
            maxPoints = Math.Max(play.Points, maxPoints);
        }
        b.NeighborCheck();
        Console.WriteLine($"Moves: {nMoves}, Total points: {totPoints}, Average: {totPoints / (double)nMoves:F1} points/move");
        Console.WriteLine($"Min points: {minPoints}, Max points: {maxPoints}, Bonus for 6-block count: {totBonus}");
    }

    private static void Tests_EmptyBoard()
    {
        var b = new Board();

        var h1 = new Tile(Shape.Circle, Color.Yellow, 1);
        var h2 = new Tile(Shape.Circle, Color.Green, 1);
        var h3 = new Tile(Shape.Square, Color.Orange, 1);
        var h4 = new Tile(Shape.Clover, Color.Blue, 1);
        var h5 = new Tile(Shape.Clover, Color.Red, 1);
        var h6 = new Tile(Shape.Circle, Color.Red, 1);

        var hand = new Hand([h1, h2, h3, h4, h5, h6]);

        Console.WriteLine($"Hand: {hand.AsString(true)}");
        var play = b.Play(hand);
        Console.WriteLine($"Play: {play.AsString(true)}");
        Debug.Assert(play.Moves.All(m => m.Row == 50) || play.Moves.All(m => m.Col == 50));
        Debug.Assert(play.Moves.All(m => m.Tile.Shape == Shape.Circle));
        Debug.Assert(play.Points == 3);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h5])));
        b.AddMoves(play.Moves);
        b.Print();
    }

    static void Tests_Play()
    {
        var b = new Board();

        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);
        var t4 = new Tile(Shape.Lozange, Color.Yellow, 1);
        var t5 = new Tile(Shape.Lozange, Color.Green, 1);

        b.AddMove(new Move(50, 50, t1));
        b.AddMove(new Move(50, 51, t2));
        b.AddMove(new Move(50, 52, t3));
        b.AddMove(new Move(49, 51, t4));
        b.AddMove(new Move(48, 51, t5));

        b.Print();

        var h1 = new Tile(Shape.Circle, Color.Yellow, 1);
        var h2 = new Tile(Shape.Circle, Color.Green, 1);
        var h3 = new Tile(Shape.Square, Color.Orange, 1);
        var h4 = new Tile(Shape.Clover, Color.Blue, 1);
        var h5 = new Tile(Shape.Clover, Color.Red, 1);
        var h6 = new Tile(Shape.Star, Color.Red, 1);

        var hand = new Hand([h1, h2, h3, h4, h5, h6]);

        Console.WriteLine($"Hand: {hand.AsString(true)}");
        var play = b.Play(hand);
        Console.WriteLine($"Play: {play.AsString(true)}");
        Debug.Assert(play.Moves.SetEquals(new HashSet<Move> { new(48, 52, h2), new(49, 52, h1) }));
        Debug.Assert(play.Points == 7);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h5, h6])));
        b.AddMoves(play.Moves);
        b.Print();
        hand = play.NewHand;

        var h7 = new Tile(Shape.Cross, Color.Yellow, 1);
        var h8 = new Tile(Shape.Cross, Color.Red, 1);
        hand.Add(h7);
        hand.Add(h8);

        Console.WriteLine($"Hand: {hand.AsString(true)}");
        play = b.Play(hand);
        Console.WriteLine($"Play: {play.AsString(true)}");
        Debug.Assert(play.Moves.Count == 3);
        Debug.Assert(play.Moves.All(m => m.Row == 50));
        Debug.Assert(play.Moves.All(m => m.Tile.Color == Color.Red));
        Debug.Assert(play.Points == 12);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h7])));
        b.AddMoves(play.Moves);
        b.Print();
        hand = play.NewHand;
    }

    // Same basic tests than in test project, but with visual output
    static void Tests_Base()
    {
        var b = new Board();

        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);
        var t4 = new Tile(Shape.Circle, Color.Blue, 1);
        var t5 = new Tile(Shape.Circle, Color.Green, 1);
        var t6 = new Tile(Shape.Circle, Color.Orange, 1);

        b.AddMove(new Move(50, 50, t1));
        b.AddMove(new Move(50, 51, t2));
        b.AddMove(new Move(50, 52, t3));
        b.AddMove(new Move(49, 52, t4));
        b.AddMove(new Move(48, 52, t5));
        b.AddMove(new Move(47, 52, t6));

        b.Print();

        //var h1 = new Tile(Color.Blue, Shape.Lozange, 1);
        //var h2 = new Tile(Color.Blue, Shape.Square, 1);
        //var h3 = new Tile(Color.Blue, Shape.Star, 1);
        //var h4 = new Tile(Color.Yellow, Shape.Star, 1);
        //var h5 = new Tile(Color.Purple, Shape.Square, 1);
        //var h6 = new Tile(Color.Green, Shape.Square, 1);

        //var hand = new Hand([h1, h2, h3, h4, h5, h6]);
        //Console.WriteLine($"Hand before: {hand.AsString(true)}");

        HashSet<Move> moves =
        [
            new(49, 51, new Tile(Shape.Lozange, Color.Blue, 1)),
            new(49, 53, new Tile(Shape.Square, Color.Blue, 1)),
            new(49, 54, new Tile(Shape.Star, Color.Blue, 1)),
        ];
        var pb = b.CountPoints(moves);
        Debug.Assert(pb.Points == 6);
        b.AddMoves(moves);
        Console.WriteLine(pb.AsString());
        b.Print();

        moves =
        [
            new(49, 55, new Tile(Shape.Cross, Color.Blue, 1)),
            new(49, 56, new Tile(Shape.Clover, Color.Blue, 1)),
        ];
        pb = b.CountPoints(moves);
        Debug.Assert(pb.Points == 12);
        b.AddMoves(moves);
        Console.WriteLine(pb.AsString());
        b.Print();

        moves =
        [
            new(48, 51, new Tile(Shape.Lozange, Color.Green, 1)),
            new(48, 53, new Tile(Shape.Square, Color.Green, 1)),
        ];
        pb = b.CountPoints(moves);
        Debug.Assert(pb.Points == 8);
        b.AddMoves(moves);
        Console.WriteLine(pb.AsString());
        b.Print();

        moves =
        [
            new(48, 54, new Tile(Shape.Star, Color.Green, 1)),
            new(50, 54, new Tile(Shape.Star, Color.Yellow, 1)),
            new(51, 54, new Tile(Shape.Star, Color.Orange, 1)),
        ];
        pb = b.CountPoints(moves);
        Debug.Assert(pb.Points == 8);
        b.AddMoves(moves);
        Console.WriteLine(pb.AsString());
        b.Print();
    }
}
