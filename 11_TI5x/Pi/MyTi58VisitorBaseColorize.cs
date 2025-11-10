// Simple visitor on terminals that colorizes a program
// Each terminal is processed independently, there is no statement context
// Terminal is colorized either based on its type (ex: piLexer.LineComment)
// or on its parent (direct or indirect) class (ex: piParser.RULE_indmemory)
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

namespace SimpleParser;

// Inherit from the generated base visitor.
// The 'object' type means your visit methods can return any type.
public class MypiVisitorTerminal: piBaseVisitor<object>
{

    private readonly piParser _parser;

    public MypiVisitorTerminal(piParser parser)
    {
        _parser = parser;
    }

    // This method is called for every single token in the tree.
    public override object VisitTerminal(ITerminalNode node)
    {
        Console.WriteLine(node);
        return base.VisitTerminal(node);
    }
}