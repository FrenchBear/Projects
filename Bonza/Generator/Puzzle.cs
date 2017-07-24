// Puzzle.cs
// Subset of Grille class to manage Chunks after layout has been built
//
// 2017-07-22   PV      Split from program.cs

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;


namespace Bonza.Generator
{
    public partial class Grille
    {
        ChunkLayout Puzzle;

        public void BuildPuzzle(int averageCount = 4)
        {
            Debug.Assert(Puzzle == null);
            Puzzle = new ChunkLayout();

            // Find a starting letter on the first row
            (int minRow, int maxRow, int minColumn, int maxColumn) = Layout.GetBounds();
            int loopCol;
            for (loopCol = minColumn; loopCol <= maxColumn; loopCol++)
                if (Layout.GetSquare(minRow, loopCol) != null)
                    break;
            Debug.Assert(loopCol <= maxColumn);

            // Start accumulation process from this initial square
            Stack<Square> myStack = new Stack<Square>();
            myStack.Push(Layout.GetSquare(minRow, loopCol));

            // We stop once all squares have been placed
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
                        sq = Layout.GetSquare(row + 1, column);
                        if (sq != null && !sq.IsInChunk)
                            connectedSquares.Enqueue(sq);
                        sq = Layout.GetSquare(row - 1, column);
                        if (sq != null && !sq.IsInChunk)
                            connectedSquares.Enqueue(sq);
                        sq = Layout.GetSquare(row, column + 1);
                        if (sq != null && !sq.IsInChunk)
                            connectedSquares.Enqueue(sq);
                        sq = Layout.GetSquare(row, column - 1);
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

        // Save chunks in a .json file
        public void SavePuzzle(string outFile)
        {
            Puzzle.SaveToFile(outFile);
        }

        // Load chunks from a .json file
        public void LoadPuzzle(string inFile)
        {
            Puzzle.LoadFromFile(inFile);
        }

    }
}
