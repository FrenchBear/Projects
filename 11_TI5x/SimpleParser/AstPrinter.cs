// AstPrinter
// Example of Ast use after build and post-processing: print a clean listing of instructions
// reformatted and with instruction addresses and opCodes
//
// 2025-11-12   PV

using System;
using System.Linq;
using static SimpleParser.MyTi58VisitorBaseColorize;

namespace SimpleParser;

public partial class MyTi58VisitorBaseAst
{
    internal void PrintFormattedAst(bool OnlyLabelsAndComments = false)
    {
        // Number of max opcodes per line in reformatted listing
        const int OpCols = 6;

        Console.WriteLine();
        int cp = 0;

        foreach (AstStatementBase sta in Program.Statements)
        {
            switch (sta)
            {
                // Should do a better formatting, if comment is at the end of a source line, it should
                // be printed at the correct location, not on following line...
                case AstComment(var astTokens):
                    Colorize(astTokens[0]);
                    Console.WriteLine();
                    break;

                case AstInterStatementWhiteSpace(_):
                    //Console.WriteLine("Inter-statement WhiteSpace");
                    break;

                case AstNumber(var astTokens, var opCodes) num:
                    int skip = 0;
                    if (!OnlyLabelsAndComments)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{cp:D3}: ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"{string.Join(" ", opCodes.Take(OpCols).Select(b => b.ToString("D2"))),-3 * OpCols}   ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(num.GetMnemonic());

                        while (opCodes.Count - skip > OpCols)
                        {
                            cp += OpCols;
                            skip += OpCols;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write($"{cp:D3}: ");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"{string.Join(" ", opCodes.Skip(skip).Take(OpCols).Select(b => b.ToString("D2"))),-3 * OpCols} ");
                        }
                    }

                    cp += opCodes.Count - skip;
                    break;

                case AstInstruction(var astTokens, var opCodes, _) inst:
                    bool doPrint = !OnlyLabelsAndComments || (opCodes[0] == 76);
                    if (doPrint)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{cp:D3}: ");
                        Console.ForegroundColor = ConsoleColor.Gray;

                        Console.Write($"{string.Join(" ", opCodes.Take(OpCols).Select(b => b.ToString("D2"))),-3 * OpCols} ");
                        if (astTokens[0].Text.StartsWith("LBL", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("■ ");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else
                            Console.Write("  ");

                        int len = 0;
                        foreach (AstToken token in astTokens)
                            if (token.Cat != SyntaxCategory.WhiteSpace)
                            {
                                //len += 3 + token.Text.Length;
                                //Colorize("‹" + token.Text + "› ", token.Cat);
                                len += 1 + token.Text.Length;
                                Colorize(token.Text, token.Cat);
                                Console.Write(" ");
                            }
                        //Console.Write($"{new string(' ', 25 - len)} {inst.GetMnemonic()}");
                        Console.WriteLine();

                        while (opCodes.Count > OpCols)
                        {
                            cp += OpCols;
                            opCodes.RemoveRange(0, OpCols);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write($"{cp:D3}: ");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"{string.Join(" ", opCodes.Take(OpCols).Select(b => b.ToString("D2"))),-3 * OpCols} ");
                        }
                    }

                    cp += opCodes.Count;
                    break;
            }
        }
    }
}
