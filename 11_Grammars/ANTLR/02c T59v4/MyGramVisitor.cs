// Fourth variant of T59 grammar - parser
// Don't use INVALID_TOKEN anymore
//
// 2025-11-20   PV

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace LexerModes;

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

    public override string VisitDi_statement(GramParser.Di_statementContext context)
    {
        // Probably need to group all errors as here, errors nuances are irrelevant
        if (context.exception != null || ContainsErrorNode(context.children) || context.INVALID_TOKEN() != null)
        {
            var ct = string.Join(" ", GetChildrenText(context.children));
            _results.AppendLine($"[%%% DD or ind DD] {ct}");
            goto Cont;
        }

        // No flag means statement Ok, parsing normal
        // $$$ means that the statement contains an invalid token, but full rule has been parsed correctly (Sto Ind Ind 12)
        // ERR means that the statement contains an invalid token (Sto ZZZ) and the rule hadn't been parsed correctly
        // $$$+ERR means that the statement contains an invalid token, and parsing was incomplete (Sto Ind Z CLR)
        // %%% means an exception in the context, it's a variant of ERR, statement hasn't been parsed correctly
        bool containsErr = ContainsErrorNode(context.children);
        var cer = containsErr ? "$$$ " : "";

        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;
        string ind = context.ind?.Text ?? "";
        bool err = context.INVALID_TOKEN() != null;
        string target = context.t?.Text ?? context.INVALID_TOKEN()?.GetText() ?? "??";

        string errs = err ? "ERR " : "";
        // In case of $$$ (statement contains an invalid token), it's not printed here...
        _results.AppendLine($"[{cer}{errs}DD or ind DD] {inv} {sta} {ind} {target}");

    // but during the recursive descent, in VisitErrorNode
    Cont:
        return base.VisitDi_statement(context);
    }

    private List<string> GetChildrenText(IList<IParseTree> contextChildren)
    {
        List<string> res = [];
        foreach (var ch in contextChildren)
            switch (ch)
            {
                case IErrorNode ien:
                    res.Add($"«{ien.GetText()}»");
                    break;

                case ITerminalNode itn:
                    if (itn.Symbol.Type==Vocab.INVALID_TOKEN)
                        res.Add($"‹{itn.GetText()}›");
                    else
                        res.Add(itn.GetText());
                    break;

                case ParserRuleContext ruleContext:
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
                case ParserRuleContext ruleContext when ContainsErrorNode(ruleContext.children):
                    return true;
            }
        return false;
    }

    public override string VisitD_statement(GramParser.D_statementContext context)
    {
        if (context.exception != null)
        {
            var ct = string.Join(" ", GetChildrenText(context.children));
            _results.AppendLine($"[%%% DD] {ct}");
            goto Cont;
        }

        var cer = ContainsErrorNode(context.children) ? "$$$ " : "";

        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;
        bool err = context.INVALID_TOKEN() != null;
        string target = err ? $"«{context.INVALID_TOKEN().GetText()}»" : context.t.Text;

        string errs = err ? "ERR " : "";
        _results.AppendLine($"[{cer}{errs}DD] {inv} {sta} {target}");
    Cont:
        return base.VisitD_statement(context);
    }

    public override string VisitLai_statement(GramParser.Lai_statementContext context)
    {
        if (context.exception != null)
        {
            var ct = string.Join(" ", GetChildrenText(context.children));
            _results.AppendLine($"[%%% Lai] {ct}");
            goto Cont;
        }

        var cer = ContainsErrorNode(context.children) ? "$$$ " : "";
        var errs = context.INVALID_TOKEN() != null ? "ERR: " : "";
        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;
        string ind = context.I40_indirect()?.GetText() ?? "";

        var arg = context.GetChild(1).GetText();    // Without INV prefix
        var t1 = context.t?.Text;
        var t2 = context.mnemonic()?.GetText();
        var t3 = context.INVALID_TOKEN()?.GetText();

        var t = context.t?.Text ?? context.mnemonic()?.GetText() ?? $"«{context.INVALID_TOKEN()?.GetText()}»";
        _results.AppendLine($"[{cer}{errs}Lai] {inv} {sta} {ind} {t}");
    Cont:
        return base.VisitLai_statement(context);
    }

    /*
    public override string VisitLai_statement(GramParser.Lai_statementContext context)
    {
        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;

        if (context.MNEMONIC3() != null)
        {
            var target = context.MNEMONIC3().GetText();
            _results.AppendLine($"[Lai LabelMnemonic] {inv} {sta}, dest: {target}");
        }
        else if (context.INDIRECT3() != null)   // Must be tested before DD3
        {
            var ind = context.INDIRECT3();
            if (context.DD3() != null)
            {
                var target = context.DD3().GetText();
                _results.AppendLine($"[Lai Indirect] {inv} {sta}, dest: {ind} {target}");
            }
            else
            {
                var err = context.INVALID3().GetText();
                _results.AppendLine($"[ERR: Lai Indirect] {inv} {sta}, dest: {ind} {err}");
            }
        }
        else if (context.DD3() != null)
        {
            var labNum = context.DD3().GetText();
            if (labNum.StartsWith('0'))
                _results.AppendLine($"[ERR: Lai LabelNum] {inv} {sta}, dest: {labNum}");
            else
                _results.AppendLine($"[Lai LabelNum] {inv} {sta}, dest: {labNum}");
        }
        else if (context.ADD3a() != null)
        {
            var addr = context.ADD3a().GetText();
            _results.AppendLine($"[Lai Addr A] {inv} {sta}, dest: {addr}");
        }
        else if (context.ADD3b() != null)
        {
            var addr = context.ADD3b().GetText();
            _results.AppendLine($"[Lai Addr B] {inv} {sta}, dest: {addr}");
        }
        else if (context.INVALID3() != null)
        {
            var err = context.INVALID3().GetText();
            _results.AppendLine($"[ERR: Lai] {inv} {sta}, dest: {err}");
        }
        else
        {
            Debugger.Break();
        }

        return base.VisitLai_statement(context);
    }


    // Use a simpler approach than VisitLai_statement
    public override string VisitDoi_lai_statement(GramParser.Doi_lai_statementContext context)
    {
        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;

        if (context.INVALID4a() != null || context.INVALID4b() != null)
        {
            _results.AppendLine($"[ERR: Doi_lai] {context.GetText()}");     // Puts the whole instruction in error
        }
        else
        {
            var ind1 = context.INDIRECT4a()?.GetText() ?? "";
            var d1 = context.DD4a().GetText();

            string? j1 = context.MNEMONIC4()?.GetText();
            string? j2 = context.ADD4a()?.GetText();
            string? j3 = context.ADD4b()?.GetText();
            var ind2 = context.INDIRECT4b()?.GetText() ?? "";
            string? d2 = context.DD4b()?.GetText();

            var line = $"{inv} {sta} {ind1} {d1} {j1 ?? j2 ?? j3 ?? ind2 + " " + d2}";
            _results.AppendLine($"[Doi_lai] {line}");
        }

        return base.VisitDoi_lai_statement(context);
    }

    public override string VisitLabel_statement(GramParser.Label_statementContext context)
    {
        string sta = context.sta.Text;

        if (context.DD5() != null)
        {
            var labNum = context.DD5().GetText();
            _results.AppendLine($"[Label LabelNum] {sta} {labNum}");
        }
        else if (context.MNEMONIC5() != null)
        {
            var mnemonic = context.MNEMONIC5().GetText();
            _results.AppendLine($"[Label LabelMnemonic] {sta} {mnemonic}");
        }
        else
        {
            var err = $"«{context.INVALID5().GetText()}»";
            _results.AppendLine($"[Err: Label] {sta} {err}");
        }

        return base.VisitLabel_statement(context);
    }
   */

    public override string VisitInv_statement(GramParser.Inv_statementContext context)
    {
        string sta = context.sta.Text;

        _results.AppendLine($"[Invert] {sta}");
        return base.VisitInv_statement(context);
    }

    // Invalid element at the main level (ex: CLS instead of CLR)
    public override string VisitUnknown_statement(GramParser.Unknown_statementContext context)
    {
        var err = $"«{context.GetText()}»";
        _results.AppendLine($"[ERR: Unknown statement]: {err}");
        return base.VisitUnknown_statement(context);
    }

    public override string VisitErrorNode(IErrorNode node)
    {
        _results.AppendLine($"[ErrorNode] {node.GetText()}");
        var res = base.VisitErrorNode(node);
        return res;
    }
}