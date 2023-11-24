// A Qwirkle test project with xUnit (also my first use of xUnit)
//
// 2023-11-23   PV

using LibQwirkle;
using System.Diagnostics;

namespace xUnit_QwirkleTestProject;

public class UnitTests1
{
    readonly Board b = new();

    public UnitTests1()
    {
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
    }

    [Fact]
    public void IsCompatibleTest1()
    {
        string s = "   4950515253\r\n" +
                   "51   · · x   \r\n" +
                   "50 · D C A · \r\n" +
                   "49   · · A x \r\n" +
                   "48     x A x \r\n" +
                   "47     x A x \r\n" +
                   "46       x   \r\n";
        string s2 = b.AsString(false, new Tile(Color.Purple, Shape.Circle, 2));
        Debug.Assert(s == s2);
    }

    [Fact]
    public void IsCompatibleTest2()
    {
        string s = "   4950515253\r\n" +
                   "51   · x ·   \r\n" +
                   "50 · D C A · \r\n" +
                   "49   · · A · \r\n" +
                   "48     x A x \r\n" +
                   "47     · A · \r\n" +
                   "46       ·   \r\n";
        string s2 = b.AsString(false, new Tile(Color.Green, Shape.Lozange, 2));
        Debug.Assert(s == s2);
    }

    [Fact]
    public void CountPointsTest1()
    {
        List<Move> moves =
        [
            new(49,51,new Tile(Color.Blue, Shape.Lozange, 1)),
            new(49,53,new Tile(Color.Blue, Shape.Square, 1)),
            new(49,54,new Tile(Color.Blue, Shape.Star, 1)),
        ];
        Debug.Assert(b.CountPoints(moves)==6);
        
        b.AddMoves(moves);
        moves =
        [
            new(49,55,new Tile(Color.Blue, Shape.Cross, 1)),
            new(49,56,new Tile(Color.Blue, Shape.Clover, 1)),
        ];
        Debug.Assert(b.CountPoints(moves) == 12);

        b.AddMoves(moves);
        moves =
        [
            new(48, 51, new Tile(Color.Green, Shape.Lozange, 1)),
            new(48, 53, new Tile(Color.Green, Shape.Square, 1)),
        ];
        Debug.Assert(b.CountPoints(moves) == 8);
        b.AddMoves(moves);
    }
}
