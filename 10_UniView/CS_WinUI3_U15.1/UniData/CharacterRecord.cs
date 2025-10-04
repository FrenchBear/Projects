// CharacterRecord.cs
// Store information about a Unicode Codepoint.
//
// 2018-08-30   PV
// 2020-09-09   PV      1.2: .Net FW 4.8, UnicodeData.txt as embedded resource, UnicodeVersion.txt, Unicode 13
// 2020-12-14   PV      1.5.3: Name override for some ASCII control characters
// 2020-12-14   PV      1.5.4: NonCharacters added manually to charname_map for specific naming
// 2020-12-30   PV      Getting closer to the equivalent in UniSearch.  Renamed from UnicodeData to UniData.  Added scripts info.
// 2023-01-23   PV      Net7/C#11
// 2023-03-24   PV      Synonyms for names (ex: ZWJ synonym of Zero Width Joiner)
// 2023-03-27   PV      Emoji attributes from emoji-data.txt; Emoji expansion
// 2023-08-16   PV      Class extracted in its own file

namespace UniDataNS;

public class CharacterRecord
{
    /// <summary>Unicode Character Codepoint, between 0 and 0x10FFFF (from UnicodeData.txt).</summary>
    public int Codepoint { get; private set; }

    /// <summary>Unicode Character Name, uppercase string such as LATIN CAPITAL LETTER A (from UnicodeData.txt).</summary>
    public string Name { get; private set; }

    // To speed-up name searches
    private string _CanonizedName = "";     // Upper case, no spaces or dashes
    public string CanonizedName
    {
        get
        {
            if (string.IsNullOrEmpty(_CanonizedName))
                _CanonizedName = Name.Replace('-', ' ').Replace(" ", "").ToUpper();
            return _CanonizedName;
        }
    }

    /// <summary>Unicode General Category, 2 characters such as Lu (from UnicodeData.txt).</summary>
    public string Category { get; private set; }

    /// <summary>Block the character blongs to.  Public setter for efficient initialization</summary>
    public BlockRecord Block { get; set; } = DefaultBlockRecord;

    private static readonly BlockRecord DefaultBlockRecord = new(0, 0, "No name", "?", "?", "?");     // Fake initializer for Block property since it can't be initialized in the contructor, and I don't want to make it nullable

    /// <summary>Unicode script (from Scripts.txt)</summary>
    private string? _Script;
    public string Script
    {
        get => _Script ?? "Unknown";
        set => _Script = value;
    }

    /// <summary>When true, Character method will return an hex codepoint representation instead of the actual string.</summary>
    public bool IsPrintable { get; private set; }

    // Emoji information; For documentation and usage, see https://www.unicode.org/reports/tr51
    // Setter is not private beacause emoju properties are filled after object creation
    public bool IsEmoji { get; internal set; }
    public bool IsEmojiPresentation { get; internal set; }
    public bool IsEmojiModifier { get; internal set; }
    public bool IsEmojiModifierBase { get; internal set; }
    public bool IsEmojiComponent { get; internal set; }
    public bool IsEmojiExtendedPictographic { get; internal set; }

    public bool IsEmojiXXX => IsEmoji || IsEmojiPresentation || IsEmojiModifier || IsEmojiModifierBase || IsEmojiComponent || IsEmojiExtendedPictographic;

    public CharacterRecord(int cp, string name, string cat, bool isPrintable) => (Codepoint, Name, Category, IsPrintable) = (cp, name, cat, isPrintable);
}
