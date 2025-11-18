using Antlr4.Runtime;
using System;

namespace LexerModes;

public static class Program
{
    public static void Main(string[] args)
    {
        //string inputText = "STO 42 LBL 99 STO Ind 5 CLR";
        //string inputText = "LBL 99 Lbl STO CLR CE STO 12 STO IND 42 RCL 5";
        //string inputText = "cos INV Sin INV CLR";
        string inputText = @"1 12 123 1234
CLS
STO 12 RCL IND 4 SUM 7 INV SUM 8 SUM IND 9 INV SUM IND 10 
INV CLR inv tan 
HIR 40 SM* 4 INV SM* 12
GTO 25 GTO CLR GTO IND 33 GTO 123 GTO 02 45 INV x=t Ind 12";

        //inputText = "CLS STO Z";

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