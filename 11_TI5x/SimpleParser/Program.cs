// SimpleParser
// Experiments on an antlr4 grammar for TI-5x programming language
// 
// 2025-11-10   PV

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

        //const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\Master Library\ML-02.t59";         // Long program
        //const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\Master Library\ML-25.t59";       // Program with numeric constants
        const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\Master Library\ML-01.t59";
        //const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\Master Library\all.t59";
        //const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\indirect.t59";

        //#pragma warning disable IDE0059 // Unnecessary assignment of a value
        string input = File.ReadAllText(filePath);
        Console.WriteLine($"Parsing file: {filePath}...");

        //input = "1234\n1.414\n.333\nLBL A\n-6.02e23\n1E10\n1E+10\n-1234567890e-79";
        //input = "1 . 6 +/- EE 1 9 +/-";
        //input = "# Test program\nLbl STA inv sig+ Clr GTO Σ+\nIFF Ind 12 123 Dsz 3 01 23\nLNX";
        //input = "STO 12 SUM IND 25 RCL 3 RC* 5\nSTF 3 INV STF 7 STF IND 7 IFF 3 CLR INV IFF 7 IND 2\nDsz 3 |x| Lbl STA INV DSZ Ind 7 421\nGTO 001 GTO 12 GTO 123 GTO 01 23 GO* 7\nINV SBR RTN INV!";
        //input = "SUM IND 25 SBR 25 SBR CLR GTO 25 GTO IND 25 GO* 25 SBR IND 25 SBR 250";
        //input = "GTO IND 25";
        //input = "STO IND 25";
        //input = "123 45 6\n12 . 34 +/- EE +/- 12";
        //input = "LBL A 1234567890 LBL B";
        //input = "STO 12 RTN END SIG+ INV Dsz 03 123 END Nop";
        //input = ". +/- 5";

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

        // Build Ast, print clean listing and table of labels
        var astVisitor = new MyTi58VisitorAstBuilder(parser);
        var programs = astVisitor.BuildPrograms(tree);

        // Post_processing
        var astPostProcessor = new AstPostProcessor();
        foreach (var program in programs)
            astPostProcessor.PostProcessAst(program);

        // Printing
        var astPrinter = new AstPrinter();
        foreach (var program in programs)
        {
            Console.WriteLine("\n--------\nDetailed listing");
            astPrinter.PrintFormattedAst(program);
            Console.WriteLine("\nLabels");
            astPrinter.PrintFormattedAst(program, true);
        }
    }
}
