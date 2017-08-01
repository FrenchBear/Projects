// WordPosition.cs
// A simple representation of a word, its position and orientation
// 2017-07-21   PV      Split from program.cs


namespace Bonza.Generator
{
    public struct PositionOrientation
    {
        public int StartRow { get; set; }
        public int StartColumn { get; set; }
        public bool IsVertical { get; set; }
    }

    public class WordPosition
    {
        public string Word;
        public PositionOrientation PoOr;

        public int StartRow { get => PoOr.StartRow; set => PoOr.StartRow = value; }
        public int StartColumn { get => PoOr.StartColumn; set => PoOr.StartColumn = value; }
        public bool IsVertical { get => PoOr.IsVertical; set => PoOr.IsVertical = value; }

        public override string ToString() => "'" + Word + "' " + (IsVertical ? "V" : "H") + $"({StartRow}, {StartColumn})";
    }
}
