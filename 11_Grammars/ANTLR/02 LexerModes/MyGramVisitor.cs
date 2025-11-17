//using Antlr4.Runtime.Tree;
using System.Text;

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

    // This is called when the parser matches a 'storage_statement'
    public override string VisitStorage_statement(GramParser.Storage_statementContext context)
    {
        // Get the text of the tokens from the context
        string keyword = context.STO_KEYWORD().GetText();
        string address = context.STO_ADDRESS().GetText();

        _results.AppendLine($"[Storage] Keyword: {keyword}, Address: {address}");

        return base.VisitStorage_statement(context);
    }

    // This is called when the parser matches an 'other_statement'
    public override string VisitOther_statement(GramParser.Other_statementContext context)
    {
        // Get the text of the 'LBL' and 'NUMBER' tokens
        // Note: ANTLR creates a method for the 'LBL' literal
        string label = context.LBL_KEYWORD().GetText();
        string number = context.NUMBER().GetText();

        _results.AppendLine($"[Other] Label: {label}, Number: {number}");

        return base.VisitOther_statement(context);
    }
}