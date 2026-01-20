// ChunkLayout.cs
// Represent a list of chunks created after layout has been built
// The sum of all chunks is the Layout
//
// 2017-07-22   PV      Split from program.cs
// 2017-08-05   PV      Update for .Net Core
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12
// 2024-11-15	PV		Net9 C#13
// 2026-01-20	PV		Net10 C#14

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bonza.Generator;

public class ChunkLayout
{
    private readonly List<Chunk> m_Chunks = [];

    public ReadOnlyCollection<Chunk> Chunks => m_Chunks.AsReadOnly();
    public int ChunksCount => m_Chunks.Count;
    public int SquaresCount => m_Chunks.Sum(ch => ch.SquaresCount);

    public IList<Chunk> GetAdjacentChunks(Chunk it)
    {
        ArgumentNullException.ThrowIfNull(it);
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
        NextChunk:
            ;
        }
        return l;
    }

    public void AddChunk(Chunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        m_Chunks.Add(chunk);
    }

    public void RemoveChunk(Chunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        m_Chunks.Remove(chunk);
        chunk.IsDeleted = true;
    }
}
