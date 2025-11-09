using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
//using static System.Console;

namespace SimpleParser;

enum SyntexCategories
{
    WhiteSpace,
    Comment,
    Instruction,
    KeyLabel,
    Number,
    Memory,
    IndirectMemory,
    AddressLabel,
}

// Inherit from the generated base visitor.
// The 'object' type means your visit methods can return any type.
public class MyTi58Visitor: ti58BaseVisitor<object>
{
    private void Colorize(ITerminalNode node, SyntexCategories cat)
    {
        Console.ForegroundColor = cat switch
        {
            SyntexCategories.Comment => ConsoleColor.Green,
            SyntexCategories.Instruction => ConsoleColor.Blue,
            SyntexCategories.KeyLabel => ConsoleColor.DarkYellow,
            SyntexCategories.Number => ConsoleColor.Gray,
            SyntexCategories.Memory => ConsoleColor.Red,
            SyntexCategories.IndirectMemory => ConsoleColor.Magenta,
            SyntexCategories.AddressLabel => ConsoleColor.Yellow,
            _ => ConsoleColor.White,
        };
        Console.Write(node.GetText());
        Console.ForegroundColor = ConsoleColor.White;
    }

    // 1. Add a private field to hold the parser
    private readonly ti58Parser _parser;

    // 2. Add a constructor that takes the parser
    public MyTi58Visitor(ti58Parser parser)
    {
        _parser = parser;
    }

    // This method is called for every single token in the tree.
    // It's a good way to see the visitor in action.
    public override object VisitTerminal(ITerminalNode node)
    {
        // Get the token's integer type
        int tokenType = node.Symbol.Type;

        // First we handle white space, comments and numbers
        if (tokenType == ti58Lexer.WS)
        {
            Colorize(node, SyntexCategories.WhiteSpace);
            goto exit;
        }
        if (tokenType == ti58Lexer.LineComment)
        {
            Colorize(node, SyntexCategories.Comment);
            goto exit;
        }
        if (tokenType == ti58Lexer.Eof)
        {
            Colorize(node, SyntexCategories.WhiteSpace);
            goto exit;
        }

        // Store hierarchy as an int
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
            // Move to the next parent
            current = current.Parent;
        }
        // Don't invert this tree

        if (hierarchyInt.Contains(ti58Parser.RULE_number))
        {
            Colorize(node, SyntexCategories.Number);
            goto exit;
        }

        if (hierarchyInt.Contains(ti58Parser.RULE_memory) || hierarchyInt.Contains(ti58Parser.RULE_op_number) || hierarchyInt.Contains(ti58Parser.RULE_pgm_number) || hierarchyInt.Contains(ti58Parser.RULE_flag_number))
        {
            Colorize(node, SyntexCategories.Memory);
            goto exit;
        }

        if (hierarchyInt.Contains(ti58Parser.RULE_indmemory))
        {
            Colorize(node, SyntexCategories.IndirectMemory);
            goto exit;
        }

        // Detect Key label before label
        if (hierarchyInt.Contains(ti58Parser.RULE_key_label) || hierarchyInt.Contains(ti58Parser.RULE_numeric_key_label))
        {
            Colorize(node, SyntexCategories.KeyLabel);
            goto exit;
        }

        if (hierarchyInt.Contains(ti58Parser.RULE_address_label))
        {
            Colorize(node, SyntexCategories.AddressLabel);
            goto exit;
        }


        // Get the symbolic name from the parser's vocabulary
        string symbolicName = _parser.Vocabulary.GetSymbolicName(tokenType);
        if (symbolicName != null && symbolicName.StartsWith('I'))
        {
            Colorize(node, SyntexCategories.Instruction);
            goto exit;
        }



        // No match, print all info

        // Create a list to hold the rule names
        var hierarchy = new List<string>();

        // Start walking up the tree from the terminal's parent
        current = node.Parent;
        while (current != null)
        {
            // We only care about RuleContexts (parser rules), not other nodes
            if (current is ParserRuleContext ruleContext)
            {
                // Use the parser's RuleNames array to get the name
                string ruleName = _parser.RuleNames[ruleContext.RuleIndex];
                hierarchy.Add(ruleName);
            }
            // Move to the next parent
            current = current.Parent;
        }

        // The list is "child-to-root", so reverse it to "root-to-child"
        hierarchy.Reverse();


        // Get the symbolic name from the parser's vocabulary
        //string symbolicName = _parser.Vocabulary.GetSymbolicName(tokenType);

        // Print the hierarchy and the new symbolic name
        Console.WriteLine($"  -> Visiting: {node.GetText()} (Type: {symbolicName})");
        Console.WriteLine($"     Hierarchy: {string.Join(" / ", hierarchy)}");

    exit:
        return base.VisitTerminal(node);
    }
}