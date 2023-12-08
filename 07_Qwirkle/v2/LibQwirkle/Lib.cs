// LibQwirkle
// Core library for Qwirkle reboot
//
// 2023-11-23   PV

using System.Collections;
using System.Diagnostics;
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

[DebuggerDisplay("Tile {this.AsString(null)}")]
public record Tile(Shape S, Color C, int Instance)
{
    public Shape Shape { get; } = S;
    public Color Color { get; } = C;
    public int Instance { get; } = Instance;
}

[DebuggerDisplay("Move ({Row}, {Col}) {Tile.AsString(false)}")]
public record Move(int Row, int Col, Tile T)
{
    public int Row { get; } = Row;
    public int Col { get; } = Col;
    public Tile Tile { get; } = T;

    public string AsString(bool? Color)
        => $"({Row}, {Col}) {Tile.AsString(Color)}";
}

public enum CellState
{
    EmptyIsolated,
    PotentiallyPlayable,
    Tiled,
}

[DebuggerDisplay("Play: {AsString(null)}")]
public record Play(HashSet<Move> Moves, int Points, Hand NewHand)
{
    public HashSet<Move> Moves { get; init; } = Moves;
    public int Points { get; init; } = Points;
    public Hand NewHand { get; init; } = NewHand;

    public string AsString(bool? Color)
        => string.Join(", ", Moves.Select(m => m.AsString(Color))) + $" -> {Points} points, hand={NewHand.AsString(Color)}";
}

[DebuggerDisplay("Hand: {AsString(null)}")]
public class Hand: HashSet<Tile>, IEquatable<Hand>
{
    public Hand(IEnumerable<Tile> tiles) : base(tiles) { }
    public Hand(HashSet<Tile> tiles) : base(tiles) { }

    public string AsString(bool? Color)
        => string.Join(" ", this.Select(m => m.AsString(Color)));

    public bool Equals(Hand? hand) 
        => hand != null && SetEquals(hand);

    public override bool Equals(object? obj) 
        => Equals(obj as Hand);

    public override int GetHashCode() 
        => base.GetHashCode();
}
public class Board: IEnumerable<Move>
{
    readonly Board? BaseBoard = null;
    readonly HashSet<Move> Moves = [];

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

    public void AddMoves(HashSet<Move> moves)
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

    public int CountPoints(HashSet<Move> moves)
    {
        if (moves.Count == 0)
            return 0;

        // Use a temp new board with moves actually played
        var nb = new Board(this);
        nb.AddMoves(moves);

        bool isHorizontal = true;
        bool isVertical = true;

        int mRow = -1;
        int mCol = -1;
        foreach(Move m in moves)
        {
            if (mRow==-1)
                mRow = m.Row;
            else if (isHorizontal && mRow!=m.Row)
                isHorizontal = false;

            if (mCol==-1)
                mCol = m.Col;
            else if (isVertical && mCol!=m.Col)
                isVertical = false;
        }
        if (moves.Count>1)
            Debug.Assert(isHorizontal ^ isVertical);

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
            row = mRow;
            col = mCol;
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
            row = mRow;
            col = mCol;
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
        var PossiblePlays = new List<Play>();

        for (int row = RowMax + 1; row >= RowMin - 1; row--)
            for (int col = ColMin - 1; col <= ColMax + 1; col++)
            {
                var state = GetCellState(row, col);
                if (state == CellState.PotentiallyPlayable)
                {
                    //Console.WriteLine($"PotentiallyPlayable {row}, {col}");
                    foreach (Tile t in hand)
                        if (IsCompatible(row, col, t))
                        {
                            //Console.WriteLine("  Compatible: " + t.AsString(true));
                            var CurrentMoves = new HashSet<Move>();
                            ExploreMove(this, this, hand, CurrentMoves, PossiblePlays, new Move(row, col, t), true, true);
                        }
                }

            }

        // Show all possible plays
        //foreach (Play p in PossiblePlays)
        //    Console.WriteLine(p.AsString(true));

        // For now, we just implement simple max(points) strategy, just return a random solution from the list
        // since they all have the same max(points)
        // In rare cases, this list could be empty
        var rnd = new Random();
        if (PossiblePlays.Count == 0)
            return new Play([], 0, hand);
        return PossiblePlays[rnd.Next(PossiblePlays.Count)];
    }

    private static void ExploreMove(Board startBoard, Board b, Hand h, HashSet<Move> CurrentMoves, List<Play> PossiblePlays, Move move, bool NS, bool EW)
    {
        //Console.WriteLine($"\nExploreMove {move.AsString(true)}  NS={NS} EW={EW}");

        var newB = new Board(b);
        newB.AddMove(move);
        var newH = new Hand(h.Except([move.Tile]));
        var newCurrentMoves = new HashSet<Move>(CurrentMoves) { move };

        // Quick and dirty firtering, only keep move if it produces equal or more points than current max(points)
        // if points are actually greater than max, forget all previous possible plays
        int points = startBoard.CountPoints(newCurrentMoves);
        int pMax = PossiblePlays.Count == 0 ? 0 : PossiblePlays.Max(p => p.Points);
        if (points > pMax)
            PossiblePlays.Clear();
        if (points >= pMax)
            PossiblePlays.Add(new Play(newCurrentMoves, points, newH));

        // If there are no remaining tiles in hand, no need to continue
        if (newH.Count == 0)
            return;

        if (NS)
        {
            TryExplore(startBoard, newB, newH, newCurrentMoves, PossiblePlays, move.Row, move.Col, 1, 0);
            TryExplore(startBoard, newB, newH, newCurrentMoves, PossiblePlays, move.Row, move.Col, -1, 0);
        }
        if (EW)
        {
            TryExplore(startBoard, newB, newH, newCurrentMoves, PossiblePlays, move.Row, move.Col, 0, 1);
            TryExplore(startBoard, newB, newH, newCurrentMoves, PossiblePlays, move.Row, move.Col, 0, -1);
        }
    }

    private static void TryExplore(Board startBoard, Board b, Hand h, HashSet<Move> CurrentMoves, List<Play> PossiblePlays, int row, int col, int deltaRow, int deltaCol)
    {
        //Console.WriteLine($"\nTryExplore ({row}, {col})  deltaRow={deltaRow} deltaCol={deltaCol}");

        for (; ; )
        {
            row += deltaRow;
            col += deltaCol;
            var state = b.GetCellState(row, col);
            if (state == CellState.Tiled)
                continue;
            if (state == CellState.EmptyIsolated)
                return;
            Debug.Assert(state == CellState.PotentiallyPlayable);
            //Console.WriteLine($"PotentiallyPlayable {row}, {col}");
            foreach (Tile t in h)
                if (b.IsCompatible(row, col, t))
                {
                    //Console.WriteLine($"  Compatible: {t.AsString(true)}");
                    ExploreMove(startBoard, b, h, CurrentMoves, PossiblePlays, new Move(row, col, t), deltaRow != 0, deltaCol != 0);
                }
        }
    }

    // Make Board enumerable, returning all moves, could be convenient
    public IEnumerator<Move> GetEnumerator()
    {
        if (BaseBoard != null)
            foreach (var move in BaseBoard)
                yield return move;
        foreach (var move in Moves)
            yield return move;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
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

    public static string AsString(this Tile t, bool? color)
    {
        var sb = new StringBuilder();

        if (color == true)
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
            Shape.Circle => "O",
            Shape.Cross => "+",
            Shape.Lozange => "<",
            Shape.Square => "[",
            Shape.Star => "*",
            Shape.Clover => "%",
            _ => "?"
        });
        if (color == true)
            sb.Append(ConsoleColorDefault);
        else if (color == null)
            sb.Append(t.Color switch
            {
                Color.Red => 'r',
                Color.Orange => 'o',
                Color.Yellow => 'y',
                Color.Green => 'g',
                Color.Blue => 'b',
                Color.Purple => 'm',
                _ => ConsoleColorDefault
            });
        return sb.ToString();
    }
}
