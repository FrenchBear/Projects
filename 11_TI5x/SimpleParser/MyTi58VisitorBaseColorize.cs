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
    enum SyntaxCategories
    {
        WhiteSpace,
        Comment,
        Instruction,
        KeyLabel,
        Number,
        MemoryOrNumber,
        IndirectMemory,
        AddressLabel,
    }


    private void Colorize(ITerminalNode node, SyntaxCategories cat)
    {
        Console.ForegroundColor = cat switch
        {
            SyntaxCategories.Comment => ConsoleColor.Green,
            SyntaxCategories.Instruction => ConsoleColor.Blue,
            SyntaxCategories.KeyLabel => ConsoleColor.DarkYellow,
            SyntaxCategories.Number => ConsoleColor.Gray,
            SyntaxCategories.MemoryOrNumber => ConsoleColor.Red,
            SyntaxCategories.IndirectMemory => ConsoleColor.Magenta,
            SyntaxCategories.AddressLabel => ConsoleColor.Yellow,
            _ => ConsoleColor.White,
        };
        Console.Write(node.GetText());
        Console.ForegroundColor = ConsoleColor.White;
    }

    private readonly ti58Parser _parser;

    public MyTi58VisitorBaseColorize(ti58Parser parser)
    {
        _parser = parser;
    }

    // This method is called for every single token in the tree.
    public override object VisitTerminal(ITerminalNode node)
    {
        int tokenType = node.Symbol.Type;

        // First we handle white space, comments and numbers that can be determined
        // directly from type without looking at parents rules
        if (tokenType == ti58Lexer.WS)
        {
            // Just a test to separate WS between instructions and WS in instructions
            // We don't attempt to normalize WS in structions or remove \n in instructions
            if (node.Parent is ParserRuleContext ruleContext && ruleContext.RuleIndex == ti58Parser.RULE_program)
                Console.WriteLine();
            else
                Colorize(node, SyntaxCategories.WhiteSpace);
            goto Exit;
        }
        if (tokenType == ti58Lexer.LineComment)
        {
            Colorize(node, SyntaxCategories.Comment);
            goto Exit;
        }
        if (tokenType == ti58Lexer.Eof)
        {
            Console.WriteLine();
            goto Exit;
        }
        if (tokenType == ti58Lexer.Bang)
        {
            Colorize(node, SyntaxCategories.Instruction);
            goto Exit;
        }
        if (tokenType == ti58Lexer.I40_indirect)
        {
            Colorize(node, SyntaxCategories.IndirectMemory);
            goto Exit;
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
                if (ruleContext.RuleIndex == ti58Parser.RULE_instruction_or_comment)
                    break;
                hierarchyInt.Add(ruleContext.RuleIndex);
            }
            current = current.Parent;
        }

        if (hierarchyInt.Contains(ti58Parser.RULE_number))
        {
            Colorize(node, SyntaxCategories.Number);
            goto Exit;
        }

        if (hierarchyInt.Contains(ti58Parser.RULE_memory) || hierarchyInt.Contains(ti58Parser.RULE_op_number) || hierarchyInt.Contains(ti58Parser.RULE_pgm_number) || hierarchyInt.Contains(ti58Parser.RULE_single_digit))
        {
            Colorize(node, SyntaxCategories.MemoryOrNumber);
            goto Exit;
        }

        if (hierarchyInt.Contains(ti58Parser.RULE_indmemory))
        {
            Colorize(node, SyntaxCategories.IndirectMemory);
            goto Exit;
        }

        // Detect Key label before label
        if (hierarchyInt.Contains(ti58Parser.RULE_key_label) || hierarchyInt.Contains(ti58Parser.RULE_numeric_key_label))
        {
            Colorize(node, SyntaxCategories.KeyLabel);
            goto Exit;
        }

        if (hierarchyInt.Contains(ti58Parser.RULE_address_label))
        {
            Colorize(node, SyntaxCategories.AddressLabel);
            goto Exit;
        }

    // If there is no match, print all info for easy debugging
    Trace:
        // Get the symbolic name from the parser's vocabulary
        string symbolicName = _parser.Vocabulary.GetSymbolicName(tokenType);
        if (symbolicName != null && symbolicName.StartsWith('I'))
        {
            Colorize(node, SyntaxCategories.Instruction);
            goto Exit;
        }

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
        Console.WriteLine($"  -> Visiting: {node.GetText()} (Type: {symbolicName})");
        Console.WriteLine($"     Hierarchy: {string.Join(" / ", hierarchy)}");

    Exit:
        return base.VisitTerminal(node);
    }
}