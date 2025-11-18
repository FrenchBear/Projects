// Calc program
// Play with ANTR4 book examples
//
// 2025-11-17   PV
// 2025-11-19   PV      Added clear statement

using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SimpleParser;

public class Program
{
    public static void Main(string[] args)
    {
        string input;
        if (args.Length == 0)
            input = @"193
a = 5
b = 6
a+b*2
(1+2)*3
"; // Final newline required
        else
        {
            try
            {
                input = System.IO.File.ReadAllText(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading file {args[0]}: {e}");
                return;
            }
        }

        // ANTLR parsing pipeline
        var inputStream = new AntlrInputStream(input);
        var lexer = new LabeledExprLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new LabeledExprParser(tokenStream);


        // Set up our custom error listener for validation
        var myParserErrorListener = new MyParserErrorListener();
        parser.RemoveErrorListeners(); // Remove default console error listener
        parser.AddErrorListener(myParserErrorListener); // Add our custom one

        var myLexerErrorListener = new MyLexerErrorListener();
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(myLexerErrorListener);

        // Start parsing at the "start rule" prog
        IParseTree tree = parser.prog();

        // Check for validation
        if (myLexerErrorListener.HadError || myParserErrorListener.HadError)
        {
            Console.WriteLine("Validation FAILED.");
            // Don't run the visitor if the syntax is bad
            return;
        }
        else
            Console.WriteLine("Validation SUCCESSFUL.");

        // Eval visitor
        Console.WriteLine("\nRunning eval visitor on the parse tree...");
        var terminalVisitor = new EvalVisitor();
        var res = terminalVisitor.Visit(tree);
        //Console.WriteLine($"\nres: {res}");
    }
}
