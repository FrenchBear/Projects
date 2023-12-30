// LibQwirkle
// Core library for Qwirkle reboot
//
// 2023-11-23   PV
// 2023-12-08   PV      Bug, generates non-contiguous solutions and not homogeneous!!!
// 2023-12-09   PV      Fixed, a return was missing in TryExplore at the end ou outer loop

using System.Collections;
using System.Diagnostics;
using System.Text;

namespace LibQwirkle;

// ANSI sequences for color on console
public static class ConsoleSupport
{
    public const string ConsoleColorRed = "\x1b[91m";
    public const string ConsoleColorDarkYellow = "\x1b[33m";
    public const string ConsoleColorYellow = "\x1b[93m";
    public const string ConsoleColorGreen = "\x1b[92m";
    public const string ConsoleColorBlue = "\x1b[94m";
    public const string ConsoleColorPurple = "\x1b[95m";
    public const string ConsoleColorCyan = "\x1b[96m";
    public const string ConsoleColorDarkGray = "\x1b[90m";
    public const string ConsoleColorDefault = "\x1b[39m";
}

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

[DebuggerDisplay("Tile: {this.AsString(null, true)}")]
public record Tile(Shape S, Color C, int Instance)
{
    public Shape Shape { get; } = S;
    public Color Color { get; } = C;
    public int Instance { get; } = Instance;

    public string ShapeColor
        => Shape.ToString() + Color.ToString();

    public string AsString(bool? color, bool includeInstance=false)
    {
        var sb = new StringBuilder();

        if (color == true)
            sb.Append(Color switch
            {
                Color.Red => ConsoleSupport.ConsoleColorRed,
                Color.Orange => ConsoleSupport.ConsoleColorDarkYellow,
                Color.Yellow => ConsoleSupport.ConsoleColorYellow,
                Color.Green => ConsoleSupport.ConsoleColorGreen,
                Color.Blue => ConsoleSupport.ConsoleColorBlue,
                Color.Purple => ConsoleSupport.ConsoleColorPurple,
                _ => ConsoleSupport.ConsoleColorDefault
            });
        sb.Append(Shape switch
        {
            Shape.Circle => "O",
            Shape.Cross => "+",
            Shape.Lozange => "<",
            Shape.Square => "[",
            Shape.Star => "*",
            Shape.Clover => "%",
            //Shape.Circle => "C",
            //Shape.Cross => "X",
            //Shape.Lozange => "Z",
            //Shape.Square => "Q",
            //Shape.Star => "S",
            //Shape.Clover => "T",
            _ => "?"
        });
        if (color == true)
            sb.Append(ConsoleSupport.ConsoleColorDefault);
        else if (color == null)
            sb.Append(Color switch
            {
                Color.Red => 'r',
                Color.Orange => 'o',
                Color.Yellow => 'y',
                Color.Green => 'g',
                Color.Blue => 'b',
                Color.Purple => 'p',
                _ => ConsoleSupport.ConsoleColorDefault
            });
        if (includeInstance)
            sb.Append((char)('0' + Instance));
        return sb.ToString();
    }
}

[DebuggerDisplay("RowCol: ({Row}, {Col})")]
public readonly record struct RowCol(int Row, int Col)
{
    public override string ToString() => $"RowCol: r={Row} c={Col}";
}

[DebuggerDisplay("TileRowCol: {this.AsString(null, true)}")]
public record TileRowCol(Tile T, RowCol RC)
{
    public TileRowCol(Tile t, int row, int col): this(t, new RowCol(row, col)) { }
    public Tile Tile { get; } = T;
    public RowCol RC { get; } = RC;

    public int Row => RC.Row;
    public int Col => RC.Col;

    // Potentially, can add a round# or player Id that triggered this move

    public string AsString(bool? color, bool includeInstance = false)
        => $"TileRowCol: ({RC.Row}, {RC.Col}) {Tile.AsString(color, includeInstance)}";
}

public enum CellState
{
    EmptyIsolated,
    PotentiallyPlayable,
    Tiled,
}

[DebuggerDisplay("Play: {AsString(null)}")]
public record PlaySuggestion(HashSet<TileRowCol> Moves, PointsBonus PB, Hand NewHand)
{
    public HashSet<TileRowCol> Moves { get; } = Moves;
    public PointsBonus PB { get; } = PB;
    public Hand NewHand { get; } = NewHand;

    public string AsString(bool? Color)
        => string.Join(", ", Moves.Select(m => m.AsString(Color))) + $" -> Points: {PB.Points}, Bonus: {PB.Bonus}, Hand: {NewHand.AsString(Color)}";
}

[DebuggerDisplay("PointsBonus: {AsString()}")]
public readonly struct PointsBonus(int Points, int Bonus)
{
    public int Points { get; } = Points;
    public int Bonus { get; } = Bonus;

    public readonly string AsString()
        => $"Points {Points}, Bonus {Bonus}";
}

[DebuggerDisplay("Hand: {AsString(null)}")]
public class Hand: HashSet<Tile>, IEquatable<Hand>
{
    public Hand() : base() { }
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

public static class RandomGenerator
{
    // Can use a seed to make tests reproductible
    static readonly Random rnd = new();

    public static int Next(int MaxValue)
        => rnd.Next(MaxValue);
}

// Bag is a shuffled container for 6x6x3 = 108 standard tiles
public class Bag
{
    readonly List<Tile> Tiles = [];

    public Bag()
    {
        foreach (Shape s in Enum.GetValues(typeof(Shape)))
            foreach (Color c in Enum.GetValues(typeof(Color)))
                for (int i = 1; i <= 3; i++)
                    Tiles.Add(new(s, c, i));

        ShuffleTiles();
    }

    private void ShuffleTiles()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            int k = RandomGenerator.Next(Tiles.Count);
            (Tiles[i], Tiles[k]) = (Tiles[k], Tiles[i]);
        }
    }

    public bool IsEmpty => Tiles.Count == 0;

    /// <summary>
    /// Returns a random tile and remove it from the dock.
    /// </summary>
    /// <returns>A random tile. An exception is raised if dock is empty</returns>
    public Tile GetTile()
    {
        if (Tiles.Count == 0)
            throw new InvalidOperationException();
        var tile = Tiles[0];
        Tiles.RemoveAt(0);
        return tile;
    }

    public void ReturnTiles(Hand hand)
    {
        foreach (Tile t in hand)
        {
            Debug.Assert(!Tiles.Contains(t));
            Tiles.Add(t);
        }
        ShuffleTiles();
    }
}

public class Board: IEnumerable<TileRowCol>
{
    readonly Board? BaseBoard = null;
    readonly HashSet<TileRowCol> Moves = [];

    public Board() { }
    public Board(Board baseBoard)
        => BaseBoard = baseBoard;

    public int RowMin => IsEmpty ? 50 : BaseBoard == null ? rowMin : Math.Min(rowMin, BaseBoard.RowMin);
    public int RowMax => IsEmpty ? 50 : BaseBoard == null ? rowMax : Math.Max(rowMax, BaseBoard.RowMax);
    public int ColMin => IsEmpty ? 50 : BaseBoard == null ? colMin : Math.Min(colMin, BaseBoard.ColMin);
    public int ColMax => IsEmpty ? 50 : BaseBoard == null ? colMax : Math.Max(colMax, BaseBoard.ColMax);

    private int rowMin = 100, rowMax = 0;
    private int colMin = 100, colMax = 0;

    public bool IsEmpty => Moves.Count == 0 && (BaseBoard == null || BaseBoard.IsEmpty);

    public int TilesCount => Moves.Count + (BaseBoard == null ? 0 : BaseBoard.TilesCount);

    public string AsString(bool color, Tile? checkTileCompat = null)
    {
        var sb = new StringBuilder();

        sb.Append("Col");
        for (int col = ColMin - 1; col <= ColMax + 1; col++)
        {
            string s = $"{col:D02}";
            sb.Append(s[0]).Append(' ');
        }
        sb.Append("\r\nRow");
        for (int col = ColMin - 1; col <= ColMax + 1; col++)
        {
            string s = $"{col:D02}";
            sb.Append(s[1]).Append(' ');
        }
        sb.AppendLine();
        for (int row = RowMin - 1; row <= RowMax + 1; row++)
        {
            sb.Append($"{row:D02} ");
            for (int col = ColMin - 1; col <= ColMax + 1; col++)
            {
                var state = GetCellState(row, col);
                switch (state)
                {
                    case CellState.EmptyIsolated:
                        sb.Append("  ");
                        break;

                    case CellState.PotentiallyPlayable:
                        if (checkTileCompat != null && IsCompatible(checkTileCompat, row, col))
                        {
                            if (color)
                                sb.Append(ConsoleSupport.ConsoleColorCyan);
                            sb.Append("x ");
                        }
                        else
                        {
                            if (color)
                                sb.Append(ConsoleSupport.ConsoleColorDarkGray);
                            sb.Append("· ");
                        }
                        if (color)
                            sb.Append(ConsoleSupport.ConsoleColorDefault);
                        break;

                    case CellState.Tiled:
                        Tile t = GetTile(row, col) ?? throw new Exception("GetTile shouldn't return null");
                        sb.Append(t.AsString(color));
                        sb.Append(' ');
                        break;
                }
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public void AddMove(TileRowCol trc)
    {
        if (!IsEmpty)
            Debug.Assert(GetCellState(trc.Row, trc.Col) == CellState.PotentiallyPlayable && IsCompatible(trc.T, trc.Row, trc.Col));
        else
            Debug.Assert(trc.Row==50 && trc.Col==50);

        Moves.Add(trc);
        rowMin = Math.Min(rowMin, trc.Row);
        rowMax = Math.Max(rowMax, trc.Row);
        colMin = Math.Min(colMin, trc.Col);
        colMax = Math.Max(colMax, trc.Col);
    }

    public void AddMoves(HashSet<TileRowCol> moves)
    {
        var tmp = new HashSet<TileRowCol>(moves);
        while (tmp.Count > 0)
        {
            TileRowCol? todel = null;
            foreach (var trc in tmp)
            {
                if ((IsEmpty && trc.Row == 50 && trc.Col == 50)
                    || (GetCellState(trc.Row, trc.Col) == CellState.PotentiallyPlayable && IsCompatible(trc.T, trc.Row, trc.Col)))
                {
                    AddMove(trc);
                    todel = trc;
                    break;
                }
            }
            Debug.Assert(todel != null);
            tmp.Remove(todel);
        }
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

    public CellState GetCellState(RowCol rc)
        => GetCellState(rc.Row, rc.Col);

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

    enum ConstraintMode
    {
        NotDefined,
        ColorConstraint,
        ShapeConstraint,
    }

    public bool IsCompatible(TileRowCol trc)
        => IsCompatible(trc.Tile, trc.RC.Row, trc.RC.Col);

    public bool IsCompatible(in Tile t, int row, int col)
    {
        // Special case, on empty board, (50, 50) is compatible with any tile
        if (IsEmpty)
            return row == 50 && col == 50;

        // Shouldn't call IsCompatible on a tiled cell
        Debug.Assert(GetTile(row, col) == null);

        // Check row
        ConstraintMode constraintMode = ConstraintMode.NotDefined;
        if (!IsDirectionCompatible(row, col, -1, 0, in t, ref constraintMode, out bool nn))
            return false;
        if (!IsDirectionCompatible(row, col, 1, 0, in t, ref constraintMode, out bool sn))
            return false;

        // Check column
        constraintMode = ConstraintMode.NotDefined;
        if (!IsDirectionCompatible(row, col, 0, 1, in t, ref constraintMode, out bool en))
            return false;
        if (!IsDirectionCompatible(row, col, 0, -1, in t, ref constraintMode, out bool wn))
            return false;
        Debug.Assert(nn || sn || en || wn);    // Check that we have at least one neighbor
        return true;
    }

    private bool IsDirectionCompatible(int row, int col, int deltaRow, int deltaCol, in Tile refTile, ref ConstraintMode constraintMode, out bool neighborFound)
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
            if (!(tmpTile.Color == refTile.Color ^ tmpTile.Shape == refTile.Shape))
                return false;

            switch (constraintMode)
            {
                case ConstraintMode.NotDefined:
                    constraintMode = tmpTile.Color == refTile.Color ? ConstraintMode.ColorConstraint : ConstraintMode.ShapeConstraint;
                    break;

                case ConstraintMode.ColorConstraint:

                    if (tmpTile.Color != refTile.Color)
                        return false;
                    break;

                case ConstraintMode.ShapeConstraint:
                    if (tmpTile.Shape != refTile.Shape)
                        return false;
                    break;
            }
        }
    }

    public void Print() => Console.WriteLine(AsString(true));

    public (bool, string) EvaluateMoves(HashSet<TileRowCol> moves)
    {
        // Just a safeguard, this should not be called with an empty HashSet
        if (moves.Count == 0)
            return (false, "No move to evaluate");

        // If the board is empty, cell (50, 50) must be covered
        if (IsEmpty && !moves.Any(trc => trc.Row==50 && trc.Col==50))
            return (false, "La cellule (50, 50) doit être couverte par le premier placement");

        // Check that all tiles have same color or same shape
        int numColors = moves.Select(trc => trc.T.Color).Distinct().Count();
        int numShapes = moves.Select(trc => trc.T.Shape).Distinct().Count();
        if (numColors > 1 && numShapes > 1)
            return (false, $"{numColors} couleurs et {numShapes} formes différentes, les tuiles jouées doivent être de la même couleur ou de la même forme");

        // Check that all tiles have the same row or same column
        int numRows = moves.Select(trc => trc.Row).Distinct().Count();
        int numCols = moves.Select(trc => trc.Col).Distinct().Count();
        if (numRows > 1 && numCols > 1)
            return (false, "Les tuiles jouées doivent être situées sur une même ligne ou une même colonne");

        // Check that all tiles are in a playable cell
        // Since moves hes no order, we must add individual tiles to board for tiles in a playable position
        var nb = new Board(this);
        while (moves.Count>0)
        {
            TileRowCol? playable = null;
            foreach (var trc in moves)
            {
                var cellState = nb.GetCellState(trc.RC);
                Debug.Assert(cellState != CellState.Tiled);
                if (cellState == CellState.PotentiallyPlayable)
                {
                    if (!nb.IsCompatible(trc))
                        return (false, $"La tuile {trc.Tile.Shape} {trc.Tile.Color} #{trc.Tile.Instance} en position ({trc.Row}, {trc.Col}) n'est pas compatible avec les tuiles qui l'entourent");
                    playable = trc;
                    break;
                }
            }
            if (playable == null)
            {
                var trc=moves.First();
                return (false, $"La tuile {trc.Tile.Shape} {trc.Tile.Color} #{trc.Tile.Instance} en position ({trc.Row}, {trc.Col}) n'est pas posée sur une cellule jouable");
            }
            nb.AddMove(playable);
            moves.Remove(playable);
        }

        // All cells are on playable position, compatible with surroundings, it's Ok!
        return (true, "");
    }

    public PointsBonus CountPoints(HashSet<TileRowCol> moves)
    {
        if (moves.Count == 0)
            return new(0, 0);

        // Use a temp new board with moves actually played
        var nb = new Board(this);
        nb.AddMoves(moves);

        bool isHorizontal = true;
        bool isVertical = true;
        int bonus = 0;

        int mRow = -1;
        int mCol = -1;
        foreach (TileRowCol m in moves)
        {
            if (mRow == -1)
                mRow = m.Row;
            else if (isHorizontal && mRow != m.Row)
                isHorizontal = false;

            if (mCol == -1)
                mCol = m.Col;
            else if (isVertical && mCol != m.Col)
                isVertical = false;
        }
        if (moves.Count > 1)
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
                // Find the bottommost position
                while (nb.GetCellState(row + 1, col) == CellState.Tiled)
                    row++;
                dp = 0;
                // Explore to topmost position
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
                if (dp == 6)
                    bonus++;
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
            // Cont only if we have at least two horizontal blocks
            if (dp > 1)
            {
                points += dp == 6 ? 12 : dp;
                if (dp == 6)
                    bonus++;
            }
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
                if (dp == 6)
                    bonus++;
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
            // Cont only if we have at least two vertical blocks
            if (dp > 1)
            {
                points += dp == 6 ? 12 : dp;
                if (dp == 6)
                    bonus++;
            }
        }

        return new(points, bonus);
    }

    public PlaySuggestion Play(Hand hand)
    {
        var PossiblePlays = new List<PlaySuggestion>();

        void ExplorePotentiallyPlayable(int row, int col)
        {
            //Console.WriteLine($"PotentiallyPlayable {row}, {col}");
            foreach (Tile t in hand)
                if (IsCompatible(t, row, col))
                {
                    //Console.WriteLine("  Compatible: " + t.AsString(true));
                    var CurrentMoves = new HashSet<TileRowCol>();
                    ExploreMove(this, this, hand, CurrentMoves, PossiblePlays, new TileRowCol(t, row, col), true, true);
                }
        }

        if (IsEmpty)
            ExplorePotentiallyPlayable(50, 50);
        else
            for (int row = RowMin - 1; row <= RowMax + 1; row++)
                for (int col = ColMin - 1; col <= ColMax + 1; col++)
                    if (GetCellState(row, col) == CellState.PotentiallyPlayable)
                    {
                        //if (PossiblePlays.Count == 9)
                        //    Debugger.Break();
                        ExplorePotentiallyPlayable(row, col);
                    }

        // Show all possible plays
        //for (int i = 0; i < PossiblePlays.Count; i++)
        //    Console.WriteLine($"{i}: {PossiblePlays[i].AsString(true)}");

        // For now, we just implement simple max(points) strategy, just return a random solution from the list
        // since they all have the same max(points)
        // In some cases, this list could be empty
        if (PossiblePlays.Count == 0)
            return new PlaySuggestion([], new PointsBonus(0, 0), hand);
        var randIndex = RandomGenerator.Next(PossiblePlays.Count);
        var sol = PossiblePlays[randIndex];
        Console.WriteLine($"Play: ix={randIndex} {sol.AsString(true)}");
        if (sol.Moves.Count > 1)
        {
            var t = sol.Moves.First().Tile;
            Debug.Assert(sol.Moves.All(m => m.Tile.Color == t.Color) ^ sol.Moves.All(m => m.Tile.Shape == t.Shape));
        }
        return sol;
    }

    private static void ExploreMove(Board startBoard, Board b, Hand h, HashSet<TileRowCol> CurrentMoves, List<PlaySuggestion> PossiblePlays, TileRowCol move, bool NS, bool EW)
    {
        //Console.WriteLine($"\nExploreMove {move.AsString(true)}  NS={NS} EW={EW}");

        var newB = new Board(b);
        newB.AddMove(move);
        var newH = new Hand(h.Except([move.Tile]));
        var newCurrentMoves = new HashSet<TileRowCol>(CurrentMoves) { move };

        // Quick and dirty firtering, only keep move if it produces equal or more points than current max(points)
        // if points are actually greater than max, forget all previous possible plays
        PointsBonus pb = startBoard.CountPoints(newCurrentMoves);
        int pMax = PossiblePlays.Count == 0 ? 0 : PossiblePlays.Max(p => p.PB.Points);
        if (pb.Points > pMax)
            PossiblePlays.Clear();
        if (pb.Points >= pMax)
        {
            var possiblePlay = new PlaySuggestion(newCurrentMoves, pb, newH);
            //Console.WriteLine($"{PossiblePlays.Count}: {possiblePlay.AsString(true)}");
            PossiblePlays.Add(possiblePlay);
        }

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

    private static void TryExplore(Board startBoard, Board b, Hand h, HashSet<TileRowCol> CurrentMoves, List<PlaySuggestion> PossiblePlays, int row, int col, int deltaRow, int deltaCol)
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
                if (b.IsCompatible(t, row, col))
                {
                    //Console.WriteLine($"  Compatible: {t.AsString(true)}");
                    ExploreMove(startBoard, b, h, CurrentMoves, PossiblePlays, new TileRowCol(t, row, col), deltaRow != 0, deltaCol != 0);
                }
            // The outer loop is just here to find the 1st potentially playable square found in the direction.
            // Once each remaining hand tile has been tested, we're done.
            return;
        }
    }

    // Make Board enumerable, returning all moves, could be convenient
    public IEnumerator<TileRowCol> GetEnumerator()
    {
        if (BaseBoard != null)
            foreach (var move in BaseBoard)
                yield return move;
        foreach (var move in Moves)
            yield return move;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    // Dev helper
    // Simple neighbor validation, does not check that a bloc contains twice the same shape or same color
    // Only check neighbor on row+1 and on col+1, checking row-1 and col-1 would check twice the same pair
    public void NeighborCheck()
    {
        for (int row = RowMin; row <= RowMax; row++)
            for (int col = ColMin; col <= ColMax; col++)
            {
                var t1 = GetTile(row, col);
                if (t1 != null)
                {
                    var t2 = GetTile(row + 1, col);
                    if (t2 != null)
                        Debug.Assert(t1.Shape == t2.Shape ^ t1.Color == t2.Color);
                    t2 = GetTile(row, col + 1);
                    if (t2 != null)
                        Debug.Assert(t1.Shape == t2.Shape ^ t1.Color == t2.Color);
                }
            }
    }
}
