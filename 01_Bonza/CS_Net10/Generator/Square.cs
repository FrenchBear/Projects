// Square.cs
// A 1x1 cell representing a letter
//
// 2017-07-21   PV      Split from program.cs
// 2017-08-03   PV      Manage ShareCount
// 2017-08-07   PV      Performance refactoring
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12
// 2024-11-15	PV		Net9 C#13
// 2026-01-20	PV		Net10 C#14

using System;

namespace Bonza.Generator;

public class Square
{
    public int Row { get; }
    public int Column { get; }
    public char Letter { get; }
    public bool IsInChunk { get; set; }     // Temp property when chunks are built
    public int ShareCount { get; set; }     // To manage squares removal

    // Specialized constructor
    public Square(int row, int column, char letter, bool isInChunk, int shareCount)
    {
        Row = row;
        Column = column;
        Letter = letter;
        IsInChunk = isInChunk;
        ShareCount = shareCount;
    }

    // Copy constructor
    public Square(Square copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        Row = copy.Row;
        Column = copy.Column;
        Letter = copy.Letter;
        IsInChunk = copy.IsInChunk;
        ShareCount = copy.ShareCount;
    }

    public override string ToString() => $"{Letter}({Row}, {Column}) ";
}
