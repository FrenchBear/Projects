// 5th variant of T59 grammar - Couleurs
// Manage syntax categories and colors
// Use french Couleur instead of Color to avoid names collisions
//
// 2025-11-20   PV

namespace T59v5;

#pragma warning disable IDE0072 // Add missing cases

public enum SyntaxCategory: byte
{
    Unknown = 0,            // Default, if not painted
    Uninitialized,          // For internal use
    Invalid,                // L1InvalidToken (ex: ZYP). Incorrect/incomplete statements will be stored in a L2InvalidStatement, and its individual tokens may still keey their categegory
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

public static class Couleurs
{
    private static string CC(int r, int g, int b) => "\x1b[38;2;" + $"{r};{g};{b}m";

    public static string GetCategoryColor(SyntaxCategory sc)
        => sc switch
        {
            SyntaxCategory.LineComment => CC(64, 192, 64),
            SyntaxCategory.Invalid => CC(255, 64, 64),
            SyntaxCategory.Unknown => CC(255, 0, 0),
            SyntaxCategory.Instruction => CC(128, 192, 255),
            SyntaxCategory.Number => CC(210, 210, 210),
            SyntaxCategory.DirectMemoryOrNumber => CC(255, 192, 255),
            SyntaxCategory.IndirectMemory => CC(255, 128, 255),
            SyntaxCategory.Tag => CC(255, 192, 128),
            SyntaxCategory.Label => CC(255, 192, 0),
            SyntaxCategory.DirectAddress => CC(255, 144, 0),
            _ => CC(255, 255, 255)      // Uninitialized, Eof
        };

    public static string GetDefaultColor() => "\x1b[39m";
}
