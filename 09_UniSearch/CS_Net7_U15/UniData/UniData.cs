// UniData
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
// 2022-04-27   PV      1.14.0 Unicode 14 (new numbering scheme)
// 2022-11-18   PV      1.15.0 Unicode 15

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

/// <summary>
/// Represents an Unicode codepoint and various associated information 
/// </summary>
public class CharacterRecord
{
    /// <summary>Unicode Character Codepoint, between 0 and 0x10FFFF (from UnicodeData.txt).</summary>
    public int Codepoint { get; }

    /// <summary>Unicode Character Name, uppercase string such as LATIN CAPITAL LETTER A (from UnicodeData.txt).</summary>
    public string Name { get; }

    /// <summary>Unicode General Category, 2 characters such as Lu (from UnicodeData.txt).</summary>
    public string Category { get; }

    /// <summary>Unicode _Script (from Scripts.txt)</summary>
    public string Script => _Script ?? "Unknown";

    internal string? _Script;

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
    public bool IsPrintable { get; }

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
                return $"{0xE0 + Codepoint / 0x40 / 0x40:X2} {0x80 + Codepoint / 0x40 % 0x40:X2} {0x80 + Codepoint % 0x40:X2}";
            else if (Codepoint <= 0x1FFFFF)
                return $"{0xF0 + Codepoint / 0x40 / 0x40 / 0x40:X2} {0x80 + Codepoint / 0x40 / 0x40 % 0x40:X2} {0x80 + Codepoint / 0x40 % 0x40:X2} {0x80 + Codepoint % 0x40:X2}";
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
    internal static bool IsNonCharacter(int cp) => (cp >= 0xFDD0 && cp <= 0xFDEF) || (cp & 0xFFFF) == 0xFFFE || (cp & 0xFFFF) == 0xFFFF;
}

/// <summary>
/// Represent an Unicode block (range of codepoints) and its hierarchical classification
/// </summary>
public class BlockRecord
{
    /// <summary>First codepoint of the block.  Beware, this is based on block definition and not guaranteed to be a valid codepoint: block Gurmukhi 0A00..0A7F but 0A00 is not a valid codepoint.  Use property FirstBlockCodepoint to get the first valid Codepoint of the block.</summary>
    public int Begin { get; }

    /// <summary>Last codepoint of the block (may or may not be an assigned codepoint)</summary>
    public int End { get; }

    /// <summary>First assigned codepoint of the block, it's sometimes different of begin.</summary>
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
    public string BlockName { get; }

    /// <summary>Name of first level of block hierarchy such as "Latin" (from MetaBlocks.txt)</summary>
    public string Level1Name { get; }

    /// <summary>Name of second level of block hierarchy such as "European Scripts" (from MetaBlocks.txt)</summary>
    public string Level2Name { get; }

    /// <summary>Name of third level of block hierarchy such as "Scripts" (from MetaBlocks.txt)</summary>
    public string Level3Name { get; }

    /// <summary>Sorting key matching hierarchy order </summary>
    public int Rank { get; internal set; }

    /// <summary>Block name followed by range of codepoints such as "Basic Latin (ASCII) 0020..007F"</summary>
    public string BlockNameAndRange => $"{BlockName} {Begin:X4}..{End:X4}";

    /// <summary>internal constructor</summary>
    internal BlockRecord(int begin, int end, string blockName, string level1Name, string level2Name, string level3Name)
    {
        Begin = begin;
        End = end;
        BlockName = blockName;
        Level1Name = level1Name;
        Level2Name = level2Name;
        Level3Name = level3Name;
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
    public string Code { get; }

    /// <summary>Unicode General Category name with spaces replaced by underscores such as Uppercase_Letter</summary>
    public string Name { get; }

    /// <summary>For metacategories, a pipe-separated string of included categories such as "Ll|Lm|Lo|Lt|Lu" for category L</summary>
    public string Include { get; }

    /// <summary>For metacategories, a list of strings of included categories such as ["Ll", "Lm", "Lo", "Lt", "Lu"] for category L</summary>
    public IList<string> CategoriesList { get; }

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
public static partial class UniData
{
    // Real internal dictionaries used to store Unicode data
    private static readonly Dictionary<int, CharacterRecord> CharMap = new(65536);
    private static readonly Dictionary<string, CategoryRecord> CatMap = new();
    private static readonly Dictionary<int, BlockRecord> BlockMap = new();

    // Public read only dictionaries to access Unicode data

    /// <summary>Dictionary of all valid character records indexed by Codepoints.</summary>
    public static ReadOnlyDictionary<int, CharacterRecord> CharacterRecords { get; } = new(CharMap);

    /// <summary>Dictionary of all categories, indexed by category (case sensitive).</summary>
    public static ReadOnlyDictionary<string, CategoryRecord> CategoryRecords { get; } = new(CatMap);

    /// <summary>Dictionary of all blocks, indexed by 1st Codepoint of the block.</summary>
    public static ReadOnlyDictionary<int, BlockRecord> BlockRecords { get; } = new(BlockMap);

    /// <summary>Last valid codepoint.</summary>
    public const int MaxCodepoint = 0x10FFFF;

    // To extract a CP from a Cross-Ref
    [GeneratedRegex("\\b1?[0-9A-F]{4,5}\\b")]
    private static partial Regex CPRegex();

    /// <summary>True for assigned codepoints.  By convention, codepoints in surrogates ranges are not valid.</summary>
    public static bool IsValidCodepoint(int cp) => !IsSurrogate(cp) && CharMap.ContainsKey(cp) && cp <= MaxCodepoint;

    /// <summary>True for any character in surrogate range 0xD800..0xDFFF (no distinction between low and high surrogates, or private high surrogates</summary>
    internal static bool IsSurrogate(int cp) => cp >= 0xD800 && cp <= 0xDFFF;

    /// <summary>True for "Not a character": FDD0..FDED and last two characters of each page</summary>
    internal static bool IsNonCharacter(int cp) => (cp >= 0xFDD0 && cp <= 0xFDEF) || (cp & 0xFFFF) == 0xFFFE || (cp & 0xFFFF) == 0xFFFF;

    /// <summary>Static constructor, loads data from resources</summary>
    static UniData()
    {
        ReadBlocks();
        ReadCategories();
        ReadUnicodeData();
        AddBlockBegin();
        ReadAges();
        ReadNamesList();
        ReadScripts();
    }

    /// <summary>Read blocks info from MetaBlocks.txt: Block Name, Level 1 Name, Level 2 Name and Level3 Name.  Note that this file does not come from Unicode.org but is manually managed.</summary>
    private static Stopwatch ReadBlocks()
    {
        var blocksStopwatch = Stopwatch.StartNew();
        using (var sr = new StreamReader(GetResourceStream("MetaBlocks.txt")))
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine() ?? string.Empty;
                if (line.Length == 0 || line[0] == '#') continue;
                string[] fields = line.Split(';');
                string[] field0 = fields[0].Replace("..", ";").Split(';');
                int begin = int.Parse(field0[0], NumberStyles.HexNumber);
                int end = int.Parse(field0[1], NumberStyles.HexNumber);
                var br = new BlockRecord(begin, end, fields[1], fields[2], fields[3], fields[4]);
                BlockMap.Add(begin, br);
            }

        // Compute rank using an integer of format 33221100 where 33 is index of L3 block, 22 index of L2 block in L3...
        int rank3 = 0;
        foreach (var l3 in BlockMap.Values.GroupBy(b => b.Level3Name).OrderBy(g => g.Key))
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
        blocksStopwatch.Stop();
        return blocksStopwatch;
    }

    /// <summary>Initialize Categories, from https://unicode.org/reports/tr44/#GC_Values_Table</summary>
    private static Stopwatch ReadCategories()
    {
        var categoriesStopwatch = Stopwatch.StartNew();
        foreach (var cat in new CategoryRecord[] {
                     new("", "Unassigned"),
                     new("C", "Other", "Cc|Cf|Cn|Co|Cs"),
                     new("Cc", "Control"),
                     new("Cf", "Format"),
                     new("Cn", "Unassigned"),
                     new("Co", "Private_Use"),
                     new("Cs", "Surrogate"),
                     new("L", "Letter", "Ll|Lm|Lo|Lt|Lu"),
                     new("LC", "Cased_Letter", "Ll|Lt|Lu"),
                     new("Ll", "Lowercase_Letter"),
                     new("Lm", "Modifier_Letter"),
                     new("Lo", "Other_Letter"),
                     new("Lt", "Titlecase_Letter"),
                     new("Lu", "Uppercase_Letter"),
                     new("M", "Mark", "Mc|Me|Mn"),
                     new("Mc", "Spacing_Mark"),
                     new("Me", "Enclosing_Mark"),
                     new("Mn", "Nonspacing_Mark"),
                     new("N", "Number", "Nd|Nl|No"),
                     new("Nd", "Decimal_Number"),
                     new("Nl", "Letter_Number"),
                     new("No", "Other_Number"),
                     new("P", "Punctuation", "Pc|Pd|Pe|Pf|Pi|Po|Ps"),
                     new("Pc", "Connector_Punctuation"),
                     new("Pd", "Dash_Punctuation"),
                     new("Pe", "Close_Punctuation"),
                     new("Pf", "Final_Punctuation"),
                     new("Pi", "Initial_Punctuation"),
                     new("Po", "Other_Punctuation"),
                     new("Ps", "Open_Punctuation"),
                     new("S", "Symbol", "Sc|Sk|Sm|So"),
                     new("Sc", "Currency_Symbol"),
                     new("Sk", "Modifier_Symbol"),
                     new("Sm", "Math_Symbol"),
                     new("So", "Other_Symbol"),
                     new("Z", "Separator", "Zl|Zp|Zs"),
                     new("Zl", "Line_Separator"),
                     new("Zp", "Paragraph_Separator"),
                     new("Zs", "Space_Separator"),})
        {
            CatMap.Add(cat.Code, cat);
        }

        // Second pass, fill AllCategories
        foreach (CategoryRecord cr in CatMap.Values.Where(c => c.Include.Length > 0))
        foreach (string other in cr.Include.Split('|'))
            CatMap[other].CategoriesList.Add(cr.Code);
        categoriesStopwatch.Stop();
        return categoriesStopwatch;
    }

    /// <summary>Read characters info from UnicodeData.txt: Name, Category, IsPrintable</summary>
    private static Stopwatch ReadUnicodeData()
    {
        var unicodeDataStopwatch = Stopwatch.StartNew();
        using (var sr = new StreamReader(GetResourceStream("UnicodeData.txt")))
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

        unicodeDataStopwatch.Stop();
        return unicodeDataStopwatch;
    }

    // Add BlockBegin info to each character, done separately for efficiency.
    // Code is separated from ReadUnicodeData() to share its source with apps that don't use block info (ex: UniView)
    private static void AddBlockBegin()
    {
        foreach (var br in BlockMap.Values)
            for (int ch = br.Begin; ch <= br.End; ch++)
                if (CharMap.TryGetValue(ch, out CharacterRecord? value))
                    value.BlockBegin = br.Begin;
    }

    /// <summary>Read characters age info from DerivedAge.txt.  Age is the version of Unicode standard that character appeared for the first time.</summary>
    private static Stopwatch ReadAges()
    {
        var ageStopwatch = Stopwatch.StartNew();
        using (var sr = new StreamReader(GetResourceStream("DerivedAge.txt")))
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine() ?? string.Empty;
                if (line.Length == 0 || line[0] == '#') continue;
                int p = line.IndexOf('#');
                if (p >= 0) line = line[..(p - 1)];
                string[] fields = line.Split(';');
                p = fields[0].IndexOf('.');
                int codepoint = int.Parse(p < 0 ? fields[0] : fields[0][..p], NumberStyles.HexNumber);
                string age = fields[1].Trim();
                if (p < 0)
                {
                    if (CharMap.TryGetValue(codepoint, out CharacterRecord? value1))
                        value1.Age = age;
                }
                else
                {
                    int endCharCode = int.Parse(fields[0][(p + 2)..], NumberStyles.HexNumber);
                    // Skip planes 15 and 16 private use
                    if (codepoint != 0xF0000 && codepoint != 0x100000)
                        for (int code = codepoint; code <= endCharCode; code++)
                            if (CharMap.TryGetValue(code, out CharacterRecord? value2))
                                value2.Age = age;
                }
            }
        ageStopwatch.Stop();
        return ageStopwatch;
    }

    /// <summary>Read block subheaders, synonyms and cross-references from NamesList.txt.</summary>
    private static Stopwatch ReadNamesList()
    {
        var namesStopwatch = Stopwatch.StartNew();
        string subheader = string.Empty;

        // Subheaders merging
        HashSet<int>? blockCodepoints = null;
        HashSet<string>? blockSubheaders = null;

        // Some subheaders in a block are almost the same and should be merged
        // This dictionary contains subheaders that do not differ only by final s
        var extraMergeSubheaders = new Dictionary<string, string>
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
                        if (CharMap[cp].Subheader == singular)
                            CharMap[cp].Subheader += "s";
                }
                else if (extraMergeSubheaders.ContainsKey(singular))
                {
                    foreach (int cp in blockCodepoints!)
                        if (CharMap[cp].Subheader == singular)
                            CharMap[cp].Subheader = extraMergeSubheaders[singular];
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
                    if (!CharMap.ContainsKey(cp16))
                        continue;

                    CharMap[cp16].Synonyms ??= new List<string>();
                    CharMap[cp16].Synonyms!.Add(line[3..4].ToUpperInvariant() + line[4..]);
                }
                else if (line.StartsWith("\tx", StringComparison.Ordinal))
                {
                    // Cross-references, new in 1.6
                    if (!CharMap.ContainsKey(cp16))
                        continue;

                    string crossRef = line[3..].Replace(" - ", " ");
                    if (crossRef[0] == '(' && crossRef[^1] == ')')
                        crossRef = crossRef[1..^1];
                    CharMap[cp16].CrossRefs ??= new List<string>();
                    CharMap[cp16].CrossRefs!.Add(crossRef[..1].ToUpperInvariant() + crossRef[1..]);
                }
                else if (line.StartsWith("\t*", StringComparison.Ordinal) || line.StartsWith("\t~", StringComparison.Ordinal))
                {
                    // Comments, new in 1.6
                    if (!CharMap.ContainsKey(cp16))
                        continue;

                    string comment = line[3..4].ToUpperInvariant() + line[4..];

                    // Special processing for variations, add variation combination at the end
                    if (line.StartsWith("\t~", StringComparison.Ordinal))
                    {
                        var maColl = CPRegex().Matches(line);
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

                    CharMap[cp16].Comments ??= new List<string>();
                    CharMap[cp16].Comments!.Add(comment);
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

                    if (CharMap.TryGetValue(cp16, out CharacterRecord? value))
                    {
                        blockCodepoints?.Add(cp16);
                        blockSubheaders?.Add(subheader);
                        value.Subheader = subheader;
                    }
                }
            }
        if (blockCodepoints != null)
            MergeSubheaders();
        namesStopwatch.Stop();
        return namesStopwatch;
    }

    /// <summary>Read _Script associated with characters from Scripts.txt.</summary>
    private static Stopwatch ReadScripts()
    {
        var scriptsStopwatch = Stopwatch.StartNew();
        using (var sr = new StreamReader(GetResourceStream("Scripts.txt")))
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine() ?? string.Empty;
                if (line.Length == 0 || line[0] == '#') continue;
                int p = line.IndexOf('#');
                if (p >= 0) line = line[..(p - 1)];
                string[] fields = line.Split(';');
                p = fields[0].IndexOf('.');
                int codepoint = int.Parse(p < 0 ? fields[0] : fields[0][..p], NumberStyles.HexNumber);
                string script = fields[1].Trim();
                if (p < 0)
                {
                    if (CharMap.TryGetValue(codepoint, out CharacterRecord? value1))
                        value1._Script = script;
                }
                else
                {
                    int end_char_code = int.Parse(fields[0][(p + 2)..], NumberStyles.HexNumber);
                    for (int code = codepoint; code <= end_char_code; code++)
                        if (CharMap.TryGetValue(code, out CharacterRecord? value2))
                            value2._Script = script;
                }
            }

        scriptsStopwatch.Stop();
        return scriptsStopwatch;
    }

    /// <summary>Returns stream from embedded resource name.</summary>
    private static Stream GetResourceStream(string name)
    {
        name = "." + name;
        var assembly = typeof(UniData).GetTypeInfo().Assembly;
        var qualifiedName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException("Can't get resource (#1) " + name);
        var st = assembly.GetManifestResourceStream(qualifiedName);
        return st ?? throw new ArgumentException("Can't get resource (#2) " + name);
    }

    /// <summary>
    /// Converts a codepoint to an UTF-16 encoded string (.Net string).
    /// Do not name it ToString since it's a static class and it can't override object.ToString(.)
    /// </summary>
    /// <param name="cp">Codepoint to convert.</param>
    /// <returns>A string of one character for cp&lt;0xFFFF, or two surrogate characters for cp&gt;=0x10000.
    /// No check is made for invalid codepoints, returned string is undefined in this case.</returns>
    public static string AsString(int cp) => cp <= 0xD7FF || (cp >= 0xE000 && cp <= 0xFFFF) ? new string((char)cp, 1) : new string((char)(0xD800 + (cp - 0x10000 >> 10)), 1) + new string((char)(0xDC00 + (cp & 0x3ff)), 1);

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