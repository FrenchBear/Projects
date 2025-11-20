using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SimpleParser;

public class Program
{
    public static void Main(string[] args)
    {
        string input = "x≥t";


        // 2. The ANTLR parsing pipeline
        var inputStream = new AntlrInputStream(input);
        var lexer = new piLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new piParser(tokenStream);


        // 3. Set up our custom error listener for validation
        var myParserErrorListener = new MyParserErrorListener();
        parser.RemoveErrorListeners(); // Remove default console error listener
        parser.AddErrorListener(myParserErrorListener); // Add our custom one

        var myLexerErrorListener = new MyLexerErrorListener();
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(myLexerErrorListener);

        // 4. Start parsing at the "start rule"
        //    !!! YOU MUST CHANGE THIS !!!
        //    Replace 'program' with the name of your grammar's first rule.
        //    It's case-sensitive! Find it at the top of pi.g4.
        IParseTree tree = parser.main();

        // Check for validation
        if (myLexerErrorListener.HadError || myParserErrorListener.HadError)
        {
            Console.WriteLine("Validation FAILED.");
            // Don't run the visitor if the syntax is bad
            return;
        }
        else
            Console.WriteLine("Validation SUCCESSFUL.");

        // Colorization visitor (only analyzing terminals)
        Console.WriteLine("\nRunning colorization visitor on the parse tree...");
        var terminalVisitor = new MypiVisitorTerminal(parser);
        terminalVisitor.Visit(tree);
        Console.WriteLine("\nColorization visitor finished.\n");
    }
}