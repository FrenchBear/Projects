// UniData
// Static class providing characters and blocks information reading Unicode UCD files
//
// 2018-09-11   PV
//
// ToDo: Manage General Category.  Maybe script? PropertyValueAliases.txt and Scripts.txt
// ToDo: Add unit tests

/*
# General_Category (gc)

gc ; C                                ; Other                            # Cc | Cf | Cn | Co | Cs
gc ; Cc                               ; Control                          ; cntrl
gc ; Cf                               ; Format
gc ; Cn                               ; Unassigned
gc ; Co                               ; Private_Use
gc ; Cs                               ; Surrogate
gc ; L                                ; Letter                           # Ll | Lm | Lo | Lt | Lu
gc ; LC                               ; Cased_Letter                     # Ll | Lt | Lu
gc ; Ll                               ; Lowercase_Letter
gc ; Lm                               ; Modifier_Letter
gc ; Lo                               ; Other_Letter
gc ; Lt                               ; Titlecase_Letter
gc ; Lu                               ; Uppercase_Letter
gc ; M                                ; Mark                             ; Combining_Mark                   # Mc | Me | Mn
gc ; Mc                               ; Spacing_Mark
gc ; Me                               ; Enclosing_Mark
gc ; Mn                               ; Nonspacing_Mark
gc ; N                                ; Number                           # Nd | Nl | No
gc ; Nd                               ; Decimal_Number                   ; digit
gc ; Nl                               ; Letter_Number
gc ; No                               ; Other_Number
gc ; P                                ; Punctuation                      ; punct                            # Pc | Pd | Pe | Pf | Pi | Po | Ps
gc ; Pc                               ; Connector_Punctuation
gc ; Pd                               ; Dash_Punctuation
gc ; Pe                               ; Close_Punctuation
gc ; Pf                               ; Final_Punctuation
gc ; Pi                               ; Initial_Punctuation
gc ; Po                               ; Other_Punctuation
gc ; Ps                               ; Open_Punctuation
gc ; S                                ; Symbol                           # Sc | Sk | Sm | So
gc ; Sc                               ; Currency_Symbol
gc ; Sk                               ; Modifier_Symbol
gc ; Sm                               ; Math_Symbol
gc ; So                               ; Other_Symbol
gc ; Z                                ; Separator                        # Zl | Zp | Zs
gc ; Zl                               ; Line_Separator
gc ; Zp                               ; Paragraph_Separator
gc ; Zs                               ; Space_Separator
# @missing: 0000..10FFFF; General_Category; Unassigned
*/



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
        public int BlockBegin { get; internal set; }

        // When True, Character method will return an hex codepoint representation instead of the actual string
        private readonly bool IsPrintable;

        // Convert to a C# string representation of the character, except for control characters
        // 'Safe version' of UnicodeData.CPtoString
        public string Character => IsPrintable ? UnicodeData.CPtoString(Codepoint) : CodepointHexa;

        // Used by binding
        public BlockRecord Block => UnicodeData.BlockRecords[BlockBegin];

        public string CodepointHexa => $"U+${Codepoint:X4}";


        internal CharacterRecord(int Codepoint, string Name, string Category, bool IsPrintable)
        {
            this.Codepoint = Codepoint;
            this.Name = Name;
            this.Category = Category;
            this.IsPrintable = IsPrintable;
        }

        public override string ToString() => $"CharacterRecord(CP=${Codepoint:X4}, Name={Name}, GC={Category})";
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


    public static class UnicodeData
    {
        // Real internal dictionaries used to store unicode data
        private static readonly Dictionary<int, CharacterRecord> char_map = new Dictionary<int, CharacterRecord>(65536);
        private static readonly Dictionary<int, BlockRecord> block_map = new Dictionary<int, BlockRecord>();


        public static ReadOnlyDictionary<int, CharacterRecord> CharacterRecords => new ReadOnlyDictionary<int, CharacterRecord>(char_map);

        public static ReadOnlyDictionary<int, BlockRecord> BlockRecords => new ReadOnlyDictionary<int, BlockRecord>(block_map);


        static UnicodeData()
        {
            // First read blocks
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


            /*
            // Efficient search of the block a character belongs to using binary search
            var SortedBlocksArray = block_map.Values.OrderBy(br => br.Begin).ToArray();

            // Returns first codepoint of the found block, or -1 if no matching block is found (should not happen)
            int GetBlockBegin(int Codepoint)
            {
                bool InBlock(int n) => Codepoint >= SortedBlocksArray[n].Begin && Codepoint <= SortedBlocksArray[n].End;

                int lower = 0;
                int upper = SortedBlocksArray.Length - 1;
                for (; ; )
                {
                    if (upper == lower)
                        return InBlock(upper) ? SortedBlocksArray[upper].Begin : -1;
                    else if (upper - lower == 1)
                    {
                        if (InBlock(lower)) return SortedBlocksArray[lower].Begin;
                        if (InBlock(upper)) return SortedBlocksArray[upper].Begin;
                        return -1;
                    }
                    int middle = (lower + upper) / 2;
                    if (InBlock(middle)) return SortedBlocksArray[middle].Begin;
                    if (Codepoint < SortedBlocksArray[middle].Begin)
                        upper = middle - 1;
                    else
                        lower = middle + 1;
                }
            }
            */


            // Read character blocks
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
            void AddNonCharacter(int codepoint) => char_map.Add(codepoint, new CharacterRecord(codepoint, $"<NOT A CHARACTER-{codepoint:X4}>", "?", false));
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


            /*
            // Check that all characters are assigned to a block --> Passed
            foreach (var cr in char_map.Values)
                Debug.Assert(cr.BlockBegin >= 0);
            */
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

        // Returns number of unicode characters in a (valid) unicode string
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
