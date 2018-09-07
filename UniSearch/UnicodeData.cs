// Class UnicodeData.cs
// Reads and stores Unicode UCD file UnicodeData.txt (currently use version from Unicode 11)
// Provides official name and category for each valid codepoint. Not restricted to BMP.
//
// 2018-08-30   PV


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;


namespace UniViewNS
{

    public class CharacterRecord
    {
        public int Codepoint;
        public string Name;
        public string Category;
    }

    public static class UnicodeData
    {
        // Starts with an initial size of 64K
        static readonly Dictionary<int, CharacterRecord> charname_map = new Dictionary<int, CharacterRecord>(65536);

        static UnicodeData()
        {
            string[] unicodedata = File.ReadAllLines("UnicodeData.txt", Encoding.UTF8);
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
                        charname_map.Add(code_in_range, new CharacterRecord { Codepoint=code_in_range, Name = $"{char_name}-{code_in_range:X}", Category = char_category });
                }
                else
                    charname_map.Add(codepoint, new CharacterRecord { Codepoint=codepoint, Name = char_name, Category = char_category });
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
