// Class UnicodeData.cs
// Reads and stores Unicode UCD file UnicodeData.txt (currently use version from Unicode 11)
// Provides official name and category for each valid codepoint. Not restricted to BMP.
//
// 2018-08-30   PV


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;


namespace UniData
{
    public class CharacterRecord
    {
        public int Codepoint { get; private set; }
        public string Name { get; private set; }
        public string Category { get; private set; }

        public BlockRecord BlockRecord => UnicodeData.BlockRecords.Values.FirstOrDefault(br => Codepoint >= br.Begin && Codepoint <= br.End);

        public CharacterRecord(int Codepoint, string Name, string Category)
        {
            this.Codepoint = Codepoint;
            this.Name = Name;
            this.Category = Category;
        }

        public override string ToString() => $"CR(CP=${Codepoint:X4}, Name={Name}, GC={Category})";
    }

    public class BlockRecord
    {
        public int Begin { get; private set; }
        public int End { get; private set; }
        public string BlockName { get; private set; }
        public string Level1Name { get; private set; }
        public string Level2Name { get; private set; }
        public string Level3Name { get; private set; }


        public BlockRecord(int Begin, int End, string BlockName, string Level1Name, string Level2Name, string Level3Name)
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
            return $"BR(Block={BlockName}, L1={Level1Name}, L2={Level2Name}, L3={Level3Name}";
        }
    }


    public static class UnicodeData
    {
        private static readonly Dictionary<int, CharacterRecord> charname_map = new Dictionary<int, CharacterRecord>(65536);
        private static readonly Dictionary<int, BlockRecord> block_map = new Dictionary<int, BlockRecord>();

        public static ReadOnlyDictionary<int, CharacterRecord> CharacterRecords => new ReadOnlyDictionary<int, CharacterRecord>(charname_map);


        public static ReadOnlyDictionary<int, BlockRecord> BlockRecords => new ReadOnlyDictionary<int, BlockRecord>(block_map);


        static UnicodeData()
        {
            // First read blocks
            //string[] blocksdata = File.ReadAllLines("UCD/UnicodeData.txt", Encoding.UTF8);
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


            string[] unicodedata = File.ReadAllLines("UCD/UnicodeData.txt", Encoding.UTF8);
            for (int i = 0; i < unicodedata.Length; i++)
            {
                string[] fields = unicodedata[i].Split(';');
                int codepoint = int.Parse(fields[0], NumberStyles.HexNumber);
                string char_name = fields[1];
                string char_category = fields[2];
                if (codepoint < 32)
                    char_name = "CONTROL-" + fields[10];
                bool is_range = char_name.EndsWith(", First>", StringComparison.InvariantCultureIgnoreCase);
                if (is_range) //add all characters within a specified range
                {
                    char_name = char_name.Replace(", First>", String.Empty).Replace("<", string.Empty).ToUpperInvariant(); //remove range indicator from name
                    fields = unicodedata[++i].Split(';');
                    int end_char_code = int.Parse(fields[0], NumberStyles.HexNumber);
                    if (!fields[1].EndsWith(", Last>", StringComparison.InvariantCultureIgnoreCase))
                        throw new Exception("Expected end-of-range indicator.");
                    for (int code_in_range = codepoint; code_in_range <= end_char_code; code_in_range++)
                        charname_map.Add(code_in_range, new CharacterRecord(code_in_range, $"{char_name}-{code_in_range:X4}", char_category));
                }
                else
                    charname_map.Add(codepoint, new CharacterRecord(codepoint, char_name, char_category));
            }
        }

        public static int GetCPFromName(string name)
        {
            CharacterRecord cr = charname_map.Values.FirstOrDefault(c => string.Compare(c.Name, name, true) == 0);
            if (cr == null)
                return -1;
            return cr.Codepoint;
        }

        public static string CPtoString(int cp) => cp <= 0xD7FF || (cp >= 0xE000 && cp <= 0xFFFF) ? new string((char)cp, 1) : new string((char)(0xD800 + ((cp - 0x10000) >> 10)), 1) + new string((char)(0xDC00 + (cp & 0x3ff)), 1);


        public static string GetName(int cp)
        {
            if (charname_map.ContainsKey(cp))
                return charname_map[cp].Name;
            else
                return "Unknown character";
        }


        public static string GetCategory(int cp)
        {
            if (charname_map.ContainsKey(cp))
                return charname_map[cp].Category;
            else
                return "??";
        }

        public static bool IsValidCodepoint(int cp) => charname_map.ContainsKey(cp);
    }
}
