// 5th variant of T59 grammar - MyErrorListeners
// Classes to manage errors for lexer during analysis
//
// 2025-11-10   PV

using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace T59v5;

internal class MyLexerErrorListener(SourcePainter sp, bool showErrors): IAntlrErrorListener<int>
{
    public bool HadError { get; private set; }
    public List<string> Errors = new List<string>();

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        HadError = true;
        Errors.Add($"Lexer Error at {line}:{charPositionInLine} -> {msg}");
        return;


        // Print error to the console
        if (showErrors)
            Console.Error.WriteLine($"*** Lexer Error: line {line}:{charPositionInLine} -> {msg}");

        //sp.Paint(line, charPositionInLine, SyntaxCategory.LexerError);

        // 1. Cast the recognizer to Lexer to get access to the stream
        var lexer = (Lexer)recognizer;
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
                    sp.Paint(line, charPositionInLine + i, SyntaxCategory.Invalid);
                //Debugger.Break();
                //errorText = input.GetText(new Antlr4.Runtime.Misc.Interval(startIndex, stopIndex));
            }
        }
        else
        {
            // Fallback if exception is missing or different type (rare in Lexer)
            // Just grab the current character
            sp.Paint(line, charPositionInLine, SyntaxCategory.Invalid);
        }

    }
}
