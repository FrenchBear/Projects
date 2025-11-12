using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SimpleParser;

public static class Program
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

        const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\Master Library\ML-02.t59";
        //const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\Master Library\all.t59";
        //const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\indirect.t59";

        //#pragma warning disable IDE0059 // Unnecessary assignment of a value
        string input = File.ReadAllText(filePath);
        Console.WriteLine($"Parsing file: {filePath}...");

        //input = "1234\n1.414\n.333\nLBL A\n-6.02e23\n1E10\n1E+10\n-1234567890e-79";
        //input = "1 . 6 +/- EE 1 9 +/-";
        //input = "# Test program\nLbl STA inv sig+ Clr GTO Σ+\nIFF Ind 12 123 Dsz 3 01 23\nLNX";
        //input = "STO 12 SUM IND 25 RCL 3 RC* 5\nSTF 3 INV STF 7 STF IND 7 IFF 3 CLR INV IFF 7 IND 2\nDsz 3 |x| Lbl STA INV DSZ Ind 7 421\nGTO 001 GTO 12 GTO 123 GTO 01 23 GO* 7";

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
        Console.WriteLine();

        // Ast Visitor
        var astVisitor = new MyTi58VisitorBaseAst(parser);
        astVisitor.Visit(tree);
        astVisitor.PostProcessAst();
        astVisitor.PrintFormattedAst();
    }
}