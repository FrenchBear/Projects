// LibQwirkle
// Core library for Qwirkle reboot
//
// 2023-11-23   PV

using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace LibQwirkle;

public enum Shape
{
    Circle,
    Cross,
    Lozange,
    Square,
    Star,
    Clover,
}

public enum Color
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Purple,
}

public record Tile(Color C, Shape S, int Instance)
{
    public Color Color { get; } = C;
    public Shape Shape { get; } = S;
    public int Instance { get; } = Instance;
}

public record Move(int Row, int Col, Tile T)
{
    public int Row { get; } = Row;
    public int Col { get; } = Col;
    public Tile Tile { get; } = T;
}

public enum CellState
{
    EmptyIsolated,
    PotentiallyPlayable,
    Tiled,
}

public record Play(List<Move> Moves, int Points, Hand NewHand)
{
    public List<Move> Moves { get; init; } = Moves;
    public int Points { get; init; } = Points;
    public Hand NewHand { get; init; } = NewHand;

    public string AsString(bool Color)
        => string.Join(", ", Moves.Select(m => $"({m.Row}, {m.Col}) {m.Tile.AsString(Color)}"));
}

public class Hand: HashSet<Tile>
{
    public Hand(IEnumerable<Tile> tiles) : base(tiles) { }
    public Hand(HashSet<Tile> tiles) : base(tiles) { }

    public string AsString(bool Color)
        => String.Join(" ", this.Select(m => m.AsString(Color)));
};

public class Board
{
    readonly Board? BaseBoard = null;
    readonly List<Move> Moves = [];

    public Board() { }
    public Board(Board baseBoard)
        => BaseBoard = baseBoard;

    public int RowMin => BaseBoard == null ? rowMin : Math.Min(rowMin, BaseBoard.RowMin);
    public int RowMax => BaseBoard == null ? rowMax : Math.Max(rowMax, BaseBoard.RowMax);
    public int ColMin => BaseBoard == null ? colMin : Math.Min(colMin, BaseBoard.ColMin);
    public int ColMax => BaseBoard == null ? colMax : Math.Max(colMax, BaseBoard.ColMax);

    private int rowMin = 100, rowMax = 0;
    private int colMin = 100, colMax = 0;

    public bool IsEmpty => BaseBoard == null && Moves.Count == 0;

    public void AddMove(Move m)
    {
        if (!IsEmpty)
            Debug.Assert(GetCellState(m.Row, m.Col) == CellState.PotentiallyPlayable && IsCompatible(m.Row, m.Col, m.T));

        Moves.Add(m);
        rowMin = Math.Min(rowMin, m.Row);
        rowMax = Math.Max(rowMax, m.Row);
        colMin = Math.Min(colMin, m.Col);
        colMax = Math.Max(colMax, m.Col);
    }

    public void AddMoves(List<Move> moves)
    {
        foreach (var move in moves)
            AddMove(move);
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

    public CellState GetCellState(int row, int col)
    {
        Tile? t = GetTile(row, col);
        if (t != null)
            return CellState.Tiled;

        Tile? tn = GetTile(row - 1, col);
        Tile? ts = GetTile(row + 1, col);
        Tile? te = GetTile(row, col + 1);
        Tile? tw = GetTile(row, col - 1);
        if (tn == null && ts == null && te == null && tw == null)
            return CellState.EmptyIsolated;
        return CellState.PotentiallyPlayable;
    }

    public bool IsCompatible(int row, int col, in Tile t)
    {
        Debug.Assert(GetTile(row, col) == null);
        if (!IsDirectionCompatible(row, col, -1, 0, in t, out bool nn))
            return false;
        if (!IsDirectionCompatible(row, col, 1, 0, in t, out bool sn))
            return false;
        if (!IsDirectionCompatible(row, col, 0, 1, in t, out bool en))
            return false;
        if (!IsDirectionCompatible(row, col, 0, -1, in t, out bool wn))
            return false;
        Debug.Assert(nn || sn || en || wn);    // Check that we have at least one neighbor
        return true;
    }

    private bool IsDirectionCompatible(int row, int col, int deltaRow, int deltaCol, in Tile refTile, out bool neighborFound)
    {
        neighborFound = false;
        for (; ; )
        {
            row += deltaRow;
            col += deltaCol;
            var tmpTile = GetTile(row, col);
            if (tmpTile == null)
                return true;
            neighborFound = true;
            if (tmpTile.Color != refTile.Color && tmpTile.Shape != refTile.Shape)
                return false;
            if (tmpTile.Color == refTile.Color && tmpTile.Shape == refTile.Shape)
                return false;
        }
    }

    public void Print() => Console.WriteLine(this.AsString(true));

    public int CountPoints(List<Move> moves)
    {
        // Comptage, use a temp new board with moves actually played
        var nb = new Board(this);
        nb.AddMoves(moves);
        bool isHorizontal = moves.Count == 1 || moves[0].Row == moves[1].Row;
        int points = 0;
        int row, col, dp;
        if (isHorizontal)
        {
            // Count tiles in vertical blocks of more than 1 tile for each played tile
            foreach (var move in moves)
            {
                row = move.Row;
                col = move.Col;
                // Find the topmost position
                while (nb.GetCellState(row + 1, col) == CellState.Tiled)
                    row++;
                dp = 0;
                // Explore to bottommost position
                for (; ; )
                {
                    dp++;
                    row--;
                    if (nb.GetCellState(row, col) != CellState.Tiled)
                        break;
                }
                // Only vertical bands with more than 1 tile are actually counted
                if (dp > 1)
                    points += dp == 6 ? 12 : dp;
            }

            // Count tiles in horizontal block
            // Find the leftmost position
            row = moves[0].Row;
            col = moves[0].Col;
            // Start with any placed tile, search for leftmost tile of the block
            while (nb.GetCellState(row, col - 1) == CellState.Tiled)
                col--;
            // Loop on horizontal block of tiles
            dp = 0;
            for (; ; )
            {
                dp++;
                col++;
                if (nb.GetCellState(row, col) != CellState.Tiled)
                    break;
            }
            points += dp == 6 ? 12 : dp;
        }
        else
        {
            // Count tiles in horizontal blocks of more than 1 tile for each placed tile
            foreach (var move in moves)
            {
                row = move.Row;
                col = move.Col;
                // Find the leftmost position
                while (nb.GetCellState(row, col - 1) == CellState.Tiled)
                    col--;
                dp = 0;
                // Explore to rightmost position
                for (; ; )
                {
                    dp++;
                    col++;
                    if (nb.GetCellState(row, col) != CellState.Tiled)
                        break;
                }
                // Only horizontal bands with more than 1 tile are actually counted
                if (dp > 1)
                    points += dp == 6 ? 12 : dp;
            }

            // Count tiles in vertical block
            // Find the topmost position
            row = moves[0].Row;
            col = moves[0].Col;
            // Start with any placed tile, search for topmost tile of the block
            while (nb.GetCellState(row + 1, col) == CellState.Tiled)
                row++;
            // Loop on vertical block of tiles
            dp = 0;
            for (; ; )
            {
                dp++;
                row--;
                if (nb.GetCellState(row, col) != CellState.Tiled)
                    break;
            }
            points += dp == 6 ? 12 : dp;
        }

        return points;
    }

    public Play Play(Hand hand)
    {
        // For now, that's just a simulation
        Tile t1 = hand.First(t => t.Shape == Shape.Lozange && t.Color == Color.Blue);
        Tile t2 = hand.First(t => t.Shape == Shape.Square && t.Color == Color.Blue);
        Tile t3 = hand.First(t => t.Shape == Shape.Star && t.Color == Color.Blue);
        var moves = new List<Move>()
        {
            new(49,51,t1),
            new(49,53,t2),
            new(49,54,t3),
        };

        return new Play(moves, CountPoints(moves), new Hand(hand.Except([t1, t2, t3])));
    }
}

public static class ConsoleSupport
{
    const string ConsoleColorRed = "\x1b[91m";
    const string ConsoleColorDarkYellow = "\x1b[33m";
    const string ConsoleColorYellow = "\x1b[93m";
    const string ConsoleColorGreen = "\x1b[92m";
    const string ConsoleColorBlue = "\x1b[94m";
    const string ConsoleColorMagenta = "\x1b[95m";
    const string ConsoleColorCyan = "\x1b[96m";
    const string ConsoleColorDarkGray = "\x1b[90m";
    const string ConsoleColorDefault = "\x1b[39m";

    public static string AsString(this Board b, bool color, Tile? checkTileCompat = null)
    {
        var sb = new StringBuilder();

        sb.Append("   ");
        for (int col = b.ColMin - 1; col <= b.ColMax + 1; col++)
            sb.Append($"{col:D02}");
        sb.AppendLine();
        for (int row = b.RowMax + 1; row >= b.RowMin - 1; row--)
        {
            sb.Append($"{row:D02} ");
            for (int col = b.ColMin - 1; col <= b.ColMax + 1; col++)
            {
                var state = b.GetCellState(row, col);
                switch (state)
                {
                    case CellState.EmptyIsolated:
                        sb.Append("  ");
                        break;

                    case CellState.PotentiallyPlayable:
                        if (checkTileCompat != null && b.IsCompatible(row, col, checkTileCompat))
                        {
                            if (color)
                                sb.Append(ConsoleColorCyan);
                            sb.Append("x ");
                        }
                        else
                        {
                            if (color)
                                sb.Append(ConsoleColorDarkGray);
                            sb.Append("· ");
                        }
                        if (color)
                            sb.Append(ConsoleColorDefault);
                        break;

                    case CellState.Tiled:
                        Tile t = b.GetTile(row, col) ?? throw new Exception("GetTile shouldn't return null");
                        sb.Append(t.AsString(color));
                        sb.Append(' ');
                        break;
                }
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public static string AsString(this Tile t, bool color)
    {
        var sb = new StringBuilder();

        if (color)
            sb.Append(t.Color switch
            {
                Color.Red => ConsoleColorRed,
                Color.Orange => ConsoleColorDarkYellow,
                Color.Yellow => ConsoleColorYellow,
                Color.Green => ConsoleColorGreen,
                Color.Blue => ConsoleColorBlue,
                Color.Purple => ConsoleColorMagenta,
                _ => ConsoleColorDefault
            });
        sb.Append(t.Shape switch
        {
            //Shape.Circle => "A",
            //Shape.Cross => "B",
            //Shape.Lozange => "C",
            //Shape.Square => "D",
            //Shape.Star => "E",
            //Shape.Clover => "F",
            Shape.Circle => "O",
            Shape.Cross => "+",
            Shape.Lozange => "<",
            Shape.Square => "[",
            Shape.Star => "*",
            Shape.Clover => "%",
            _ => "🞌 "
        });
        if (color)
            sb.Append(ConsoleColorDefault);

        return sb.ToString();
    }
}
