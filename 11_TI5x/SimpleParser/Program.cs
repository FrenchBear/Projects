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

        //const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\sum.t59";
        const string filePath = @"C:\Development\GitHub\Projects\11_TI5x\Master Library\ML-02.T59";

        string input = File.ReadAllText(filePath);
        Console.WriteLine($"Parsing file: {filePath}...");

        input = "# Comment\n3.1416\nCLR\ne^x\nSin\nINVSin\nINV     Cos\n";

        // 2. The ANTLR parsing pipeline
        var inputStream = new AntlrInputStream(input);
        var lexer = new ti58Lexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ti58Parser(tokenStream);

        // 3. Set up our custom error listener for validation
        parser.RemoveErrorListeners(); // Remove default console error listener
        var errorListener = new MyErrorListener();
        parser.AddErrorListener(errorListener); // Add our custom one

        // 4. Start parsing at the "start rule"
        //    !!! YOU MUST CHANGE THIS !!!
        //    Replace 'program' with the name of your grammar's first rule.
        //    It's case-sensitive! Find it at the top of ti58.g4.
        IParseTree tree = parser.startRule();

        // Check for validation
        if (errorListener.HadError)
        {
            Console.WriteLine("Validation FAILED.");
            // Don't run the visitor if the syntax is bad
            return;
        }
        else
            Console.WriteLine("Validation SUCCESSFUL.");

        // Colorization visitor (only analyzing terminals)
        Console.WriteLine("\nRunning colorization visitor on the parse tree...");
        var colorVisitor = new MyTi58VisitorBaseColorize(parser);
        colorVisitor.Visit(tree);
        Console.WriteLine("\nColorization visitor finished.\n");

        // AST Visitor
        var astVisitor = new MyTi58VisitorBaseAST(parser);
        astVisitor.Visit(tree);
        astVisitor.PrintAST();
    }
}