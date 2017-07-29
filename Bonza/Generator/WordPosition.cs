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
        public PositionOrientation PO;

        public int StartRow { get => PO.StartRow; set { PO.StartRow = value; } }
        public int StartColumn { get => PO.StartColumn; set { PO.StartColumn = value; } }
        public bool IsVertical { get => PO.IsVertical; set { PO.IsVertical = value; } }

        public override string ToString() => "'" + Word + "' " + (IsVertical ? "V" : "H") + $"({StartRow}, {StartColumn})";
    }
}
