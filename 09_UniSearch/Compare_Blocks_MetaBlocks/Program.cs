// Compare_Blocks_MetaBlocks
// Tool to compare Blocks.txt from Unicode UCD and MetaBlocks.txt (personal file) when a new version of Unicode is released
//
// 2022-04-27   PV
// 2022-11-18   PV      Net7, Unicode 15
// 2024-09-12   PV      Net8, Unicode 16
// 2025-10-03   PV      Net8, Unicode 17; Added DecodeHierarchy.txt file built by VBA macro from https://unicode.org/charts/

/*
Normal residue for UCD 16
Comparing:
C:\Users\Pierr\OneDrive\DocumentsOD\Doc tech\Unicode\Unicode 16 UCD\Blocks.txt
C:\Development\GitVSTS\DevForFun\07_UniSearch\CS_Net8_U16_J\UniData\UCD\MetaBlocks.txt

Missing new blocks (in Blocks, not in MetaBlocks)
0000..007F;Basic Latin;Level1;Level2;Level3

Blocks with different ranges
0080..00FF      00A0..00FF      Latin-1 Supplement
F0000..FFFFF    F0000..FFFFD    Supplementary Private Use Area-A
100000..10FFFF  100000..10FFFD  Supplementary Private Use Area-B

In DecodeHierarchy, there are duplicate blocks:
Hierarchy dup block: Halfwidth and Fullwidth Forms
Hierarchy dup block: Coptic Epact Numbers
Hierarchy dup block: Cuneiform Numbers and Punctuation
Hierarchy dup block: Sinhala Archaic Numbers
Hierarchy dup block: Mathematical Alphanumeric Symbols
Hierarchy dup block: Arabic Mathematical Alphabetic Symbols
Hierarchy dup block: Letterlike Symbols
Hierarchy dup block: Miscellaneous Symbols and Arrows
Hierarchy dup block: Invisible Operators

By convention, to simplify processing, we only keep the hierarchy of the last version
*/

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using static System.Console;

namespace Compare_Blocks_MetaBlocks;

sealed record BlockData()   //int Begin, int End, string Name, string Level1, string Level2, string Level3);
{
    public int Begin;
    public int End;
    public required string Name;
    public required string Level1;
    public required string Level2;
    public required string Level3;
}

internal sealed class App
{
    const string Blocks_File = @"C:\DocumentsOD\Doc tech\Unicode\Unicode 17\UCD\Blocks.txt";
    const string MetaBlocks_File = @"C:\Development\GitHub\Projects\09_UniSearch\CS_WinUI3_Net9_U17\UniDataWinUI3\UCD\MetaBlocks.txt";
    const string DecodedHierarchy_File = @"C:\DocumentsOD\Doc tech\Unicode\Unicode 17\DecodeHierarchy.txt";

    static readonly Dictionary<string, BlockData> BlocksDic = [];
    static readonly Dictionary<string, BlockData> MetaBlocksDic = [];
    static readonly Dictionary<string, BlockData> HierarchyDic = [];

    static void Main()
    {
        WriteLine("Comparing:");
        WriteLine(Blocks_File);
        WriteLine(MetaBlocks_File);

        ReadBlocks(Blocks_File, BlocksDic);
        ReadBlocks(MetaBlocks_File, MetaBlocksDic);
        ReadDecodedHierarchy(DecodedHierarchy_File, HierarchyDic);

        WriteLine("\nMissing new blocks (in Blocks, not in MetaBlocks)");
        foreach (var kb in BlocksDic.Keys)
            if (!MetaBlocksDic.ContainsKey(kb))
                WriteLine($"{BlocksDic[kb].Begin:X4}..{BlocksDic[kb].End:X4};{kb};Level1;Level2;Level3");

        WriteLine("\nBlocks with different ranges");
        foreach (var b in BlocksDic.Values)
            if (MetaBlocksDic.TryGetValue(b.Name, out BlockData? mb))
                if (b.Begin != mb.Begin || b.End != mb.End)
                    WriteLine($"{b.Begin:X4}..{b.End:X4}\t{mb.Begin:X4}..{mb.End:X4}\t{b.Name}");

        WriteLine("\nMetaBlocks with different hierachy");
        foreach (var b in MetaBlocksDic.Values)
            if (HierarchyDic.TryGetValue(b.Name, out BlockData? h))
            {
                if (h.Level1 != b.Level1)
                    WriteLine($"L1 diff: name={b.Name}  L1.MB={b.Level1}  L1.H={h.Level1}");
            }
            else
            {
                if (!b.Name.StartsWith("Specials End of Plane"))
                    WriteLine($"MetaBlock {b.Name} not found in hierarchy");
            }
    }

    private static void ReadDecodedHierarchy(string file, Dictionary<string, BlockData> dic)
    {
        using var sr = new StreamReader(file);
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine() ?? string.Empty;
            if (line.Length == 0 || line[0] == '#')
                continue;
            string[] fields = line.Split(';');

            var name = fields[0].Trim();
            var l1 = fields[1].Trim();
            var name_original = name;

            //if (name.StartsWith("Combining Diacritical Marks"))
            //    Debugger.Break();

            // Some named are shortened or different on HTML page, update them to match name in Blocks.txt
            if (name.StartsWith("CJK Extension"))
                name = name.Replace("CJK Extension", "CJK Unified Ideographs Extension");
            else if (name == "N'Ko")
                name = "NKo";
            else if (name == "Bengali and Assamese")
                name = "Bengali";
            else if (name == "Oriya (Odia)")
                name = "Oriya";
            else if (name == "UCAS Extended")
                name = "Unified Canadian Aboriginal Syllabics Extended";
            else if (name == "UCAS Extended-A")
                name = "Unified Canadian Aboriginal Syllabics Extended-A";
            else if (name == "Optical Character Recognition (OCR)")
                name = "Optical Character Recognition";
            else if (name == "CJK Radicals / Kangxi Radicals")
                name = "Kangxi Radicals";
            else if (name == "CJK Unified Ideographs (Han)")
                name = "CJK Unified Ideographs";
            else if (name == "Phags-Pa")
                name = "Phags-pa";
            else if (name == "Aramaic, Imperial")
                name = "Imperial Aramaic";
            else if (name == "Parthian, Inscriptional")
                name = "Inscriptional Parthian";
            else if (name == "Pahlavi, Inscriptional")
                name = "Inscriptional Pahlavi";
            else if (name == "Pahlavi, Psalter")
                name = "Psalter Pahlavi";
            else if (name == "Miscellaneous Symbols And Pictographs")
                name = "Miscellaneous Symbols and Pictographs";
            else if (name == "Greek")
                name = "Greek and Coptic";

            // Replicate name updates to L1
            if (name_original == l1
                    && name != "Greek and Coptic"
                    && name != "Kangxi Radicals"
                    && name != "CJK Unified Ideographs"
                    && name != "Arabic Mathematical Alphabetic Symbols")
                l1 = name;

            var bd = new BlockData { Begin = 0, End = 0, Name = name, Level1 = l1, Level2 = fields[2].Trim(), Level3 = fields[3].Trim() };
            // In case of dup names, keep the last one, except "Miscellaneous Symbols and Arrows"
            if (!dic.TryAdd(bd.Name, bd) && bd.Name != "Miscellaneous Symbols and Arrows")
            {
                dic.Remove(bd.Name);
                dic.Add(bd.Name, bd);
            }

            // Add extra names
            if (name == "High Surrogates")
            {
                var bd2 = bd with { Name = "High Private Use Surrogates" };
                dic.Add(bd2.Name, bd2);
            }
            if (name == "Basic Latin (ASCII)")
            {
                var bd2 = new BlockData { Name = "ASCII Controls C0", Level1 = "", Level2 = "Specials", Level3 = "Symbols and Punctuation" };
                dic.Add(bd2.Name, bd2);
                bd2 = new BlockData { Name = "ASCII Controls C1", Level1 = "", Level2 = "Specials", Level3 = "Symbols and Punctuation" };
                dic.Add(bd2.Name, bd2);
            }
        }

        // Remove names that do not exist in Blocks.txt, since we have more than 40 extra (ex: "Dollar Sign, Euro Sign")
        var toDelete = dic.Keys.Where(k => !BlocksDic.ContainsKey(k)).ToList();
        foreach (var k in toDelete)
            dic.Remove(k);

        WriteLine("Hierarchy Normalization");
        // Normalization of hierarchy
        // Each Level1|Level2|Level3 group with a single member, if blockname==Level1, then Level1=""
        var groups = dic.Values.GroupBy(bd => bd.Level1 + "|" + bd.Level2 + "|" + bd.Level3).Where(group => group.Count() == 1).ToList();
        foreach (var zz in groups)
        {
            var key = zz.Key.Split('|')[0];
            if (key == "Kangxi Radicals")
                Debugger.Break();
            if (dic.TryGetValue(key, out BlockData? value) && value.Name == value.Level1)
                value.Level1 = "";
        }
    }

    /// <summary>Read blocks info from MetaBlocks.txt: Block Name, Level 1 Name, Level 2 Name and Level3 Name.  Note that this file does not come from Unicode.org but is manually managed.</summary>
    private static void ReadBlocks(string file, Dictionary<string, BlockData> dic)
    {
        using var sr = new StreamReader(file);
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine() ?? string.Empty;
            if (line.Length == 0 || line[0] == '#')
                continue;
            string[] fields = line.Split(';');
            string[] field0 = fields[0].Replace("..", ";").Split(';');
            int begin = int.Parse(field0[0], NumberStyles.HexNumber);
            int end = int.Parse(field0[1], NumberStyles.HexNumber);

            var name = fields[1].Trim();
            if (name == "Basic Latin")
                name = "Basic Latin (ASCII)";
            if (fields.Length == 2)     // Blocks.txt
            {
                var bd = new BlockData { Begin = begin, End = end, Name = name, Level1 = "", Level2 = "", Level3 = "" };
                dic.Add(bd.Name, bd);
            }
            else if (fields.Length == 5)    // MetaBlocks.txt
            {
                var bd = new BlockData { Begin = begin, End = end, Name = name, Level1 = fields[2].Trim(), Level2 = fields[3].Trim(), Level3 = fields[4].Trim() };
                dic.Add(bd.Name, bd);
            }
            else
                Debugger.Break();
        }
    }
}
