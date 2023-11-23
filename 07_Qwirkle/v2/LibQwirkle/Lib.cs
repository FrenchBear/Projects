// LibQwirkle
// Core library for Qwirkle reboot
//
// 2023-11-23   PV

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

public class Board
{
    readonly Board? BaseBoard = null;
    readonly List<Move> Moves = [];

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
        Debug.Assert(nn | sn | en | wn);    // Check that we have at least one neighbor
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

    public void Print() => Console.WriteLine(AsString(true));

    const string ConsoleColorRed = "\x1b[91m";
    const string ConsoleColorDarkYellow = "\x1b[33m";
    const string ConsoleColorYellow = "\x1b[93m";
    const string ConsoleColorGreen = "\x1b[92m";
    const string ConsoleColorBlue = "\x1b[94m";
    const string ConsoleColorMagenta = "\x1b[95m";
    const string ConsoleColorCyan = "\x1b[96m";
    const string ConsoleColorDarkGray = "\x1b[90m";
    const string ConsoleColorDefault = "\x1b[39m";

    public string AsString(bool color, Tile? checkTileCompat = null)
    {
        var sb = new StringBuilder();

        sb.Append("   ");
        for (int col = ColMin - 1; col <= ColMax + 1; col++)
            sb.Append($"{col:D02}");
        sb.AppendLine();
        for (int row = RowMax + 1; row >= RowMin - 1; row--)
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
                        if (checkTileCompat != null && IsCompatible(row, col, checkTileCompat))
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
                        Tile t = GetTile(row, col) ?? throw new Exception("GetTile shouldn't return null");
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
                            Shape.Circle => "A",
                            Shape.Cross => "B",
                            Shape.Lozange => "C",
                            Shape.Square => "D",
                            Shape.Star => "E",
                            Shape.Clover => "F",
                            _ => "🞌 "
                        });
                        if (color)
                            sb.Append(ConsoleColorDefault);
                        sb.Append(' ');
                        break;
                }
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
