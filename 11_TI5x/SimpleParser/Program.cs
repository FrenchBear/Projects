using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SimpleParser;

public class Program
{
    public static void Main(string[] args)
    {
        /*
        // 1. Check for input file
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: SimpleParser.exe <path_to_file>");
            return;
        }

        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File not found '{filePath}'");
            return;
        }
        */

        const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\Master LIbrary\ML-25.t59";
        //const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\Master LIbrary\all.t59";
        //const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\indirect.t59";

        string input = File.ReadAllText(filePath);
        Console.WriteLine($"Parsing file: {filePath}...");

        //input = "1234\n1.414\n.333\nLBL A\n-6.02e23\n1E10\n1E+10\n-1234567890e-79";
        //input = "1 . 6 +/- EE 1 9 +/-";
        input = "Lbl STA SIG+ GTO Σ+";

        // 2. The ANTLR parsing pipeline
        var inputStream = new AntlrInputStream(input);
        var lexer = new ti58Lexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ti58Parser(tokenStream);


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
        //    It's case-sensitive! Find it at the top of ti58.g4.
        IParseTree tree = parser.startRule();

        // Check for validation
        if (myLexerErrorListener.HadError || myParserErrorListener.HadError)
        {
            Console.WriteLine("Validation FAILED.");
            // Don't run the visitor if the syntax is bad
            return;
        }
        else
            Console.WriteLine("Validation successful.\n");

        // Colorization visitor (only analyzing terminals)
        var colorVisitor = new MyTi58VisitorBaseColorize(parser);
        colorVisitor.Visit(tree);

        // AST Visitor
        var astVisitor = new MyTi58VisitorBaseAST(parser);
        astVisitor.Visit(tree);
        astVisitor.PostProcessAST();
        astVisitor.PrintFormattedAST();
    }
}