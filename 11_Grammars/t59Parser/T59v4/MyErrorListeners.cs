// Fourth variant of T59 grammar - MyErrorListeners
// Classes to manage errors for lexer and parser during analysis
//
// 2025-11-10   PV

using System;
using System.Diagnostics;
using Antlr4.Runtime;
using System.IO;

namespace T59v4;

internal class MyLexerErrorListener(SourcePainter sp, bool showErrors): IAntlrErrorListener<int>
{
    public bool HadError { get; private set; }

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        HadError = true;

        // Print error to the console
        if (showErrors)
            Console.Error.WriteLine($"*** Lexer Error: line {line}:{charPositionInLine} -> {msg}");

        //sp.Paint(line, charPositionInLine, SyntaxCategory.LexerError);

        // 1. Cast the recognizer to Lexer to get access to the stream
        Lexer lexer = (Lexer)recognizer;
        var input = lexer.InputStream;

        // 2. Calculate the text using the exception's start index
        // LexerNoViableAltException contains the index where the token attempt started
        string errorText = "";

        if (e is LexerNoViableAltException lexerError)
        {
            // Get text from the start of the attempt to the current position
            // Note: input.Index is the current position of the stream
            int startIndex = lexerError.StartIndex;
            int stopIndex = input.Index;

            // Safety check constraints
            if (startIndex >= 0 && stopIndex >= startIndex)
            {
                for (int i = 0; i <= stopIndex - startIndex; i++)
                    sp.Paint(line, charPositionInLine + i, SyntaxCategory.LexerError);
                //Debugger.Break();
                //errorText = input.GetText(new Antlr4.Runtime.Misc.Interval(startIndex, stopIndex));
            }
        }
        else
        {
            // Fallback if exception is missing or different type (rare in Lexer)
            // Just grab the current character
            sp.Paint(line, charPositionInLine, SyntaxCategory.LexerError);
        }

    }
}

internal class MyParserErrorListener(SourcePainter sp, bool showErrors): BaseErrorListener
{
    // Public property to check if an error occurred
    public bool HadError { get; private set; }

    // Override the SyntaxError method
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        // Set the flag to true
        HadError = true;

        // Print error to the console
        if (showErrors)
            Console.Error.WriteLine($"*** Parser Error: line {line}:{charPositionInLine} os=«{offendingSymbol.Text}» -> {msg} / {e}");

        // Painting this error is completely useless since the error is reported at the following token, not the one actually in error
        // Since parsing will restart at this point, this statement may be correct.
        // For instance, "Sto Sin", SyntaxError offendingToken is "Sin", but when restarting parsing, Sin is a valid instruction
        // In conclusion, MyParserErrorListener reports useful information in msg: "mismatched input 'Sin' expecting {'Ind', D1, D2}"
        // but this is not really helpful for painting

        //for (int i = 0; i <= offendingSymbol.StopIndex - offendingSymbol.StartIndex; i++)
        //    sp.PaintParserError(line, charPositionInLine + i);
    }
}

