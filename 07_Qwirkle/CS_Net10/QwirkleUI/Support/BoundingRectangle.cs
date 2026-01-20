using LibQwirkle;

namespace QwirkleUI.Support;

//[DebuggerDisplay("RowCol: r={Row} c={Col}")]
//public readonly record struct RowCol(int Row, int Col)
//{
//    public override string ToString() => $"RowCol: r={Row} c={Col}";
//}

public record BoundingRectangle(RowCol Min, RowCol Max)
{
    public BoundingRectangle(int minRow, int maxRow, int minColumn, int maxColumn) : this(new RowCol(minRow, minColumn), new RowCol(maxRow, maxColumn)) { }

    public override string ToString()
        => $"BoundingRectangle ({Min.Row}, {Min.Col})-({Max.Row}, {Max.Col})";
}
