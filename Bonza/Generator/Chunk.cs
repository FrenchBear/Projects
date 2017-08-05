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
            BoundingRectangle r = new BoundingRectangle(int.MaxValue,int.MinValue,int.MaxValue,int.MinValue);

            foreach (Square sq in Squares)
            {
                r.MinRow = Math.Min(r.MinRow, sq.Row);
                r.MaxRow = Math.Max(r.MaxRow, sq.Row);
                r.MinColumn = Math.Min(r.MinColumn, sq.Column);
                r.MaxColumn = Math.Max(r.MaxColumn, sq.Column);
            }

            return r;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            BoundingRectangle r = GetBounds();
            sb.Append($"{ChunkId}[{SquaresCount}]: {r.MaxRow - r.MinRow + 1}x{r.MaxColumn - r.MinColumn + 1}:");
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
