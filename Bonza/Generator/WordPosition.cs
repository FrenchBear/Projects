// WordPosition.cs
// A simple representation of a word, its position and orientation
// 2017-07-21   PV      Split from program.cs


namespace Bonza.Generator
{
    public class WordPosition
    {
        public string Word { get; set; }
        public int StartRow { get; set; }
        public int StartColumn { get; set; }
        public bool IsVertical { get; set; }

        public override string ToString() => "'" + Word + "' " + (IsVertical ? "V" : "H") + $"({StartRow}, {StartColumn})";
    }
}
