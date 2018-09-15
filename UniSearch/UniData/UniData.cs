// UniData
// Static class providing characters and blocks information reading Unicode UCD files
//
// 2018-09-11   PV
//
// ToDo: Manage General Category.  Maybe script? PropertyValueAliases.txt and Scripts.txt
// ToDo: Add unit tests



using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace UniData
{
    public class CharacterRecord
    {
        public int Codepoint { get; private set; }
        public string Name { get; private set; }
        public string Category { get; private set; }
        public string Age { get; internal set; }
        public int Block { get; internal set; } = -1;       // -1 for tests


        // When True, Character method will return an hex codepoint representation instead of the actual string
        private readonly bool IsPrintable;

        // Convert to a C# string representation of the character, except for control characters
        // 'Safe version' of UnicodeData.CPtoString
        public string Character => IsPrintable ? UnicodeData.CPtoString(Codepoint) : CodepointHexa;


        // Used by binding
        public BlockRecord BlockRecord => UnicodeData.BlockRecords[Block];
        public CategoryRecord CategoryRecord => UnicodeData.CategoryRecords[Category];

        public string CodepointHexa => $"U+{Codepoint:X4}";


        internal CharacterRecord(int Codepoint, string Name, string Category, bool IsPrintable)
        {
            this.Codepoint = Codepoint;
            this.Name = Name;
            this.Category = Category;
            this.IsPrintable = IsPrintable;
        }

        public override string ToString() => $"CharacterRecord(CP=U+{Codepoint:X4}, Name={Name}, Category={Category}, IsPrintable={IsPrintable})";
    }


    public class BlockRecord
    {
        public int Begin { get; private set; }
        public int End { get; private set; }
        public string BlockName { get; private set; }
        public string Level1Name { get; private set; }
        public string Level2Name { get; private set; }
        public string Level3Name { get; private set; }

        public string BlockNameRange => $"{BlockName} {Begin:X4}..{End:X4}";


        internal BlockRecord(int Begin, int End, string BlockName, string Level1Name, string Level2Name, string Level3Name)
        {
            this.Begin = Begin;
            this.End = End;
            this.BlockName = BlockName;
            this.Level1Name = Level1Name;
            this.Level2Name = Level2Name;
            this.Level3Name = Level3Name;
        }

        public override string ToString()
        {
            return $"BlockRecord(Range={Begin:X4}..{End:X4}, Block={BlockName}, L1={Level1Name}, L2={Level2Name}, L3={Level3Name})";
        }
    }

    public class CategoryRecord
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string Include { get; private set; }

        public List<string> CategoriesList { get; private set; }

        public string Categories => CategoriesList.Aggregate((prev, c) => prev + ", " + c);


        public CategoryRecord(string code, string name, string include = "")
        {
            this.Code = code;
            this.Name = name;
            this.Include = include;
            CategoriesList = new List<string> { code };
        }

        public override string ToString() => $"CategoryRecord(Code={Code}, Name={Name}, Include={Include})";
    }


    public static class UnicodeData
    {
        // Real internal dictionaries used to store Unicode data
        private static readonly Dictionary<int, CharacterRecord> char_map = new Dictionary<int, CharacterRecord>(65536);
        private static readonly Dictionary<string, CategoryRecord> cat_map = new Dictionary<string, CategoryRecord>();
        private static readonly Dictionary<int, BlockRecord> block_map = new Dictionary<int, BlockRecord>();


        public static ReadOnlyDictionary<int, CharacterRecord> CharacterRecords => new ReadOnlyDictionary<int, CharacterRecord>(char_map);

        public static ReadOnlyDictionary<int, BlockRecord> BlockRecords => new ReadOnlyDictionary<int, BlockRecord>(block_map);

        public static ReadOnlyDictionary<string, CategoryRecord> CategoryRecords => new ReadOnlyDictionary<string, CategoryRecord>(cat_map);


        static UnicodeData()
        {
            // Read blocks
            using (var tr = new StreamReader("UCD/MetaBlocks.txt", Encoding.UTF8))
                while (!tr.EndOfStream)
                {
                    string line = tr.ReadLine();
                    if (line.Length == 0 || line[0] == '#') continue;
                    string[] fields = line.Split(';');
                    string[] field0 = fields[0].Replace("..", ";").Split(';');
                    int begin = int.Parse(field0[0], NumberStyles.HexNumber);
                    int end = int.Parse(field0[1], NumberStyles.HexNumber);
                    BlockRecord br = new BlockRecord(begin, end, fields[1], fields[2], fields[3], fields[4]);
                    block_map.Add(begin, br);
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
            foreach (CategoryRecord cr in cat_map.Values)
                foreach (string other in cr.Include.Split('|'))
                    cat_map[other].CategoriesList.Add(cr.Code);


            // Read characters
            string[] unicodedata = File.ReadAllLines("UCD/UnicodeData.txt", Encoding.UTF8);
            for (int i = 0; i < unicodedata.Length; i++)
            {
                string[] fields = unicodedata[i].Split(';');
                int codepoint = int.Parse(fields[0], NumberStyles.HexNumber);
                string char_name = fields[1];
                string char_category = fields[2];
                if (char_name == "<control>")
                    char_name = "CONTROL-" + fields[10];
                bool is_range = char_name.EndsWith(", First>", StringComparison.InvariantCultureIgnoreCase);
                bool is_printable = !(codepoint < 32 || codepoint >= 0x7f && codepoint < 0xA0 || codepoint >= 0xD800 && codepoint <= 0xDFFF);
                if (is_range)   // Add all characters within a specified range
                {
                    char_name = char_name.Replace(", First>", String.Empty).Replace("<", string.Empty).ToUpperInvariant(); //remove range indicator from name
                    fields = unicodedata[++i].Split(';');
                    int end_char_code = int.Parse(fields[0], NumberStyles.HexNumber);
                    if (!fields[1].EndsWith(", Last>", StringComparison.InvariantCultureIgnoreCase))
                        throw new Exception("Expected end-of-range indicator.");
                    for (int code = codepoint; code <= end_char_code; code++)
                        char_map.Add(code, new CharacterRecord(code, $"{char_name}-{code:X4}", char_category, is_printable));
                }
                else
                {
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
                        char_map[ch].Block = br.Begin;

            // Read age
            using (var tr = new StreamReader("UCD/DerivedAge.txt", Encoding.UTF8))
                while (!tr.EndOfStream)
                {
                    string line = tr.ReadLine();
                    if (line.Length == 0 || line[0] == '#') continue;
                    int p = line.IndexOf('#');
                    if (p >= 0) line = line.Substring(0, p - 1);
                    string[] fields = line.Split(';');
                    p = fields[0].IndexOf('.');
                    int codepoint = int.Parse(p < 0 ? fields[0] : fields[0].Substring(0, p), NumberStyles.HexNumber);
                    string age = fields[1].Trim();
                    if (p < 0)
                        char_map[codepoint].Age = age;
                    else
                    {
                        int end_char_code = int.Parse(fields[0].Substring(p + 2), NumberStyles.HexNumber);
                        for (int code = codepoint; code <= end_char_code; code++)
                            char_map[code].Age = age;
                    }
                }


            // For development
            //InternalTests();
        }

        private static void InternalTests()
        {
            foreach (var cr in char_map.Values)
            {
                // Check that all characters are assigned to a valid block
                Debug.Assert(BlockRecords.ContainsKey(cr.Block));
                // Check that all characters are assigned to a valid category
                Debug.Assert(CategoryRecords.ContainsKey(cr.Category));

            }
        }


        public static int GetCPFromName(string name)
        {
            CharacterRecord cr = char_map.Values.FirstOrDefault(c => string.Compare(c.Name, name, true) == 0);
            if (cr == null)
                return -1;
            return cr.Codepoint;
        }

        public static string CPtoString(int cp) => cp <= 0xD7FF || (cp >= 0xE000 && cp <= 0xFFFF) ? new string((char)cp, 1) : new string((char)(0xD800 + ((cp - 0x10000) >> 10)), 1) + new string((char)(0xDC00 + (cp & 0x3ff)), 1);


        public static string GetName(int cp)
        {
            if (char_map.ContainsKey(cp))
                return char_map[cp].Name;
            else
                return "Unknown character";
        }


        public static string GetCategory(int cp)
        {
            if (char_map.ContainsKey(cp))
                return char_map[cp].Category;
            else
                return "??";
        }

        public static bool IsValidCodepoint(int cp) => char_map.ContainsKey(cp);


        // Test
        //Debug.Assert(UnicodeData.CharacterLength("Aé♫𝄞🐗") == 5);

        // Returns number of Unicode characters in a (valid) UTF-16 encoded string
        public static int CharacterLength(string s)
        {
            int l = 0;
            bool surrogate = false;
            foreach (char c in s)
            {
                if (surrogate)
                    surrogate = false;
                else
                {
                    l++;
                    if ((int)c >= 0xD800 && (int)c <= 0xDBFF)
                        surrogate = true;
                }
            }
            return l;
        }
    }
}
