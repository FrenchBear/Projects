// Fourth variant of T59 grammar - SourcePainter
// Manage source code painting, that is, for each character of source code, store information on related token
// ANTLR4 parser reports lines in 1..n and columns in 0..n-1
//
// 2025-11-10   PV

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T59v4;

public enum SyntaxCategory : byte
{
    Unknown = 0,
    LexerError,
    ParserError,
    ProgramSeparator,
    Eof,
    InterInstructionWhiteSpace,
    WhiteSpace,
    Comment,
    Instruction,
    Label,
    Number,
    DirectMemoryOrNumber,
    IndirectMemory,
    DirectAddress,
}

internal class SourcePainter
{
    private readonly string[] Lines;
    private readonly SyntaxCategory[][] Cats;

    public SourcePainter(string s)
    {
        s = s.Replace("\r\n", "\n").Replace("\r", "\n");
        Lines = s.Split("\n");

        Cats = new SyntaxCategory[Lines.Length][];
        for (int i = 0; i < Lines.Length; i++)
            Cats[i] = new SyntaxCategory[Lines[i].Length];
    }

    // Beware, ANTLR line index starts at 1
    public void Paint(int line, int charPositionInLine, SyntaxCategory cat)
    {
        if (charPositionInLine<Cats[line-1].Length)
            Cats[line - 1][charPositionInLine] = cat;
    }

    public void Print()
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            for (int j = 0; j < Lines[i].Length; j++)
            {
                if (Cats[i][j] == SyntaxCategory.LexerError)
                    Console.ForegroundColor= ConsoleColor.Red;
                else if (Cats[i][j] == SyntaxCategory.ParserError)
                    Console.ForegroundColor = ConsoleColor.Magenta;

                Console.Write(Lines[i][j]);
                
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.WriteLine();
        }
    }
}
