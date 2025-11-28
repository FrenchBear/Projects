// Representation of a T95 program
//
// 2025-11-25   PV

// ToDo: Use T59Error class instead of a string, and replace L2Statement Problem flag by a reference to T59Error

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace T59v6Core;

public sealed class T59Program
{
    public List<L1Token> L1Tokens = [];
    public List<L2StatementBase> L2Statements = [];
    public int OpCodesCount;
    public Dictionary<int, bool> ValidAddresses = [];
    public List<T59Message> Messages = [];

    public IEnumerable<L1Token> L1TokensWithoutWhiteSpace()
    {
        foreach (var t1 in L1Tokens)
            if (t1 is not L1WS)
                yield return t1;
    }

    public void PrintL1Debug()
    {
        foreach (L1Token t1 in L1Tokens)
            Console.WriteLine(t1.AsDebugString());
    }

    public void PrintL2Debug()
    {
        foreach (L2StatementBase l2s in L2Statements)
            Console.WriteLine(l2s.AsDebugString());
    }

    public void PrintL3Debug()
    {
        foreach (L2StatementBase l2s in L2Statements)
            Console.WriteLine(l2s.AsDebugStringWithOpcodes());
    }

    public string OriginalColorizedTagged()
    {
        var sb = new StringBuilder();
        foreach (var l1t in L1Tokens)
            if (l1t is not L1Eof)
            {
                if (l1t is not L1WS)
                    sb.Append(Categories.GetCategoryOpenTag(l1t.Parent is L2InvalidStatement ? SyntaxCategory.Invalid : l1t.Cat));
                foreach (var l0t in l1t.L0Tokens)
                    sb.Append(l0t.Text);
                if (l1t is not L1WS)
                    sb.Append(Categories.GetCategoryCloseTag(l1t.Parent is L2InvalidStatement ? SyntaxCategory.Invalid : l1t.Cat));
            }
        sb.AppendLine();
        return sb.ToString();
    }

    public string L3ReformattedTagged()
    {
        // Number of max opcodes per line in reformatted listing
        const int opCols = 6;

        static string FormattedOpCode(byte opCode) => Categories.GetTaggedText(opCode == 100 ? "??" : opCode.ToString("D2"), SyntaxCategory.OpCode);

        var sb = new StringBuilder();
        foreach (L2StatementBase l2s in L2Statements)
        {
            switch (l2s)
            {
                case L2LineComment or L2InvalidStatement or L2Tag:
                    sb.Append(new string(' ', 3 * opCols + 7));
                    if (l2s is L2InvalidStatement)
                        sb.Append("  ");
                    sb.AppendLine(l2s.AsFormattedString());
                    break;

                case L2AddressableInstructionBase l2i:      // L2Tag is L2AddressableInstructionBase, but don't need to print address or opCodes
                    int skip = 0;
                    int cp = l2i.Address;
                    sb.Append(Categories.GetTaggedText($"{cp:D3}: ", SyntaxCategory.LineNumber));
                    sb.Append(string.Join(" ", l2i.OpCodes.Take(opCols).Select(FormattedOpCode)));
                    sb.Append(' ', 3 + 3 * (opCols - Math.Min(l2i.OpCodes.Count, opCols)));

                    if (l2i.OpCodes[0] == 76)
                        sb.Append(Categories.GetTaggedText("■ ", l2i.Message != null ? SyntaxCategory.Invalid : SyntaxCategory.Label));
                    else
                    {
                        if (l2i.Message != null)
                            sb.Append(Categories.GetTaggedText("? ", SyntaxCategory.Invalid));
                        else
                            sb.Append(ValidAddresses.TryGetValue(l2i.Address, out bool value) && value ? "› " : "  ");
                    }

                    sb.AppendLine(l2i.AsFormattedString());

                    while (l2i.OpCodes.Count - skip > opCols)
                    {
                        cp += opCols;
                        skip += opCols;
                        sb.Append(Categories.GetTaggedText($"{cp:D3}: ", SyntaxCategory.LineNumber));
                        sb.AppendLine(string.Join(" ", l2i.OpCodes.Skip(skip).Take(opCols).Select(FormattedOpCode)));
                    }
                    break;
            }
        }

        return sb.ToString();
    }

    public string LabelsTagged()
    {
        var sb = new StringBuilder();
        foreach (L2StatementBase l2s in L2Statements)
            if (l2s is L2AddressableInstructionBase l2a and (L2Tag or L2Instruction { OpCodes: [76, _] }))
            {
                sb.Append(Categories.GetTaggedText($"{l2a.Address:D3}: ", SyntaxCategory.LineNumber));
                sb.AppendLine(l2a.AsFormattedString());
            }
        return sb.ToString();
    }

    public string ErrorsTagged()
    {
        var sb = new StringBuilder();
        foreach (var msg in Messages)
            sb.AppendLine(msg.Message);
        return sb.ToString();
    }
}
