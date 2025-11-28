// Representation of a T95 program
//
// 2025-11-25   PV

// ToDo: Use T59Error class instead of a string, and replace L2Statement Problem flag by a reference to T59Error

using System;
using System.Linq;
using System.Collections.Generic;

namespace T59v5;

internal sealed class T59Program
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

    public void PrintOriginalColorized()
    {
        foreach (var l1t in L1Tokens)
        {
            if (l1t is not L1WS)
                Console.Write(Couleurs.GetCategoryColor(l1t.Parent is L2InvalidStatement ? SyntaxCategory.Invalid : l1t.Cat));
            foreach (var l0t in l1t.L0Tokens)
                Console.Write(l0t.Text);
            if (l1t is not L1WS)
                Console.Write(Couleurs.GetDefaultColor());
        }
        Console.WriteLine();
    }

    public void PrintL3Reformatted()
    {
        // Number of max opcodes per line in reformatted listing
        const int opCols = 6;

        static string FormattedOpCode(byte opCode) => opCode == 100 ? "??" : opCode.ToString("D2");

        foreach (L2StatementBase l2s in L2Statements)
        {
            switch (l2s)
            {
                case L2LineComment or L2InvalidStatement or L2Tag:
                    Console.Write(new string(' ', 3 * opCols + 8));
                    if (l2s is L2InvalidStatement)
                        Console.Write("  ");
                    Console.WriteLine(l2s.AsFormattedString());
                    break;

                case L2AddressableInstructionBase l2i:      // L2Tag is L2AddressableInstructionBase, but don't need to print address or opCodes
                    int skip = 0;
                    int cp = l2i.Address;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{cp:D3}: ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{string.Join(" ", l2i.OpCodes.Take(opCols).Select(FormattedOpCode)),-3 * opCols}   ");

                    if (l2i.OpCodes[0] == 76)
                    {
                        Console.Write(Couleurs.GetCategoryColor(l2i.Message != null ? SyntaxCategory.Invalid : SyntaxCategory.Label));
                        Console.Write("■ ");
                        Console.Write(Couleurs.GetDefaultColor());
                    }
                    else
                    {
                        if (l2i.Message != null)
                        {
                            Console.Write(Couleurs.GetCategoryColor(SyntaxCategory.Invalid));
                            Console.Write("? ");
                            Console.Write(Couleurs.GetDefaultColor());
                        }
                        else
                        {
                            if (ValidAddresses.TryGetValue(l2i.Address, out bool value) && value)
                                Console.Write("› ");
                            else
                                Console.Write("  ");
                        }
                    }

                    Console.WriteLine(l2i.AsFormattedString());

                    while (l2i.OpCodes.Count - skip > opCols)
                    {
                        cp += opCols;
                        skip += opCols;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{cp:D3}: ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"{string.Join(" ", l2i.OpCodes.Skip(skip).Take(opCols).Select(FormattedOpCode)),-3 * opCols} ");
                    }
                    break;
            }
        }
    }

    public void PrintLabels()
    {
        bool isHeaderPrinted = false;
        foreach (L2StatementBase l2s in L2Statements)
            if (l2s is L2AddressableInstructionBase l2a and (L2Tag or L2Instruction { OpCodes: [76, _] }))
            {
                if (!isHeaderPrinted)
                {
                    isHeaderPrinted = true;
                    Console.WriteLine("\nLabels and Tags:");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{l2a.Address:D3}: ");
                //Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(l2a.AsFormattedString());
            }
    }

    public void PrintErrors()
    {
        if (Messages.Count > 0)
        {
            Console.WriteLine("\nErrors/Warnings:");
            foreach (var msg in Messages)
                Console.WriteLine(msg.Message);
        }
    }
}
