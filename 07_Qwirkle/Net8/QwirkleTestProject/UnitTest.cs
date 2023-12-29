// A Qwirkle test project with xUnit (also my first use of xUnit)
//
// 2023-11-23   PV      UnitTests_Base
// 2023-12-08   PV      UnitTests_Play

using LibQwirkle;
using System.Diagnostics;

namespace xUnit_QwirkleTestProject;

public class UnitTests_Base
{
    readonly Board board = new();

    public UnitTests_Base()
    {
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
        string s2 = board.AsString(false, new Tile(Shape.Circle, Color.Purple, 2));
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

         string s2 = board.AsString(false, new Tile(Shape.Lozange, Color.Green, 2));
        Debug.Assert(s == s2);
    }

    [Fact]
    public void CountPointsTest()
    {
        HashSet<TileRowCol> moves =
        [
            new(new Tile(Shape.Lozange, Color.Blue, 1), 49, 51),
            new(new Tile(Shape.Square, Color.Blue, 1), 49, 53),
            new(new Tile(Shape.Star, Color.Blue, 1), 49, 54),
        ];
        Debug.Assert(board.CountPoints(moves).Points == 6);
        board.AddMoves(moves);

        moves =
        [
            new(new Tile(Shape.Cross, Color.Blue, 1), 49, 55),
            new(new Tile(Shape.Clover, Color.Blue, 1), 49, 56),
        ];
        Debug.Assert(board.CountPoints(moves).Points == 12);
        board.AddMoves(moves);

        moves =
        [
            new(new Tile(Shape.Lozange, Color.Green, 1), 48, 51),
            new(new Tile(Shape.Square, Color.Green, 1), 48, 53),
        ];
        Debug.Assert(board.CountPoints(moves).Points == 8);
        board.AddMoves(moves);

        moves =
        [
            new(new Tile(Shape.Star, Color.Green, 1), 48, 54),
            new(new Tile(Shape.Star, Color.Yellow, 1), 50, 54),
            new(new Tile(Shape.Star, Color.Orange, 1), 51, 54),
        ];
        Debug.Assert(board.CountPoints(moves).Points == 8);
        board.AddMoves(moves);
    }

    [Fact]
    public void TwoPointsTest()
    {
        HashSet<TileRowCol> moves = [
            new(new Tile(Shape.Square, Color.Red, 2), 51, 51),
        ];

        // Should be 2 points, but evaluates to 3 points...
        PointsBonus pb = board.CountPoints(moves);
        Debug.Assert(pb.Points == 2);
    }
}

public class UnitTests_Play
{
    readonly Board board = new();

    public UnitTests_Play()
    {
        var t1 = new Tile(Shape.Square, Color.Red, 1);
        var t2 = new Tile(Shape.Lozange, Color.Red, 1);
        var t3 = new Tile(Shape.Circle, Color.Red, 1);

        board.AddMove(new TileRowCol(t1, 50, 50));
        board.AddMove(new TileRowCol(t2, 50, 51));
        board.AddMove(new TileRowCol(t3, 50, 52));
    }

    [Fact]
    public void NotPlayable()
    {
        var t4 = new Tile(Shape.Clover, Color.Blue, 1);
        var t5 = new Tile(Shape.Clover, Color.Green, 1);
        var t6 = new Tile(Shape.Clover, Color.Orange, 1);

        var hand = new Hand([t4, t5, t6]);
        var play = board.Play(hand);
        Debug.Assert(play.PB.Points == 0);
    }

    [Fact]
    public void PlayTest()
    {
        var t4 = new Tile(Shape.Lozange, Color.Yellow, 1);
        var t5 = new Tile(Shape.Lozange, Color.Green, 1);
        board.AddMove(new TileRowCol(t4, 49, 51));
        board.AddMove(new TileRowCol(t5, 48, 51));

        var h1 = new Tile(Shape.Circle, Color.Yellow, 1);
        var h2 = new Tile(Shape.Circle, Color.Green, 1);
        var h3 = new Tile(Shape.Square, Color.Orange, 1);
        var h4 = new Tile(Shape.Clover, Color.Blue, 1);
        var h5 = new Tile(Shape.Clover, Color.Red, 1);
        var h6 = new Tile(Shape.Star, Color.Red, 1);

        var hand = new Hand([h1, h2, h3, h4, h5, h6]);
        var play = board.Play(hand);
        Debug.Assert(play.Moves.SetEquals(new HashSet<TileRowCol> { new(h2, 48, 52), new(h1, 49, 52) }));
        Debug.Assert(play.PB.Points == 7);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h5, h6])));
        board.AddMoves(play.Moves);
        hand = play.NewHand;

        var h7 = new Tile(Shape.Cross, Color.Yellow, 1);
        var h8 = new Tile(Shape.Cross, Color.Red, 1);
        hand.Add(h7);
        hand.Add(h8);

        play = board.Play(hand);
        Debug.Assert(play.Moves.Count == 3);
        Debug.Assert(play.Moves.All(m => m.Row == 50));
        Debug.Assert(play.Moves.All(m => m.Tile.Color == Color.Red));
        Debug.Assert(play.PB.Points == 12);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h7])));
        board.AddMoves(play.Moves);
        hand = play.NewHand;
    }
}

public class UnitTests_Misc
{
    [Fact]
    public void PlayOnEmptyBoardTest()
    {
        var board = new Board();

        var h1 = new Tile(Shape.Circle, Color.Yellow, 1);
        var h2 = new Tile(Shape.Circle, Color.Green, 1);
        var h3 = new Tile(Shape.Square, Color.Orange, 1);
        var h4 = new Tile(Shape.Clover, Color.Blue, 1);
        var h5 = new Tile(Shape.Clover, Color.Red, 1);
        var h6 = new Tile(Shape.Circle, Color.Red, 1);

        var hand = new Hand([h1, h2, h3, h4, h5, h6]);

        var play = board.Play(hand);
        Debug.Assert(play.Moves.All(m => m.Row == 50) || play.Moves.All(m => m.Col == 50));
        Debug.Assert(play.Moves.All(m => m.Tile.Shape == Shape.Circle));
        Debug.Assert(play.PB.Points == 3);
        Debug.Assert(play.NewHand.Equals(new Hand([h3, h4, h5])));
        board.AddMoves(play.Moves);
    }

    [Fact]
    public void BagTest()
    {
        var bag = new Bag();
        var check = new HashSet<Tile>();

        while (!bag.IsEmpty)
        {
            var t = bag.GetTile();
            Debug.Assert(!check.Contains(t));
            check.Add(t);
        }
        Debug.Assert(check.Count == 6 * 6 * 3);
        foreach (Shape s in Enum.GetValues(typeof(Shape)))
            foreach (Color c in Enum.GetValues(typeof(Color)))
                for (int i = 1; i <= 3; i++)
                    Debug.Assert(check.Contains(new(s, c, i)));

        Assert.Throws<InvalidOperationException>(() => bag.GetTile());
    }

    [Fact]
    private static void FullPlayTest()
    {
        // Do 30 full plays, takes about 5.5s on WOTAN
        for (int i = 0; i < 30; i++)
        {
            var board = new Board();
            var bag = new Bag();

            var hand = new Hand();
            for (; ; )
            {
                while (!bag.IsEmpty && hand.Count < 6)
                    hand.Add(bag.GetTile());
                if (hand.Count == 0)
                {
                    // Normal end, full dock played
                    Debug.Assert(board.TilesCount == 6 * 6 * 3);
                    break;
                }

                var play = board.Play(hand);
                if (play.PB.Points == 0)
                {
                    // If dock is empty, no possibility to return tiles
                    if (bag.IsEmpty)
                        break;

                    // Returning hand to dock and take 6 random tiles again
                    bag.ReturnTiles(hand);
                    hand.Clear();
                    continue;
                }
                board.AddMoves(play.Moves);
                hand = play.NewHand;
            }
        }
    }

}

public class UnitTests_Evaluate
{
    readonly Board board = new();

    public UnitTests_Evaluate()
    {
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
    }

    [Fact]
    public void NotInSameRowOrSameColumn()
    {
        HashSet<TileRowCol> moves = [
            new(new Tile(Shape.Circle, Color.Green, 1), 48, 51),
            new(new Tile(Shape.Square, Color.Green, 1), 47, 53),
        ];
        bool status;
        string msg;
        (status, msg) = board.EvaluateMoves(moves);
        Debug.Assert(status == false);
        Debug.Assert(msg== "Les tuiles jouées doivent être situées sur une même ligne ou une même colonne");
    }

    [Fact]
    public void NotSameShapeOrSameColor()
    {
        HashSet<TileRowCol> moves = [
            new(new Tile(Shape.Circle, Color.Green, 1), 48, 51),
            new(new Tile(Shape.Square, Color.Orange, 1), 48, 53),
        ];
        bool status;
        string msg;
        (status, msg) = board.EvaluateMoves(moves);
        Debug.Assert(status == false);
        Debug.Assert(msg == "2 couleurs et 2 formes différentes, les tuiles jouées doivent être de la même couleur ou de la même forme");
    }

    [Fact]
    public void NotOnPlayableCell()
    {
        HashSet<TileRowCol> moves = [
            new(new Tile(Shape.Circle, Color.Green, 1), 48, 50),
        ];
        bool status;
        string msg;
        (status, msg) = board.EvaluateMoves(moves);
        Debug.Assert(status == false);
        Debug.Assert(msg == "La tuile Circle Green #1 en position (48, 50) n'est pas posée sur une cellule jouable");
    }

    [Fact]
    public void NotCompatibleTile()
    {
        HashSet<TileRowCol> moves = [
            new(new Tile(Shape.Star, Color.Blue, 1), 49, 51),
        ];
        bool status;
        string msg;
        (status, msg) = board.EvaluateMoves(moves);
        Debug.Assert(status == false);
        Debug.Assert(msg == "La tuile Star Blue #1 en position (49, 51) n'est pas compatible avec les tuiles qui l'entourent");
    }

    [Fact]
    public void EvaluateOk()
    {
        HashSet<TileRowCol> moves = [
            new(new Tile(Shape.Lozange, Color.Blue, 1), 49, 51),
        ];
        bool status;
        string msg;
        (status, msg) = board.EvaluateMoves(moves);
        Debug.Assert(status == true);
        Debug.Assert(msg == "");
    }
}
