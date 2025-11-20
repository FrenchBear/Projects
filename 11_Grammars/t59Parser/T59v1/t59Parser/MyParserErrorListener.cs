// MyParserErrorListener
// Classes to manage errors for lexer and parser during analysis
//
// 2025-11-10   PV

using Antlr4.Runtime;
using System.IO;

namespace SimpleParser;

// Inherit from BaseErrorListener to override the default error handling
public class MyParserErrorListener: BaseErrorListener
{
    // Public property to check if an error occurred
    public bool HadError { get; private set; }

    // Override the SyntaxError method
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        // Set the flag to true
        HadError = true;

        // You can also log the error to the console if you want
        System.Console.Error.WriteLine($"ERROR: line {line}:{charPositionInLine} {msg}");
    }
}

public class MyLexerErrorListener: IAntlrErrorListener<int>
{
    public bool HadError { get; private set; }

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        HadError = true;

        // You can also log the error to the console if you want
        System.Console.Error.WriteLine($"*** Lexer Error: line {line}:{charPositionInLine} {msg}");
    }
}