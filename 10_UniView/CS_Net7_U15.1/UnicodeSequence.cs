// UnicodeSequence.cs
// Represent either a codepoint (a sequence of length 1) or an Emoji sequence or a ZWJ sequence as defined by Unicode
//
// 2023-08-16   PV

using System.Linq;
using System.Text;

namespace UniViewNS;

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

public record EmojiSequenceFilter(bool Keycap, bool Flag, bool Modifier);
public record ZWJSequenceFilter(bool Family, bool Roles, bool Gendered, bool Hair, bool Other);

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

public record UnicodeSequence(string Name, int[] Sequence, SequenceType SequenceType, SequenceSubtype SequenceSubtype)
{
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
