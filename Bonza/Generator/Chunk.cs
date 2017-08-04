﻿// Chunk.cs
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

        public (int minRow, int maxRow, int minColumn, int maxColumn) GetBounds()
        {
            int minRow = int.MaxValue;
            int maxRow = int.MinValue;
            int minColumn = int.MaxValue;
            int maxColumn = int.MinValue;

            foreach (Square sq in Squares)
            {
                minRow = Math.Min(minRow, sq.Row);
                maxRow = Math.Max(maxRow, sq.Row);
                minColumn = Math.Min(minColumn, sq.Column);
                maxColumn = Math.Max(maxColumn, sq.Column);
            }

            return (minRow, maxRow, minColumn, maxColumn);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            (int minRow, int maxRow, int minColumn, int maxColumn) = GetBounds();
            sb.Append($"{ChunkId}[{SquaresCount}]: {maxRow - minRow + 1}x{maxColumn - minColumn + 1}:");
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
