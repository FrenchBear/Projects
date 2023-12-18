using System.Diagnostics;

namespace QwirkleUI;

[DebuggerDisplay("Position: r={Row} c={Col}")]
public record Position(int Row, int Col)
{
    public override string ToString() => $"Position: r={Row} c={Col}";
}

public record BoundingRectangle(Position Min, Position Max)
{
    public BoundingRectangle(int minRow, int maxRow, int minColumn, int maxColumn) : this(new Position(minRow, minColumn), new Position(maxRow, maxColumn)) { }
}
