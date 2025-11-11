// Simple visitor on terminals that colorizes a program
// Each terminal is processed independently, there is no statement context
// Terminal is colorized either based on its type (ex: ti58Lexer.LineComment)
// or on its parent (direct or indirect) class (ex: ti58Parser.RULE_indmemory)
// or on 1st letter of symbolic name such as 'I' for an instruction
// Order is important, for instance check for parent rule RULE_address_label
// before checking for instruction to format CLR in Lbl CLR differently from
// instruction CLR
//
// 2025-11-10   PV

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SimpleParser;

// Inherit from the generated base visitor.
// The 'object' type means your visit methods can return any type.
public class MyTi58VisitorBaseColorize: ti58BaseVisitor<object>
{
    public enum SyntaxCategory
    {
        EOF,
        InterInstructionWhiteSpace,
        WhiteSpace,
        Comment,
        Instruction,
        KeyLabel,
        Number,
        DirectMemoryOrNumber,
        IndirectMemory,
        AddressLabel,
        Unknown,
    }


    public void Colorize(ASTToken token)
        => Colorize(token.text, token.cat);

    public void Colorize(string text, SyntaxCategory cat)
    {
        if (cat==SyntaxCategory.EOF) return;

        Console.ForegroundColor = cat switch
        {
            SyntaxCategory.Comment => ConsoleColor.Green,
            SyntaxCategory.Instruction => ConsoleColor.Cyan,
            SyntaxCategory.KeyLabel => ConsoleColor.DarkYellow,
            SyntaxCategory.Number => ConsoleColor.Gray,
            SyntaxCategory.DirectMemoryOrNumber => ConsoleColor.Red,
            SyntaxCategory.IndirectMemory => ConsoleColor.Magenta,
            SyntaxCategory.AddressLabel => ConsoleColor.Yellow,
            _ => ConsoleColor.White,
        };
        Console.Write(text);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    private readonly ti58Parser _parser;

    public MyTi58VisitorBaseColorize(ti58Parser parser)
    {
        _parser = parser;
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
        if (tokenType == ti58Lexer.WS)
        {
            // Just a test to separate WS between instructions and WS in instructions
            // We don't attempt to normalize WS in structions or remove \n in instructions
            if (node.Parent is ParserRuleContext ruleContext && ruleContext.RuleIndex == ti58Parser.RULE_program)
                return SyntaxCategory.InterInstructionWhiteSpace;
            else
                return SyntaxCategory.WhiteSpace;
        }

        if (tokenType == ti58Lexer.LineComment)
            return SyntaxCategory.Comment;

        if (tokenType == ti58Lexer.Eof)
            return SyntaxCategory.EOF;

        if (tokenType == ti58Lexer.Bang)
            return SyntaxCategory.Instruction;

        if (tokenType == ti58Lexer.I40_indirect)
            return SyntaxCategory.IndirectMemory;

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
                if (ruleContext.RuleIndex == ti58Parser.RULE_instruction_or_comment)
                    break;
                hierarchyInt.Add(ruleContext.RuleIndex);
            }
            current = current.Parent;
        }

        if (hierarchyInt.Contains(ti58Parser.RULE_number))
            return SyntaxCategory.Number;

        if (hierarchyInt.Contains(ti58Parser.RULE_memory) || hierarchyInt.Contains(ti58Parser.RULE_op_number) || hierarchyInt.Contains(ti58Parser.RULE_pgm_number) || hierarchyInt.Contains(ti58Parser.RULE_single_digit))
            return SyntaxCategory.DirectMemoryOrNumber;

        if (hierarchyInt.Contains(ti58Parser.RULE_indmemory))
            return SyntaxCategory.IndirectMemory;

        // Detect Key label before label
        if (hierarchyInt.Contains(ti58Parser.RULE_key_label) || hierarchyInt.Contains(ti58Parser.RULE_numeric_key_label))
            return SyntaxCategory.KeyLabel;

        if (hierarchyInt.Contains(ti58Parser.RULE_address_label))
            return SyntaxCategory.AddressLabel;

        // Get the symbolic name from the parser's vocabulary
        string symbolicName = _parser.Vocabulary.GetSymbolicName(tokenType);
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
                string ruleName = _parser.RuleNames[ruleContext.RuleIndex];
                hierarchy.Add(ruleName);
            }
            current = current.Parent;
        }
        // The list is "child-to-root", so reverse it to "root-to-child"
        hierarchy.Reverse();

        // Print the symbon lame and the hierarchy
        Console.WriteLine($"*** Can't determine Terminal SyntaxCategory: {node.GetText()} (Type: {symbolicName})  Hierarchy: {string.Join(" / ", hierarchy)}");
        return SyntaxCategory.Unknown;
    }

}