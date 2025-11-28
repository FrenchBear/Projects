// 5th variant of T59 grammar - MyErrorListeners
// Classes to manage errors for lexer during analysis
//
// 2025-11-10   PV

using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;

#pragma warning disable CS9113 // Parameter is unread.

namespace T59Core;

internal sealed class MyLexerErrorListener(bool showErrors): IAntlrErrorListener<int>
{
    public bool HadError { get; private set; }
    public List<string> Errors = [];

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        HadError = true;
        Errors.Add($"Lexer Error at {line}:{charPositionInLine} -> {msg}");
    }
}
