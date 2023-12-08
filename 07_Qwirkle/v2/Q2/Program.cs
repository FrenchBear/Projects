// Q2 -- Simple test program for development (actual tests should be stored in QwirkleTestProject)
// Qwirkle reboot
//
// 2023-11-23   PV

using LibQwirkle;
using System.Diagnostics;

namespace Q2;

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Test2();
    }

    static void Test2()
    {
        var b = new Board();

        var t1 = new Tile(Color.Red, Shape.Square, 1);
        var t2 = new Tile(Color.Red, Shape.Lozange, 1);
        var t3 = new Tile(Color.Red, Shape.Circle, 1);

        b.AddMove(new Move(50, 50, t1));
        b.AddMove(new Move(50, 51, t2));
        b.AddMove(new Move(50, 52, t3));

        b.Print();

        var t4 = new Tile(Color.Blue, Shape.Circle, 1);
        var t5 = new Tile(Color.Green, Shape.Circle, 1);
        var t6 = new Tile(Color.Orange, Shape.Circle, 1);

        var hand = new Hand([t4, t5, t6]);

        b.Play(hand);
        Debugger.Break();
    }

    static void Test1()
    {
        var b = new Board();

        var t1 = new Tile(Color.Red, Shape.Square, 1);
        var t2 = new Tile(Color.Red, Shape.Lozange, 1);
        var t3 = new Tile(Color.Red, Shape.Circle, 1);
        var t4 = new Tile(Color.Blue, Shape.Circle, 1);
        var t5 = new Tile(Color.Green, Shape.Circle, 1);
        var t6 = new Tile(Color.Orange, Shape.Circle, 1);

        b.AddMove(new Move(50, 50, t1));
        b.AddMove(new Move(50, 51, t2));
        b.AddMove(new Move(50, 52, t3));
        b.AddMove(new Move(49, 52, t4));
        b.AddMove(new Move(48, 52, t5));
        b.AddMove(new Move(47, 52, t6));

        b.Print();

        var h1 = new Tile(Color.Blue, Shape.Lozange, 1);
        var h2 = new Tile(Color.Blue, Shape.Square, 1);
        var h3 = new Tile(Color.Blue, Shape.Star, 1);
        var h4 = new Tile(Color.Yellow, Shape.Star, 1);
        var h5 = new Tile(Color.Purple, Shape.Square, 1);
        var h6 = new Tile(Color.Green, Shape.Square, 1);

        var hand = new Hand([h1, h2, h3, h4, h5, h6]);
        Console.WriteLine($"Hand before: {hand.AsString(true)}");

        Play play = b.Play(hand);
        Console.WriteLine($"Play:        {play.AsString(true)}   Points={play.Points}");
        Console.WriteLine($"Hand after:  {play.NewHand.AsString(true)}");

        Console.WriteLine();
        b.AddMoves(play.Moves);
        b.Print();

        List<Move> moves =
        [
            new(49, 55, new Tile(Color.Blue, Shape.Cross, 1)),
            new(49, 56, new Tile(Color.Blue, Shape.Clover, 1)),
        ];
        int points = b.CountPoints(moves);
        Debug.Assert(points == 12);
        b.AddMoves(moves);
        Console.WriteLine($"{points} points:");
        b.Print();

        moves =
        [
            new(48, 51, new Tile(Color.Green, Shape.Lozange, 1)),
            new(48, 53, new Tile(Color.Green, Shape.Square, 1)),
        ];
        points = b.CountPoints(moves);
        Debug.Assert(points == 8);
        b.AddMoves(moves);
        Console.WriteLine($"{points} points:");
        b.Print();

        moves =
        [
            new(50, 53, new Tile(Color.Purple, Shape.Star, 1)),
            new(48, 53, new Tile(Color.Green, Shape.Star, 1)),
            new(49, 53, new Tile(Color.Green, Shape.Star, 1)),
        ];
        points = b.CountPoints(moves);
        //Debug.Assert(points == 8);
        b.AddMoves(moves);
        Console.WriteLine($"{points} points:");
        b.Print();
    }
}
