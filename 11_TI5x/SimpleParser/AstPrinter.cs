// AstPrinter
// Example of Ast use after build and post-processing: print a clean listing of instructions
// reformatted and with instruction addresses and opCodes
//
// ToDo: Build a list of labels and a list of statements start instruction to validate GTO/SBR/tests to direct address and labels,
// or duplicate labels, or invalid labels (Lbl Ind/Lbl 40, can't be used, Ind interpretation has priority)
// ToDo: Error detection (ex: two consecutive operators, operator after opening parenthesis, ...)

//
// 2025-11-12   PV

using System;
using System.Linq;
using static SimpleParser.MyTi58VisitorBaseColorize;

#pragma warning disable CA1822 // Mark members as static

namespace SimpleParser;

public class AstPrinter
{
    // Could be made static, but for consistency of main program, better keep this as an instance method
    internal void PrintFormattedAst(AstProgram program, bool onlyLabelsAndComments = false)
    {
        // Number of max opcodes per line in reformatted listing
        const int opCols = 6;

        Console.WriteLine();

        foreach (AstStatementBase sta in program.Statements)
        {
            switch (sta)
            {
                // ToDo: if comment is at the end of a source line, it should be printed at the correct location, not on following line...
                // Probably needs to store move info in AstInstruction/AstNumber, and parser or AstBuilder update
                // Maybe AstBuilder can detect that instruction and comment are on the same line so it can add info to instruction or comment
                // Note that comments on successive numbers may be grouped/merged with first number
                case AstComment(var astTokens):
                    Colorize(astTokens[0]);
                    Console.WriteLine();
                    break;

                //case AstInterStatementWhiteSpace(_):
                //    break;

                case AstNumber(_, var opCodes) num:
                    int skip = 0;
                    if (!onlyLabelsAndComments)
                    {
                        int cp = num.Address;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{cp:D3}: ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"{string.Join(" ", opCodes.Take(opCols).Select(b => b.ToString("D2"))),-3 * opCols}   ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(num.GetMnemonic());

                        while (opCodes.Count - skip > opCols)
                        {
                            cp += opCols;
                            skip += opCols;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write($"{cp:D3}: ");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"{string.Join(" ", opCodes.Skip(skip).Take(opCols).Select(b => b.ToString("D2"))),-3 * opCols} ");
                        }
                    }
                    break;

                case AstInstruction(var astTokens, var opCodes, _) inst:
                    bool doPrint = !onlyLabelsAndComments || (opCodes[0] == 76);
                    if (doPrint)
                    {
                        int cp = inst.Address;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{cp:D3}: ");
                        Console.ForegroundColor = ConsoleColor.Gray;

                        Console.Write($"{string.Join(" ", opCodes.Take(opCols).Select(b => b.ToString("D2"))),-3 * opCols} ");
                        if (astTokens[0].Text.StartsWith("LBL", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("■ ");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else
                            Console.Write("  ");

                        foreach (AstToken token in astTokens)
                            if (token.Cat != SyntaxCategory.WhiteSpace)
                            {
                                Colorize(token.Text, token.Cat);
                                Console.Write(" ");
                            }
                        Console.WriteLine();

                        while (opCodes.Count > opCols)
                        {
                            cp += opCols;
                            opCodes.RemoveRange(0, opCols);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write($"{cp:D3}: ");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"{string.Join(" ", opCodes.Take(opCols).Select(b => b.ToString("D2"))),-3 * opCols} ");
                        }
                    }
                    break;
            }
        }
    }
}
