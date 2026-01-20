// 5th variant of T59 grammar - Parser
// Use my own parser to control parser recovery in case of error (invalid token, unexpected token) since
// it seems impossible to do in a reliable way using ANTLR parser
// I still use ANTLR4 lexer to generate initial flow of Vocab tokens/lexer tokens
//
// 2025-11-20   PV

using System;
using System.IO;
using System.Linq;
using T59v7Core;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0059 // Unnecessary assignment of a value

namespace T59v7Console;

public static class Program
{
    public static void Main(string[] args)
    {
        foreach (var folder in Directory.EnumerateDirectories(@"D:\Pierre\HomeShared\Calculators\TI\TI-58C TI-59 Docs and extra"))
            foreach (var file in Directory.EnumerateFiles(folder, "*.src", SearchOption.AllDirectories))
            {
                var source = File.ReadAllText(file);
                ProcessSource(source, file);
            }
    }

    public static void MainColorDemo()
    {
        var s = @"
comment:     [comment]Text 123[/comment]
invalid:     [invalid]Text 123[/invalid]
unknown:     [unknown]Text 123[/unknown]
instruction: [instruction]Text 123[/instruction]
number:      [number]Text 123[/number]
direct:      [direct]Text 123[/direct]
indirect:    [indirect]Text 123[/indirect]
tag:         [tag]Text 123[/tag]
label:       [label]Text 123[/label]
address:     [address]Text 123[/address]

"
+ Categories.GetTaggedText("// Example", SyntaxCategory.Comment) + "\n"
+ Categories.GetTaggedText("@Loop:", SyntaxCategory.Tag) + " "
+ Categories.GetTaggedText("-1.6E-19", SyntaxCategory.Number) + " "
+ Categories.GetTaggedText("PD*", SyntaxCategory.Instruction) + " "
+ Categories.GetTaggedText("04", SyntaxCategory.Indirect) + " "
+ Categories.GetTaggedText("ZYP", SyntaxCategory.Invalid) + " "
+ Categories.GetTaggedText("Dsz", SyntaxCategory.Instruction) + " "
+ Categories.GetTaggedText("12", SyntaxCategory.Direct) + " "
+ Categories.GetTaggedText("CLR", SyntaxCategory.Label) + "\n";

        Console.WriteLine(ConsoleRendering.RenderTaggedText(s));
    }

    public static void MainSimpleTests()
    {
        string input = @"LBL 99 Lbl STO CLR CE STO 12 STO IND 42 RCL 5
cos INV Sin INV CLR
Nop sin INV cos CLR

1 12 123 1234
CLS
// This is a comment
STO 12 RCL IND 4 SUM 7 INV SUM 8 SUM IND 9 INV SUM IND 10 
INV CLR inv tan 
@Loop1:
HIR 40 SM* 4 INV SM* 12
GTO 25 GTO CLR GTO IND 33 GTO 123 GTO 02 45 INV x=t Ind 12

SUM 12 SUM IND 12 SUM Z SUM IND Z
INV SUM 12 INV SUM IND 12 INV SUM Z INV SUM IND Z
SAM INV SAM

HIR 40 INV SM* 12 HIR Z RC* Z INV SM* Z

@Loop2:
GTO CLR GTO 25 GTO IND 04 GTO 421 GTO 04 21 GTO 10 10 GTO Z GTO IND Z GTO 04 CLR

Dsz 0 CLR

Dsz 0 CLR Dsz 1 123 DSZ 2 01 23 DSZ 12 11 23 DSZ 3 25 DSZ 4 Ind 12 DSZ 5 @Loop2
Iff Ind 10 CLR Iff Ind 11 123 Iff Ind 12 01 23 Iff Ind 22 11 23 Iff Ind 13 25 Iff Ind 14 Ind 12
DSZ Z CLR DSZ Ind Z CLR DSZ 3 Z CLR DSZ 3 Ind Z CLR

12345 CLR INV Cos CLS INV CLS
STO 12 Sto Ind 4 INV SUM Ind 7 Sto Z Sto Ind Z Sto Ind Ind 12 Sto Ind Ind Z

PAU POS

GTO CLR GTO 25 GTO 123 GTO 01 23 GTO IND 7
GTO 7 GTO Z GTO Ind Z GTO 43 21 GTO IND IND 25 GTO IND IND IND 25

Sto Ind Z CLR";

        //input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\all-1.t59");
        //input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\all-2.t59");
        //input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\Master Library\ML-01.T59");
        //input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\Master Library\ML-02.T59");
        //input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\Master Library\ML-15.T59");
        //input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\Master Library\ML-25.T59");
        //input = System.IO.File.ReadAllText(@"C:\Development\GitHub\Projects\11_Grammars\Ti Programs\Master Library\ML-01-!ALL.t59");
        //input = "// Initial comment\nLbl CLR STO 12 @Loop3: STO IND 12 Lbl Σ+ GTO CLR GTO 25 GTO 123 Lbl 25 GTO 01 23 END ZYP 123456 GTO @Tag Sto Ind Ind 12 Nop Sto Sin e^x INV SBR";
        //input = "SBR 240 INV SBR STO IND 12 INV SUM IND 33 OP 23 OP IND 23";
        //input = "// A simple calculation\n1.414 * 6.02e-23 =";
        //input = "Lbl CLR Lbl 85 Gto CLR GTO + GTO 005 Lbl 25 GTO 00 05 GTO * GTO 75 GTO 124 GTO 01 24";
        //input = "// Simple test\nLBL A STO 0 CLR @Loop: + RCL 0 Dsz 0 @Loop = R/S END";
        //input = "GTO 00 15   Pgm 02  SBR  02  40";

        input = @"
; My (partial) recreation of the ""Master Library"" module
; that came with the original TI-58C/59 calculators.
;
; Pgm 17: Moving Avegares
; Written by Lawrence D'Oliveiro <ldo@geek-central.gen.nz>.
;
; Register usage (different from original):
;     01 -- operand-accumulation pointer
;     02 -- n
;     03 -- # operands
;     04 -- loop counter
;     05 -- average-computation pointer
;     06 .. 05 + n -- operands
;

lbl A
; enter n
    sto 02
    x<>t 0 x≥t 1/x ; error if non-positive
    rcl 02 int inv x=t 1/x ; error if non-integral
    0 sto 03
    ( rcl 02 + 6 ) sto 01 ; end of operand array + 1
    gto √x
lbl 1/x
    0 1/x ; trigger error
lbl √x
    rcl 02  ; return showing what user entered
    inv sbr

lbl B
; enter next number and show average so far
    op 31   ; step to next location in operand array
    sto ind 01 ; save operand
    6 x<>t rcl 01 inv x=t |x| ; reached start?
    ( rcl 02 + 6 ) sto 01 ; wraparound if so
lbl |x|
    rcl 02 x<>t rcl 03 x=t ∑+ ; branch if already got n numbers
    op 23 ; else increase count
lbl ∑+
; compute and return new average
    rcl 03 sto 04
    ( rcl 02 + 6 ) sto 05
    0 ; initialize total
lbl mean
    op 35
    ( ce + rcl ind 05 ) ; accumulate next term
    dsz 4 mean
    ( ce ÷ rcl 03 ) ; convert total to average
    inv sbr

lbl E´
; initialize--no-op
    inv sbr";
        ProcessSource(input, "");
    }

    static void ProcessSource(string input, string file)
    {
        Console.WriteLine("\n\n########## " + file + "\n");
        var programs = T59Processor.GetPrograms(input);

        foreach (var (ix, p) in Enumerable.Index(programs))
        {
            if (ix > 0)
                Console.WriteLine("\n--------------\n");

            Console.WriteLine("--- Original in color");
            Render(p.OriginalColorizedTagged());

            Console.WriteLine("--- Reformatted");
            Render(p.L3ReformattedTagged());

            var l = p.LabelsTagged();
            if (!string.IsNullOrWhiteSpace(l))
            {
                Console.WriteLine("--- Labels");
                Render(l);
            }

            var e = p.ErrorsTagged();
            if (!string.IsNullOrWhiteSpace(e))
            {
                Console.WriteLine("--- Errors/Warnings");
                Render(e);
            }
        }
    }

    private static void Render(string s) => Console.WriteLine(ConsoleRendering.RenderTaggedText(s));
}