// WordPosition.cs
// A simple representation of a word, its position and orientation
// 2017-07-21   PV      Split from program.cs


namespace Bonza.Generator
{
    public class WordPosition
    {
        public string Word;
        public PositionOrientation PositionOrientation;

        public int StartRow { get => PositionOrientation.StartRow; set => PositionOrientation.StartRow = value; }
        public int StartColumn { get => PositionOrientation.StartColumn; set => PositionOrientation.StartColumn = value; }
        public bool IsVertical { get => PositionOrientation.IsVertical; set => PositionOrientation.IsVertical = value; }

        public override string ToString() => "'" + Word + "' " + (IsVertical ? "V" : "H") + $"({StartRow}, {StartColumn})";

    }
}
