// WordPosition.cs
// A simple representation of a word, its position and orientation
//
// 2017-07-21   PV      Split from program.cs
// 2017-08-05   PV      OriginalWord
// 2017-08-07   PV      Performance refactoring, StartRow and StartColumn must use m_PositionOrientation instead of PositionOrientation



namespace Bonza.Generator
{
    public class WordPosition
    {
        public string Word { get; }                 // Canonized form, uppercase
        public string OriginalWord { get; }         // Original word that was added

        private PositionOrientation m_PositionOrientation;
        public PositionOrientation PositionOrientation => m_PositionOrientation;

        // Extra accessors for 'old' code
        public int StartRow => m_PositionOrientation.StartRow;

        public int StartColumn => m_PositionOrientation.StartColumn;
        public bool IsVertical => m_PositionOrientation.IsVertical;

        public WordPosition(string word, string originalWord, PositionOrientation positionOrientation)
        {
            Word = word;
            OriginalWord = originalWord;
            m_PositionOrientation = positionOrientation;
        }

        public override string ToString() => "'" + Word + "' " + (IsVertical ? "V" : "H") + $"({StartRow}, {StartColumn})";

        // Not immutable because of this
        public void SetNewPositionOrientation(PositionOrientation positionOrientation)
        {
            m_PositionOrientation = positionOrientation;
        }
    }
}