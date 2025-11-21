// Fourth variant of T59 grammar - Parser
// Don't use INVALID_TOKEN anymore
//
// 2025-11-20   PV

using Antlr4.Runtime;
using System;

namespace T59v4;

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

        input = @"Nop Sto 12 Sto Ind 12 HIR 7 ST* 8 Lbl CLR Lbl 25";
        input = @"1 12 123 01 23 1234";
        input = @"GTO CLR GTO 25 GTO 123 GTO 01 23 GTO IND 12";
        input = @"Lbl CLR Lbl 25";
        input = @"DSZ 1 CLR Dsz 2 25 DSZ 3 125 DSZ 4 01 25 DSZ 11 Ind 12";
        input = @"DSZ 11 25 Dsz 11 Ind 25 Dsz Ind 11 25 Dsz Ind 11 Ind 25";
        input = @"Lbl CLR Lbl 25";
        input = "Fix 4 Fix 04 Fix Ind 12 INV Fix CLR";
        input = @"Hir Ind 09";
        //input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\all-1.t59");
        //input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\all-2.t59");
        //input = "Lbl A Sto 0 CLR @Loop: 1 + Dsz 0 @LOOP R/S";
        input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\Master Library\ML-01-!ALL.t59");
        //input = @"DSZ 01 00 15";
        //input = @"DSZ 05 12 25";

        //Console.WriteLine("--- Parsing Input ---");
        //Console.WriteLine(input);

        // Build antlr pipeline and parse input for rune program
        var inputStream = new AntlrInputStream(input);
        var lexer = new Vocab(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new GramParser(tokenStream);

        var sp = new SourcePainter(input);

        // Set up custom error listener for validation
        bool showErrors = true;
        var myLexerErrorListener = new MyLexerErrorListener(sp, showErrors);
        lexer.RemoveErrorListeners();       // Remove default console error listener
        lexer.AddErrorListener(myLexerErrorListener);
        var myParserErrorListener = new MyParserErrorListener(sp, showErrors);
        parser.RemoveErrorListeners(); 
        parser.AddErrorListener(myParserErrorListener);

        var tree = parser.program();

        var paintingVisitor = new MyPaintingVisitor();
        paintingVisitor.VisitTerminals(parser, tree, sp);

        Console.WriteLine("--- Painter output ---");
        sp.Print();

        /*
        var gramVisitor = new MyGramVisitor();
        string result = gramVisitor.Visit(tree);

        Console.WriteLine("\n--- Visitor Output ---");
        Console.WriteLine(result);
        */
    }
}