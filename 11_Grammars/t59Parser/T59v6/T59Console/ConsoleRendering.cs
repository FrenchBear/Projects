// ConsoreRendering
// Support tagged text rendering to ANSI console
//
// 2025-11-20   PV
// 2025-11-28   PV      Split tagging and redering

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace T59v6Console;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

public static class ConsoleRendering
{
    const bool DoRendering = true;

    static readonly Dictionary<string, string> TagColors = new()
    {
        {"comment",     ConsoleColorFromHexColor("40C040") },
        {"invalid",     ConsoleColorFromHexColor("ff4040")},
        {"unknown",     ConsoleColorFromHexColor("ff0000")},
        {"instruction", ConsoleColorFromHexColor("80c0ff")},
        {"number",      ConsoleColorFromHexColor("d2d2d2")},
        {"direct",      ConsoleColorFromHexColor("ffc0ff")},
        {"indirect",    ConsoleColorFromHexColor("ff80ff")},
        {"tag",         ConsoleColorFromHexColor("ffc080")},
        {"label",       ConsoleColorFromHexColor("ffc000")},
        {"address",     ConsoleColorFromHexColor("ff9000")},

        {"linenumber",  ConsoleColorFromHexColor("A0A0A0")},
        {"opcode",      ConsoleColorFromHexColor("C0A080")},
    };

    private static string ConsoleColorFromRGB(int r, int g, int b) => "\x1b[38;2;" + $"{r};{g};{b}m";

    public static string ConsoleDefaultColor() => "\x1b[39m";

    private static string ConsoleColorFromHexColor(string hexcolor)
    {
        var r = int.Parse(hexcolor[0..2], System.Globalization.NumberStyles.HexNumber);
        var g = int.Parse(hexcolor[2..4], System.Globalization.NumberStyles.HexNumber);
        var b = int.Parse(hexcolor[4..6], System.Globalization.NumberStyles.HexNumber);
        return ConsoleColorFromRGB(r, g, b);
    }

    static readonly Regex reTag = new(@"\[([^]]+)\]");

    private static string Evaluator(Match match)
    {
        string tag = match.Groups[1].Value.ToLower();
#pragma warning disable IDE0046 // Convert to conditional expression
        if (tag.StartsWith('/'))
            return TagColors.ContainsKey(tag[1..]) ? ConsoleDefaultColor() : "[" + tag + "]";
        else
            return TagColors.TryGetValue(tag, out var ccolor) ? ccolor : "[" + tag + "]";
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    public static string RenderTaggedText(string s) => DoRendering ? reTag.Replace(s, Evaluator) : s;
}
