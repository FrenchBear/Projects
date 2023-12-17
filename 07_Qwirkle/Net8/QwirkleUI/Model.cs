// QuirkleUI
// Model
//
// 2023-12-11   PV      First version

using LibQwirkle;

namespace QwirkleUI;

internal class Model(MainViewModel viewModel)
{
    private readonly MainViewModel viewModel = viewModel;
    public Board Board = new();
    public Hand[] Hands = [new()];      // Just 1 hand for how

    internal void NewBoard() => Board = new Board();

    public BoundingRectangle Bounds() => new(new Position(Board.RowMin, Board.ColMin), new Position(Board.RowMax, Board.ColMax));

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
        Board.AddMove(new Move(49, 52, t4));
        Board.AddMove(new Move(48, 52, t5));
        Board.AddMove(new Move(47, 52, t6));

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
