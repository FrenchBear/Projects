// Fourth variant of T59 grammar - SourcePainter
// Manage source code painting, that is, for each character of source code, store information on related token
// ANTLR4 parser reports lines in 1..n and columns in 0..n-1
//
// 2025-11-10   PV

using System;
using System.Text;

namespace T59v5;

public enum SyntaxCategory: byte
{
    Unknown = 0,            // Default, if not painted
    Uninitialized,          // For internal use
    Invalid,                // L1InvalidToken (ex: ZYP). Incorrect/incomplete statements will be stored in a L2InvalidStatement, and its individual tokens may still keey their categegory
    ProgramSeparator,       // END
    Eof,                    // Eof
    LineComment,            // souble slash and following text up to EOL
    Number,                 // Number
    Tag,                    // @xxx (and also :)
    Instruction,            // Any valid instruction
    Label,                  // Either instruction or D2 with D2 value>=10
    DirectMemoryOrNumber,   // D1|D2 in direct statements
    IndirectMemory,         // D1|D2 in indirect statements
    DirectAddress,          // A3 (123) or D2 D2 with 1st D2 <10> (01 23)
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

    public void PaintParserError(int line, int charPositionInLine)
    {
        if (charPositionInLine < Cats[line - 1].Length)
            Cats[line - 1][charPositionInLine].ParserError = true;
    }

    public void Print()
    {
        Paint at = new Paint { cat = SyntaxCategory.Uninitialized, ParserError = false };
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
                var a = c=='\n' ? new Paint { cat = SyntaxCategory.Eof, ParserError = false } : Cats[i][j];      // Trick to force printing and reset to 'no background' before printing newline visually better
                if (at.cat == SyntaxCategory.Uninitialized || at==a)
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

    private void SetColorMode(Paint p)
    {
        switch (p.cat)
        {
            case SyntaxCategory.LineComment:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case SyntaxCategory.Invalid:
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Red;
                break;
            case SyntaxCategory.Unknown:
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Blue;
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
        if (p.ParserError)
            Console.Write("\x1b[4m");
    }

    private void ResetColorMode()
    {
        Console.Write("\x1b[0m");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.BackgroundColor = ConsoleColor.Black;
    }
}
