// CurrentSearchOptions.cs
// Stores SearchWindows set of options in a static class so they're preserved after SearchWindow is closed
//
// 2023-008-18  PV

using System.Collections.Generic;

namespace UniView_WinUI3;

internal static class CurrentSearchOptions
{
    // Options
    public static bool? outputName = true;
    public static bool? outputCharacters = false;
    public static bool? outputCodepoints = false;

    // Source Symbols
    public static bool? symbolNone = false;
    public static bool? symbolEmoji = false;
    public static bool? symbolAll = true;

    // Source Scripts
    public static bool? scriptNone = false;
    public static bool? scriptLatin = false;
    public static bool? scriptAll = true;

    // Source Emoji sequences
    public static bool? emojiSequenceAll = true;
    public static readonly Dictionary<string, bool> EmojiSequenceDictionary = new()
    {
        { "Keycaps", true },
        { "Flags", true },
        { "Modifiers", true },
    };

    // SOurce ZWJ sequences
    public static bool? zwjSequenceAll = true;
    public static readonly Dictionary<string, bool> ZWJSequenceDictionary = new()
    {
        { "Family", true },
        { "Role", true },
        { "Gendered", true },
        { "Hair", true },
        { "Other", true },
    };
}
