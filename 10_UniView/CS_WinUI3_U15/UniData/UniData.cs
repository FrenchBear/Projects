// UniData.cs
// Reads and stores Unicode UCD file UnicodeData.txt (currently use version from Unicode 11)
// Provides official name and category for each valid codepoint. Not restricted to BMP.
//
// 2018-08-30   PV
// 2020-09-09   PV      1.2   Net4.8, UnicodeData.txt as embedded resource, UnicodeVersion.txt, Unicode 13
// 2020-12-14   PV      1.5.3 Name override for some ASCII control characters
// 2020-12-14   PV      1.5.4 NonCharacters added manually to charname_map for specific naming
// 2020-12-30   PV      Getting closer to the equivalent in UniSearch.  Renamed from UnicodeData to UniData.  Added scripts info.
// 2023-01-23   PV      Net7/C#11
// 2023-03-24   PV      Synonyms for names (ex: ZWJ synonym of Zero Width Joiner)
// 2023-03-27   PV      Emoji attributes from emoji-data.txt; Emoji expansion
// 2023-08-16   PV      Added back block reading for character search filtering
// 2023-08-24   PV      With Search character

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace UniDataNS;

public static class UniData
{
    private static string _UnicodeVersion = "";

    // Real internal dictionary used to store Unicode data
    private static readonly Dictionary<int, CharacterRecord> CharMap = new(65536);
    private static readonly Dictionary<int, BlockRecord> BlockMap = new();

    // Public read only dictionary to access Unicode data

    /// <summary>Dictionary of all valid character records indexed by Codepoint.</summary>
    public static ReadOnlyDictionary<int, CharacterRecord> CharacterRecords { get; } = new(CharMap);

    /// <summary>Dictionary of all blocks, indexed by 1st Codepoint of the block.</summary>
    public static ReadOnlyDictionary<int, BlockRecord> BlockRecords { get; } = new(BlockMap);

    internal static string GetUnicodeVersion() => _UnicodeVersion;

    /// <summary>Last valid codepoint.</summary>
    public const int MaxCodepoint = 0x10FFFF;

    // Allow for simplified names of common characters
    static readonly Dictionary<string, string> ShortNames = new(StringComparer.CurrentCultureIgnoreCase)
    {
        {"ZWJ","ZERO WIDTH JOINER"},
        {"VS15","VARIATION SELECTOR-15"},           // text style (monochrome)
        {"VS16","VARIATION SELECTOR-16"},           // emoji-style (with color)
        {"TEXT","VARIATION SELECTOR-15"},           // text style (monochrome)
        {"EMOJI","VARIATION SELECTOR-16"},          // emoji-style (with color)
    };

    public static int GetCpFromName(string name)
    {
        name = name.Replace('-', ' ').Replace(" ", "").ToUpper();
        if (ShortNames.TryGetValue(name, out string? value))
            name = value.Replace('-', ' ').Replace(" ", "");
        // Should use a dictionary rather than FirstOfDefault to avoid linear search in 150000 records...
        return CharMap.Values.FirstOrDefault(c => name == c.CanonizedName)?.Codepoint ?? -1;
    }

    /// <summary>
    /// Converts a codepoint to an UTF-16 encoded string (.Net string).
    /// Do not name it ToString since it's a static class and it can't override object.ToString(.)
    /// </summary>
    /// <param name="cp">Codepoint to convert.</param>
    /// <returns>A string of one character for cp&lt;0xFFFF, or two surrogate characters for cp&gt;=0x10000.
    /// No check is made for invalid codepoints, returned string is undefined in this case.</returns>
    public static string AsString(int cp) => cp <= 0xD7FF || (cp >= 0xE000 && cp <= 0xFFFF) ? new string((char)cp, 1) : new string((char)(0xD800 + ((cp - 0x10000) >> 10)), 1) + new string((char)(0xDC00 + (cp & 0x3ff)), 1);

    /// <summary>Returns Unicode official name for codpoint such as LATIN CAPITAL LETTER A</summary>
    public static string GetName(int cp) => CharMap.TryGetValue(cp, out CharacterRecord? cr) ? cr.Name : $"Unassigned codepoint - {cp:X4}";

    public static string GetCategory(int cp) => CharMap.ContainsKey(cp) ? CharMap[cp].Category : "??";

    public static string GetScript(int cp) => CharMap.ContainsKey(cp) ? CharMap[cp].Script : "Unknown";

    /// <summary>True for assigned codepoints.  By convention, codepoints in surrogates range D800..DFFF are not valid.</summary>
    public static bool IsValidCodepoint(int cp) => !IsSurrogate(cp) && !IsNonCharacter(cp) && CharMap.ContainsKey(cp) && cp <= MaxCodepoint;

    /// <summary>True for any character in suggogate range 0xD800..0xDFFF (no distinction between low and high surrogates, or private high surrogates</summary>
    internal static bool IsSurrogate(int cp) => cp >= 0xD800 && cp <= 0xDFFF;

    /// <summary>True for "Not a character": FDD0..FDED and last two characters of each page</summary>
    internal static bool IsNonCharacter(int cp) => (cp >= 0xFDD0 && cp <= 0xFDEF) || (cp & 0xFFFF) == 0xFFFE || (cp & 0xFFFF) == 0xFFFF;

    /// <summary>Static constructor, loads data from resources</summary>
    static UniData()
    {
        var TotalStopwatch = Stopwatch.StartNew();
        Stopwatch blocksStopwatch = ReadBlocks();
        Stopwatch UnicodeDataStopwatch = ReadUnicodeData();
        blocksStopwatch.Start();
        AddBlockBegin();
        blocksStopwatch.Stop();
        Stopwatch ScriptsStopwatch = ReadScripts();
        ReadUnicodeVersion();
        Stopwatch EmojiStopwatch = ReadEmoji();
        TotalStopwatch.Stop();

        Debug.WriteLine("UniData initialization times:");
        Debug.WriteLine($"Blocks:         {blocksStopwatch.Elapsed}");
        Debug.WriteLine($"UnicodeData:    {UnicodeDataStopwatch.Elapsed}");
        Debug.WriteLine($"Scripts:        {ScriptsStopwatch.Elapsed}");
        Debug.WriteLine($"Emoji:          {EmojiStopwatch.Elapsed}");
        Debug.WriteLine($"TOTAL:          {TotalStopwatch.Elapsed}");
    }

    // Add BlockBegin info to each character, done separately for efficiency.
    // Code is separated from ReadUnicodeData() to share its source with apps that don't use block info (ex: UniView)
    private static void AddBlockBegin()
    {
        foreach (var br in BlockMap.Values)
            for (int ch = br.Begin; ch <= br.End; ch++)
                if (CharMap.TryGetValue(ch, out CharacterRecord? value))
                    //value.BlockBegin = br.Begin;
                    value.Block = BlockMap[br.Begin];

        // Check that all characters have a valid block
        foreach (var cr in CharMap.Values)
            if (cr.Block == null)
                Debugger.Break();
    }

    /// <summary>Read blocks info from MetaBlocks.txt: Block Name, Level 1 Name, Level 2 Name and Level3 Name.  Note that this file does not come from Unicode.org but is manually managed.</summary>
    private static Stopwatch ReadBlocks()
    {
        var blocksStopwatch = Stopwatch.StartNew();
        using (var sr = new StreamReader(GetResourceStream("MetaBlocks.txt")))
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine() ?? string.Empty;
                if (line.Length == 0 || line[0] == '#')
                    continue;
                string[] fields = line.Split(';');
                string[] field0 = fields[0].Replace("..", ";").Split(';');
                int begin = int.Parse(field0[0], NumberStyles.HexNumber);
                int end = int.Parse(field0[1], NumberStyles.HexNumber);
                var br = new BlockRecord(begin, end, fields[1], fields[2], fields[3], fields[4]);
                BlockMap.Add(begin, br);
            }

        //// Compute rank using an integer of format 33221100 where 33 is index of L3 block, 22 index of L2 block in L3...
        //int rank3 = 0;
        //foreach (var l3 in BlockMap.Values.GroupBy(b => b.Level3Name).OrderBy(g => g.Key))
        //{
        //    int rank2 = 0;
        //    foreach (var l2 in l3.GroupBy(b => b.Level2Name))
        //    {
        //        int rank1 = 0;
        //        foreach (var l1 in l2.GroupBy(b => b.Level1Name))
        //        {
        //            int rank0 = 0;
        //            foreach (var l0 in l1)
        //            {
        //                l0.Rank = rank0 + 100 * (rank1 + 100 * (rank2 + 100 * rank3));
        //                rank0++;
        //            }
        //            rank1++;
        //        }
        //        rank2++;
        //    }
        //    rank3++;
        //}
        blocksStopwatch.Stop();
        return blocksStopwatch;
    }

    /// <summary>Read characters info from UnicodeData.txt: Name, Category, IsPrintable</summary>
    private static Stopwatch ReadUnicodeData()
    {
        var UnicodeDataStopwatch = Stopwatch.StartNew();
        using (var sr = new StreamReader(GetResourceStream("UnicodeData.txt"), Encoding.UTF8))
            while (!sr.EndOfStream)
            {
                string[] fields = (sr.ReadLine() ?? string.Empty).Split(';');
                int codepoint = int.Parse(fields[0], NumberStyles.HexNumber);
                string charName = fields[1];
                string charCategory = fields[2];

                // Special name overrides
                if (codepoint == 28)
                    charName = "CONTROL - FILE SEPARATOR";
                else if (codepoint == 29)
                    charName = "CONTROL - GROUP SEPARATOR";
                else if (codepoint == 30)
                    charName = "CONTROL - RECORD SEPARATOR";
                else if (codepoint == 31)
                    charName = "CONTROL - UNIT SEPARATOR";
                else if (codepoint < 32 || (codepoint >= 0x7f && codepoint < 0xA0))
                    charName = "CONTROL - " + (fields[10].Length > 0 ? fields[10] : fields[0][2..]);

                bool isRange = charName.EndsWith(", First>", StringComparison.OrdinalIgnoreCase);
                bool isPrintable = !(codepoint < 32                                // Control characters 0-31
                                    || (codepoint >= 0x7f && codepoint < 0xA0)        // Control characters 127-160
                                    || (codepoint >= 0xD800 && codepoint <= 0xDFFF)   // Surrogates
                                    || codepoint == 0x2028                          // U+2028  LINE SEPARATOR
                                    || codepoint == 0x2029                          // U+2029  PARAGRAPH SEPARATOR
                                    );
                if (isRange)   // Add all characters within a specified range
                {
                    charName = charName.Replace(", First>", string.Empty).Replace("<", string.Empty).ToUpperInvariant(); //remove range indicator from name
                    fields = (sr.ReadLine() ?? string.Empty).Split(';');
                    int endCharCode = int.Parse(fields[0], NumberStyles.HexNumber);
                    if (!fields[1].EndsWith(", Last>", StringComparison.OrdinalIgnoreCase))
                        Debugger.Break();
                    // Skip planes 15 and 16 private use
                    if (codepoint != 0xF0000 && codepoint != 0x100000)
                        for (int code = codepoint; code <= endCharCode; code++)
                            CharMap.Add(code, new CharacterRecord(code, $"{charName}-{code:X4}", charCategory, isPrintable));
                }
                else
                {
                    if (codepoint < 0x20000)
                        CharMap.Add(codepoint, new CharacterRecord(codepoint, charName, charCategory, isPrintable));
                }
            }

        // Add missing non-characters
        static void AddNonCharacter(int codepoint) => CharMap.Add(codepoint, new CharacterRecord(codepoint, $"Not a character - {codepoint:X4}", "", false));

        // 2 last characters of each plane
        for (int plane = 0; plane <= 16; plane++)
        {
            AddNonCharacter((plane << 16) + 0xFFFE);
            AddNonCharacter((plane << 16) + 0xFFFF);
        }
        // FDD0..FDEF: 16 non-characters in Arabic Presentation Forms-A
        for (int code = 0xFDD0; code <= 0xFDEF; code++)
            AddNonCharacter(code);

        return UnicodeDataStopwatch;
    }

    /// <summary>Read script associated with characters from Scripts.txt.</summary>
    private static Stopwatch ReadScripts()
    {
        var ScriptsStopwatch = Stopwatch.StartNew();
        using (var sr = new StreamReader(GetResourceStream("Scripts.txt"), Encoding.UTF8))
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine() ?? string.Empty;
                if (line.Length == 0 || line[0] == '#')
                    continue;
                int p = line.IndexOf('#');
                if (p >= 0)
                    line = line[..(p - 1)];
                string[] fields = line.Split(';');
                p = fields[0].IndexOf('.');
                int codepoint = int.Parse(p < 0 ? fields[0] : fields[0][..p], NumberStyles.HexNumber);
                string script = fields[1].Trim();
                if (p < 0)
                {
                    if (CharMap.ContainsKey(codepoint))
                        CharMap[codepoint].Script = script;
                }
                else
                {
                    int endCharCode = int.Parse(fields[0][(p + 2)..], NumberStyles.HexNumber);
                    for (int code = codepoint; code <= endCharCode; code++)
                        if (CharMap.ContainsKey(code))
                            CharMap[code].Script = script;
                }
            }

        ScriptsStopwatch.Stop();
        return ScriptsStopwatch;
    }

    /// <summary>Read version information from UnicodeVersion.txt.  This file is manually managed.</summary>
    private static void ReadUnicodeVersion()
    {
        using var sr = new StreamReader(GetResourceStream("UnicodeVersion.txt"), Encoding.UTF8);
        _UnicodeVersion = sr.ReadLine() ?? "Unknown version";
    }

    static readonly Regex EmojiDataRegex = new(@"^([0-9A-F]{4,5})(?:\.\.([0-9A-F]{4,5}))?\s+;\s+([A-Za-z_]+)");

    private static Stopwatch ReadEmoji()
    {
        var EmojiStopwatch = Stopwatch.StartNew();

        using (var sr = new StreamReader(GetResourceStream("emoji-data.txt"), Encoding.UTF8))
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(line) || line[0] == '#')
                    continue;

                var ma = EmojiDataRegex.Match(line);
                if (!ma.Success)
                    Debugger.Break();

                int startcodepoint = int.Parse(ma.Groups[1].Value, NumberStyles.HexNumber);
                int endcodepoint = ma.Groups[2].Length == 0 ? startcodepoint : int.Parse(ma.Groups[2].Value, NumberStyles.HexNumber);

                // Some ranges such as E0020..E007F  ; Emoji_Component      # E0.0  [96] tag space..cancel tag
                // are not present in char_map, we just ignore them
                for (int codepoint = startcodepoint; codepoint <= endcodepoint; codepoint++)
                    if (CharMap.TryGetValue(codepoint, out var cr))
                    switch (ma.Groups[3].Value)
                    {
                        case "Emoji":
                            cr.IsEmoji = true;
                            break;
                        case "Emoji_Presentation":
                            cr.IsEmojiPresentation = true;
                            break;
                        case "Emoji_Modifier":
                            cr.IsEmojiModifier = true;
                            break;
                        case "Emoji_Modifier_Base":
                            cr.IsEmojiModifierBase = true;
                            break;
                        case "Emoji_Component":
                            cr.IsEmojiComponent = true;
                            break;
                        case "Extended_Pictographic":
                            cr.IsEmojiExtendedPictographic = true;
                            break;
                        default:
                            Debugger.Break();
                            break;
                    }
            }

        return EmojiStopwatch;
    }

    /// <summary>Returns stream from embedded resource name.</summary>
    public static Stream GetResourceStream(string name)
    {
        name = "." + name;
        var assembly = typeof(UniData).GetTypeInfo().Assembly;
        var qualifiedName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException("Can't get resource (#1) " + name);
        var st = assembly.GetManifestResourceStream(qualifiedName) ?? throw new ArgumentException("Can't get resource (#2) " + name);
        return st;
    }
}
