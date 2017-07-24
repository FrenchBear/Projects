// Grille.cs
// A grille represent a set of crossing words
// 2017-07-21   PV      Split from program.cs


namespace Bonza.Generator
{
    internal class Square
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public char Letter { get; set; }
        public bool IsInChunk { get; set; }

        public override string ToString() => $"{Letter}({Row}, {Column}) ";
    }
}
