// Q2 -- Simple test program for development (actual tests should be stored in QwirkleTestProject)
// Qwirkle reboot
//
// 2023-11-23   PV

using LibQwirkle;
using System;
using System.Diagnostics;
using System.Linq;

namespace Q2;

internal class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        //Tests_Base();
        //Tests_Play();
        //Tests_EmptyBoard();
        //Tests_FullPlay();
        //Tests_Evaluate();
        //Tests_CountPoints();
        Tests_Fill_Hole();

    }

    private static void Tests_Fill_Hole()
    {
        var board = new Board();

        var t1 = new Tile(Shape.Circle, Color.Orange, 1);
        var t2 = new Tile(Shape.Star, Color.Orange, 1);
        var t3 = new Tile(Shape.Star, Color.Orange, 2);
        var t4 = new Tile(Shape.Circle, Color.Orange, 2);

        board.AddMove(new TileRowCol(t1, 50, 50));
        board.AddMove(new TileRowCol(t2, 50, 51));
        board.AddMove(new TileRowCol(t3, 50, 53), true);        // Skip tests bacause of a hole
        board.AddMove(new TileRowCol(t4, 50, 54));

        var t5 = new Tile(Shape.Square, Color.Orange, 1);
        var b = board.IsCompatible(t5, 50, 52);
        Console.WriteLine($"IsCompatible: {b}");
    }

    private static void Tests_CountPoints()
    {
        var board = new Board();

        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);

        board.AddMove(new TileRowCol(t1, 50, 50));
        board.AddMove(new TileRowCol(t2, 50, 51));
        board.AddMove(new TileRowCol(t3, 50, 52));

        Moves moves = [
            new(new Tile(Shape.Square, Color.Red, 2), 51, 51),
        ];

        // Should be 2 points, but evaluates to 3 points...
        PointsBonus pb = board.CountPoints(moves);
        Console.WriteLine(pb.AsString());
    }

    private static void Tests_Evaluate()
    {
        var board = new Board();

        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);
        var t4 = new Tile(Shape.Circle, Color.Blue, 1);
        var t5 = new Tile(Shape.Circle, Color.Green, 1);
        var t6 = new Tile(Shape.Circle, Color.Orange, 1);

        board.AddMove(new TileRowCol(t1, 50, 50));
        board.AddMove(new TileRowCol(t2, 50, 51));
        board.AddMove(new TileRowCol(t3, 50, 52));
        board.AddMove(new TileRowCol(t4, 49, 52));
        board.AddMove(new TileRowCol(t5, 48, 52));
        board.AddMove(new TileRowCol(t6, 47, 52));

        board.Print();

        // Not is same row or in same column
        Moves moves = [
            new(new Tile(Shape.Circle, Color.Green, 1), 48, 51),
            new(new Tile(Shape.Square, Color.Green, 1), 47, 53),
        ];
        bool status;
        string msg;
        (status, msg) = board.EvaluateMoves(moves);
        Console.WriteLine($"1: {status}: {msg}");

        // Not same shape or same color
        moves = [
            new(new Tile(Shape.Circle, Color.Green, 1), 48, 51),
            new(new Tile(Shape.Square, Color.Orange, 1), 48, 53),
        ];
        (status, msg) = board.EvaluateMoves(moves);
        Console.WriteLine($"2: {status}: {msg}");

        // Tile placed in isolated position
        moves = [
            new(new Tile(Shape.Circle, Color.Green, 1), 48, 50),
        ];
        (status, msg) = board.EvaluateMoves(moves);
        Console.WriteLine($"3: {status}: {msg}");

        // Tile not compatible
        moves = [
            new(new Tile(Shape.Star, Color.Blue, 1), 49, 51),
        ];
        (status, msg) = board.EvaluateMoves(moves);
        Console.WriteLine($"4: {status}: {msg}");

        // Tile Ok
        moves = [
            new(new Tile(Shape.Lozange, Color.Blue, 1), 49, 51),
        ];
        (status, msg) = board.EvaluateMoves(moves);
        Console.WriteLine($"5: {status}: {msg}");
    }

    private static void Tests_FullPlay()
    {
        var board = new Board();
        var bag = new Bag();
        var hand = new Hand();

        int nMoves = 0;
        int totPoints = 0;
        int totBonus = 0;
        int minPoints = 20;
        int maxPoints = 0;
        for (; ; )
        {
            while (!bag.IsEmpty && hand.Count < 6)
                hand.Add(bag.GetTile());
            if (hand.Count == 0)
            {
                // Normal end, full dock played
                Debug.Assert(board.TilesCount == 6 * 6 * 3);
                Console.WriteLine("Normal endgame, all tiles played");
                break;
            }

            Console.WriteLine($"Hand: {hand.AsString(true)}");
            var play = board.Play(hand);
            if (play.PB.Points == 0)
            {
                Console.WriteLine("No solution");
                // If dock is empty, no possibility to return tiles
                if (bag.IsEmpty)
                {
                    Console.WriteLine("Dock is empty, no possible shuffle, endgame");
                    break;
                }
                Console.WriteLine("Returning hand to dock and take 6 random tiles again");
                bag.ReturnTiles(hand);
                hand.Clear();
                continue;
            }
            Console.WriteLine($"Play: {play.AsString(true)}");
            board.AddMoves(play.Moves);
            board.Print();
            hand = play.NewHand;

            nMoves++;
            totPoints += play.PB.Points;
            totBonus += play.PB.Bonus;
            minPoints = Math.Min(play.PB.Points, minPoints);
            maxPoints = Math.Max(play.PB.Points, maxPoints);
        }
        board.NeighborCheck();
        Console.WriteLine($"Moves: {nMoves}, Total points: {totPoints}, Average: {totPoints / (double)nMoves:F1} points/move");
        Console.WriteLine($"Min points: {minPoints}, Max points: {maxPoints}, Bonus for 6-block count: {totBonus}");
    }

    private static void Tests_EmptyBoard()
    {
        var board = new Board();

        var h1 = new Tile(Shape.Circle, Color.Yellow, 1);
        var h2 = new Tile(Shape.Circle, Color.Green, 1);
        var h3 = new Tile(Shape.Square, Color.Orange, 1);
        var h4 = new Tile(Shape.Clover, Color.Blue, 1);
        var h5 = new Tile(Shape.Clover, Color.Red, 1);
        var h6 = new Tile(Shape.Circle, Color.Red, 1);

        var hand = new Hand([h1, h2, h3, h4, h5, h6]);

        Console.WriteLine($"Hand: {hand.AsString(true)}");
        var play = board.Play(hand);
        Console.WriteLine($"Play: {play.AsString(true)}");
        Debug.Assert(play.Moves.All(m => m.Row == 50) || play.Moves.All(m => m.Col == 50));
        Debug.Assert(play.Moves.All(m => m.Tile.Shape == Shape.Circle));
        Debug.Assert(play.PB.Points == 3);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h5])));
        board.AddMoves(play.Moves);
        board.Print();
    }

    static void Tests_Play()
    {
        var board = new Board();

        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);
        var t4 = new Tile(Shape.Lozange, Color.Yellow, 1);
        var t5 = new Tile(Shape.Lozange, Color.Green, 1);

        board.AddMove(new TileRowCol(t1, 50, 50));
        board.AddMove(new TileRowCol(t2, 50, 51));
        board.AddMove(new TileRowCol(t3, 50, 52));
        board.AddMove(new TileRowCol(t4, 49, 51));
        board.AddMove(new TileRowCol(t5, 48, 51));

        board.Print();

        var h1 = new Tile(Shape.Circle, Color.Yellow, 1);
        var h2 = new Tile(Shape.Circle, Color.Green, 1);
        var h3 = new Tile(Shape.Square, Color.Orange, 1);
        var h4 = new Tile(Shape.Clover, Color.Blue, 1);
        var h5 = new Tile(Shape.Clover, Color.Red, 1);
        var h6 = new Tile(Shape.Star, Color.Red, 1);

        var hand = new Hand([h1, h2, h3, h4, h5, h6]);

        Console.WriteLine($"Hand: {hand.AsString(true)}");
        var play = board.Play(hand);
        Console.WriteLine($"Play: {play.AsString(true)}");
        Debug.Assert(play.Moves.SetEquals(new Moves { new(h2, new(48, 52)), new(h1, new(49, 52)) }));
        Debug.Assert(play.PB.Points == 7);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h5, h6])));
        board.AddMoves(play.Moves);
        board.Print();
        hand = play.NewHand;

        var h7 = new Tile(Shape.Cross, Color.Yellow, 1);
        var h8 = new Tile(Shape.Cross, Color.Red, 1);
        hand.Add(h7);
        hand.Add(h8);

        Console.WriteLine($"Hand: {hand.AsString(true)}");
        play = board.Play(hand);
        Console.WriteLine($"Play: {play.AsString(true)}");
        Debug.Assert(play.Moves.Count == 3);
        Debug.Assert(play.Moves.All(m => m.RC.Row == 50));
        Debug.Assert(play.Moves.All(m => m.Tile.Color == Color.Red));
        Debug.Assert(play.PB.Points == 12);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h7])));
        board.AddMoves(play.Moves);
        board.Print();
        hand = play.NewHand;
    }

    // Same basic tests than in test project, but with visual output
    static void Tests_Base()
    {
        var board = new Board();

        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);
        var t4 = new Tile(Shape.Circle, Color.Blue, 1);
        var t5 = new Tile(Shape.Circle, Color.Green, 1);
        var t6 = new Tile(Shape.Circle, Color.Orange, 1);

        board.AddMove(new TileRowCol(t1, 50, 50));
        board.AddMove(new TileRowCol(t2, 50, 51));
        board.AddMove(new TileRowCol(t3, 50, 52));
        board.AddMove(new TileRowCol(t4, 49, 52));
        board.AddMove(new TileRowCol(t5, 48, 52));
        board.AddMove(new TileRowCol(t6, 47, 52));

        board.Print();

        Moves moves =
        [
            new(new Tile(Shape.Lozange, Color.Blue, 1), new(49, 51)),
            new(new Tile(Shape.Square, Color.Blue, 1), new(49, 53)),
            new(new Tile(Shape.Star, Color.Blue, 1), new(49, 54)),
        ];
        var pb = board.CountPoints(moves);
        Debug.Assert(pb.Points == 6);
        board.AddMoves(moves);
        Console.WriteLine(pb.AsString());
        board.Print();

        moves =
        [
            new(new Tile(Shape.Cross, Color.Blue, 1), new(49, 55)),
            new(new Tile(Shape.Clover, Color.Blue, 1), new(49, 56)),
        ];
        pb = board.CountPoints(moves);
        Debug.Assert(pb.Points == 12);
        board.AddMoves(moves);
        Console.WriteLine(pb.AsString());
        board.Print();

        moves =
        [
            new(new Tile(Shape.Lozange, Color.Green, 1), new(48, 51)),
            new(new Tile(Shape.Square, Color.Green, 1), new(48, 53)),
        ];
        pb = board.CountPoints(moves);
        Debug.Assert(pb.Points == 8);
        board.AddMoves(moves);
        Console.WriteLine(pb.AsString());
        board.Print();

        moves =
        [
            new(new Tile(Shape.Star, Color.Green, 1), new(48, 54)),
            new(new Tile(Shape.Star, Color.Yellow, 1), new(50, 54)),
            new(new Tile(Shape.Star, Color.Orange, 1), new(51, 54)),
        ];
        pb = board.CountPoints(moves);
        Debug.Assert(pb.Points == 8);
        board.AddMoves(moves);
        Console.WriteLine(pb.AsString());
        board.Print();
    }
}
