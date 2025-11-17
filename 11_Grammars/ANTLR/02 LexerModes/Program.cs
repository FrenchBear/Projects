using Antlr4.Runtime;
using System;

namespace LexerModes;

public static class Program
{
    public static void Main(string[] args)
    {
        // Define an input that uses both of your statement types
        string inputText = "STO 42 LBL 99 STO 5";

        Console.WriteLine("--- Parsing Input ---");
        Console.WriteLine(inputText);

        // 1. Create an ANTLR input stream
        var inputStream = new AntlrInputStream(inputText);

        // 2. Create the Lexer (using Vocab.cs)
        var lexer = new Vocab(inputStream);

        // 3. Create a token stream
        var tokenStream = new CommonTokenStream(lexer);

        // 4. Create the Parser (using Gram.cs)
        var parser = new GramParser(tokenStream);

        // 5. Start parsing at the 'program' rule
        var tree = parser.program();

        // 6. Create our custom visitor
        var visitor = new MyGramVisitor();

        // 7. Walk the tree with the visitor
        string result = visitor.Visit(tree);

        // 8. Print the visitor's output
        Console.WriteLine("\n--- Visitor Output ---");
        Console.WriteLine(result);

        /*
        Expected Output:

            --- Parsing Input ---
            STO 42 LBL 99 STO 5
           
            --- Visitor Output ---
            [Storage] Keyword: STO, Address: 42
            [Other] Label: LBL, Number: 99
            [Storage] Keyword: STO, Address: 5
        */
    }
}