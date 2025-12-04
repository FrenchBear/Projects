// Categories.cs
// Tag source elements with [tag]...[/tag] for later to be rendered in color on various output (console, HTML, ...)
//
// 2025-11-20   PV
// 2025-11-28   PV      Tagging

namespace T59v7Core;

public enum SyntaxCategory: byte
{
    // For internal processing
    Unknown = 0,    // Default, if not painted
    Uninitialized,  // For internal use
    Invalid,        // L1InvalidToken (ex: ZYP). Incorrect/incomplete statements will be stored in a L2InvalidStatement, and its individual tokens may still keey their categegory
    Eof,            // Eof
    WS,             // White Space

    // Program code
    Comment,        // souble slash and following text up to EOL
    Number,         // Number
    Tag,            // @xxx (and also :)
    Instruction,    // Any valid instruction
    Label,          // Either instruction or D2 with D2 value>=10
    Direct,         // D1|D2 in direct statements
    Indirect,       // D1|D2 in indirect statements
    Address,        // A3 (123) or D2 D2 with 1st D2 <10> (01 23)

    // Extra
    LineNumber,     // For formatted program/error messages
    OpCode,         // For formatted program OpCodes
}

public record struct Paint
{
    public SyntaxCategory cat;
    public bool ParserError;
}

public static class Categories
{
    internal static string GetCategoryTag(SyntaxCategory cat) => cat.ToString().ToLower();
    internal static string GetCategoryOpenTag(SyntaxCategory cat) => $"[{GetCategoryTag(cat)}]";
    internal static string GetCategoryCloseTag(SyntaxCategory cat) => $"[/{GetCategoryTag(cat)}]";
    public static string GetTaggedText(string s, SyntaxCategory cat) => GetCategoryOpenTag(cat) + s + GetCategoryCloseTag(cat);
}
