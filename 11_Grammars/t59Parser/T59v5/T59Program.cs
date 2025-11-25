// Representation of a T9 program
//
// 2025-11-25   PV

using System;
using System.Linq;
using System.Collections.Generic;

namespace T59v5;

internal sealed class T59Program
{
    public List<L1Token> L1Tokens = [];
    public List<L2StatementBase> L2Statements = [];
    public int OpCodesCount;

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

    public void PrintL3Reformatted(bool onlyLabelsAndComments = false)
    {
        // Number of max opcodes per line in reformatted listing
        const int opCols = 6;

        foreach (L2StatementBase l2s in L2Statements)
        {
            switch (l2s)
            {
                case L2LineComment l2lc:
                    Console.WriteLine(l2lc.AsFormattedString());
                    break;

                case L2InvalidStatement l2is:
                    Console.Write(new string(' ', 3 * opCols + 10));
                    Console.WriteLine(l2is.AsFormattedString());
                    break;

                case L2ActualInstruction l2i:
                    int skip = 0;
                    if (!onlyLabelsAndComments)
                    {
                        int cp = l2i.Address;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{cp:D3}: ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"{string.Join(" ", l2i.OpCodes.Take(opCols).Select(b => b.ToString("D2"))),-3 * opCols}   ");

                        if (l2i.OpCodes[0] == 76)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("■ ");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else
                            Console.Write("  ");

                        Console.WriteLine(l2i.AsFormattedString());

                        while (l2i.OpCodes.Count - skip > opCols)
                        {
                            cp += opCols;
                            skip += opCols;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write($"{cp:D3}: ");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"{string.Join(" ", l2i.OpCodes.Skip(skip).Take(opCols).Select(b => b.ToString("D2"))),-3 * opCols} ");
                        }
                    }
                    break;
            }
        }
    }
}
