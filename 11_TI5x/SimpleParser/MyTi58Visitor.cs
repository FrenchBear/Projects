using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
//using static System.Console;

namespace SimpleParser;

// Inherit from the generated base visitor.
// The 'object' type means your visit methods can return any type.
public class MyTi58Visitor: ti58BaseVisitor<object>
{
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
        // Create a list to hold the rule names
        var hierarchy = new List<string>();

        // Start walking up the tree from the terminal's parent
        IParseTree current = node.Parent;
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

        // Get the token's integer type
        int tokenType = node.Symbol.Type;

        // Get the symbolic name from the parser's vocabulary
        string symbolicName = _parser.Vocabulary.GetSymbolicName(tokenType);

        // Print the hierarchy and the new symbolic name
        Console.WriteLine($"  -> Visiting: {node.GetText()} (Type: {symbolicName})");
        Console.WriteLine($"     Hierarchy: {string.Join(" / ", hierarchy)}");

        return base.VisitTerminal(node);
    }
}