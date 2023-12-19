// WordPosition.cs
// A simple representation of a word, its position and orientation
//
// 2017-07-21   PV      Split from program.cs
// 2017-08-05   PV      OriginalWord
// 2017-08-07   PV      Performance refactoring, StartRow and StartColumn must use m_PositionOrientation instead of PositionOrientation
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12

namespace Bonza.Generator;

public class WordPosition(string word, string originalWord, PositionOrientation positionOrientation)
{
    public string Word { get; } = word;
    public string OriginalWord { get; } = originalWord;

    private PositionOrientation m_PositionOrientation = positionOrientation;
    public PositionOrientation PositionOrientation => m_PositionOrientation;

    // Extra accessors for 'old' code
    public int StartRow => m_PositionOrientation.StartRow;

    public int StartColumn => m_PositionOrientation.StartColumn;
    public bool IsVertical => m_PositionOrientation.IsVertical;

    public override string ToString() => "'" + Word + "' " + (IsVertical ? "V" : "H") + $"({StartRow}, {StartColumn})";

    // Not immutable because of this
    public void SetNewPositionOrientation(PositionOrientation positionOrientation) => m_PositionOrientation = positionOrientation;
}
