using Antlr4.Runtime;
using System.IO;

namespace SimpleParser;

// Inherit from BaseErrorListener to override the default error handling
public class MyErrorListener : BaseErrorListener
{
    // Public property to check if an error occurred
    public bool HadError { get; private set; } = false;

    // Override the SyntaxError method
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        // Set the flag to true
        HadError = true;

        // You can also log the error to the console if you want
        System.Console.Error.WriteLine($"ERROR: line {line}:{charPositionInLine} {msg}");
    }
}