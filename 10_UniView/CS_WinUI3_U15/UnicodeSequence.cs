// UnicodeSequence.cs
// Represent either a codepoint (a sequence of length 1) or an Emoji sequence or a ZWJ sequence as defined by Unicode
//
// 2023-08-16   PV
// 2023-08-24   PV      UWP version (without records)

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

public struct EmojiSequenceFilter
{ 
    public bool Keycap { get; set; }
    public bool Flag { get; set; }
    public bool Modifier { get; set; }

    public EmojiSequenceFilter(bool keycap, bool flag, bool modifier)
    {
        Keycap = keycap;
        Flag = flag;
        Modifier = modifier;
    }
}
public struct ZWJSequenceFilter {
    public bool Family { get; set; }
    public bool Roles { get; set; }
    public bool Gendered { get; set; }
    public bool Hair { get; set; }
    public bool Other { get; set; }

    public ZWJSequenceFilter(bool family, bool roles, bool gendered, bool hair, bool other) 
    {
        Family = family;
        Roles = roles;
        Gendered = gendered;
        Hair = hair;
        Other = other;
    }
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

public class UnicodeSequence
{
    public string Name { get; set; }
    public int[] Sequence { get; set; }
    public SequenceType SequenceType { get; set; }
    public SequenceSubtype SequenceSubtype { get; set; }

    public UnicodeSequence(string name, int[] sequence, SequenceType sequenceType, SequenceSubtype sequenceSubtype)
    {
        Name = name;
        Sequence = sequence;
        SequenceType = sequenceType;
        SequenceSubtype = sequenceSubtype;
    }

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
