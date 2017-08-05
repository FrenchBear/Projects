// Square.cs
// A 1x1 cell representing a letter
//
// 2017-07-21   PV      Split from program.cs
// 2017-08-03   PV      Manage ShareCount


namespace Bonza.Generator
{
    public class Square
    {
        // Copy constructor
        public Square(Square copy)
        {
            Row = copy.Row;
            Column = copy.Column;
            Letter = copy.Letter;
            IsInChunk = copy.IsInChunk;
            ShareCount = copy.ShareCount;
        }

        // Specialized constructor
        public Square(int row, int column, char letter, bool isInChunk, int shareCount)
        {
            Row = row;
            Column = column;
            Letter = letter;
            IsInChunk = isInChunk;
            ShareCount = shareCount;
        }

        public int Row { get; set; }
        public int Column { get; set; }
        public char Letter { get; set; }
        public bool IsInChunk { get; set; }     // Temp property when chunks are built
        public int ShareCount { get; set; }     // To manage squares removal

        public override string ToString() => $"{Letter}({Row}, {Column}) ";
    }
}
