﻿// UniData
// Static class providing characters and blocks information reading Unicode UCD files
// Aé♫山??
//
// 2018-09-11   PV
// 2018-09-17   PV      1.1 Store UCD Data in embedded streams; Skip characters from planes 15 and 16
// 2018-09-20   PV      1.2 Read NamesList.txt
// 2018-09-28	PV		1.2.1 Subheaders merging
// 2018-10-08	PV		XML comments
// 2019-03-06   PV      1.3 Unicode 12 UCD

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;


namespace UniDataNS
{
    /// <summary>
    /// Represents an Unicode codepoint and various associated information 
    /// </summary>
    public class CharacterRecord
    {
        /// <summary>
        /// Unicode character codepoint, between 0 and 0x10FFFF (from UnicodeData.txt)
        /// </summary>
        public int Codepoint { get; private set; }

        /// <summary>
        /// Unicode character name, uppercase string such as LATIN CAPITAL LETTER A (from UnicodeData.txt)
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Unicode general category, 2 characters such as Lu (from UnicodeData.txt)
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// First version of Unicode standard the character appeared, such as 3.0 (from DerivedAge.txt)
        /// </summary>
        public string Age { get; internal set; }

        /// <summary>
        /// First codepoint of the block assigned to the character, such as 0x100 (from MetaBlocks.txt)
        /// </summary>
        public int BlockBegin { get; internal set; } = -1;       // -1 for tests, to make sure at the end all supported chars have a block

        /// <summary>
        /// Name of block subdivision the character belongs to, such as ASCII punctuation and symbols (from NamesList.txt, marker @)
        /// If both singular and plural form exist for a subheader in a block (ex: "Additional letter" and "Additional letters"), only plural form is used
        /// Because of name merging, sorting by subheader does not sort characters in codepoint order
        /// </summary>
        public string Subheader { get; internal set; }


        /// <summary>
        /// When True, Character method will return an hex codepoint representation instead of the actual string.
        /// </summary>
        public bool IsPrintable { get; private set; }

        /// <summary>
        /// Convert to a C# string representation of the character, except for control characters.
        /// 'Safe version' of UnicodeData.CPtoString. U+FFFD is the official replacement character. 
        /// </summary>
        public string Character => UniData.CodepointToString(IsPrintable ? Codepoint : 0xFFFD);

        // For grouping, but not used in UWP since grouping mechanism is different
        // and use instead private chasses BSHGroupKey and GroupKeyComparer of ViewModel
        //public string GroupName => UniData.BlockRecords[BlockBegin].BlockName + (string.IsNullOrEmpty(Subheader) ? "" : ": " + Subheader);

        // Used by binding
        public BlockRecord Block => UniData.BlockRecords[BlockBegin];
        public CategoryRecord CategoryRecord => UniData.CategoryRecords[Category];

        /// <summary>
        /// Standard hexadecimal representation of codepoint such as U+0041 (4 to 6 uppercase hex digits)
        /// </summary>
        public string CodepointHex => $"U+{Codepoint:X4}";

        /// <summary>
        /// string representation of UTF-16 encoding such as "D83D DC17" for U+1F417
        /// </summary>
        public string UTF16 => Codepoint <= 0xD7FF || (Codepoint >= 0xE000 && Codepoint <= 0xFFFF) ? Codepoint.ToString("X4") : (0xD800 + ((Codepoint - 0x10000) >> 10)).ToString("X4") + " " + (0xDC00 + (Codepoint & 0x3ff)).ToString("X4");

        /// <summary>
        /// string representation of UTF-8 encoding such as "F0 9F 90 97" for U+1F417
        /// </summary>
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

        // internal constructor
        internal CharacterRecord(int Codepoint, string Name, string Category, bool IsPrintable)
        {
            this.Codepoint = Codepoint;
            this.Name = Name;
            this.Category = Category;
            this.IsPrintable = IsPrintable;
        }

        /// <summary>
        /// Text representation of the form "char(tab)codepoint(tab)name"
        /// </summary>
        public string AsString => $"{Character}\t{CodepointHex}\t{Name}";

        public override string ToString() => $"CharacterRecord({AsString})";
    }


    /// <summary>
    /// Represent an Unicode block (range of codepoints) and its hierarchical classification
    /// </summary>
    public class BlockRecord
    {
        /// <summary>
        /// First codepoint of the block
        /// </summary>
        public int Begin { get; private set; }

        /// <summary>
        /// Last codepoint of the block (may or may not be an assigned codepoint)
        /// </summary>
        public int End { get; private set; }

        /// <summary>
        /// Unicode block name such as "Basic Latin (ASCII)" (from MetaBlocks.txt)
        /// </summary>
        public string BlockName { get; private set; }

        /// <summary>
        /// Name of first level of block hierarchy such as "Latin" (from MetaBlocks.txt)
        /// </summary>
        public string Level1Name { get; private set; }

        /// <summary>
        /// Name of second level of block hierarchy such as "European Scripts" (from MetaBlocks.txt)
        /// </summary>
        public string Level2Name { get; private set; }

        /// <summary>
        /// Name of third level of block hierarchy such as "Scripts" (from MetaBlocks.txt)
        /// </summary>
        public string Level3Name { get; private set; }

        /// <summary>
        /// Sorting key matching hierarchy order
        /// </summary>
        public int Rank { get; internal set; }

        /// <summary>
        /// Block name followed by range of codepoints such as "Basic Latin (ASCII) 0020..007F"
        /// </summary>
        public string BlockNameAndRange => $"{BlockName} {Begin:X4}..{End:X4}";

        // internal constructor
        internal BlockRecord(int Begin, int End, string BlockName, string Level1Name, string Level2Name, string Level3Name)
        {
            this.Begin = Begin;
            this.End = End;
            this.BlockName = BlockName;
            this.Level1Name = Level1Name;
            this.Level2Name = Level2Name;
            this.Level3Name = Level3Name;
        }

        public override string ToString() =>
            $"BlockRecord(Range={Begin:X4}..{End:X4}, Block={BlockName}, L1={Level1Name}, L2={Level2Name}, L3={Level3Name})";
    }


    /// <summary>
    /// Represents Unicode general category of characters
    /// </summary>
    public class CategoryRecord
    {
        /// <summary>
        /// Unicode general category code, one or two letters such as Lu
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Unicode General Category name with spaces replaced by underscores such as Uppercase_Letter
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// For metacategories, a pipe-separated string of included categories such as "Ll|Lm|Lo|Lt|Lu" for category L
        /// </summary>
        public string Include { get; private set; }

        /// <summary>
        /// For metacategories, a list of strings of included categories such as ["Ll", "Lm", "Lo", "Lt", "Lu"] for category L
        /// </summary>
        public IList<string> CategoriesList { get; private set; }

        /// <summary>
        /// For metacategories, a comma-separated string of included categories such as "Ll, Lm, Lo, Lt, Lu" for category L
        /// </summary>
        public string Categories => CategoriesList.Aggregate((prev, c) => prev + ", " + c);

        // internal constructor
        internal CategoryRecord(string code, string name, string include = "")
        {
            this.Code = code;
            this.Name = name;
            this.Include = include;
            CategoriesList = new List<string> { code };
        }

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
        public static ReadOnlyDictionary<int, CharacterRecord> CharacterRecords { get; } = new ReadOnlyDictionary<int, CharacterRecord>(char_map);
        public static ReadOnlyDictionary<string, CategoryRecord> CategoryRecords { get; } = new ReadOnlyDictionary<string, CategoryRecord>(cat_map);
        public static ReadOnlyDictionary<int, BlockRecord> BlockRecords { get; } = new ReadOnlyDictionary<int, BlockRecord>(block_map);


        // Static constructor, loads data from resources, executed when application starts
        static UniData()
        {
            // Read blocks
            using (var sr = new StreamReader(GetResourceStream("MetaBlocks.txt")))
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line.Length == 0 || line[0] == '#') continue;
                    string[] fields = line.Split(';');
                    string[] field0 = fields[0].Replace("..", ";", StringComparison.InvariantCulture).Split(';');
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


            // Initialize Categories
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


            // Read characters
            using (var sr = new StreamReader(GetResourceStream("UnicodeData.txt")))
                while (!sr.EndOfStream)
                {
                    string[] fields = sr.ReadLine().Split(';');
                    int codepoint = int.Parse(fields[0], NumberStyles.HexNumber);
                    string char_name = fields[1];
                    string char_category = fields[2];
                    if (char_name == "<control>")
                        char_name = "CONTROL-" + fields[10];
                    bool is_range = char_name.EndsWith(", First>", StringComparison.OrdinalIgnoreCase);
                    bool is_printable = !(codepoint < 32                                // Control characters 0-31
                                        || codepoint >= 0x7f && codepoint < 0xA0        // Control characters 127-160
                                        || codepoint >= 0xD800 && codepoint <= 0xDFFF   // Surrogates
                                        || codepoint == 0x2028                          // U+2028  LINE SEPARATOR
                                        || codepoint == 0x2029                          // U+2029  PARAGRAPH SEPARATOR
                                        );
                    if (is_range)   // Add all characters within a specified range
                    {
                        char_name = char_name.Replace(", First>", String.Empty, StringComparison.InvariantCulture).Replace("<", string.Empty, StringComparison.InvariantCulture).ToUpperInvariant(); //remove range indicator from name
                        fields = sr.ReadLine().Split(';');
                        int end_char_code = int.Parse(fields[0], NumberStyles.HexNumber);
                        if (!fields[1].EndsWith(", Last>", StringComparison.OrdinalIgnoreCase))
                            Debugger.Break();
                        // Skip planes 15 and 16 private use
                        if (codepoint != 0xF0000 && codepoint != 0x100000 && codepoint < 0x20000)
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
            void AddNonCharacter(int codepoint) => char_map.Add(codepoint, new CharacterRecord(codepoint, $"<NOT A CHARACTER-{codepoint:X4}>", "", false));
            // 2 last characters of each plane
            for (int plane = 0; plane <= 16; plane++)
            {
                AddNonCharacter((plane << 16) + 0xFFFE);
                AddNonCharacter((plane << 16) + 0xFFFF);
            }
            // FDD0..FDEF: 16 non-characters in Arabic Presentation Forms-A
            for (int code = 0xFDD0; code <= 0xFDEF; code++)
                AddNonCharacter(code);


            // Add BlockBegin info to each character, done separately for efficiency
            foreach (var br in block_map.Values)
                for (int ch = br.Begin; ch <= br.End; ch++)
                    if (char_map.ContainsKey(ch))
                        char_map[ch].BlockBegin = br.Begin;

            // Read age
            using (var sr = new StreamReader(GetResourceStream("DerivedAge.txt")))
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
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
                        int end_char_code = int.Parse(fields[0].Substring(p + 2), NumberStyles.HexNumber);
                        // Skip planes 15 and 16 private use
                        if (codepoint != 0xF0000 && codepoint != 0x100000)
                            for (int code = codepoint; code <= end_char_code; code++)
                                if (char_map.ContainsKey(code))
                                    char_map[code].Age = age;
                    }
                }

            // Read NamesList
            string subheader = null;
            // Optimizations: Do not use Regex, looks costly to performance profiler
            //Regex CodepointRegex = new Regex(@"^[0-9A-F]{4,6}\t");

            // Subheaders merging
            HashSet<int> blockCodepoints = null;
            HashSet<string> blockSubheaders = null;

            void MergeSubheaders()
            {
                foreach (string sungularsh in blockSubheaders.Where(s => !s.EndsWith("s", StringComparison.Ordinal)))
                    if (blockSubheaders.Contains(sungularsh+"s"))
                        foreach (int cp in blockCodepoints)
                            if (char_map[cp].Subheader == sungularsh)
                                char_map[cp].Subheader += "s";
                blockCodepoints = null;
                blockSubheaders = null;
            }

            using (var sr = new StreamReader(GetResourceStream("NamesList.txt")))
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
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
                        subheader = null;
                    }
                    else if (line.StartsWith("@@", StringComparison.Ordinal)) { }
                    else if (line.StartsWith("@+", StringComparison.Ordinal)) { }
                    else if (line.StartsWith("@~", StringComparison.Ordinal)) { }
                    else if (line.StartsWith("@", StringComparison.Ordinal))
                    {
                        subheader = line.Substring(3);
                    }
                    else if (line.StartsWith("\t", StringComparison.Ordinal)) { }
                    else
                    {
                        int cp16 = 0;
                        for (int p = 0; ; p++)
                        {
                            char c = line[p];

                            if (c >= '0' && c <= '9')
                                cp16 = 16 * cp16 + ((int)(c)) - 48;
                            else if (c >= 'A' && c <= 'F')
                                cp16 = 16 * cp16 + ((int)(c)) - 65 + 10;
                            else
                            {
                                if (c != '\t') Debugger.Break();
                                break;
                            }
                        }

                        //Match ma = CodepointRegex.Match(line);
                        //if (!ma.Success) Debugger.Break();
                        //int cp = int.Parse(line.Substring(0, ma.Length - 1), NumberStyles.HexNumber);
                        //if (cp != cp16) Debugger.Break();

                        if (char_map.ContainsKey(cp16))
                        {
                            blockCodepoints.Add(cp16);
                            blockSubheaders.Add(subheader);
                            char_map[cp16].Subheader = subheader;
                        }
                    }
                }
            if (blockCodepoints != null)
                MergeSubheaders();

        }

        // Returns stream from embedded resource name
        private static Stream GetResourceStream(string name)
        {
            name = "." + name;
            var assembly = typeof(UniData).GetTypeInfo().Assembly;
            var qualifiedName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase));
            if (qualifiedName == null)
                return null;
            else
                return assembly.GetManifestResourceStream(qualifiedName);
        }


        // Converts a codepoint to an UTF-16 encoded string (.Net string)
        public static string CodepointToString(int cp) => cp <= 0xD7FF || (cp >= 0xE000 && cp <= 0xFFFF) ? new string((char)cp, 1) : new string((char)(0xD800 + ((cp - 0x10000) >> 10)), 1) + new string((char)(0xDC00 + (cp & 0x3ff)), 1);


        // Returns number of Unicode characters in a (valid) UTF-16 encoded string
        public static int UnicodeLength(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            int l = 0;
            bool surrogate = false;
            foreach (char c in str)
            {
                if (surrogate)
                    surrogate = false;
                else
                {
                    l++;
                    surrogate = ((int)c >= 0xD800 && (int)c <= 0xDBFF);
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
                int cp = (int)str[i];
                if (cp >= 0xD800 && cp <= 0xDBFF)
                {
                    i += 1;
                    cp = 0x10000 + ((cp & 0x3ff) << 10) + ((int)str[i] & 0x3ff);
                }
                var cr = UniData.CharacterRecords[cp];
                yield return cr;
            }
        }
    }
}