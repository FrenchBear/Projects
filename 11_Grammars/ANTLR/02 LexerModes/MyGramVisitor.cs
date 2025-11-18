using Antlr4.Runtime.Tree;
using System;
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
        string number = context.NUMBER().GetText();
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
        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;
        string ind = context.ind?.Text ?? "";
        bool err = context.INVALID1() != null;
        string target = err ? $"«{context.INVALID1().GetText()}»" : context.DD1()?.GetText();

        string errs = err ? "ERR: " : "";
        _results.AppendLine($"[{errs}DD or ind DD] {inv} {sta} {ind} {target}");
        return base.VisitDi_statement(context);
    }

    public override string VisitD_statement(GramParser.D_statementContext context)
    {
        string inv = context.inv?.Text ?? "";
        string sta = context.sta.Text;
        bool err = context.INVALID2() != null;
        string target = err ? $"«{context.INVALID2().GetText()}»" : context.DD2()?.GetText();

        string errs = err ? "ERR: " : "";
        _results.AppendLine($"[{errs}DD] {inv} {sta} {target}");

        return base.VisitD_statement(context);
    }

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

    public override string VisitInv_statement(GramParser.Inv_statementContext context)
    {
        string sta = context.sta.Text;

        _results.AppendLine($"[Invert] {sta}");
        return base.VisitInv_statement(context);
    }

    public override string VisitUnknown_statement(GramParser.Unknown_statementContext context)
    {
        var err = $"«{context.GetText()}»";
        _results.AppendLine($"[ERR: Unknown statement]: {err}");
        return base.VisitUnknown_statement(context);
    }
}