// Square.cs
// A 1x1 cell representing a letter
//
// 2017-07-21   PV      Split from program.cs


namespace Bonza.Generator
{
    public class Square
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public char Letter { get; set; }
        public bool IsInChunk { get; set; }     // Temp property when chunks are built

        public override string ToString() => $"{Letter}({Row}, {Column}) ";
    }
}
