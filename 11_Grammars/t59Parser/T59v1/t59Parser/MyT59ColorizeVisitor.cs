// MyTi58VisitorBaseColorize
//
// Simple visitor on grammar terminals that colorizes a program
//
// This colorization process respects original formatting, output layout is
// identical to source layout, with some color added. No reformatting or
// standardization is attempted
//
// Each terminal is processed independently, there is no statement context
// Terminal is colorized either based on its type (ex: t59Lexer.LineComment)
// or on its parent (direct or indirect) class (ex: t59Parser.RULE_indmemory)
// or on 1st letter of symbolic name such as 'I' for an instruction
// Order is important, for instance check for parent rule RULE_address_label
// before checking for instruction to format CLR in Lbl CLR differently from
// instruction CLR
//
//
// 2025-11-10   PV

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;

namespace SimpleParser;

// Inherit from the generated base visitor.
// The 'object' type means your visit methods can return any type.
public class MyT59ColorizeVisitor(t59Parser parser): t59BaseVisitor<object>
{
    public enum SyntaxCategory
    {
        ProgramSeparator,
        Eof,
        InterInstructionWhiteSpace,
        WhiteSpace,
        Comment,
        Instruction,
        KeyLabel,
        Number,
        DirectMemoryOrNumber,
        IndirectMemory,
        DirectAddress,
        Unknown,
    }

    public static void Colorize(AstToken token)
        => Colorize(token.Text, token.Cat);

    public static void Colorize(string text, SyntaxCategory cat)
    {
        if (cat == SyntaxCategory.Eof)
            return;
        if (cat == SyntaxCategory.ProgramSeparator)
        {
            var bc = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(text);
            Console.BackgroundColor = bc;
            Console.ForegroundColor = ConsoleColor.Gray;
            return;
        }
        if (cat == SyntaxCategory.Unknown)
        {
            Console.Write($"\x1b[7m{text}\u001b[0m");
            return;
        }

#pragma warning disable IDE0072 // Add missing cases
        Console.ForegroundColor = cat switch
        {
            SyntaxCategory.Comment => ConsoleColor.Green,
            SyntaxCategory.Instruction => ConsoleColor.Cyan,
            SyntaxCategory.KeyLabel => ConsoleColor.DarkYellow,
            SyntaxCategory.Number => ConsoleColor.Gray,
            SyntaxCategory.DirectMemoryOrNumber => ConsoleColor.Red,
            SyntaxCategory.IndirectMemory => ConsoleColor.Magenta,
            SyntaxCategory.DirectAddress => ConsoleColor.Yellow,
            _ => ConsoleColor.White,
        };
        // Use bold for labels
        bool isLabel = false;
        if (cat == SyntaxCategory.Instruction &&
            string.Equals(text, "Lbl", StringComparison.CurrentCultureIgnoreCase))
        {
            Console.Write("\x1b[1m");
            isLabel = true;
        }
        Console.Write(text);
        Console.Write("\x1b[0m");
        if (isLabel)
            Console.ForegroundColor = ConsoleColor.Gray;
    }

    // This method is called for every single token in the tree.
    public override object VisitTerminal(ITerminalNode node)
    {
        var sc = GetTerminalSyntaxCategory(node);
        var text = node.GetText();

        //if (sc == SyntaxCategory.InterInstructionWhiteSpace)
        //    Console.WriteLine();
        //else
        // Print exactly as is, just with color
        Colorize(text, sc);

        return base.VisitTerminal(node);
    }

    public SyntaxCategory GetTerminalSyntaxCategory(ITerminalNode node)
    {
        int tokenType = node.Symbol.Type;

        // First we handle white space, comments and numbers that can be determined
        // directly from type without looking at parents rules
        switch (tokenType)
        {
            // Just a test to separate WS between instructions and WS in instructions
            // We don't attempt to normalize WS in instructions or remove \n in instructions
            case t59Lexer.Program_separator:
                return SyntaxCategory.ProgramSeparator;
            case t59Lexer.WS when node.Parent is ParserRuleContext { RuleIndex: t59Parser.RULE_program }:
                return SyntaxCategory.InterInstructionWhiteSpace;
            case t59Lexer.WS:
                return SyntaxCategory.WhiteSpace;
            case t59Lexer.LineComment:
                return SyntaxCategory.Comment;
            case t59Lexer.Eof:
                return SyntaxCategory.Eof;
            case t59Lexer.Bang:
                return SyntaxCategory.Instruction;
            case t59Lexer.I40_indirect:
                return SyntaxCategory.Instruction;      // Considering it's IndirectMemory doesn't work during AST build when grouping tokens
            case t59Lexer.INVALID_TOKEN:
                return SyntaxCategory.Unknown;
        }

        // Build a list of parent types (rules) so later we can easily check whether
        // a terminal descend from a specific rule
        // [0]=Parent, [1]=PArent's parent...
        // Anonymous lexer tokens are not included in the list (so I gave a name Bang to "!" to include it)
        var hierarchyInt = new List<int>();
        IParseTree current = node.Parent;
        while (current != null)
        {
            // We only care about RuleContexts (parser rules), not other nodes
            if (current is ParserRuleContext ruleContext)
            {
                // Stop at RULE_instruction or RULE_comment
                if (ruleContext.RuleIndex == t59Parser.RULE_instruction_or_comment)
                    break;
                hierarchyInt.Add(ruleContext.RuleIndex);
            }
            current = current.Parent;
        }

        if (hierarchyInt.Contains(t59Parser.RULE_number))
            return SyntaxCategory.Number;

        if (hierarchyInt.Contains(t59Parser.RULE_memory) || hierarchyInt.Contains(t59Parser.RULE_op_number) || hierarchyInt.Contains(t59Parser.RULE_pgm_number) || hierarchyInt.Contains(t59Parser.RULE_single_digit))
            return SyntaxCategory.DirectMemoryOrNumber;

        if (hierarchyInt.Contains(t59Parser.RULE_indmemory))
            return SyntaxCategory.IndirectMemory;

        // Detect Key label before label
        if (hierarchyInt.Contains(t59Parser.RULE_key_label) || hierarchyInt.Contains(t59Parser.RULE_numeric_key_label))
            return SyntaxCategory.KeyLabel;

        if (hierarchyInt.Contains(t59Parser.RULE_address_label))
            return SyntaxCategory.DirectAddress;

        // Get the symbolic name from the parser's vocabulary
        string symbolicName = parser.Vocabulary.GetSymbolicName(tokenType);
        if (symbolicName != null && symbolicName.StartsWith('I'))
            return SyntaxCategory.Instruction;

        // If there is no match, print all info for easy debugging

        // Create a list to hold the rule names
        var hierarchy = new List<string>();
        current = node.Parent;
        while (current != null)
        {
            if (current is ParserRuleContext ruleContext)
            {
                string ruleName = parser.RuleNames[ruleContext.RuleIndex];
                hierarchy.Add(ruleName);
            }
            current = current.Parent;
        }
        // The list is "child-to-root", so reverse it to "root-to-child"
        hierarchy.Reverse();

        // Print the symbol name and the hierarchy
        Console.WriteLine($"*** Can't determine Terminal SyntaxCategory: {node.GetText()} (Type: {symbolicName})  Hierarchy: {string.Join(" / ", hierarchy)}");
        return SyntaxCategory.Unknown;
    }

}