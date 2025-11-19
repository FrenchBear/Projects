using Antlr4.Runtime;
using System;

namespace LexerModes;

public static class Program
{
    public static void Main(string[] args)
    {
        //string input = "LBL 99 Lbl STO CLR CE STO 12 STO IND 42 RCL 5";
        //string input = "cos INV Sin INV CLR";
        string input = @"Nop sin INV cos CLR";

        input = @"1 12 123 1234
CLS
STO 12 RCL IND 4 SUM 7 INV SUM 8 SUM IND 9 INV SUM IND 10 
INV CLR inv tan 
HIR 40 SM* 4 INV SM* 12
GTO 25 GTO CLR GTO IND 33 GTO 123 GTO 02 45 INV x=t Ind 12";

        input = @"SUM 12 SUM IND 12 SUM Z SUM IND Z
                      INV SUM 12 INV SUM IND 12 INV SUM Z INV SUM IND Z
                      SAM INV SAM";

        input = @"HIR 40 INV SM* 12 HIR Z RC* Z INV SM* Z";

        input = @"GTO CLR GTO 25 GTO IND 04 GTO 421 GTO 04 21 GTO 10 10 GTO Z GTO IND Z GTO 04 CLR";

        input = "Dsz 0 CLR";

        input = @"Dsz 0 CLR Dsz 1 123 DSZ 2 01 23 DSZ 12 11 23 DSZ 3 25 DSZ 4 Ind 12
Iff Ind 10 CLR Iff Ind 11 123 Iff Ind 12 01 23 Iff Ind 22 11 23 Iff Ind 13 25 Iff Ind 14 Ind 12
DSZ Z CLR DSZ Ind Z CLR DSZ 3 Z CLR DSZ 3 Ind Z CLR";

        input = @"12345 CLR INV Cos CLS INV CLS
STO 12 Sto Ind 4 INV SUM Ind 7 Sto Z Sto Ind Z Sto Ind Ind 12 Sto Ind Ind Z";

        input = "PAU POS";

        input = @"GTO CLR GTO 25 GTO 123 GTO 01 23 GTO IND 7
GTO 7 GTO Z GTO Ind Z GTO 43 21 GTO IND IND 25 GTO IND IND IND 25";

        input = @"Sto Ind Z CLR";

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