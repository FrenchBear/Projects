// WordPosition.cs
// A simple representation of a word, its position and orientation
//
// 2017-07-21   PV      Split from program.cs
// 2017-08-05   PV      OriginalWord


namespace Bonza.Generator
{
    public class WordPosition
    {
        public string Word { get; }                 // Canonized form, uppercase
        public string OriginalWord { get; }         // Original word that was added

        private PositionOrientation m_PositionOrientation;
        public PositionOrientation PositionOrientation { get => m_PositionOrientation; set => m_PositionOrientation=value; }


        // Extra accessors for 'old' code
        public int StartRow => PositionOrientation.StartRow;
        public int StartColumn => PositionOrientation.StartColumn;

        public bool IsVertical
        {
            get => PositionOrientation.IsVertical;
            set => m_PositionOrientation.IsVertical = value;
        }


        public WordPosition(string word, string originalWord, PositionOrientation positionOrientation)
        {
            Word = word;
            OriginalWord = originalWord;
            PositionOrientation = positionOrientation;
        }

        public override string ToString() => "'" + Word + "' " + (IsVertical ? "V" : "H") + $"({StartRow}, {StartColumn})";

    }
}
