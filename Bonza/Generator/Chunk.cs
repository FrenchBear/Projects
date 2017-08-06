// Chunk.cs
// A group of connected letters taken from layout that will appear on final puzzle to solve
//
// 2017-07-22   PV      Split from program.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;


namespace Bonza.Generator
{
    // A group of letters taken from global layout
    public class Chunk
    {
        private readonly List<Square> m_Squares = new List<Square>();

        public int ChunkId { get; }
        public ReadOnlyCollection<Square> Squares => m_Squares.AsReadOnly();
        public int SquaresCount => m_Squares.Count;
        public bool IsDeleted { get; set; }

        // Get next ID on creation
        public Chunk(int newId)
        {
            ChunkId = newId;
        }

        public BoundingRectangle GetBounds()
        {
            BoundingRectangle r = new BoundingRectangle(new Position(int.MinValue, int.MinValue), new Position(int.MaxValue, int.MaxValue));

            foreach (Square sq in Squares)
            {
                r.Min = new Position(Math.Min(r.Min.Row, sq.Row), Math.Min(r.Min.Column, sq.Column));
                r.Max = new Position(Math.Max(r.Max.Row, sq.Row), Math.Max(r.Max.Column, sq.Column));
            }

            return r;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            BoundingRectangle r = GetBounds();
            sb.Append($"{ChunkId}[{SquaresCount}]: {r.Max.Row - r.Min.Row + 1}x{r.Max.Column - r.Min.Column + 1}:");
            foreach (Square sq in Squares)
                sb.Append(' ').Append(sq);
            return sb.ToString();
        }

        public void AddSquare(Square square)
        {
            if (square == null) throw new ArgumentNullException(nameof(square));
            m_Squares.Add(square);
        }

        public void AddSquares(IEnumerable<Square> squares)
        {
            if (squares == null) throw new ArgumentNullException(nameof(squares));
            m_Squares.AddRange(squares);
        }
    }

}
