// Chunk.cs
// A group of connected letters taken from layout that will appear on final puzzle to solve
//
// 2017-07-22   PV      Split from program.cs
// 2021-11-13   PV      Net6 C#10
// 2024-11-15	PV		Net9 C#13

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Bonza.Generator;

// A group of letters taken from global layout
public class Chunk(int newId)
{
    private readonly List<Square> m_Squares = [];

    public int ChunkId { get; } = newId;
    public ReadOnlyCollection<Square> Squares => m_Squares.AsReadOnly();
    public int SquaresCount => m_Squares.Count;
    public bool IsDeleted { get; set; }

    public BoundingRectangle Bounds
    {
        get
        {
            int minRow = int.MaxValue, minColumn = int.MaxValue, maxRow = int.MinValue, maxColumn = int.MinValue;
            foreach (Square sq in Squares)
            {
                minRow = Math.Min(minRow, sq.Row);
                maxRow = Math.Max(maxRow, sq.Row);
                minColumn = Math.Min(minColumn, sq.Column);
                maxColumn = Math.Max(maxColumn, sq.Column);
            }

            return new BoundingRectangle(minRow, maxRow, minColumn, maxColumn);
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        BoundingRectangle r = Bounds;
        sb.Append($"{ChunkId}[{SquaresCount}]: {r.Max.Row - r.Min.Row + 1}x{r.Max.Column - r.Min.Column + 1}:");
        foreach (Square sq in Squares)
            sb.Append(' ').Append(sq);
        return sb.ToString();
    }

    public void AddSquare(Square square)
    {
        ArgumentNullException.ThrowIfNull(square);
        m_Squares.Add(square);
    }

    public void AddSquares(IEnumerable<Square> squares)
    {
        ArgumentNullException.ThrowIfNull(squares);
        m_Squares.AddRange(squares);
    }
}
