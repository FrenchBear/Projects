// A Qwirkle test project with xUnit (also my first use of xUnit)
//
// 2023-11-23   PV      UnitTests_Base
// 2023-12-08   PV      UnitTests_Play

using LibQwirkle;
using System.Diagnostics;

namespace xUnit_QwirkleTestProject;

public class UnitTests_Base
{
    readonly Board b = new();

    public UnitTests_Base()
    {
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
    }

    [Fact]
    public void IsCompatibleTest1()
    {
        string s = "Col4 5 5 5 5 \r\n" +
                   "Row9 0 1 2 3 \r\n" +
                   "46       x   \r\n" +
                   "47     x O x \r\n" +
                   "48     x O x \r\n" +
                   "49   · · O x \r\n" +
                   "50 · [ < O · \r\n" +
                   "51   · · x   \r\n";
        string s2 = b.AsString(false, new Tile(Shape.Circle, Color.Purple, 2));
        Debug.Assert(s == s2);
    }

    [Fact]
    public void IsCompatibleTest2()
    {
        string s = "Col4 5 5 5 5 \r\n" +
                   "Row9 0 1 2 3 \r\n" +
                   "46       ·   \r\n" +
                   "47     · O · \r\n" +
                   "48     x O x \r\n" +
                   "49   · · O · \r\n" +
                   "50 · [ < O · \r\n" +
                   "51   · x ·   \r\n";

         string s2 = b.AsString(false, new Tile(Shape.Lozange, Color.Green, 2));
        Debug.Assert(s == s2);
    }

    [Fact]
    public void CountPointsTest()
    {
        HashSet<Move> moves =
        [
            new(49, 51, new Tile(Shape.Lozange, Color.Blue, 1)),
            new(49, 53, new Tile(Shape.Square, Color.Blue, 1)),
            new(49, 54, new Tile(Shape.Star, Color.Blue, 1)),
        ];
        Debug.Assert(b.CountPoints(moves).Points == 6);
        b.AddMoves(moves);

        moves =
        [
            new(49, 55, new Tile(Shape.Cross, Color.Blue, 1)),
            new(49, 56, new Tile(Shape.Clover, Color.Blue, 1)),
        ];
        Debug.Assert(b.CountPoints(moves).Points == 12);
        b.AddMoves(moves);

        moves =
        [
            new(48, 51, new Tile(Shape.Lozange, Color.Green, 1)),
            new(48, 53, new Tile(Shape.Square, Color.Green, 1)),
        ];
        Debug.Assert(b.CountPoints(moves).Points == 8);
        b.AddMoves(moves);

        moves =
        [
            new(48, 54, new Tile(Shape.Star, Color.Green, 1)),
            new(50, 54, new Tile(Shape.Star, Color.Yellow, 1)),
            new(51, 54, new Tile(Shape.Star, Color.Orange, 1)),
        ];
        Debug.Assert(b.CountPoints(moves).Points == 8);
        b.AddMoves(moves);
    }
}

public class UnitTests_Play
{
    readonly Board b = new();

    public UnitTests_Play()
    {
        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);

        b.AddMove(new Move(50, 50, t1));
        b.AddMove(new Move(50, 51, t2));
        b.AddMove(new Move(50, 52, t3));
    }

    [Fact]
    public void NotPlayable()
    {
        var t4 = new Tile(Shape.Clover, Color.Blue, 1);
        var t5 = new Tile(Shape.Clover, Color.Green, 1);
        var t6 = new Tile(Shape.Clover, Color.Orange, 1);

        var hand = new Hand([t4, t5, t6]);
        var play = b.Play(hand);
        Debug.Assert(play.Points == 0);
    }

    [Fact]
    public void PlayTest()
    {
        var t4 = new Tile(Shape.Lozange, Color.Yellow, 1);
        var t5 = new Tile(Shape.Lozange, Color.Green, 1);
        b.AddMove(new Move(49, 51, t4));
        b.AddMove(new Move(48, 51, t5));

        var h1 = new Tile(Shape.Circle, Color.Yellow, 1);
        var h2 = new Tile(Shape.Circle, Color.Green, 1);
        var h3 = new Tile(Shape.Square, Color.Orange, 1);
        var h4 = new Tile(Shape.Clover, Color.Blue, 1);
        var h5 = new Tile(Shape.Clover, Color.Red, 1);
        var h6 = new Tile(Shape.Star, Color.Red, 1);

        var hand = new Hand([h1, h2, h3, h4, h5, h6]);
        var play = b.Play(hand);
        Debug.Assert(play.Moves.SetEquals(new HashSet<Move> { new(48, 52, h2), new(49, 52, h1) }));
        Debug.Assert(play.Points == 7);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h5, h6])));
        b.AddMoves(play.Moves);
        hand = play.NewHand;

        var h7 = new Tile(Shape.Cross, Color.Yellow, 1);
        var h8 = new Tile(Shape.Cross, Color.Red, 1);
        hand.Add(h7);
        hand.Add(h8);

        play = b.Play(hand);
        Debug.Assert(play.Moves.Count == 3);
        Debug.Assert(play.Moves.All(m => m.Row == 50));
        Debug.Assert(play.Moves.All(m => m.Tile.Color == Color.Red));
        Debug.Assert(play.Points == 12);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h7])));
        b.AddMoves(play.Moves);
        hand = play.NewHand;
    }
}

public class UnitTests_Misc
{
    [Fact]
    public void PlayOnEmptyBoardTest()
    {
        var b = new Board();

        var h1 = new Tile(Shape.Circle, Color.Yellow, 1);
        var h2 = new Tile(Shape.Circle, Color.Green, 1);
        var h3 = new Tile(Shape.Square, Color.Orange, 1);
        var h4 = new Tile(Shape.Clover, Color.Blue, 1);
        var h5 = new Tile(Shape.Clover, Color.Red, 1);
        var h6 = new Tile(Shape.Circle, Color.Red, 1);

        var hand = new Hand([h1, h2, h3, h4, h5, h6]);

        var play = b.Play(hand);
        Debug.Assert(play.Moves.All(m => m.Row == 50) || play.Moves.All(m => m.Col == 50));
        Debug.Assert(play.Moves.All(m => m.Tile.Shape == Shape.Circle));
        Debug.Assert(play.Points == 3);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h5])));
        b.AddMoves(play.Moves);
    }

    [Fact]
    public void DockTest()
    {
        var d = new Dock();
        var check = new HashSet<Tile>();

        while (!d.IsEmpty)
        {
            var t = d.GetTile();
            Debug.Assert(!check.Contains(t));
            check.Add(t);
        }
        Debug.Assert(check.Count == 6 * 6 * 3);
        foreach (Shape s in Enum.GetValues(typeof(Shape)))
            foreach (Color c in Enum.GetValues(typeof(Color)))
                for (int i = 1; i <= 3; i++)
                    Debug.Assert(check.Contains(new(s, c, i)));

        Assert.Throws<InvalidOperationException>(() => d.GetTile());
    }

    [Fact]
    private static void FullPlayTest()
    {
        // Do 30 full plays, takes about 5.5s on WOTAN
        for (int i = 0; i < 30; i++)
        {
            var b = new Board();
            var d = new Dock();

            var hand = new Hand();
            for (; ; )
            {
                while (!d.IsEmpty && hand.Count < 6)
                    hand.Add(d.GetTile());
                if (hand.Count == 0)
                {
                    // Normal end, full dock played
                    Debug.Assert(b.TilesCount == 6 * 6 * 3);
                    break;
                }

                var play = b.Play(hand);
                if (play.Points == 0)
                {
                    // If dock is empty, no possibility to return tiles
                    if (d.IsEmpty)
                        break;

                    // Returning hand to dock and take 6 random tiles again
                    d.ReturnTiles(hand);
                    hand.Clear();
                    continue;
                }
                b.AddMoves(play.Moves);
                hand = play.NewHand;
            }
        }
    }

}
