// T59Processor
// Convert source code into a list of T59Program
//
// 2025-11-28   PV

using Antlr4.Runtime;
using System.Collections.Generic;

namespace T59v7Core;

public class T59Processor
{
    public static List<T59Program> GetPrograms(string input)
    {
        // Build antlr pipeline and parse input for rune program
        var inputStream = new AntlrInputStream(input);
        var lexer = new Vocab(inputStream);

        // Set up custom error listener for validation
        bool showErrors = true;
        var myLexerErrorListener = new MyLexerErrorListener(showErrors);
        lexer.RemoveErrorListeners();       // Remove default console error listener
        lexer.AddErrorListener(myLexerErrorListener);

        // Clean the list of tokens returned by Vocab lexter into L1Tokens, easier to manage in later stages
        var l1t = new L1Tokenizer(lexer);
        var programs = l1t.GetPrograms();

        // Build statements from tokens
        foreach (var p in programs)
            new L2aParser(p).L2Process();

        // Generate OpCodes and addresses
        foreach (var p in programs)
            new L2bEncoder(p).L3Process();

        return programs;
    }
}
