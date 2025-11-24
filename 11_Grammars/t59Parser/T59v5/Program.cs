// Fourth variant of T59 grammar - Parser
// Don't use INVALID_TOKEN anymore
//
// 2025-11-20   PV

using Antlr4.Runtime;
using System;

namespace T59v5;

public static class Program
{
    public static void Main(string[] args)
    {
        string input = @"LBL 99 Lbl STO CLR CE STO 12 STO IND 42 RCL 5
cos INV Sin INV CLR
Nop sin INV cos CLR

1 12 123 1234
CLS
STO 12 RCL IND 4 SUM 7 INV SUM 8 SUM IND 9 INV SUM IND 10 
INV CLR inv tan 
HIR 40 SM* 4 INV SM* 12
GTO 25 GTO CLR GTO IND 33 GTO 123 GTO 02 45 INV x=t Ind 12

SUM 12 SUM IND 12 SUM Z SUM IND Z
INV SUM 12 INV SUM IND 12 INV SUM Z INV SUM IND Z
SAM INV SAM

HIR 40 INV SM* 12 HIR Z RC* Z INV SM* Z

GTO CLR GTO 25 GTO IND 04 GTO 421 GTO 04 21 GTO 10 10 GTO Z GTO IND Z GTO 04 CLR

Dsz 0 CLR

Dsz 0 CLR Dsz 1 123 DSZ 2 01 23 DSZ 12 11 23 DSZ 3 25 DSZ 4 Ind 12
Iff Ind 10 CLR Iff Ind 11 123 Iff Ind 12 01 23 Iff Ind 22 11 23 Iff Ind 13 25 Iff Ind 14 Ind 12
DSZ Z CLR DSZ Ind Z CLR DSZ 3 Z CLR DSZ 3 Ind Z CLR

12345 CLR INV Cos CLS INV CLS
STO 12 Sto Ind 4 INV SUM Ind 7 Sto Z Sto Ind Z Sto Ind Ind 12 Sto Ind Ind Z

PAU POS

GTO CLR GTO 25 GTO 123 GTO 01 23 GTO IND 7
GTO 7 GTO Z GTO Ind Z GTO 43 21 GTO IND IND 25 GTO IND IND IND 25

Sto Ind Z CLR";

        input = @"Fix 4 INV Fix SBR 240 INV SBR";

        //Console.WriteLine("--- Parsing Input ---");
        //Console.WriteLine(input);

        // Build antlr pipeline and parse input for rune program
        var inputStream = new AntlrInputStream(input);
        var lexer = new Vocab(inputStream);

        var sp = new SourcePainter(input);

        // Set up custom error listener for validation
        bool showErrors = true;
        var myLexerErrorListener = new MyLexerErrorListener(sp, showErrors);
        lexer.RemoveErrorListeners();       // Remove default console error listener
        lexer.AddErrorListener(myLexerErrorListener);

        var l1t = new L1Tokenizer(lexer);
        //Console.WriteLine("=== L1 Tokens ===");
        //foreach (L1Token t1 in l1t.EnumerateL1Tokens())
        //    Console.WriteLine(t1.AsString());

        var l2p = new L2Parser(l1t);
        Console.WriteLine("=== L2 Statements ===");
        foreach (L2StatementBase l2s in l2p.EnumerateL2Statements())
            Console.WriteLine(l2s.AsString());
    }
}