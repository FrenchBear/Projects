// UnicodeSequence.cs
// Represent either a codepoint (a sequence of length 1) or an Emoji sequence or a ZWJ sequence as defined by Unicode
//
// 2023-08-16   PV
// 2023-08-24   PV      UWP version (without records)
// 2023-11-20   PV      Net8 C#12 (primary constructor)

using System.Linq;
using System.Text;

namespace UniView_WinUI3;

public enum SymbolFilter
{
    None,
    Emoji,
    All,
}

public enum ScriptFilter
{
    None,
    Latin,
    All,
}

public struct EmojiSequenceFilter(bool keycap, bool flag, bool modifier)
{
    public bool Keycap { get; set; } = keycap;
    public bool Flag { get; set; } = flag;
    public bool Modifier { get; set; } = modifier;
}
public struct ZWJSequenceFilter(bool family, bool roles, bool gendered, bool hair, bool other)
{
    public bool Family { get; set; } = family;
    public bool Roles { get; set; } = roles;
    public bool Gendered { get; set; } = gendered;
    public bool Hair { get; set; } = hair;
    public bool Other { get; set; } = other;
};

/// <summary>Source of sequence</summary>
public enum SequenceType
{
    None,
    CodepointScript,
    CodepointSymbol,
    EmojiSequence,
    ZWJSequence,
}

/// <summary>For each source, specifies a subtype</summary>
public enum SequenceSubtype
{
    None,

    // For CodepointScript, indicates it's a Latin or a Greek Codepoint
    CodepointLatinAndGreek,

    // For CodepointSymbol, indicates it's an Emoji or a Pictogram (it None, it's punctuation for example)
    CodepointEmoji,

    // For EmojiSequences
    SequenceKeycap,
    SequenceFlag,
    SequenceModifier,

    // For ZWJSequences
    ZWJSequenceFamily,
    ZWJSequenceRoles,
    ZWJSequenceGendered,
    ZWJSequenceHair,
    ZWJSequenceOther,
}

public class UnicodeSequence(string name, int[] sequence, SequenceType sequenceType, SequenceSubtype sequenceSubtype)
{
    public string Name { get; set; } = name;
    public int[] Sequence { get; set; } = sequence;
    public SequenceType SequenceType { get; set; } = sequenceType;
    public SequenceSubtype SequenceSubtype { get; set; } = sequenceSubtype;

    // To speed-up name searches
    private string _CanonizedName = "";     // Upper case, no spaces or dashes
    public string CanonizedName
    {
        get
        {
            if (string.IsNullOrEmpty(_CanonizedName))
                _CanonizedName = Name.Replace('-', ' ').Replace(':',' ').Replace(" ", "").ToUpper();
            return _CanonizedName;
        }
    }

    public string SequenceHexString
        => string.Join(" ", Sequence.Select(cp => $"U+{cp:X4}"));

    public string SequenceAsString
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var cp in Sequence)
                sb.Append(UniDataNS.UniData.AsString(cp));
            return sb.ToString();
        }
    }

    public string SequenceTypeAsString
        => SequenceType switch
        {
            SequenceType.CodepointScript => "Codepoint Script",
            SequenceType.CodepointSymbol => "Codepoint Symbol",
            SequenceType.EmojiSequence => "Emoji Sequence",
            SequenceType.ZWJSequence => "ZWJ Sequence",
            _ => "None",
        };

    public string SequenceSubtypeAsString
        => SequenceSubtype switch
        {
            SequenceSubtype.CodepointLatinAndGreek => "Latin+Greek",
            SequenceSubtype.CodepointEmoji => "Emoji",
            SequenceSubtype.SequenceKeycap => "Keycap",
            SequenceSubtype.SequenceFlag => "Flag",
            SequenceSubtype.SequenceModifier => "Modifier",
            SequenceSubtype.ZWJSequenceFamily => "Family",
            SequenceSubtype.ZWJSequenceRoles => "Roles",
            SequenceSubtype.ZWJSequenceGendered => "Gendered",
            SequenceSubtype.ZWJSequenceHair => "Hair",
            SequenceSubtype.ZWJSequenceOther => "Other",
            _ => "None",
        };

    public string TypeAsString
        => (SequenceTypeAsString + "/" + SequenceSubtypeAsString).Replace("/None","");
}
