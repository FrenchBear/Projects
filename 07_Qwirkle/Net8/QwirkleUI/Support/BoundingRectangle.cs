using System.Diagnostics;

namespace QwirkleUI;

[DebuggerDisplay("RowCol: r={Row} c={Col}")]
public record RowCol(int Row, int Col)
{
    public override string ToString() => $"RowCol: r={Row} c={Col}";
}

public record BoundingRectangle(RowCol Min, RowCol Max)
{
    public BoundingRectangle(int minRow, int maxRow, int minColumn, int maxColumn) : this(new RowCol(minRow, minColumn), new RowCol(maxRow, maxColumn)) { }
}
