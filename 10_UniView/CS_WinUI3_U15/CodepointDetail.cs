// UniView CS UWP
// Simple prototype of an app showing Unicode text details
// CodepointDetail: contains CodePoint detailed information
//
// 2020-12-18   PV      Moved out MainWindow.  Renamed from CharDetail
// 2020-12-14   PV      1.5.3: ToDisplayString
// 2020-12-30   PV      1.6.0: Scripts and UniData refactoring
// 2023-01-23   PV      C#11

using System.ComponentModel;
using UniDataNS;

namespace UniView_WinUI3;

/// <summary>
/// Detailed info about a Codepoint (Unicode unit) after grouping suggogates and processing regex macros
/// Implements INotifyPropertyChanged in UWP since GlyphIndex is updated asynchronously after CodepointList has been displayed
/// </summary>
internal class CodepointDetail: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public int Codepoint { get; private set; }
    public int CharIndexStart { get; private set; }             // Before macro transformations
    public int CharIndexEnd { get; private set; }
    public int CodepointIndexStart { get; private set; }    // After macro transformations
    public int CodepointIndexEnd { get; private set; }
    public int CodepointIndex { get; private set; }

    // Implemented as a bindable property for UWP since it's updated after initial binding
    private int _GlyphIndex;
    public int GlyphIndex
    {
        get => _GlyphIndex;
        set
        {
            if (_GlyphIndex != value)
            {
                _GlyphIndex = value;
                //NotifyPropertyChanged(nameof(GlyphIndex));	// Not needed
                NotifyPropertyChanged(nameof(GlyphIndexStr));
            }
        }
    }

    public CodepointDetail(int cp, int codepointIndex, int charIndexStart, int charIndexEnd, int codepointIndexStart, int codepointIndexEnd)
        => (Codepoint, CodepointIndex, CharIndexStart, CharIndexEnd, CodepointIndexStart, CodepointIndexEnd) = (cp, codepointIndex, charIndexStart, charIndexEnd, codepointIndexStart, codepointIndexEnd);

    public string CodepointUString => $"U+{Codepoint:X4}";

    public string Name => UniData.GetName(Codepoint);

    public string Category => UniData.GetCategory(Codepoint);

    public string Script => UniData.GetScript(Codepoint);

    // Normal string representation of the Codepoint, using 2 surrogates for chars out of BMP
    public override string ToString() => UniData.AsString(Codepoint);

    // Same as ToString, with the exception of most control characters 0..31 and 127 replaced by a visual representation ␀..␟ and ␡
    public string ToDisplayString()
    {
        if (Codepoint < 32 && Codepoint != 9 && Codepoint != 10 && Codepoint != 13)   // TAB, CR and LF keep their default representation
            return UniData.AsString(Codepoint + 0x2400);
        else if (Codepoint == 127)
            return UniData.AsString(0x2421);
        else if (Codepoint == 0x85)     // NEXT LINE character has no visual glyph, replace it by LineFeed
            return UniData.AsString(10);
        else if (Codepoint == 0xAD)     // SOFT HYPHEN character has no visual glyph, replace it by -
            return UniData.AsString('-');
        else if (Codepoint >= 0x2028 && Codepoint <= 0x2029)
            // 2028  LINE SEPARATOR
            // 2029  PARAGRAPH SEPARATOR
            return UniData.AsString(10);
        else if ((Codepoint >= 0xFFF9 && Codepoint <= 0xFFFB) || UniData.IsNonCharacter(Codepoint))
            // FFF9  INTERLINEAR ANNOTATION ANCHOR
            // FFFA  INTERLINEAR ANNOTATION SEPARATOR
            // FFFB  INTERLINEAR ANNOTATION TERMINATOR
            // FDD0..FDEF <not a character>
            // FFFE <not a character>
            // FFFF <not a character>
            return UniData.AsString(' ');
        else
            return UniData.AsString(Codepoint);
    }

    public string ToDisplayStringFull()
    {
        if (Codepoint < 32)   // All control characters, including TAB, CR and LF are replaced by a visual equivalent
            return UniData.AsString(Codepoint + 0x2400);
        else if (Codepoint == 127)      // DELETE replaced by ␡
            return UniData.AsString(0x2421);
        else if (Codepoint == 0x85)     // NEXT LINE character has no visual glyph, replace it by ␤
            return UniData.AsString(0x2424);
        else if (Codepoint == 0xAD)     // SOFT HYPHEN character has no visual glyph, replace it by -
            return UniData.AsString('-');
        else if (Codepoint >= 0x2028 && Codepoint <= 0x2029)
            // 2028  LINE SEPARATOR
            // 2029  PARAGRAPH SEPARATOR
            return UniData.AsString(0xFFFD);    // � Replacement character
        else if ((Codepoint >= 0xFFF9 && Codepoint <= 0xFFFB) || UniData.IsNonCharacter(Codepoint))
            // FFF9  INTERLINEAR ANNOTATION ANCHOR
            // FFFA  INTERLINEAR ANNOTATION SEPARATOR
            // FFFB  INTERLINEAR ANNOTATION TERMINATOR
            // FDD0..FDEF <not a character>
            // FFFE <not a character>
            // FFFF <not a character>
            return UniData.AsString(0xFFFD);    // � Replacement character
        else
            return UniData.AsString(Codepoint);
    }

    public bool IsBMP => Codepoint <= 0xD7FF || (Codepoint >= 0xE000 && Codepoint <= 0xFFFF);

    public string UTF16 => IsBMP ? Codepoint.ToString("X4") : (0xD800 + ((Codepoint - 0x10000) >> 10)).ToString("X4") + " " + (0xDC00 + (Codepoint & 0x3ff)).ToString("X4");

    public string UTF8
    {
        get
        {
            if (Codepoint <= 0x7F)
                return $"{Codepoint:X2}";
            else if (Codepoint <= 0x7FF)
                return $"{0xC0 + Codepoint / 0x40:X2} {0x80 + Codepoint % 0x40:X2}";
            else if (Codepoint <= 0xFFFF)
                return $"{0xE0 + Codepoint / 0x40 / 0x40:X2} {0x80 + Codepoint / 0x40 % 0x40:X2} {0x80 + Codepoint % 0x40:X2}";
            else if (Codepoint <= 0x1FFFFF)
                return $"{0xF0 + Codepoint / 0x40 / 0x40 / 0x40:X2} {0x80 + Codepoint / 0x40 / 0x40 % 0x40:X2} {0x80 + Codepoint / 0x40 % 0x40:X2} {0x80 + Codepoint % 0x40:X2}";
            return "?{cp}?";
        }
    }

    public string GlyphIndexStr => GlyphIndex < 0 ? "" : $"{GlyphIndex}";

    public int GlyphAlternation => GlyphIndex < 0 ? -1 : GlyphIndex % 2;

    public string CharIndexStr => CharIndexStart == CharIndexEnd ? CharIndexStart.ToString() : $"{CharIndexStart}..{CharIndexEnd}";
}