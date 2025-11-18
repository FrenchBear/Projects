//using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Tree;
using System;
using System.Net;
using System.Text;
using System.Xml.Linq;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace LexerModes;

// Note: If you add a @namespace{MyProject.Parsing} header
// to your .g4 files, you'll need to import that namespace here.

// Inherit from the generated base visitor for your 'Gram' parser
public class MyGramVisitor: GramBaseVisitor<string>
{
    private readonly StringBuilder _results = new();

    // This is called for the 'program' rule
    public override string VisitProgram(GramParser.ProgramContext context)
    {
        // Visit all children (the 'statement' nodes)
        base.VisitProgram(context);
        // Return the final string we built
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
        string invert = "";
        int statementIx = 0;
        if (context.inv != null)
        {
            invert = context.inv?.Text;
            statementIx = 1;
        }

        string keyword = context.GetChild(statementIx).GetText();
        _results.AppendLine($"[Atomic] {invert} Keyword: {keyword}");
        return base.VisitAtomic_statement(context);
    }

    public override string VisitDi_statement(GramParser.Di_statementContext context)
    {
        string invert = "";
        int statementIx = 0;
        if (context.inv != null)
        {
            invert = context.inv?.Text;
            statementIx = 1;
        }

        string keyword = context.GetChild(statementIx).GetText();
        string ind = context.INDIRECT1()?.GetText() ?? "";
        string address = context.DD1().GetText();

        _results.AppendLine($"[DD or ind DD] {invert} Keyword: {keyword}, Address: {ind} {address}");

        return base.VisitDi_statement(context);
    }

    public override string VisitD_statement(GramParser.D_statementContext context)
    {
        string invert = "";
        int statementIx = 0;
        if (context.inv != null)
        {
            invert = context.inv?.Text;
            statementIx = 1;
        }

        string keyword = context.GetChild(statementIx).GetText();
        string address = context.DD2().GetText();

        _results.AppendLine($"[DD] {invert} Keyword: {keyword}, Address: {address}");

        return base.VisitD_statement(context);
    }

    public override string VisitLai_statement(GramParser.Lai_statementContext context)
    {
        string invert = "";
        int statementIx = 0;
        if (context.inv != null)
        {
            invert = context.inv?.Text;
            statementIx = 1;
        }
        string keyword = context.GetChild(statementIx).GetText();

        if (context.MNEMONIC3() != null)
        {
            // mnemonic
            var mnemonic = context.MNEMONIC3().GetText();
            _results.AppendLine($"[Lai Mnemonic] {invert} {keyword}, dest: {mnemonic}");
        } else if (context.ADD3b() != null)
        {
            var addr = context.ADD3b().GetText();
            _results.AppendLine($"[Lai Addr B] {invert} {keyword}, dest: {addr}");
        }
        else if (context.ADD3a() != null)
        {
            var addr = context.ADD3a().GetText();
            _results.AppendLine($"[Lai Addr A] {invert} {keyword}, dest: {addr}");
        }
        else if (context.INDIRECT3()!=null)
        {
            // ATTENTION, INDIRECT3 text includes spaces
            string ind = context.INDIRECT3().GetText();
            string reg = context.DD3()?.GetText() ?? "";
            _results.AppendLine($"[Lai Ind reg] {invert} {keyword}, {ind} {reg}");
        }
        else
        {
            string numtag = context.DD3().GetText();
            _results.AppendLine($"[Lai Num tag] {invert} {keyword}, {numtag}");
        }

        return base.VisitLai_statement(context);
    }

    public override string VisitInv_statement(GramParser.Inv_statementContext context)
    {
        string keyword = context.I22_invert().GetText();
        _results.AppendLine($"[Invert] Keyword: {keyword}");
        return base.VisitInv_statement(context);
    }

    // This is called when the parser matches an 'other_statement'
    public override string VisitLabel_statement(GramParser.Label_statementContext context)
    {
        // Get the text of the 'LBL' and 'NUMBER' tokens
        // Note: ANTLR creates a method for the 'LBL' literal
        string label = context.I76_label().GetText();
        //string tag = context.NUMBER().GetText();
        var tagType = context.tag.Type;
        string tagText;
        if (tagType == Vocab.DD5)
            tagText = $"TAG_NUMBER: {context.DD5().GetText()}";
        else //if (tagType == Vocab.MNEMONIC4)
            tagText = $"TAG_MNEMONIC: {context.MNEMONIC5().GetText()}";

        _results.AppendLine($"Label: {label} {tagText}");

        return base.VisitLabel_statement(context);
    }

    public override string VisitToken_error(GramParser.Token_errorContext context)
    {
        _results.AppendLine($"Token error: {context.GetText()}");
        return base.VisitToken_error(context);
    }

    //public override string VisitErrorNode(IErrorNode node)
    //{
    //    //if (node.Symbol.Type == Vocab.INVALID_TOKEN)
    //    _results.AppendLine($"ErrorNode: {node.GetText()}");
    //    return base.VisitErrorNode(node);
    //}

    //public override string VisitTerminal(ITerminalNode node)
    //{
    //    _results.AppendLine($"Terminal: {node.GetText()}");
    //    return base.VisitTerminal(node);
    //}
}