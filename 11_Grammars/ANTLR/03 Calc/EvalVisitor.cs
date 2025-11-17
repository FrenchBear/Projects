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
public class EvalVisitor: LabeledExprBaseVisitor<object>
{

    private Dictionary<string, int> Memory = new();

    public override object VisitAssign(LabeledExprParser.AssignContext context)
    {
        string id = context.ID().GetText();
        var value = Visit(context.expr());
        Memory.Add(id, (int)value);
        return value;
    }

    public override object VisitPrintExpr(LabeledExprParser.PrintExprContext context)
    {
        var value = Visit(context.expr());
        Console.WriteLine(value);
        //return $"PrintExpr returned {value}";   // Dummy value
        return 0;
    }

    public override object VisitInt(LabeledExprParser.IntContext context)
    {
        return int.Parse(context.INT().GetText());
    }

    public override object VisitId(LabeledExprParser.IdContext context)
    {
        string id = context.ID().GetText();
        return Memory.GetValueOrDefault(id, 0);
    }

    public override object VisitMulDiv(LabeledExprParser.MulDivContext context)
    {
        int left = (int)Visit(context.expr(0));     // Value of left subexpression
        int right = (int)Visit(context.expr(1));    // Value of right subexpression
        if (context.op.Type == LabeledExprParser.MUL)
            return left * right;
        return left / right;        // int division
    }

    public override object VisitAddSub(LabeledExprParser.AddSubContext context)
    {
        int left = (int)Visit(context.expr(0));     // Value of left subexpression
        int right = (int)Visit(context.expr(1));    // Value of right subexpression
        if (context.op.Type == LabeledExprParser.ADD)
            return left + right;
        return left - right;
    }

    public override object VisitParens(LabeledExprParser.ParensContext context)
    {
        return Visit(context.expr());
    }

}