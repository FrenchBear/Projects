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
    PgmSeparator,       // END
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

internal sealed class SourcePainter
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
        var at = new Paint { cat = SyntaxCategory.Uninitialized, ParserError = false };
        var sbc = new StringBuilder();      // For characters
        var sbs = new StringBuilder();      // For spaces
        for (int i = 0; i < Lines.Length; i++)
        {
            for (int j = 0; j <= Lines[i].Length; j++)
            {
                var c = j == Lines[i].Length ? '\n' : Lines[i][j];

                // Spaces are accumulated in a separate buffer since we only print them
                // with attributes only if char before and char after have the same attribute
                if (c is ' ' or '\t')
                {
                    sbs.Append(c);
                    continue;
                }

                // c is not a space
                // If current attribute is the same as current buffer (or not assigned yet), we accumulate
                var a = c == '\n' ? new Paint { cat = SyntaxCategory.Eof, ParserError = false } : Cats[i][j];      // Trick to force printing and reset to 'no background' before printing newline visually better
                if (at.cat == SyntaxCategory.Uninitialized || at == a)
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
                Console.Write(Couleurs.GetColorMode(at.cat));
                Console.Write(sbc.ToString());
                sbc.Clear();
                Console.Write(Couleurs.GetDefaultColor());
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
        Console.Write(Couleurs.GetColorMode(at.cat));
        Console.Write(sbc.ToString());
        Console.Write(Couleurs.GetDefaultColor());
        if (sbs.Length > 0)
            Console.Write(sbs.ToString());
    }
}

public static class Couleurs
{
    private static string CC(int r, int g, int b) => "\x1b[38;2;" + $"{r};{g};{b}m";

    public static string GetColorMode(SyntaxCategory sc)
        => sc switch
        {
            SyntaxCategory.LineComment => CC(64, 192, 64),
            SyntaxCategory.Invalid => CC(255, 64, 64),
            SyntaxCategory.Unknown => CC(255, 0, 0),
            SyntaxCategory.Instruction => CC(128, 192, 255),
            SyntaxCategory.Number => CC(192, 192, 192),
            SyntaxCategory.DirectMemoryOrNumber => CC(255, 192, 255),
            SyntaxCategory.IndirectMemory => CC(255, 128, 255),
            //SyntaxCategory.Tag => CC(0, 128, 255),
            SyntaxCategory.Tag => CC(255, 192, 128),
            SyntaxCategory.Label => CC(255, 192, 0),
            SyntaxCategory.DirectAddress => CC(255, 144, 0),
            _ => CC(255, 255, 255)      // Uninitialized, PgmSeparator, Eof
        };

    public static string GetDefaultColor() => "\x1b[39m";
}
