// Puzzle.cs
// Subset of Grille class to manage Chunks after layout has been built
// 2017-07-22   PV      Split from program.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;


namespace Bonza.Generator
{
    public partial class Grille
    {
        // A group of letters taken from global layout
        class Chunk
        {
            List<Square> m_Squares = new List<Square>();

            public int ChunkID { get; }
            public ReadOnlyCollection<Square> Squares => m_Squares.AsReadOnly();
            public int SquaresCount => m_Squares.Count;
            public bool IsDeleted { get; set; }

            // Get next ID on creation
            public Chunk(int newId)
            {
                ChunkID = newId;
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
                sb.Append($"{ChunkID}[{SquaresCount}]: {maxRow - minRow + 1}x{maxColumn - minColumn + 1}:");
                foreach (Square sq in Squares)
                    sb.Append(' ').Append(sq.ToString());
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


        class PuzzleToSolve
        {
            List<Chunk> m_Chunks = new List<Chunk>();

            public ReadOnlyCollection<Chunk> Chunks => m_Chunks.AsReadOnly();
            public int ChunksCount => m_Chunks.Count;
            public int SquaresCount => m_Chunks.Sum(ch => ch.SquaresCount);


            public List<Chunk> GetAdjacentChunks(Chunk it)
            {
                if (it == null) throw new ArgumentNullException(nameof(it));
                var l = new List<Chunk>();

                foreach (var chunk in Chunks)
                {
                    if (chunk != it)
                    {
                        foreach (Square sq1 in chunk.Squares)
                            foreach (Square sq2 in it.Squares)
                            {
                                if ((sq1.Row == sq2.Row && (sq1.Column == sq2.Column - 1 || sq1.Column == sq2.Column + 1))
                                     || (sq1.Column == sq2.Column && (sq1.Row == sq2.Row - 1 || sq1.Row == sq2.Row + 1)))
                                {
                                    l.Add(chunk);
                                    goto NextChunk;
                                }
                            }
                    }
                    NextChunk:;
                }
                return l;
            }

            public void AddChunk(Chunk chunk)
            {
                if (chunk == null) throw new ArgumentNullException(nameof(chunk));
                m_Chunks.Add(chunk);
            }

            public void RemoveChunk(Chunk chunk)
            {
                if (chunk == null) throw new ArgumentNullException(nameof(chunk));
                m_Chunks.Remove(chunk);
                chunk.IsDeleted = true;
            }
        }


        PuzzleToSolve Puzzle;


        public void BuildPuzzle(int averageCount = 4)
        {
            Debug.Assert(Puzzle == null);
            Puzzle = new PuzzleToSolve();

            // Find a starting letter on the first row
            (int minRow, int maxRow, int minColumn, int maxColumn) = GetLayoutBounds();
            int loopCol;
            for (loopCol = minColumn; loopCol <= maxColumn; loopCol++)
                if (GetSquare(minRow, loopCol) != null)
                    break;
            Debug.Assert(loopCol <= maxColumn);

            Stack<Square> myStack = new Stack<Square>();
            myStack.Push(GetSquare(minRow, loopCol));


            while (myStack.Count > 0)
            {
                Square sq = myStack.Pop();
                if (sq != null && !sq.IsInChunk)
                {
                    Chunk chunk = new Chunk(Puzzle.Chunks.Count);
                    chunk.AddSquare(sq);
                    sq.IsInChunk = true;
                    Puzzle.AddChunk(chunk);

                    Queue<Square> connectedSquares = new Queue<Square>();
                    // Examine 4 possible candidates around current square
                    for (;;)
                    {
                        int row = sq.Row;
                        int column = sq.Column;
                        sq = GetSquare(row + 1, column);
                        if (sq != null && !sq.IsInChunk)
                            connectedSquares.Enqueue(sq);
                        sq = GetSquare(row - 1, column);
                        if (sq != null && !sq.IsInChunk)
                            connectedSquares.Enqueue(sq);
                        sq = GetSquare(row, column + 1);
                        if (sq != null && !sq.IsInChunk)
                            connectedSquares.Enqueue(sq);
                        sq = GetSquare(row, column - 1);
                        if (sq != null && !sq.IsInChunk)
                            connectedSquares.Enqueue(sq);

                        if (connectedSquares.Count == 0)
                            break;
                        if (OkAccumulation(chunk.SquaresCount + 1, averageCount))
                        {
                            sq = connectedSquares.Dequeue();
                            sq.IsInChunk = true;
                            chunk.AddSquare(sq);
                        }
                        else
                            break;
                    }

                    while (connectedSquares.Count > 0)
                        myStack.Push(connectedSquares.Dequeue());
                }
            }

            WriteLine();
            WriteLine($"{Puzzle.ChunksCount} chunks, {Puzzle.SquaresCount} squares, initial pass");
            PrintPuzzle();

            // Group small chunks
            List<Chunk> smallChunks = Puzzle.Chunks.Where(chunk => chunk.Squares.Count <= 2).ToList();
            foreach (var chunk in smallChunks)
                if (!chunk.IsDeleted)
                    for (;;)
                    {
                        var adjacentChunk = Puzzle.GetAdjacentChunks(chunk).OrderBy(ch => ch.Squares.Count).FirstOrDefault();
                        if (adjacentChunk == null) break;
                        if (!OkAccumulation(chunk.SquaresCount + adjacentChunk.SquaresCount, averageCount))
                            break;
                        chunk.AddSquares(adjacentChunk.Squares);
                        Puzzle.RemoveChunk(adjacentChunk);
                    }

            WriteLine();
            WriteLine($"{Puzzle.ChunksCount} chunks, {Puzzle.SquaresCount} squares, after grouping");
            PrintPuzzle();
        }

        public void PrintPuzzle()
        {
            foreach (Chunk chunk in Puzzle.Chunks)
            {
                Write("  " + chunk.ToString() + "      ");
                foreach (Chunk ac in Puzzle.GetAdjacentChunks(chunk))
                    Write($"{ac.ChunkID}[{ac.Squares.Count}] ");
                WriteLine();
            }
        }

        // Empirical function to stop accumulation of squares in a chuck around averageCount
        private bool OkAccumulation(int accumulatedCount, int averageCount)
        {
            switch (accumulatedCount - averageCount)
            {
                case int x when x < -3:
                    return true;

                case -3:
                    return rnd.NextDouble() < 0.95;

                case -2:
                    return rnd.NextDouble() < 0.9;

                case -1:
                    return rnd.NextDouble() < 0.8;

                case 0:
                    return rnd.NextDouble() < 0.5;

                case 1:
                    return rnd.NextDouble() < 0.2;

                case 2:
                    return rnd.NextDouble() < 0.9;

                case 3:
                    return rnd.NextDouble() < 0.05;

                default:
                    return false;
            }
        }

    }
}
