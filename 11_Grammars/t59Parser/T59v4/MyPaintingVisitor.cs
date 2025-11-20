// Fourth variant of T59 grammar - Parser
// Don't use INVALID_TOKEN anymore
//
// 2025-11-20   PV

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace T59v4;

internal class MyPaintingVisitor: GramBaseVisitor<string>
{
    private SourcePainter sp;
    private GramParser parser;

    //public override string VisitProgram(GramParser.ProgramContext context)
    //{
    //    return base.VisitProgram(context);
    //}

    public void VisitTerminals(GramParser parser, GramParser.ProgramContext tree, SourcePainter sourcePainter)
    {
        this.sp = sp;
        this.parser = parser;
        base.Visit(tree);
    }

    public override string VisitTerminal(ITerminalNode node)
    {
        //var sc = GetTerminalSyntaxCategory(node);

        var txt = node.GetText();
        int tokenType = node.Symbol.Type;
        string symbolicName = parser.Vocabulary.GetSymbolicName(tokenType);
        var sc = GetTerminalSyntaxCategory(node);

        Console.WriteLine($"{txt}: {symbolicName} -> {sc}");
        return null;
    }

    public string GetTerminalSyntaxCategory(ITerminalNode node)
    {
        int tokenType = node.Symbol.Type;

        // Simplification, everything van be determined from token name or parent rule name
        // NUM:   Number
        // D1|D2|A3|A4: parent=number_statement ? Number
        // D2:    parent=(bd_statement|lbl_statement) ? Label
        // D1|D2: parent=d_statement ? DirectMemoryOrNumber : parent *i_statement ? IndMemory : Err
        // A3|A4: parent=bd_statement ? DirectAddress : Number
        // I*:    parent=mnemonic ? Label : instruction

        /*
        // First we handle white space, comments and numbers that can be determined
        // directly from type without looking at parents rules
        switch (tokenType)
        {
            // Just a test to separate WS between instructions and WS in instructions
            // We don't attempt to normalize WS in instructions or remove \n in instructions
            //case Vocab.Program_separator:
            //    return SyntaxCategory.ProgramSeparator;
            case Vocab.WS when node.Parent is ParserRuleContext { RuleIndex: GramParser.RULE_program }:
                return SyntaxCategory.InterInstructionWhiteSpace;
            case Vocab.WS:
                return SyntaxCategory.WhiteSpace;
            //case Vocab.LineComment:
            //    return SyntaxCategory.Comment;
            case Vocab.Eof:
                return SyntaxCategory.Eof;
            case Vocab.I40_indirect:
                return SyntaxCategory.Instruction;      // Considering it's IndirectMemory doesn't work during AST build when grouping tokens
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
                if (ruleContext.RuleIndex == GramParser.RULE_statement)
                    break;
                hierarchyInt.Add(ruleContext.RuleIndex);
            }
            current = current.Parent;
        }

        if (hierarchyInt.Contains(GramParser.RULE_number_statement))
            return SyntaxCategory.Number;

        if (hierarchyInt.Contains(GramParser.RULE_d_statement))
            return SyntaxCategory.DirectMemoryOrNumber;

        if (hierarchyInt.Contains(GramParser.RULE_indmemory))
            return SyntaxCategory.IndirectMemory;

        // Detect Key label before label
        if (hierarchyInt.Contains(GramParser.RULE_key_label) || hierarchyInt.Contains(GramParser.RULE_numeric_key_label))
            return SyntaxCategory.KeyLabel;

        if (hierarchyInt.Contains(GramParser.RULE_address_label))
            return SyntaxCategory.DirectAddress;

        // Get the symbolic name from the parser's vocabulary
        string symbolicName = parser.Vocabulary.GetSymbolicName(tokenType);
        if (symbolicName != null && symbolicName.StartsWith('I'))
            return SyntaxCategory.Instruction;
        */
        // If there is no match, print all info for easy debugging

        // Create a list to hold the rule names
        var hierarchy = new List<string>();
        IParseTree current = node.Parent;
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
        //hierarchy.Reverse();

        return string.Join(" / ", hierarchy);

        // Print the symbol name and the hierarchy
        //Console.WriteLine($"*** Can't determine Terminal SyntaxCategory: {node.GetText()} (Type: {symbolicName})  Hierarchy: {string.Join(" / ", hierarchy)}");
        //return SyntaxCategory.Unknown;
    }
    
}