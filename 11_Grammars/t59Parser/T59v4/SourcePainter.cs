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
    Unknown = 0,        // Default, if not painted
    Uninitialized,      // For internal use
    LexerError,
    ProgramSeparator,
    Eof,
    Comment,
    Number,
    Tag,
    Instruction,
    Label,
    DirectMemoryOrNumber,
    IndirectMemory,
    DirectAddress,
}

public record struct Paint
{
    public SyntaxCategory cat;
    public bool ParserError;
}

internal class SourcePainter
{
    private readonly string[] Lines;
    private readonly Paint[][] Cats;

    public SourcePainter(string s)
    {
        s = s.Replace("\r\n", "\n").Replace("\r", "\n");
        Lines = s.Split("\n");

        Cats = new Paint[Lines.Length][];
        for (int i = 0; i < Lines.Length; i++)
            Cats[i] = new Paint[Lines[i].Length];
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
            Cats[line - 1][charPositionInLine].cat = cat;
    }

    public void Print()
    {
        SyntaxCategory at = SyntaxCategory.Uninitialized;
        var sbc = new StringBuilder();      // For characters
        var sbs = new StringBuilder();      // For spaces
        for (int i = 0; i < Lines.Length; i++)
        {
            for (int j = 0; j <= Lines[i].Length; j++)
            {
                var c = j== Lines[i].Length ? '\n' : Lines[i][j];

                // Spaces are accumulated in a separate buffer since we only print them
                // with attributes only if char before and char after have the same attribute
                if (c==' ' || c=='\t')
                {
                    sbs.Append(c);
                    continue;
                }

                // c is not a space
                // If current attribute is the same as current buffer (or not assigned yet), we accumulate
                var a = c=='\n' ? SyntaxCategory.Eof : Cats[i][j].cat;      // Trick to force printing and reset to 'no background' before printing newline visually better
                if (at == SyntaxCategory.Uninitialized || at==a)
                {
                    // If we have accumulated spaces, merge them
                    if (sbs.Length > 0)
                    {
                        sbc.Append(sbs);
                        sbs.Clear();
                    }
                    at = a;
                    sbc.Append(c);
                    continue;
                }

                // at!=a -> time to print

                // First accumulated string with at attribute
                SetColorMode(at);
                Console.Write(sbc.ToString());
                sbc.Clear();
                ResetColorMode();
                // Then spaces
                if (sbs.Length > 0)
                {
                    Console.Write(sbs.ToString());
                    sbs.Clear();
                }

                // Start new accumulation
                at = a;
                sbc.Append(c);
            }
        }
        // reliquary
        SetColorMode(at);
        Console.Write(sbc.ToString());
        ResetColorMode();
        if (sbs.Length > 0)
            Console.Write(sbs.ToString());
    }

    private void SetColorMode(SyntaxCategory sc)
    {
        switch (sc)
        {
            case SyntaxCategory.Comment:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case SyntaxCategory.LexerError:
            case SyntaxCategory.Unknown:
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Red;
                break;
            case SyntaxCategory.Instruction:
                Console.ForegroundColor = ConsoleColor.Cyan;
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
            case SyntaxCategory.Tag:
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
            case SyntaxCategory.Label:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                break;
            case SyntaxCategory.DirectAddress:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            default:
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }
    }

    private void ResetColorMode()
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.BackgroundColor = ConsoleColor.Black;
    }
}
