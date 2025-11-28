// 5th variant of T59 grammar - Couleurs
// Manage syntax categories and colors
// Use french Couleur instead of Color to avoid names collisions
//
// 2025-11-20   PV

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace T59v5;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

public enum SyntaxCategory: byte
{
    // Special
    Unknown = 0,    // Default, if not painted
    Uninitialized,  // For internal use
    Invalid,        // L1InvalidToken (ex: ZYP). Incorrect/incomplete statements will be stored in a L2InvalidStatement, and its individual tokens may still keey their categegory
    Eof,            // Eof
    WS,             // White Space

    // Program stuff
    Comment,        // souble slash and following text up to EOL
    Number,         // Number
    Tag,            // @xxx (and also :)
    Instruction,    // Any valid instruction
    Label,          // Either instruction or D2 with D2 value>=10
    Direct,         // D1|D2 in direct statements
    Indirect,       // D1|D2 in indirect statements
    Address,        // A3 (123) or D2 D2 with 1st D2 <10> (01 23)

    // Extra
    LineNumber,     // For formatted program/error messages
}

public record struct Paint
{
    public SyntaxCategory cat;
    public bool ParserError;
}

public static class Couleurs
{
    static readonly Dictionary<string, string> TagColors = new()
    {
        {"comment", "40C040" },
        {"invalid", "ff4040"},
        {"unknown", "ff0000"},
        {"eof", "ffffff"},
        {"instruction", "80c0ff"},
        {"number", "d2d2d2"},
        {"direct", "ffc0ff"},
        {"indirect", "ff80ff"},
        {"tag", "ffc080"},
        {"label", "ffc000"},
        {"address", "ff9000"},

        {"linenumber", "A0A0A0"},
    };

    private static string ConsoleColor(int r, int g, int b) => "\x1b[38;2;" + $"{r};{g};{b}m";

    public static string ConsoleDefaultColor() => "\x1b[39m";

    private static string ConsoleColorFromHexColor(string hexcolor)
    {
        var r = int.Parse(hexcolor[0..2], System.Globalization.NumberStyles.HexNumber);
        var g = int.Parse(hexcolor[2..4], System.Globalization.NumberStyles.HexNumber);
        var b = int.Parse(hexcolor[4..6], System.Globalization.NumberStyles.HexNumber);
        return ConsoleColor(r, g, b);
    }

    static readonly Regex reTag = new(@"\[([^]]+)\]");

    private static string Evaluator(Match match)
    {
        string tag = match.Groups[1].Value.ToLower();
#pragma warning disable IDE0046 // Convert to conditional expression
        if (tag.StartsWith('/'))
            return TagColors.ContainsKey(tag[1..]) ? ConsoleDefaultColor() : "[" + tag + "]";
        else
            return TagColors.TryGetValue(tag, out var hexcolor) ? ConsoleColorFromHexColor(hexcolor) : "[" + tag + "]";
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    const bool ConsoleRendering = false;

    public static string RenderTaggedText(string s) => ConsoleRendering ? reTag.Replace(s, Evaluator) : s;

    internal static string GetCategoryTag(SyntaxCategory cat) => cat.ToString().ToLower();
    internal static string GetCategoryOpenTag(SyntaxCategory cat) => $"[{GetCategoryTag(cat)}]";
    internal static string GetCategoryCloseTag(SyntaxCategory cat) => $"[/{GetCategoryTag(cat)}]";
    internal static string GetTaggedText(string s, SyntaxCategory cat) => GetCategoryOpenTag(cat) + s + GetCategoryCloseTag(cat);
}
