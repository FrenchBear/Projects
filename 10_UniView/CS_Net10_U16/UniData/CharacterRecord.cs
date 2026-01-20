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
// 2024-09-15   PV      Unicode 16; Code cleanup
// 2026-01-20	PV		Net10 C#14

namespace UniView_CS_Net10.UniData;

public class CharacterRecord
{
    /// <summary>Unicode Character Codepoint, between 0 and 0x10FFFF (from UnicodeData.txt).</summary>
    public int Codepoint { get; private set; }

    /// <summary>Unicode Character Name, uppercase string such as LATIN CAPITAL LETTER A (from UnicodeData.txt).</summary>
    public string Name { get; private set; }

    // To speed-up name searches
    public string CanonizedName
    {
        get
        {
            if (string.IsNullOrEmpty(field))
                field = Name.Replace('-', ' ').Replace(" ", "").ToUpper();
            return field;
        }
    } = "";

    /// <summary>Unicode General Category, 2 characters such as Lu (from UnicodeData.txt).</summary>
    public string Category { get; private set; }

    /// <summary>Block the character blongs to.  Public setter for efficient initialization</summary>
    public BlockRecord Block { get; set; }
    public string Script
    {
        get => field ?? "Unknown";
        set;
    }

    /// <summary>When true, Character method will return an hex codepoint representation instead of the actual string.</summary>
    public bool IsPrintable { get; private set; }

    // Emoji information; For documentation and usage, see https://www.unicode.org/reports/tr51
    // Setter is not private beacause emoji properties are filled after object creation
    public bool IsEmoji { get; internal set; }
    public bool IsEmojiPresentation { get; internal set; }
    public bool IsEmojiModifier { get; internal set; }
    public bool IsEmojiModifierBase { get; internal set; }
    public bool IsEmojiComponent { get; internal set; }
    public bool IsEmojiExtendedPictographic { get; internal set; }

    public bool IsEmojiXXX => IsEmoji || IsEmojiPresentation || IsEmojiModifier || IsEmojiModifierBase || IsEmojiComponent || IsEmojiExtendedPictographic;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public CharacterRecord(int cp, string name, string cat, bool isPrintable) => (Codepoint, Name, Category, IsPrintable) = (cp, name, cat, isPrintable);
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
