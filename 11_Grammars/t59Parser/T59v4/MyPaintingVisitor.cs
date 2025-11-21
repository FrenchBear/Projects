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
    private SourcePainter Sp;
    private GramParser Parser;

    public void VisitTerminals(GramParser parser, GramParser.ProgramContext tree, SourcePainter sp)
    {
        Sp = sp;
        Parser = parser;
        base.Visit(tree);
    }

    public override string VisitTerminal(ITerminalNode node)
    {
        var txt = node.GetText();
        var sc = GetTerminalSyntaxCategory(node);
        //Console.WriteLine($"{txt}: -> {sc}");
        for (int i=0 ; i<txt.Length;i++)
            Sp.Paint(node.Symbol.Line, node.Symbol.Column + i, sc);
        return null;
    }

    public SyntaxCategory GetTerminalSyntaxCategory(ITerminalNode node)
    {
        string tokenName = Parser.Vocabulary.GetSymbolicName(node.Symbol.Type);
        string parentName = node.Parent is ParserRuleContext ruleContext ? parentName = Parser.RuleNames[ruleContext.RuleIndex] : parentName = "???";

        // Simplification, everything van be determined from token name or parent rule name
        // NUM:   Number
        // D1|D2|A3|A4: parent=number_statement ? Number
        // D2:    parent=(bd_statement|lbl_statement) ? Label
        // D1|D2: parent=d_statement ? DirectMemoryOrNumber : parent *i_statement ? IndMemory : Err
        // A3|A4: parent=bd_statement ? DirectAddress
        // I*:    parent=mnemonic ? Label : instruction

        if (tokenName == "PROGRAM_SEPARATOR")
            return SyntaxCategory.ProgramSeparator;
        if (tokenName == "EOF")
            return SyntaxCategory.Eof;
        if (tokenName == "LINE_COMMENT")
            return SyntaxCategory.Comment;
        if (tokenName == "NUM")
            return SyntaxCategory.Number;
        if (parentName == "number_statement")
            return SyntaxCategory.Number;
        if (tokenName == "D2" && (parentName == "bd_statement" || parentName == "lbl_statement"))
            return SyntaxCategory.Label;
        if (tokenName == "D1" || tokenName== "D2")
            if (parentName.EndsWith("d_statement"))
                return SyntaxCategory.DirectMemoryOrNumber;
            else if (parentName.EndsWith("i_statement"))
                return SyntaxCategory.IndirectMemory;
            else
                Debugger.Break();
        if (tokenName=="A3" || tokenName=="A4")
            if (parentName == "ad_statement")
                return SyntaxCategory.DirectAddress;
        if (tokenName.StartsWith('I'))
            if (parentName == "mnemonic")
                return SyntaxCategory.Label;
            else
                return SyntaxCategory.Instruction;
        
        Debugger.Break();
        return SyntaxCategory.Unknown;
    }

    public string GetTerminalHierarchy(ITerminalNode node)
    {
        // Create a list to hold the rule names
        var hierarchy = new List<string>();
        IParseTree current = node.Parent;
        while (current != null)
        {
            if (current is ParserRuleContext ruleContext)
            {
                string ruleName = Parser.RuleNames[ruleContext.RuleIndex];
                hierarchy.Add(ruleName);
            }
            current = current.Parent;
        }
        // The list is "child-to-root", so reverse it to "root-to-child"
        //hierarchy.Reverse();

        return string.Join(" / ", hierarchy);
    }

}