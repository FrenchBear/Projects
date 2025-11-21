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

public enum SyntaxCategory: byte
{
    Unknown = 0,
    LexerError,
    ProgramSeparator,
    Eof,
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

    /// <summary>
    /// Apply SyntaxCategory cat to character at specified (line, column)
    /// </summary>
    /// <param name="line">1..n</param>
    /// <param name="charPositionInLine">0..len-1</param>
    /// <param name="cat">SyntaxCategory of the character</param>
    public void Paint(int line, int charPositionInLine, SyntaxCategory cat)
    {
        if (charPositionInLine < Cats[line - 1].Length)
            Cats[line - 1][charPositionInLine] = cat;
    }

    public void Print()
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            for (int j = 0; j < Lines[i].Length; j++)
            {
                // White space is not formatted at all
                if (char.IsWhiteSpace(Lines[i][j]))
                {
                    Console.Write(Lines[i][j]);
                    continue;
                }

                switch (Cats[i][j])
                {
                    case SyntaxCategory.Comment:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case SyntaxCategory.LexerError:
                    case SyntaxCategory.Unknown:
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                    case SyntaxCategory.Instruction:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case SyntaxCategory.Label:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case SyntaxCategory.Number:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case SyntaxCategory.DirectMemoryOrNumber:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case SyntaxCategory.IndirectMemory:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case SyntaxCategory.DirectAddress:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                Console.Write(Lines[i][j]);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }

            Console.WriteLine();
        }
    }
}
