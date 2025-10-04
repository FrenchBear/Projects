// BlockRecord.cs
// Stores information about a Unicode block
// Note that the hierarchy Level1, Level2, Level3 is coming from file Metablocks.txt that doesn't come from Unicode site but I manage manually
// Rank value is useful for UniSearch, but not here: we just use blocks for filtering scripts/symbols, Latin+Greek/All scripts
//
// 2023-08-17   PV      Copied from UniSearch

namespace UniDataNS;

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
    //public int Rank { get; internal set; }

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
