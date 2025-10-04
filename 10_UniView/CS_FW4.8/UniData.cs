// Class UniData.cs
// Reads and stores Unicode UCD file UnicodeData.txt (currently use version from Unicode 11)
// Provides official name and category for each valid codepoint. Not restricted to BMP.
//
// 2018-08-30   PV
// 2020-09-09   PV      1.2: .Net FW 4.8, UnicodeData.txt as embedded resource, UnicodeVersion.txt, Unicode 13
// 2020-12-14   PV      1.5.3: Name override for some ASCII control characters
// 2020-12-14   PV      1.5.4: NonCharacters added manually to charname_map for specific naming
// 2020-12-30   PV      Getting closer to the equivalent in UniSearch.  Renamed from UnicodeData to UniData.  Added scripts info.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

//#pragma warning disable IDE0057 // Use range operator: Not supported by C#8
#nullable enable

namespace UniViewNS
{
    public class CharacterRecord
    {
        /// <summary>Unicode Character Codepoint, between 0 and 0x10FFFF (from UnicodeData.txt).</summary>
        public int Codepoint { get; private set; }

        /// <summary>Unicode Character Name, uppercase string such as LATIN CAPITAL LETTER A (from UnicodeData.txt).</summary>
        public string Name { get; private set; }

        /// <summary>Unicode General Category, 2 characters such as Lu (from UnicodeData.txt).</summary>
        public string Category { get; private set; }

        /// <summary>Unicode script (from Scripts.txt)</summary>
        public string Script { get => script ?? "Unknown"; }
        internal string? script = null;

        /// <summary>When true, Character method will return an hex codepoint representation instead of the actual string.</summary>
        public bool IsPrintable { get; private set; }

        public CharacterRecord(int cp, string name, string cat, bool isPrintable) => (Codepoint, Name, Category, IsPrintable) = (cp, name, cat, isPrintable);
    }

    public static class UniData
    {
        private static string _UnicodeVersion = "";

        // Real internal dictionary used to store Unicode data
        private static readonly Dictionary<int, CharacterRecord> char_map = new Dictionary<int, CharacterRecord>(65536);

        // Public read only dictionary to access Unicode data

        /// <summary>Dictionary of all valid character records indexed by Codepoints.</summary>
        public static ReadOnlyDictionary<int, CharacterRecord> CharacterRecords { get; } = new ReadOnlyDictionary<int, CharacterRecord>(char_map);

        /// <summary>Last valid codepoint.</summary>
        public const int MaxCodepoint = 0x10FFFF;

        /// <summary>Static constructor, loads data from resources</summary>
        static UniData()
        {
            Stopwatch TotalStopwatch = Stopwatch.StartNew();
            Stopwatch UnicodeDataStopwatch = ReadUnicodeData();
            Stopwatch ScriptsStopwatch = ReadScripts();
            ReadUnicodeVersion();
            TotalStopwatch.Stop();

            Debug.WriteLine("UniData initialization times:");
            Debug.WriteLine($"UnicodeData: {UnicodeDataStopwatch.Elapsed}");
            Debug.WriteLine($"Scripts:     {ScriptsStopwatch.Elapsed}");
            Debug.WriteLine($"TOTAL:       {TotalStopwatch.Elapsed}");
        }

        /// <summary>Read characters info from UnicodeData.txt: Name, Category, IsPrintable</summary>
        private static Stopwatch ReadUnicodeData()
        {
            Stopwatch UnicodeDataStopwatch = Stopwatch.StartNew();
            using (var sr = new StreamReader(GetResourceStream("UnicodeData.txt"), Encoding.UTF8))
                while (!sr.EndOfStream)
                {
                    string[] fields = (sr.ReadLine() ?? string.Empty).Split(';');
                    int codepoint = int.Parse(fields[0], NumberStyles.HexNumber);
                    string char_name = fields[1];
                    string char_category = fields[2];

                    // Special name overrides
                    if (codepoint == 28)
                        char_name = "CONTROL - FILE SEPARATOR";
                    else if (codepoint == 29)
                        char_name = "CONTROL - GROUP SEPARATOR";
                    else if (codepoint == 30)
                        char_name = "CONTROL - RECORD SEPARATOR";
                    else if (codepoint == 31)
                        char_name = "CONTROL - UNIT SEPARATOR";
                    else if (codepoint < 32 || (codepoint >= 0x7f && codepoint < 0xA0))
                        char_name = "CONTROL - " + (fields[10].Length > 0 ? fields[10] : fields[0].Substring(2));

                    bool is_range = char_name.EndsWith(", First>", StringComparison.OrdinalIgnoreCase);
                    bool is_printable = !(codepoint < 32                                // Control characters 0-31
                                        || (codepoint >= 0x7f && codepoint < 0xA0)        // Control characters 127-160
                                        || (codepoint >= 0xD800 && codepoint <= 0xDFFF)   // Surrogates
                                        || codepoint == 0x2028                          // U+2028  LINE SEPARATOR
                                        || codepoint == 0x2029                          // U+2029  PARAGRAPH SEPARATOR
                                        );
                    if (is_range)   // Add all characters within a specified range
                    {
                        char_name = char_name.Replace(", First>", string.Empty).Replace("<", string.Empty).ToUpperInvariant(); //remove range indicator from name
                        fields = (sr.ReadLine() ?? string.Empty).Split(';');
                        int end_char_code = int.Parse(fields[0], NumberStyles.HexNumber);
                        if (!fields[1].EndsWith(", Last>", StringComparison.OrdinalIgnoreCase))
                            Debugger.Break();
                        // Skip planes 15 and 16 private use
                        if (codepoint != 0xF0000 && codepoint != 0x100000)
                            for (int code = codepoint; code <= end_char_code; code++)
                                char_map.Add(code, new CharacterRecord(code, $"{char_name}-{code:X4}", char_category, is_printable));
                    }
                    else
                    {
                        if (codepoint < 0x20000)
                            char_map.Add(codepoint, new CharacterRecord(codepoint, char_name, char_category, is_printable));
                    }
                }

            // Add missing non-characters
            static void AddNonCharacter(int codepoint) => char_map.Add(codepoint, new CharacterRecord(codepoint, $"Not a character - {codepoint:X4}", "", false));

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
            Stopwatch ScriptsStopwatch = Stopwatch.StartNew();
            using (var sr = new StreamReader(GetResourceStream("Scripts.txt"), Encoding.UTF8))
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine() ?? string.Empty;
                    if (line.Length == 0 || line[0] == '#') continue;
                    int p = line.IndexOf('#');
                    if (p >= 0) line = line.Substring(0, p - 1);
                    string[] fields = line.Split(';');
                    p = fields[0].IndexOf('.');
                    int codepoint = int.Parse(p < 0 ? fields[0] : fields[0].Substring(0, p), NumberStyles.HexNumber);
                    string script = fields[1].Trim();
                    if (p < 0)
                    {
                        if (char_map.ContainsKey(codepoint))
                            char_map[codepoint].script = script;
                    }
                    else
                    {
                        int end_char_code = int.Parse(fields[0].Substring(p + 2), NumberStyles.HexNumber);
                        for (int code = codepoint; code <= end_char_code; code++)
                            if (char_map.ContainsKey(code))
                                char_map[code].script = script;
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

        /// <summary>Returns stream from embedded resource name.</summary>
        private static Stream GetResourceStream(string name)
        {
            name = "." + name;
            var assembly = typeof(UniData).GetTypeInfo().Assembly;
            var qualifiedName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase));
            if (qualifiedName == null)
                throw new ArgumentException("Can't get resource (#1) " + name);
            var st = assembly.GetManifestResourceStream(qualifiedName);
            if (st == null)
                throw new ArgumentException("Can't get resource (#2) " + name);
            return st;
        }

        internal static string GetUnicodeVersion() => _UnicodeVersion;

        public static int GetCPFromName(string name) => char_map.Values.FirstOrDefault(c => string.Compare(c.Name, name, true) == 0)?.Codepoint ?? -1;

        /// <summary>
        /// Converts a codepoint to an UTF-16 encoded string (.Net string).
        /// Do not name it ToString since it's a static class and it can't override object.ToString(.)
        /// </summary>
        /// <param name="cp">Codepoint to convert.</param>
        /// <returns>A string of one character for cp&lt;0xFFFF, or two surrogate characters for cp&gt;=0x10000.
        /// No check is made for invalid codepoints, returned string is undefined in this case.</returns>
        public static string AsString(int cp) => cp <= 0xD7FF || (cp >= 0xE000 && cp <= 0xFFFF) ? new string((char)cp, 1) : new string((char)(0xD800 + ((cp - 0x10000) >> 10)), 1) + new string((char)(0xDC00 + (cp & 0x3ff)), 1);

        public static string GetName(int cp) => char_map.ContainsKey(cp) ? char_map[cp].Name : $"Unassigned codepoint - {cp:X4}";

        public static string GetCategory(int cp) => char_map.ContainsKey(cp) ? char_map[cp].Category : "??";

        public static string GetScript(int cp) => char_map.ContainsKey(cp) ? char_map[cp].Script : "Unknown";

        // Surrogates D800..DFFF are not valid codepoints despite range being listed in UnicodeData.txt
        // NonCharacters are added manually to charname_map for specific namind despite being invalid codepoints
        public static bool IsValidCodepoint(int cp) => !IsSurrogate(cp) && !IsNonCharacter(cp) && char_map.ContainsKey(cp) && cp <= MaxCodepoint;

        internal static bool IsSurrogate(int cp) => cp >= 0xD800 && cp <= 0xDFFF;

        // Last two characters of each page are "Not a character"
        internal static bool IsNonCharacter(int cp) => (cp >= 0xFDD0 && cp <= 0xFDEF) || (cp & 0xFFFF) == 0xFFFE || (cp & 0xFFFF) == 0xFFFF;
    }
}
