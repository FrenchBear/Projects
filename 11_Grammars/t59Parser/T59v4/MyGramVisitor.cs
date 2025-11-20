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

public class MyGramVisitor: GramBaseVisitor<string>
{
    private readonly StringBuilder _results = new();

    public override string VisitProgram(GramParser.ProgramContext context)
    {
        base.VisitProgram(context);
        return _results.ToString();
    }

    public override string VisitNumber_statement(GramParser.Number_statementContext context)
    {
        string number = context.GetText();
        _results.AppendLine($"[Number] {number}");
        return base.VisitNumber_statement(context);
    }

    public override string VisitAtomic_statement(GramParser.Atomic_statementContext context)
    {
        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;

        _results.AppendLine($"[Atomic] {inv} {sta}");
        return base.VisitAtomic_statement(context);
    }

    public override string VisitD_statement(GramParser.D_statementContext context)
    {
        if (context.exception != null || ContainsErrorNode(context.children) || context.t.TokenIndex < 0)
        {
            var ct = string.Join(" ", GetChildrenText(context.children));
            _results.AppendLine($"⚠ [d_statement] {ct}");
            goto Cont;
        }

        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;
        string target = context.t.Text;

        _results.AppendLine($"[d_statement] {inv} {sta} {target}");
        Cont:
        return base.VisitD_statement(context);
    }

    public override string VisitI_statement(GramParser.I_statementContext context)
    {
        // Need to group all errors as here, errors nuances are irrelevant
        if (context.exception != null || ContainsErrorNode(context.children) || context.t.TokenIndex<0)
        {
            var ct = string.Join(" ", GetChildrenText(context.children));
            _results.AppendLine($"⚠ [i_statement] {ct}");
            goto Cont;
        }

        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;
        string ind = context.ind?.Text ?? "";
        string target = context.t.Text;

        // In case of $$$ (statement contains an invalid token), it's not printed here...
        _results.AppendLine($"[i_statement] {inv} {sta} {ind} {target}");

    // but during the recursive descent, in VisitErrorNode
    Cont:
        return base.VisitI_statement(context);
    }

    private List<string> GetChildrenText(IList<IParseTree> contextChildren)
    {
        List<string> res = [];
        if (contextChildren == null)
        {
            res.Add("«null»");
            return res;
        }
        foreach (var ch in contextChildren)
            switch (ch)
            {
                case IErrorNode ien:
                    res.Add($"«{ien.GetText()}»");
                    break;

                case ITerminalNode itn:
                    res.Add(itn.GetText());
                    break;

                case ParserRuleContext ruleContext:
                    if (ruleContext.children==null)
                        res.Add("«null»");
                    else
                        res.AddRange(GetChildrenText(ruleContext.children));
                    break;
            }

        return res;
    }

    private static bool ContainsErrorNode(IList<IParseTree> contextChildren)
    {
        foreach (var ch in contextChildren)
            switch (ch)
            {
                case IErrorNode:
                    return true;

                case ParserRuleContext ruleContext:
                    if (ruleContext.children == null)       // A ruleContext with no children indicates an error
                        return true;
                    if (ContainsErrorNode(ruleContext.children))
                        return true;
                    break;
            }
        return false;
    }

    public override string VisitBd_statement(GramParser.Bd_statementContext context)
    {
        if (context.exception != null || ContainsErrorNode(context.children))
        {
            var ct = string.Join(" ", GetChildrenText(context.children));
            _results.AppendLine($"⚠ [bd_statement] {ct}");
            goto Cont;
        }

        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;

        var t = context.ad_statement().t?.Text ?? context.ad_statement().mnemonic()?.GetText() ?? "«??»";
        _results.AppendLine($"[bd_statement] {inv} {sta} {t}");
        Cont:
        return base.VisitBd_statement(context);
    }


    public override string VisitBi_statement(GramParser.Bi_statementContext context)
    {
        if (context.exception != null || ContainsErrorNode(context.children))
        {
            var ct = string.Join(" ", GetChildrenText(context.children));
            _results.AppendLine($"⚠ [bi_statement] {ct}");
            goto Cont;
        }

        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;
        string ind = context.ai_statement().I40_indirect().GetText();

        var t = context.ai_statement().t.Text;
        _results.AppendLine($"[bi_statement] {inv} {sta} {ind} {t}");
    Cont:
        return base.VisitBi_statement(context);
    }

    // Visit tdd, tdi, tid, tii

    public override string VisitInv_statement(GramParser.Inv_statementContext context)
    {
        string sta = context.sta.Text;

        _results.AppendLine($"[Invert] {sta}");
        return base.VisitInv_statement(context);
    }

    public override string VisitErrorNode(IErrorNode node)
    {
        _results.AppendLine($"⚠ [ErrorNode] {node.GetText()}");
        var res = base.VisitErrorNode(node);
        return res;
    }
}