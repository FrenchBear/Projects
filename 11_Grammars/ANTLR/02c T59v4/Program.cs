// Fourth variant of T59 grammar - parser
// Don't use INVALID_TOKEN anymore
//
// 2025-11-20   PV

using Antlr4.Runtime;
using System;

namespace LexerModes;

public static class Program
{
    public static void Main(string[] args)
    {
        string input = @"CLR ZAP CLR";

        Console.WriteLine("--- Parsing Input ---");
        Console.WriteLine(input);

        // Build antlr pipeline and parse input for rune program
        var inputStream = new AntlrInputStream(input);
        var lexer = new Vocab(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new GramParser(tokenStream);
        var tree = parser.program();
        var visitor = new MyGramVisitor();
        string result = visitor.Visit(tree);

        Console.WriteLine("\n--- Visitor Output ---");
        Console.WriteLine(result);
    }
}