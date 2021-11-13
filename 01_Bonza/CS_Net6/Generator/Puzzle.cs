// puzzle.cs
// Subset of Grille class to manage Chunks after layout has been built
//
// 2017-07-22   PV      Split from program.cs
// 2021-11-13   PV      Net6 C#10

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Console;

namespace Bonza.Generator;

public partial class Grille
{
    private ChunkLayout puzzle;

    public void BuildPuzzle(int averageCount = 4)
    {
        Debug.Assert(puzzle == null);
        puzzle = new ChunkLayout();

        // Find a starting letter on the first row
        BoundingRectangle r = Layout.Bounds;
        int loopCol;
        for (loopCol = r.Min.Column; loopCol <= r.Max.Column; loopCol++)
            if (Layout.GetSquare(r.Min.Row, loopCol) != null)
                break;
        Debug.Assert(loopCol <= r.Max.Column);

        // Start accumulation process from this initial square
        var myStack = new Stack<Square>();
        myStack.Push(Layout.GetSquare(r.Min.Row, loopCol));

        // We stop once all squares have been placed
        while (myStack.Count > 0)
        {
            Square sq = myStack.Pop();
            if (sq != null && !sq.IsInChunk)
            {
                var chunk = new Chunk(puzzle.Chunks.Count);
                chunk.AddSquare(sq);
                sq.IsInChunk = true;
                puzzle.AddChunk(chunk);

                var connectedSquares = new Queue<Square>();
                // Examine 4 possible candidates around current square
                for (; ; )
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
        WriteLine($"{puzzle.ChunksCount} chunks, {puzzle.SquaresCount} squares, initial pass");
        PrintPuzzle();

        // Group small chunks
        var smallChunks = puzzle.Chunks.Where(chunk => chunk.Squares.Count <= 2).ToList();
        foreach (var chunk in smallChunks)
            if (!chunk.IsDeleted)
                for (; ; )
                {
                    var adjacentChunk = puzzle.GetAdjacentChunks(chunk).OrderBy(ch => ch.Squares.Count).FirstOrDefault();
                    if (adjacentChunk == null)
                        break;
                    if (!OkAccumulation(chunk.SquaresCount + adjacentChunk.SquaresCount, averageCount))
                        break;
                    chunk.AddSquares(adjacentChunk.Squares);
                    puzzle.RemoveChunk(adjacentChunk);
                }

        WriteLine();
        WriteLine($"{puzzle.ChunksCount} chunks, {puzzle.SquaresCount} squares, after grouping");
        PrintPuzzle();
    }

    public void PrintPuzzle()
    {
        foreach (Chunk chunk in puzzle.Chunks)
        {
            Write("  " + chunk + "      ");
            foreach (Chunk ac in puzzle.GetAdjacentChunks(chunk))
                Write($"{ac.ChunkId}[{ac.Squares.Count}] ");
            WriteLine();
        }
    }

    // Empirical function to stop accumulation of squares in a chuck around averageCount
    private bool OkAccumulation(int accumulatedCount, int averageCount)
    {
        return (accumulatedCount - averageCount) switch
        {
            // ReSharper disable once PatternAlwaysMatches
            int x when x < -3 => true,
            -3 => rnd.NextDouble() < 0.95,
            -2 => rnd.NextDouble() < 0.9,
            -1 => rnd.NextDouble() < 0.8,
            0 => rnd.NextDouble() < 0.5,
            1 => rnd.NextDouble() < 0.2,
            2 => rnd.NextDouble() < 0.9,
            3 => rnd.NextDouble() < 0.05,
            _ => false,
        };
    }
}
