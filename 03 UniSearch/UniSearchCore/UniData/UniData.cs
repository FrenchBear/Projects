﻿// UniData
// Library providing characters and blocks information reading Unicode UCD files
// Aé♫山𝄞🐗
//
// 2018-09-11   PV
// 2018-09-17   PV      1.1 Store UCD Data in embedded streams; Skip characters from planes 15 and 16
// 2018-09-20   PV      1.2 Read NamesList.txt
// 2018-09-28	PV		1.2.1 Subheaders merging
// 2019-03-06	PV		1.3 Unicode 12 (no code change, only UCD data updated)
// 2019-08-09   PV      1.4 .Net Core 3.0, C#8, Nullable
// 2020-09-03   PV      1.5 .Net Core 3.1, Unicode 13
// 2020-11-11   PV      1.6 .Net 5, C#9.  Add Synonyms, Cross-Refs and Comments to CharacterRecords
// 2020-11-12   PV      1.6.1 Process ranges >=20000 (were incorrectly skipped, causing problem wuth U+FA6C -> NFD U+242EE)
// 2020-12-29   PV      1.7 Refactoring and Scripts
// 2021-01-04	PV		1.7.1 AsString Binding for LastResortFont
// 2021-01-05   PV      1.8 BlockRecord: FirstAssignedCodepoint and RepresentantCharacter

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

#nullable enable


namespace UniDataNS
{
    /// <summary>
    /// Represents an Unicode codepoint and various associated information 
    /// </summary>
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
        internal string? script;

        /// <summary>First version of Unicode standard the character appeared, such as 3.0 (from DerivedAge.txt)</summary>
        public string Age { get; internal set; }

        /// <summary>First codepoint of the block assigned to the character, such as 0x100 (from MetaBlocks.txt)</summary>
        public int BlockBegin { get; internal set; } = -1;       // -1 for tests, to make sure at the end all supported chars have a block

        /// <summary>
        /// Name of block subdivision the character belongs to, such as ASCII punctuation and symbols (from NamesList.txt, marker @)
        /// If both singular and plural form exist for a subheader in a block (ex: "Additional letter" and "Additional letters"), only plural form is used
        /// Because of name merging, sorting by subheader does not sort characters in codepoint order
        /// </summary>
        public string Subheader { get; internal set; }

        /// <summary>Lines &lt;tab> = and &lt;tab> % in NamesList.txt</summary>
        public List<string>? Synonyms { get; internal set; }

        /// <summary>Lines &lt;tab> x in NamesList.txt</summary>
        public List<string>? CrossRefs { get; internal set; }

        /// <summary>Lines &lt;tab> * and &lt;tab> ~ in NamesList.txt</summary>
        public List<string>? Comments { get; internal set; }

        /// <summary>When true, Character method will return an hex codepoint representation instead of the actual string.</summary>
        public bool IsPrintable { get; private set; }

        /// <summary>
        /// Convert to a C# string representation of the character, except for control characters.
        /// 'Safe version' of UnicodeData.CPtoString. U+FFFD is the official replacement character. 
        /// </summary>
        public string Character => UniData.AsString(IsPrintable ? Codepoint : 0xFFFD);

        /// <summary>Direct string representation, used for LastResortFont binding</summary>
        public string AsString => UniData.AsString(Codepoint);

        /// <summary>Used by ListView to support grouping</summary>
        public string GroupName => UniData.BlockRecords[BlockBegin].BlockName + (string.IsNullOrEmpty(Subheader) ? "" : ": " + Subheader);

        /// <summary>Block the character blongs to</summary>
        public BlockRecord Block => UniData.BlockRecords[BlockBegin];

        /// <summary>General category of the character</summary>
        public CategoryRecord CategoryRecord => UniData.CategoryRecords[Category];

        /// <summary>Standard hexadecimal representation of codepoint such as U+0041 (4 to 6 uppercase hex digits)</summary>
        public string CodepointHex => $"U+{Codepoint:X4}";

        /// <summary>String representation of UTF-16 encoding such as "D83D DC17" for U+1F417</summary>
        public string UTF16 => Codepoint <= 0xD7FF || (Codepoint >= 0xE000 && Codepoint <= 0xFFFF) ? Codepoint.ToString("X4") : (0xD800 + ((Codepoint - 0x10000) >> 10)).ToString("X4") + " " + (0xDC00 + (Codepoint & 0x3ff)).ToString("X4");

        /// <summary>String representation of UTF-8 encoding such as "F0 9F 90 97" for U+1F417</summary>
        public string UTF8
        {
            get
            {
                if (Codepoint <= 0x7F)
                    return $"{Codepoint:X2}";
                else if (Codepoint <= 0x7FF)
                    return $"{0xC0 + Codepoint / 0x40:X2} {0x80 + Codepoint % 0x40:X2}";
                else if (Codepoint <= 0xFFFF)
                    return $"{0xE0 + (Codepoint / 0x40) / 0x40:X2} {0x80 + (Codepoint / 0x40) % 0x40:X2} {0x80 + Codepoint % 0x40:X2}";
                else if (Codepoint <= 0x1FFFFF)
                    return $"{0xF0 + ((Codepoint / 0x40) / 0x40) / 0x40:X2} {0x80 + ((Codepoint / 0x40) / 0x40) % 0x40:X2} {0x80 + (Codepoint / 0x40) % 0x40:X2} {0x80 + Codepoint % 0x40:X2}";
                return "?{cp}?";
            }
        }

        /// <summary>Internal constructor</summary>
        internal CharacterRecord(int codepoint, string name, string category, bool isPrintable)
        {
            Codepoint = codepoint;
            Name = name;
            Category = category;
            IsPrintable = isPrintable;
            Age = string.Empty;
            Subheader = string.Empty;
        }

        /// <summary>Text representation of the form "char(tab)codepoint(tab)name"</summary>
        public string AsDetailedString => $"{Character}\t{CodepointHex}\t{Name}";

        /// <summary>String representation, mostly for debug</summary>
        public override string ToString() => $"CharacterRecord({AsDetailedString})";

        /// <summary>All codepoints in D800..DFFF range</summary>
        internal static bool IsSurrogate(int cp) => cp >= 0xD800 && cp <= 0xDFFF;

        /// <summary>Surrogates and last two characters of each page are "Not a character"</summary>
        internal static bool IsNonCharacter(int cp) => cp >= 0xFDD0 && cp <= 0xFDEF || (cp & 0xFFFF) == 0xFFFE || (cp & 0xFFFF) == 0xFFFF;
    }


    /// <summary>
    /// Represent an Unicode block (range of codepoints) and its hierarchical classification
    /// </summary>
    public class BlockRecord
    {
        /// <summary>First codepoint of the block.  Beware, this is based on block definition and not guaranteed to be a valid codepoint: block Gurmukhi 0A00..0A7F but 0A00 is not a valid codepoint.  Use property FirstBlockCodepoint to get the first valid Codepoint of the block.</summary>
        public int Begin { get; private set; }

        /// <summary>Last codepoint of the block (may or may not be an assigned codepoint)</summary>
        public int End { get; private set; }

        /// <summary>First assigned codepoint of the block, it's sometimes different of Begin.</summary>
        public int FirstAssignedCodepoint
        {
            get
            {
                for (int cp = Begin; cp <= End; cp++)
                    if (UniData.IsValidCodepoint(cp))
                        return cp;
                return Begin;
            }
        }

        /// <summary>A character that can be used with LastResortFont to represent the block.
        /// It's usually the first assigned codepoint of the block, except when it's a character rendered with a placeholder circle, which is shown using two glyphs with LRF.
        /// To avoid this case, a manual list of exceptions is maintained.  Example: for Hewbrew block 0590..05FF, representant is 05D0 Aleph instead of 0591.</summary>
        public string RepresentantCharacter
        {
            get
            {
                int cp = Begin switch
                {
                    0x0590 => 0x05D0,       // Hebrew: HEBREW LETTER ALEF
                    0xA880 => 0xA882,       // Saurashtra: SAURASHTRA LETTER A
                    0x11000 => 0x11005,     // Brahmi: BRAHMI LETTER A
                    0x11080 => 0x11083,     // Kaithi: KAITHI LETTER A
                    0x11100 => 0x11103,     // Chakma: CHAKMA LETTER AA
                    0x11180 => 0x11183,     // Sharada: SHARADA LETTER A
                    0x11300 => 0x11305,     // Grantha: GRANTHA LETTER A
                    0x13430 => 0x13437,     // Egyptian Hieroglyphs Format Controls: EGYPTIAN HIEROGLYPH BEGIN SEGMENT
                    0x1B00 => 0x1B05,       // Balinese: BALINESE LETTER AKARA
                    0xA980 => 0xA984,       // Javanese: JAVANESE LETTER A
                    0x1B80 => 0x1B83,       // Sundanese: SUNDANESE LETTER A
                    _ => FirstAssignedCodepoint
                };
                return UniData.AsString(cp);
            }
        }

        /// <summary>Unicode block name such as "Basic Latin (ASCII)" (from MetaBlocks.txt)</summary>
        public string BlockName { get; private set; }

        /// <summary>Name of first level of block hierarchy such as "Latin" (from MetaBlocks.txt)</summary>
        public string Level1Name { get; private set; }

        /// <summary>Name of second level of block hierarchy such as "European Scripts" (from MetaBlocks.txt)</summary>
        public string Level2Name { get; private set; }

        /// <summary>Name of third level of block hierarchy such as "Scripts" (from MetaBlocks.txt)</summary>
        public string Level3Name { get; private set; }

        /// <summary>Sorting key matching hierarchy order </summary>
        public int Rank { get; internal set; }

        /// <summary>Block name followed by range of codepoints such as "Basic Latin (ASCII) 0020..007F"</summary>
        public string BlockNameAndRange => $"{BlockName} {Begin:X4}..{End:X4}";

        /// <summary>internal constructor</summary>
        internal BlockRecord(int Begin, int End, string BlockName, string Level1Name, string Level2Name, string Level3Name)
        {
            this.Begin = Begin;
            this.End = End;
            this.BlockName = BlockName;
            this.Level1Name = Level1Name;
            this.Level2Name = Level2Name;
            this.Level3Name = Level3Name;
        }

        // String representation, mostly for debug
        public override string ToString() =>
            $"BlockRecord(Range={Begin:X4}..{End:X4}, Block={BlockName}, L1={Level1Name}, L2={Level2Name}, L3={Level3Name})";
    }


    /// <summary>
    /// Represents Unicode general category of characters
    /// </summary>
    public class CategoryRecord
    {
        /// <summary>Unicode General Category code, one or two letters such as Lu</summary>
        public string Code { get; private set; }

        /// <summary>Unicode General Category name with spaces replaced by underscores such as Uppercase_Letter</summary>
        public string Name { get; private set; }

        /// <summary>For metacategories, a pipe-separated string of included categories such as "Ll|Lm|Lo|Lt|Lu" for category L</summary>
        public string Include { get; private set; }

        /// <summary>For metacategories, a list of strings of included categories such as ["Ll", "Lm", "Lo", "Lt", "Lu"] for category L</summary>
        public IList<string> CategoriesList { get; private set; }

        /// <summary>
        /// For metacategories, a comma-separated string of included categories such as "Ll, Lm, Lo, Lt, Lu" for category L
        /// Used by binding in CharDetailWindow
        /// </summary>
        public string Categories
        {
            get
            {
                var sb = new StringBuilder();
                foreach (string cat in CategoriesList)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(cat + ":" + UniData.CategoryRecords[cat].Name);
                }
                return sb.ToString();
            }
        }


        /// <summary>Internal constructor</summary>
        internal CategoryRecord(string code, string name, string include = "")
        {
            Code = code;
            Name = name;
            Include = include;
            CategoriesList = new List<string> { code };
        }

        /// <summary>String representation, mostly for debug</summary>
        public override string ToString() => $"CategoryRecord(Code={Code}, Name={Name}, Include={Include})";
    }


    /// <summary>
    /// Static class exposing Unicode data dictionaries
    /// </summary>
    public static class UniData
    {
        // Real internal dictionaries used to store Unicode data
        private static readonly Dictionary<int, CharacterRecord> char_map = new Dictionary<int, CharacterRecord>(65536);
        private static readonly Dictionary<string, CategoryRecord> cat_map = new Dictionary<string, CategoryRecord>();
        private static readonly Dictionary<int, BlockRecord> block_map = new Dictionary<int, BlockRecord>();


        // Public read only dictionaries to access Unicode data

        /// <summary>Dictionary of all valid character records indexed by Codepoints.</summary>
        public static ReadOnlyDictionary<int, CharacterRecord> CharacterRecords { get; } = new ReadOnlyDictionary<int, CharacterRecord>(char_map);

        /// <summary>Dictionary of all categories, indexed by category (case sensitive).</summary>
        public static ReadOnlyDictionary<string, CategoryRecord> CategoryRecords { get; } = new ReadOnlyDictionary<string, CategoryRecord>(cat_map);

        /// <summary>Dictionary of all blocks, indexed by 1st Codepoint of the block.</summary>
        public static ReadOnlyDictionary<int, BlockRecord> BlockRecords { get; } = new ReadOnlyDictionary<int, BlockRecord>(block_map);


        /// <summary>Last valid codepoint.</summary>
        public const int MaxCodepoint = 0x10FFFF;


        // To extract a CP from a Cross-Ref
        private static readonly Regex reCP = new Regex(@"\b1?[0-9A-F]{4,5}\b");

        /// <summary>True for assigned codepoints.  By convention, codepoints in surrogates ranges are not valid.</summary>
        public static bool IsValidCodepoint(int cp) => !IsSurrogate(cp) && char_map.ContainsKey(cp) && cp <= MaxCodepoint;

        /// <summary>True for any character in suggogate range 0xD800..0xDFFF (no distinction between low and high surrogates, or private high surrogates</summary>
        internal static bool IsSurrogate(int cp) => cp >= 0xD800 && cp <= 0xDFFF;

        /// <summary>True for "Not a character": FDD0..FDED and last two characters of each page</summary>
        internal static bool IsNonCharacter(int cp) => cp >= 0xFDD0 && cp <= 0xFDEF || (cp & 0xFFFF) == 0xFFFE || (cp & 0xFFFF) == 0xFFFF;


        /// <summary>Static constructor, loads data from resources</summary>
        static UniData()
        {
            Stopwatch TotalStopwatch = Stopwatch.StartNew();
            Stopwatch BlocksStopwatch = ReadBlocks();
            Stopwatch CategoriesStopwatch = ReadCategories();
            Stopwatch UnicodeDataStopwatch = ReadUnicodeData();
            AddBlockBegin();
            Stopwatch AgeStopwatch = ReadAges();
            Stopwatch NamesStopwatch = ReadNamesList();
            Stopwatch ScriptsStopwatch = ReadScripts();
            TotalStopwatch.Stop();

            Debug.WriteLine("UniData initialization times:");
            Debug.WriteLine($"Blocks:      {BlocksStopwatch.Elapsed}");
            Debug.WriteLine($"Categories:  {CategoriesStopwatch.Elapsed}");
            Debug.WriteLine($"UnicodeData: {UnicodeDataStopwatch.Elapsed}");
            Debug.WriteLine($"Age:         {AgeStopwatch.Elapsed}");
            Debug.WriteLine($"Names:       {NamesStopwatch.Elapsed}");
            Debug.WriteLine($"Scripts:     {ScriptsStopwatch.Elapsed}");
            Debug.WriteLine($"TOTAL:       {TotalStopwatch.Elapsed}");
        }

        /// <summary>Read blocks info from MetaBlocks.txt: Block Name, Level 1 Name, Level 2 Name and Level3 Name.  Note that this file does not come from Unicode.org but is manually managed.</summary>
        private static Stopwatch ReadBlocks()
        {
            Stopwatch BlocksStopwatch = Stopwatch.StartNew();
            using (var sr = new StreamReader(GetResourceStream("MetaBlocks.txt")))
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine() ?? string.Empty;
                    if (line.Length == 0 || line[0] == '#') continue;
                    string[] fields = line.Split(';');
                    string[] field0 = fields[0].Replace("..", ";").Split(';');
                    int begin = int.Parse(field0[0], NumberStyles.HexNumber);
                    int end = int.Parse(field0[1], NumberStyles.HexNumber);
                    BlockRecord br = new BlockRecord(begin, end, fields[1], fields[2], fields[3], fields[4]);
                    block_map.Add(begin, br);
                }

            // Compute rank using an integer of format 33221100 where 33 is index of L3 block, 22 index of L2 block in L3...
            int rank3 = 0;
            foreach (var l3 in block_map.Values.GroupBy(b => b.Level3Name).OrderBy(g => g.Key))
            {
                int rank2 = 0;
                foreach (var l2 in l3.GroupBy(b => b.Level2Name))
                {
                    int rank1 = 0;
                    foreach (var l1 in l2.GroupBy(b => b.Level1Name))
                    {
                        int rank0 = 0;
                        foreach (var l0 in l1)
                        {
                            l0.Rank = rank0 + 100 * (rank1 + 100 * (rank2 + 100 * rank3));
                            rank0++;
                        }
                        rank1++;
                    }
                    rank2++;
                }
                rank3++;
            }
            BlocksStopwatch.Stop();
            return BlocksStopwatch;
        }


        /// <summary>Initialize Categories, from https://unicode.org/reports/tr44/#GC_Values_Table</summary>
        private static Stopwatch ReadCategories()
        {
            Stopwatch CategoriesStopwatch = Stopwatch.StartNew();
            foreach (var cat in new CategoryRecord[] {
                new CategoryRecord("", "Unassigned"),
                new CategoryRecord("C", "Other", "Cc|Cf|Cn|Co|Cs"),
                new CategoryRecord("Cc", "Control"),
                new CategoryRecord("Cf", "Format"),
                new CategoryRecord("Cn", "Unassigned"),
                new CategoryRecord("Co", "Private_Use"),
                new CategoryRecord("Cs", "Surrogate"),
                new CategoryRecord("L", "Letter", "Ll|Lm|Lo|Lt|Lu"),
                new CategoryRecord("LC", "Cased_Letter", "Ll|Lt|Lu"),
                new CategoryRecord("Ll", "Lowercase_Letter"),
                new CategoryRecord("Lm", "Modifier_Letter"),
                new CategoryRecord("Lo", "Other_Letter"),
                new CategoryRecord("Lt", "Titlecase_Letter"),
                new CategoryRecord("Lu", "Uppercase_Letter"),
                new CategoryRecord("M", "Mark", "Mc|Me|Mn"),
                new CategoryRecord("Mc", "Spacing_Mark"),
                new CategoryRecord("Me", "Enclosing_Mark"),
                new CategoryRecord("Mn", "Nonspacing_Mark"),
                new CategoryRecord("N", "Number", "Nd|Nl|No"),
                new CategoryRecord("Nd", "Decimal_Number"),
                new CategoryRecord("Nl", "Letter_Number"),
                new CategoryRecord("No", "Other_Number"),
                new CategoryRecord("P", "Punctuation", "Pc|Pd|Pe|Pf|Pi|Po|Ps"),
                new CategoryRecord("Pc", "Connector_Punctuation"),
                new CategoryRecord("Pd", "Dash_Punctuation"),
                new CategoryRecord("Pe", "Close_Punctuation"),
                new CategoryRecord("Pf", "Final_Punctuation"),
                new CategoryRecord("Pi", "Initial_Punctuation"),
                new CategoryRecord("Po", "Other_Punctuation"),
                new CategoryRecord("Ps", "Open_Punctuation"),
                new CategoryRecord("S", "Symbol", "Sc|Sk|Sm|So"),
                new CategoryRecord("Sc", "Currency_Symbol"),
                new CategoryRecord("Sk", "Modifier_Symbol"),
                new CategoryRecord("Sm", "Math_Symbol"),
                new CategoryRecord("So", "Other_Symbol"),
                new CategoryRecord("Z", "Separator", "Zl|Zp|Zs"),
                new CategoryRecord("Zl", "Line_Separator"),
                new CategoryRecord("Zp", "Paragraph_Separator"),
                new CategoryRecord("Zs", "Space_Separator"),})
            {
                cat_map.Add(cat.Code, cat);
            }

            // Second pass, fill AllCategories
            foreach (CategoryRecord cr in cat_map.Values.Where(c => c.Include.Length > 0))
                foreach (string other in cr.Include.Split('|'))
                    cat_map[other].CategoriesList.Add(cr.Code);
            CategoriesStopwatch.Stop();
            return CategoriesStopwatch;
        }


        /// <summary>Read characters info from UnicodeData.txt: Name, Category, IsPrintable</summary>
        private static Stopwatch ReadUnicodeData()
        {
            Stopwatch UnicodeDataStopwatch = Stopwatch.StartNew();
            using (var sr = new StreamReader(GetResourceStream("UnicodeData.txt")))
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
                    else if (codepoint < 32 || codepoint >= 0x7f && codepoint < 0xA0)
                        char_name = "CONTROL - " + (fields[10].Length > 0 ? fields[10] : fields[0][2..]);

                    bool is_range = char_name.EndsWith(", First>", StringComparison.OrdinalIgnoreCase);
                    bool is_printable = !(codepoint < 32                                // Control characters 0-31
                                        || codepoint >= 0x7f && codepoint < 0xA0        // Control characters 127-160
                                        || codepoint >= 0xD800 && codepoint <= 0xDFFF   // Surrogates
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

            UnicodeDataStopwatch.Stop();
            return UnicodeDataStopwatch;
        }

        // Add BlockBegin info to each character, done separately for efficiency.
        // Code is separated from ReadUnicodeData() to share its source with apps that don't use block info (ex: UniView)
        private static void AddBlockBegin()
        {
            foreach (var br in block_map.Values)
                for (int ch = br.Begin; ch <= br.End; ch++)
                    if (char_map.ContainsKey(ch))
                        char_map[ch].BlockBegin = br.Begin;
        }


        /// <summary>Read characters age info from DerivedAge.txt.  Age is the version of Unicode standard that character appeared for the first time.</summary>
        private static Stopwatch ReadAges()
        {
            Stopwatch AgeStopwatch = Stopwatch.StartNew();
            using (var sr = new StreamReader(GetResourceStream("DerivedAge.txt")))
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine() ?? string.Empty;
                    if (line.Length == 0 || line[0] == '#') continue;
                    int p = line.IndexOf('#');
                    if (p >= 0) line = line.Substring(0, p - 1);
                    string[] fields = line.Split(';');
                    p = fields[0].IndexOf('.');
                    int codepoint = int.Parse(p < 0 ? fields[0] : fields[0].Substring(0, p), NumberStyles.HexNumber);
                    string age = fields[1].Trim();
                    if (p < 0)
                    {
                        if (char_map.ContainsKey(codepoint))
                            char_map[codepoint].Age = age;
                    }
                    else
                    {
                        int end_char_code = int.Parse(fields[0][(p + 2)..], NumberStyles.HexNumber);
                        // Skip planes 15 and 16 private use
                        if (codepoint != 0xF0000 && codepoint != 0x100000)
                            for (int code = codepoint; code <= end_char_code; code++)
                                if (char_map.ContainsKey(code))
                                    char_map[code].Age = age;
                    }
                }
            AgeStopwatch.Stop();
            return AgeStopwatch;
        }

        /// <summary>Read block subheaders, synonyms and cross-references from NamesList.txt.</summary>
        private static Stopwatch ReadNamesList()
        {
            Stopwatch NamesStopwatch = Stopwatch.StartNew();
            string subheader = string.Empty;

            // Subheaders merging
            HashSet<int>? blockCodepoints = null;
            HashSet<string>? blockSubheaders = null;

            // Some subheaders in a block are almost the same and should be merged
            // This dictionary contains subheaders that do not differ only by final s
            Dictionary<string, string> extraMergeSubheaders = new Dictionary<string, string>
            {
                { "Extended Arabic letter for Parkari", "Extended Arabic letters for Parkari" },
                { "Sign for Yajurvedic", "Signs for Yajurvedic" },
                { "Additional diacritical mark for symbols", "Additional diacritical marks for symbols" },
                { "Map symbol from ARIB STD B24", "Map symbols from ARIB STD B24" },
                { "Letter for African languages", "Letters for African languages" },
                { "Addition for UPA", "Additions for UPA" },
            };

            void MergeSubheaders()
            {
                Debug.Assert(blockSubheaders != null);
                foreach (string singular in blockSubheaders.Where(s => !s.EndsWith("s", StringComparison.Ordinal)))
                    if (blockSubheaders!.Contains(singular + "s"))
                    {
                        foreach (int cp in blockCodepoints!)
                            if (char_map[cp].Subheader == singular)
                                char_map[cp].Subheader += "s";
                    }
                    else if (extraMergeSubheaders.ContainsKey(singular))
                    {
                        foreach (int cp in blockCodepoints!)
                            if (char_map[cp].Subheader == singular)
                                char_map[cp].Subheader = extraMergeSubheaders[singular];
                    }

                blockCodepoints = null;
                blockSubheaders = null;
            }


            int cp16 = -1;
            using (var sr = new StreamReader(GetResourceStream("NamesList.txt")))
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine() ?? string.Empty;
                    if (line.StartsWith(";", StringComparison.Ordinal)) { }
                    //else if (line.StartsWith("@@@+", StringComparison.Ordinal)) { }
                    //else if (line.StartsWith("@@@~", StringComparison.Ordinal)) { }
                    //else if (line.StartsWith("@@@", StringComparison.Ordinal)) { }
                    //else if (line.StartsWith("@@+", StringComparison.Ordinal)) { }
                    else if (line.StartsWith("@@\t", StringComparison.Ordinal))
                    {
                        // Block begin
                        if (blockCodepoints != null)
                            MergeSubheaders();
                        blockCodepoints = new HashSet<int>();
                        blockSubheaders = new HashSet<string>();
                        subheader = string.Empty;
                    }
                    else if (line.StartsWith("@@", StringComparison.Ordinal)) { }
                    else if (line.StartsWith("@+", StringComparison.Ordinal)) { }
                    else if (line.StartsWith("@~", StringComparison.Ordinal)) { }
                    else if (line.StartsWith("@", StringComparison.Ordinal))
                    {
                        subheader = line[3..];
                    }
                    else if (line.StartsWith("\t=", StringComparison.Ordinal) || line.StartsWith("\t%", StringComparison.Ordinal))
                    {
                        // Synonyms, new in 1.6
                        if (!char_map.ContainsKey(cp16))
                            continue;

                        if (char_map[cp16].Synonyms == null)
                            char_map[cp16].Synonyms = new List<string>();
                        char_map[cp16].Synonyms!.Add(line[3..4].ToUpperInvariant() + line[4..]);
                    }
                    else if (line.StartsWith("\tx", StringComparison.Ordinal))
                    {
                        // Cross-references, new in 1.6
                        if (!char_map.ContainsKey(cp16))
                            continue;

                        string crossRef = line[3..].Replace(" - ", " ");
                        if (crossRef[0] == '(' && crossRef[^1] == ')')
                            crossRef = crossRef[1..^1];
                        if (char_map[cp16].CrossRefs == null)
                            char_map[cp16].CrossRefs = new List<string>();
                        char_map[cp16].CrossRefs!.Add(crossRef[0..1].ToUpperInvariant() + crossRef[1..]);
                    }
                    else if (line.StartsWith("\t*", StringComparison.Ordinal) || line.StartsWith("\t~", StringComparison.Ordinal))
                    {
                        // Comments, new in 1.6
                        if (!char_map.ContainsKey(cp16))
                            continue;

                        string comment = line[3..4].ToUpperInvariant() + line[4..];

                        // Special processing for variations, add variation combination at the end
                        if (line.StartsWith("\t~", StringComparison.Ordinal))
                        {
                            var maColl = reCP.Matches(line);
                            if (maColl.Count == 2)
                            {
                                int cp1 = -1, cp2 = -1;
                                foreach (var ma in maColl)
                                {
                                    if (cp1 < 0)
                                        cp1 = Convert.ToInt32(ma.ToString(), 16);
                                    else
                                        cp2 = Convert.ToInt32(ma.ToString(), 16);
                                }
                                comment += " → " + AsString(cp1) + AsString(cp2);
                            }
                        }

                        if (char_map[cp16].Comments == null)
                            char_map[cp16].Comments = new List<string>();
                        char_map[cp16].Comments!.Add(comment);
                    }
                    else if (line.StartsWith("\t", StringComparison.Ordinal)) { }
                    else
                    {
                        cp16 = 0;
                        for (int p = 0; ; p++)
                        {
                            char c = line[p];

                            if (c >= '0' && c <= '9')
                                cp16 = 16 * cp16 + c - 48;
                            else if (c >= 'A' && c <= 'F')
                                cp16 = 16 * cp16 + c - 65 + 10;
                            else
                            {
                                if (c != '\t') Debugger.Break();
                                break;
                            }
                        }

                        if (char_map.ContainsKey(cp16))
                        {
                            blockCodepoints?.Add(cp16);
                            blockSubheaders?.Add(subheader);
                            char_map[cp16].Subheader = subheader;
                        }
                    }
                }
            if (blockCodepoints != null)
                MergeSubheaders();
            NamesStopwatch.Stop();
            return NamesStopwatch;
        }


        /// <summary>Read script associated with characters from Scripts.txt.</summary>
        private static Stopwatch ReadScripts()
        {
            Stopwatch ScriptsStopwatch = Stopwatch.StartNew();
            using (var sr = new StreamReader(GetResourceStream("Scripts.txt")))
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
                        int end_char_code = int.Parse(fields[0][(p + 2)..], NumberStyles.HexNumber);
                        for (int code = codepoint; code <= end_char_code; code++)
                            if (char_map.ContainsKey(code))
                                char_map[code].script = script;
                    }
                }

            ScriptsStopwatch.Stop();
            return ScriptsStopwatch;
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


        /// <summary>
        /// Converts a codepoint to an UTF-16 encoded string (.Net string).
        /// Do not name it ToString since it's a static class and it can't override object.ToString(.)
        /// </summary>
        /// <param name="cp">Codepoint to convert.</param>
        /// <returns>A string of one character for cp&lt;0xFFFF, or two surrogate characters for cp&gt;=0x10000.
        /// No check is made for invalid codepoints, returned string is undefined in this case.</returns>
        public static string AsString(int cp) => cp <= 0xD7FF || (cp >= 0xE000 && cp <= 0xFFFF) ? new string((char)cp, 1) : new string((char)(0xD800 + ((cp - 0x10000) >> 10)), 1) + new string((char)(0xDC00 + (cp & 0x3ff)), 1);


        // Returns number of Unicode characters in a (valid) UTF-16 encoded string
        public static int UnicodeLength(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            int l = 0;
            bool isSurrogate = false;
            foreach (char c in str)
            {
                if (isSurrogate)
                    isSurrogate = false;
                else
                {
                    l++;
                    isSurrogate = c >= 0xD800 && c <= 0xDBFF;
                }
            }
            return l;
        }
    }


    public static class ExtensionMethods
    {
        /// <summary>
        /// Enumerates CharacterRecord from a C# string (UTF-16 encoded).
        /// Couples of surrogates return a single CharacterRecord.
        /// </summary>
        /// <param name="str">String to decompose</param>
        /// <returns>An enumeration of CharacterRecord from str</returns>
        public static IEnumerable<CharacterRecord> EnumCharacterRecords(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            for (int i = 0; i < str.Length; i++)
            {
                int cp = str[i];
                if (cp >= 0xD800 && cp <= 0xDBFF)
                {
                    i += 1;
                    cp = 0x10000 + ((cp & 0x3ff) << 10) + (str[i] & 0x3ff);
                }
                var cr = UniData.CharacterRecords[cp];
                yield return cr;
            }
        }
    }
}
