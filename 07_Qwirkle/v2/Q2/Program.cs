using System.ComponentModel;

namespace Q2;

internal class Program
{
    static void Main(string[] args)
    {
        var b = new Board();

        var t1 = new Tile(Color.Red, Shape.Square, 1);
        var t2 = new Tile(Color.Red, Shape.Lozange, 1);
        var t3 = new Tile(Color.Red, Shape.Circle, 1);
        var t4 = new Tile(Color.Blue, Shape.Circle, 1);
        var t5 = new Tile(Color.Green, Shape.Circle, 1);
        var t6 = new Tile(Color.Orange, Shape.Circle, 1);

        b.AddMove(new Move(50, 50, t1));
        b.AddMove(new Move(51, 50, t2));
        b.AddMove(new Move(52, 50, t3));
        b.AddMove(new Move(52, 51, t4));
        b.AddMove(new Move(52, 52, t5));
        b.AddMove(new Move(52, 53, t6));

        b.Print();
    }
}

enum Shape
{
    Circle,
    Cross,
    Lozange,
    Square,
    Star,
    Clover,
}

enum Color
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Purple,
}

record Tile(Color c, Shape s, int instance)
{
    public Color Color { get; } = c;
    public Shape Shape { get; } = s;
    public int Instance { get; } = instance;
}

record Move(int row, int col, Tile t)
{
    public int Row { get; } = row;
    public int Col { get; } = col;
    public Tile Tile { get; } = t;
}

enum CellStateType
{
    EmptyIsolated,
    EmptyPlayable,
    EmptyNotPlayable,
    Tiled,
}

enum ColorConstraintType
{
    NoConstraint,
    FixedColor,
    MultiColor,
}

enum ShapeConstraintType
{
    NoConstraint,
    FixedShape,
    MultiShape,
}

record ColorConstraint(ColorConstraintType type, Color? fixedColor, HashSet<Color>? takenColors)
{
    ColorConstraintType Type { get; } = type;
    Color? FixedColor { get; } = fixedColor;
    HashSet<Color>? TakenColors { get; } = takenColors;

}

record ShapeConstraint(ShapeConstraintType type, Shape? fixedShape, HashSet<Shape>? takenShapes)
{
    ShapeConstraintType Type { get; } = type;
    Shape? FixedShape { get; } = fixedShape;
    HashSet<Shape>? TakenShapes { get; } = takenShapes;

}

record CellStatus(CellStateType type, Tile? tile = null, ColorConstraint? nc = null, ColorConstraint? sc = null, ColorConstraint? ec = null, ColorConstraint? wc = null, ShapeConstraint? ns = null, ShapeConstraint? ss = null, ShapeConstraint? ee = null, ShapeConstraint? ws = null)
{
    public CellStateType Type { get; } = type;
    public Tile? Tile;
    public ColorConstraint? NColorConstraint, SColorConstraint, EColorConstraint, WColorConstraint;
    public ShapeConstraint? NShapeConstraint, SShapeConstraint, EShapeConstraint, WShapeConstraint;
}

class Board
{
    Board? BaseBoard = null;
    List<Move> Moves = new List<Move>();

    public int RowMin { get; private set; } = 100;
    public int RowMax { get; private set; } = 0;
    public int ColMin { get; private set; } = 100;
    public int ColMax { get; private set; } = 0;

    public void AddMove(Move m)
    {
        Moves.Add(m);
        RowMin = Math.Min(RowMin, m.Row);
        RowMax = Math.Max(RowMax, m.Row);
        ColMin = Math.Min(ColMin, m.Col);
        ColMax = Math.Max(ColMax, m.Col);
    }

    public Tile? GetTile(int row, int col)
    {
        foreach (var t in Moves)
            if (t.Row == row && t.Col == col)
                return t.Tile;
        if (BaseBoard == null)
            return null;
        return BaseBoard.GetTile(row, col);
    }

    public CellStatus GetCellState(int row, int col)
    {
        Tile? t = GetTile(row, col);
        if (t != null)
            return new CellStatus(CellStateType.Tiled, tile: t);

        Tile? tn = GetTile(row - 1, col);
        Tile? ts = GetTile(row + 1, col);
        Tile? te = GetTile(row, col + 1);
        Tile? tw = GetTile(row, col - 1);
        if (tn == null && ts == null && te == null && tw == null)
            return new CellStatus(CellStateType.EmptyIsolated);

        ColorConstraint? nc = null;
        ColorConstraint? sc = null;
        ColorConstraint? ec = null;
        ColorConstraint? wc = null;
        ShapeConstraint? ns = null;
        ShapeConstraint? ss = null;
        ShapeConstraint? ee = null;
        ShapeConstraint? ws = null;

        if (tn != null)
            (nc, ns) = GetConstraints(row, col, -1, 0);
        if (ts != null)
            (sc, ss) = GetConstraints(row, col, 1, 0);
    }

    internal void Print()
    {
        for (int row = RowMax; row >= RowMin; row--)
        {
            for (int col = ColMin; col <= ColMax; col++)
            {

            }
            Console.WriteLine();
        }
    }
}