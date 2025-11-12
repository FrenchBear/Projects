// Visitor used to build an Ast tree from antlr4 rules/tokens tree
// An Ast is a more high level representation with a hierarchy of data objects holding properties
// ideal for matching
//
// 2025-11-10   PV

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static SimpleParser.MyTi58VisitorBaseColorize;
using static SimpleParser.StandardInstructions;

namespace SimpleParser;

public enum YesNoImplicit { No, Yes, Implicit }

public record AstProgram(List<AstStatementBase> Statements);

public record AstToken(string Text, SyntaxCategory Cat);

public abstract record AstStatementBase(List<AstToken> AstTokens);
public record AstInterStatementWhiteSpace(List<AstToken> AstTokens): AstStatementBase(AstTokens);
public record AstComment(List<AstToken> AstTokens): AstStatementBase(AstTokens);
public record AstNumber(List<AstToken> AstTokens, List<byte> OpCodes): AstStatementBase(AstTokens)
{
    public string GetMnemonic()
    {
        string mnemonic = "";
        var ixSign = 0;
        foreach (var opCode in OpCodes)
        {
            if (opCode is >= 0 and <= 9)
                mnemonic += (char)(opCode + '0');
            else if (opCode == 93)
                mnemonic += '.';
            else if (opCode == 52)
            {
                mnemonic += 'E';
                ixSign = mnemonic.Length;
            }
            else if (opCode == 94)
                mnemonic = mnemonic[..ixSign] + "-" + mnemonic[ixSign..];
            else
                Debugger.Break();
        }
        return mnemonic;
    }
}

public abstract record AstInstruction(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted): AstStatementBase(AstTokens)
{
    public string GetMnemonic()
    {
        int start = 0;
        if (Inverted == YesNoImplicit.Yes)
            start = 1;
        foreach (AstToken at in AstTokens.Skip(start))
        {
            if (at.Cat == SyntaxCategory.Instruction)
                return at.Text;
        }
        Debugger.Break();       // ToDo: for devtest, remove
        return "???";
    }

    // Return instruction opcode, ignoring potential 22 (INV) prefix
    // ToDo: Test with RTN and e^x
    public byte GetOpCode() => Inverted == YesNoImplicit.Yes ? OpCodes[1] : OpCodes[0];
}

public record AstInstructionAtomic(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted): AstInstruction(AstTokens, OpCodes, Inverted);
public record AstInstructionArg(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, YesNoImplicit ArgIndirect, byte ArgValue): AstInstructionAtomic(AstTokens, OpCodes, Inverted);
public record AstInstructionLabel(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, string LabelMnemonic, byte LabelOpCode): AstInstruction(AstTokens, OpCodes, Inverted);
// Branch includes GTO, GT*, SBR, [INV] x=t, [INV] x≥t
public record AstInstructionBranch(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, YesNoImplicit TargetIndirect, string TargetMnemonic, int TargetValue): AstInstructionAtomic(AstTokens, OpCodes, Inverted);
// ArgBranch includes If Flag, Dsz
public record AstInstructionArgBranch(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, YesNoImplicit ArgIndirect, byte ArgValue, YesNoImplicit TargetIndirect, string TargetMnemonic, int TargetValue): AstInstructionArg(AstTokens, OpCodes, Inverted, ArgIndirect, ArgValue);

// Inherit from the generated base visitor.
// The 'object' type means your visit methods can return any type.
public class MyTi58VisitorBaseAst(ti58Parser parser): ti58BaseVisitor<object>
{
    private readonly AstProgram Program = new([]);
    private readonly MyTi58VisitorBaseColorize ColorVisitor = new(parser);

    internal void PostProcessAst()
    {
        // Standardization of instructions: replaces STA or SIG+ by Σ+
        foreach (var sta in Program.Statements)
        {
            // Normalize instruction names and keys labels
            if (sta is AstInstruction(List<AstToken> astTokens, _, _) inst)
                for (var i = 0; i < astTokens.Count; i++)
                    if (astTokens[i].Cat is SyntaxCategory.Instruction or SyntaxCategory.KeyLabel)
                        foreach (var lsi in Sill)
                            if ((string.Equals(astTokens[i].Text, lsi[0], StringComparison.CurrentCultureIgnoreCase) && astTokens[i].Text != lsi[0]) || lsi[1..].Any(af => string.Equals(astTokens[i].Text, af, StringComparison.CurrentCultureIgnoreCase)))
                                astTokens[i] = astTokens[i] with { Text = lsi[0] };
        }

        // Group digits in registers (direct and indirect), op, pgm, ... (flags and Dsz use 1 digit by default)
        foreach (var sta in Program.Statements)
        {
            if (sta is AstInstruction(List<AstToken> astTokens, _, _) inst)
            {
                // Merge DirectMemoryOrNumber, IndirectMemory, numeric KeyLabel and DirectAddresses
                for (var i = 0; i < astTokens.Count; i++)
                    while (i < astTokens.Count - 1 && (astTokens[i].Cat == SyntaxCategory.DirectMemoryOrNumber || astTokens[i].Cat == SyntaxCategory.IndirectMemory || astTokens[i].Cat == SyntaxCategory.KeyLabel|| astTokens[i].Cat == SyntaxCategory.DirectAddress) && astTokens[i].Cat == astTokens[i + 1].Cat)
                    {
                        astTokens[i] = astTokens[i] with { Text = astTokens[i].Text + astTokens[i + 1].Text };
                        astTokens.RemoveAt(i + 1);
                    }

                // For now ensure len=2 (should be disabled for DirectMemoryOrNumber after [INV] Stf, [INV] Iff or [INV] Dsz
                var FlgDsz = false;
                for (var i = 0; i < astTokens.Count; i++)
                {
                    if (astTokens[i].Cat == SyntaxCategory.Instruction && (astTokens[i].Text == "STF" || astTokens[i].Text == "IFF" || astTokens[i].Text == "Dsz"))
                        FlgDsz = true;
                    if ((astTokens[i].Cat == SyntaxCategory.DirectMemoryOrNumber && !FlgDsz) || astTokens[i].Cat == SyntaxCategory.IndirectMemory)
                        if (astTokens[i].Text.Length == 1)
                            astTokens[i] = astTokens[i] with { Text = "0" + astTokens[i].Text };
                    if (astTokens[i].Cat == SyntaxCategory.DirectAddress)
                    {
                        string t = astTokens[i].Text;
                        if (t.Length == 4)
                        {
                            Debug.Assert(t[0] == '0');
                            astTokens[i] = astTokens[i] with { Text = t[1..] };
                        }
                        else
                            Debug.Assert(t.Length == 3);
                    }
                }
            }
        }

        // Group top level numbers
        //for (int i = 0; i < Program.Statements.Count; i++)
        //{
        //    AstStatementBase? GetNextStatement()
        //    {
        //        for (j=i+1; j<Program.Statements.Count;j++)
        //        {
        //            if (Program.Statements[j] is AstInterStatementWhiteSpace(_, _))
        //                continue;
        //            if (Program.Statements[j] is AstNumber(_, _) next_num)
        //                return Program.Statements[j];
        //            if (Program.Statements[j] is AstInstructionAtomic(_, _) next_num)
        //                $$$
        //        }
        //        return null;
        //    }

        //    // Actually, a dot can start a sequence
        //    if (Program.Statements[i] is AstNumber(List<AstToken> tokens, List<byte> opCodes) num)
        //    {
        //        bool isCompatible = false;

        //        next_num = GetNextStatement();

        //        // Next can be a top level number, +/- or EE
        //        while (i + 1 < Program.Statements.Count && Program.Statements[i + 1] is AstNumber(List<AstToken> next_tokens, List<byte> next_OpCodes) next_num)
        //        {
        //            // Determine if next is compatible with num, say yes for now
        //            isCompatible = true;

        //            num.AstTokens.AddRange(next_tokens);
        //            num.OpCodes.AddRange(next_OpCodes);
        //            Program.Statements.RemoveAt(i + 1);
        //        }
        //        if (!isCompatible)
        //            continue;
        //    }
        //}

        // ToDo: Line comment processing so they can be printed after an instruction in case they're behind an instruction in the code
        // (and maybe align comments Rust or Go style)

        // ToDo: Build a list of labels and a list of statements start instruction to validate direct address and labels

        // ToDo: Error detection (ex: two consecutive operators, ...)
    }

    internal void PrintFormattedAst()
    {
        // Number of max opcodes per line in reformatted listing
        const int OpCols = 6;

        Console.WriteLine();
        int cp = 0;

        foreach (AstStatementBase sta in Program.Statements)
        {
            switch (sta)
            {
                // Should do a better formatting, if comment is at the end of a source line, it should
                // be printed at the correct location, not on following line...
                case AstComment(var astTokens):
                    Colorize(astTokens[0]);
                    Console.WriteLine();
                    break;

                case AstInterStatementWhiteSpace(_):
                    //Console.WriteLine("Inter-statement WhiteSpace");
                    break;

                case AstNumber(var astTokens, var opCodes) num:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{cp:D3}: ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{string.Join(" ", opCodes.Take(OpCols).Select(b => b.ToString("D2"))),-3 * OpCols}   ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(num.GetMnemonic());

                    while (opCodes.Count > OpCols)
                    {
                        cp += 5;
                        opCodes.RemoveRange(0, OpCols);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{cp:D3}: ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"{string.Join(" ", opCodes.Take(OpCols).Select(b => b.ToString("D2"))),-3 * OpCols} ");
                    }

                    cp += opCodes.Count;
                    break;

                case AstInstruction(var astTokens, var opCodes, _) inst:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{cp:D3}: ");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    Console.Write($"{string.Join(" ", opCodes.Take(OpCols).Select(b => b.ToString("D2"))),-3 * OpCols} ");
                    if (astTokens[0].Text.StartsWith("LBL", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("■ ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                        Console.Write("  ");

                    int len = 0;
                    foreach (AstToken token in astTokens)
                        if (token.Cat != SyntaxCategory.WhiteSpace)
                        {
                            len += 3 + token.Text.Length;
                            Colorize("‹" + token.Text + "› ", token.Cat);
                        }
                    Console.Write($"{new string(' ', 25 - len)} {inst.GetMnemonic()}");
                    Console.WriteLine();

                    while (opCodes.Count > OpCols)
                    {
                        cp += 5;
                        opCodes.RemoveRange(0, OpCols);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{cp:D3}: ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"{string.Join(" ", opCodes.Take(OpCols).Select(b => b.ToString("D2"))),-3 * OpCols} ");
                    }

                    cp += opCodes.Count;
                    break;
            }
        }
    }

    // This method is called for every single token in the tree
    // We only process comments and top-level white space which are not handled as instructions
    public override object VisitTerminal(ITerminalNode node)
    {
        // Get the token's integer type
        int tokenType = node.Symbol.Type;

        if (tokenType == ti58Lexer.LineComment)
        {
            var at = new List<AstToken> {
                new(node.GetText(), SyntaxCategory.Comment)
            };
            var com = new AstComment(at);
            Program.Statements.Add(com);
        }
        //else if (node.Parent is ParserRuleContext ruleContext && ruleContext.RuleIndex == ti58Parser.RULE_program)
        else if (node.Parent is ParserRuleContext { RuleIndex: ti58Parser.RULE_program })
        {
            // White space is only processed if top level
            var at = new List<AstToken> {
                new(node.GetText(), SyntaxCategory.WhiteSpace)
            };
            var ws = new AstInterStatementWhiteSpace(at);
            Program.Statements.Add(ws);
        }

        return base.VisitTerminal(node);
    }

    public override object VisitNumber([NotNull] ti58Parser.NumberContext context)
    {
        var ltn = GetTerminalNodes(context);
        var text = context.GetText();
        List<byte> opCodes = [];
        foreach (var c in text)
        {
            if (c is >= '0' and <= '9')
                opCodes.Add((byte)(c - '0'));
            else if (c == '-')
                opCodes.Add(94);
            else if (c == '.')
                opCodes.Add(93);
            else if (c is 'e' or 'E')
                opCodes.Add(52);
            else if (c != '+')
                Debugger.Break();
        }

        var at = new List<AstToken>();
        foreach (var tn in ltn)
            at.Add(new AstToken(tn.GetText(), ColorVisitor.GetTerminalSyntaxCategory(tn)));

        var num = new AstNumber(at, opCodes);
        Program.Statements.Add(num);

        return null!;
    }

    public override object VisitInstruction_atomic_simple([NotNull] ti58Parser.Instruction_atomic_simpleContext context)
        => AtomicInstruction(context);

    public override object VisitInstruction_atomic_invertible([NotNull] ti58Parser.Instruction_atomic_invertibleContext context)
        => AtomicInstruction(context);

    public override object VisitInstruction_atomic_inverted([NotNull] ti58Parser.Instruction_atomic_invertedContext context)
        => AtomicInstruction(context);

    public override object VisitInstruction_invert_isolated([NotNull] ti58Parser.Instruction_invert_isolatedContext context)
        => AtomicInstruction(context);

    object AtomicInstruction(ParserRuleContext context)
    {
        var ltn = GetTerminalNodes(context);

        var at = new List<AstToken>();
        foreach (var tn in ltn)
            at.Add(new AstToken(tn.GetText(), ColorVisitor.GetTerminalSyntaxCategory(tn)));

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        List<byte> opCodes = [];

        // INV prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            ixInstruction++;
        }

        string symbolicName = parser.Vocabulary.GetSymbolicName(ltn[ixInstruction].Symbol.Type);
        if (symbolicName == "I123_e_power_x")
        {
            opCodes.Add(22);
            opCodes.Add(23);
        }
        else if (symbolicName == "I128_10_power_x")
        {
            opCodes.Add(22);
            opCodes.Add(28);
        }
        else if (symbolicName == "I71_subroutine" && opCodes.Count > 0)
        {
            // INV SBR
            opCodes[0] = 92;                    // Override 22
            inverted = YesNoImplicit.No;        // Since it's a specific instruction without INV prefix
            at.Clear();
            at.Add(new("RTN", SyntaxCategory.Instruction));     // We use RTN in AstTokens rather than INV SBR
        }
        else if (symbolicName == "Bang")
        {
            // Nop
            inverted = YesNoImplicit.No;        // A single INV is not considered as an inverted instruction
        }
        else
        {
            Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
            opCodes.Add(byte.Parse(symbolicName[1..3]));
        }

        var ai = new AstInstructionAtomic(at, opCodes, inverted);
        Program.Statements.Add(ai);

        return null!;
    }

    public override object VisitInstruction_fix([NotNull] ti58Parser.Instruction_fixContext context) => InstructionArg(context);
    public override object VisitInstruction_setflag([NotNull] ti58Parser.Instruction_setflagContext context) => InstructionArg(context);
    public override object VisitInstruction_op([NotNull] ti58Parser.Instruction_opContext context) => InstructionArg(context);
    public override object VisitInstruction_pgm([NotNull] ti58Parser.Instruction_pgmContext context) => InstructionArg(context);
    public override object VisitInstruction_memory([NotNull] ti58Parser.Instruction_memoryContext context) => InstructionArg(context);

    object InstructionArg(ParserRuleContext context)
    {
        var ltn = GetTerminalNodes(context);

        var at = new List<AstToken>();
        foreach (var tn in ltn)
            at.Add(new AstToken(tn.GetText(), ColorVisitor.GetTerminalSyntaxCategory(tn)));

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        YesNoImplicit argIndirect = YesNoImplicit.No;
        List<byte> opCodes = [];

        // INV prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            ixInstruction++;
        }

        int mainType = ltn[ixInstruction].Symbol.Type;
        string symbolicName = parser.Vocabulary.GetSymbolicName(mainType);
        Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
        opCodes.Add(byte.Parse(symbolicName[1..3]));
        if (symbolicName.Contains("indirect"))
            argIndirect = YesNoImplicit.Implicit;
        ixInstruction++;

        // Ind prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            // Merged indirect operations (GTO IND, SBR IND... is handled in InstructionBranch)
            var l = opCodes.Count - 1;
            string mergedToken = "";
            switch (opCodes[l])
            {
                case 42:        // STO
                    opCodes[l] = 72;
                    argIndirect = YesNoImplicit.Implicit;
                    mergedToken = "ST*";
                    break;
                case 43:        // RCL
                    opCodes[l] = 73;
                    argIndirect = YesNoImplicit.Implicit;
                    mergedToken = "RC*";
                    break;
                case 44:        // SUM
                    opCodes[l] = 74;
                    argIndirect = YesNoImplicit.Implicit;
                    mergedToken = "SM*";
                    break;
                case 48:        // EXC
                    opCodes[l] = 63;
                    argIndirect = YesNoImplicit.Implicit;
                    mergedToken = "EX*";
                    break;
                case 49:        // PRD
                    opCodes[l] = 64;
                    argIndirect = YesNoImplicit.Implicit;
                    mergedToken = "PD*";
                    break;
                case 36:        // PGM
                    opCodes[l] = 62;
                    argIndirect = YesNoImplicit.Implicit;
                    mergedToken = "PG*";
                    break;
                case 69:        // Op
                    opCodes[l] = 84;
                    argIndirect = YesNoImplicit.Implicit;
                    mergedToken = "OP*";
                    break;
                default:        // General case, add Ind opcode 40
                    argIndirect = YesNoImplicit.Yes;
                    opCodes.Add(40);
                    break;
            }

            // For merged indirect operations, don't keep Ind in the list of terminals
            if (!string.IsNullOrEmpty(mergedToken))
            {
                ltn.RemoveAt(ixInstruction);
                at.RemoveAt(ixInstruction);      // Remove AstToken Ind
                at[ixInstruction - 1] = new(mergedToken, SyntaxCategory.Instruction);   // Replace direct statement
            }
            else
                ixInstruction++;
        }

        byte argValue = 0;
        foreach (var d in ltn[ixInstruction..])
            argValue = (byte)(10 * argValue + byte.Parse(d.GetText()));
        opCodes.Add(argValue);

        var aa = new AstInstructionArg(at, opCodes, inverted, argIndirect, argValue);
        Program.Statements.Add(aa);

        return null!;
    }

    public override object VisitInstruction_label([NotNull] ti58Parser.Instruction_labelContext context)
    {
        var ltn = GetTerminalNodes(context);

        var at = new List<AstToken>();
        foreach (var tn in ltn)
            at.Add(new AstToken(tn.GetText(), ColorVisitor.GetTerminalSyntaxCategory(tn)));

        int ixInstruction = 0;
        List<byte> opCodes = [];

        Debug.Assert(ltn[ixInstruction].Symbol.Type == ti58Lexer.I76_label);
        opCodes.Add(76);
        ixInstruction++;

        string symbolicName = parser.Vocabulary.GetSymbolicName(ltn[ixInstruction].Symbol.Type);
        string labelMnemonic;
        byte labelOpCode = 0;
        if (symbolicName != null)
        {   // Instruction label
            labelMnemonic = ltn[ixInstruction].GetText();
            Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
            labelOpCode = byte.Parse(symbolicName[1..3]);
        }
        else
        {
            foreach (var d in ltn[ixInstruction..])
                labelOpCode = (byte)(10 * labelOpCode + byte.Parse(d.GetText()));
            labelMnemonic = $"{labelOpCode:D2}";
        }
        opCodes.Add(labelOpCode);

        var ls = new AstInstructionLabel(at, opCodes, YesNoImplicit.No, labelMnemonic, labelOpCode);
        Program.Statements.Add(ls);

        return null!;
    }

    public override object VisitInstruction_branch([NotNull] ti58Parser.Instruction_branchContext context)
        => InstructionBranch(context);
    public override object VisitInstruction_x_equals_t([NotNull] ti58Parser.Instruction_x_equals_tContext context)
        => InstructionBranch(context);
    public override object VisitInstruction_x_greater_or_equal_than_t([NotNull] ti58Parser.Instruction_x_greater_or_equal_than_tContext context)
        => InstructionBranch(context);

    object InstructionBranch(ParserRuleContext context)
    {
        var ltn = GetTerminalNodes(context);

        var at = new List<AstToken>();
        foreach (var tn in ltn)
            at.Add(new AstToken(tn.GetText(), ColorVisitor.GetTerminalSyntaxCategory(tn)));

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        YesNoImplicit targetIndirect = YesNoImplicit.No;
        List<byte> opCodes = [];

        // INV prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            ixInstruction++;
        }

        string symbolicName = parser.Vocabulary.GetSymbolicName(ltn[ixInstruction].Symbol.Type);
        Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
        opCodes.Add(byte.Parse(symbolicName[1..3]));
        if (symbolicName.Contains("indirect"))
            targetIndirect = YesNoImplicit.Implicit;
        else if (symbolicName.Contains("indextra"))
        {
            targetIndirect = YesNoImplicit.Yes;
            opCodes.Add(40);
        }
        ixInstruction++;

        // Ind prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            // Merged IND GTO
            var l = opCodes.Count - 1;
            if (opCodes[l] == 61)
            {
                opCodes[l] = 83;
                targetIndirect = YesNoImplicit.Implicit;
                // For merged GTO IND, we don't keep Terminal Ind
                ltn.RemoveAt(ixInstruction);
                // Need to replace AstToken GTO by GO*
                at.RemoveAt(ixInstruction); // Remove AstToken Ind
                at[ixInstruction - 1] = new("GO*", SyntaxCategory.Instruction);
            }
            else
            {
                targetIndirect = YesNoImplicit.Yes;
                opCodes.Add(40);
                ixInstruction++;
            }
        }

        // Determine if target is a label, a numeric label or an address
        symbolicName = parser.Vocabulary.GetSymbolicName(ltn[ixInstruction].Symbol.Type);
        string targetMnemonic = "?";
        int targetValue = 0;           // byte is not enough since addresses are 0..999
        if (symbolicName != null)
        {   // Instruction label
            targetMnemonic = ltn[ixInstruction].GetText();
            Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
            targetValue = byte.Parse(symbolicName[1..3]);
            opCodes.Add((byte)targetValue);
            ixInstruction++;
        }
        else
        {
            var cat = ColorVisitor.GetTerminalSyntaxCategory(ltn[ixInstruction]);
            for (; ; )
            {
                targetValue = (byte)(10 * targetValue + byte.Parse(ltn[ixInstruction].GetText()));
                ixInstruction++;
                if (ixInstruction == ltn.Count)
                    break;
            }

            switch (cat)
            {
                case SyntaxCategory.DirectAddress:
                    targetMnemonic = $"{targetValue:D3}";
                    opCodes.Add((byte)(targetValue / 100));
                    opCodes.Add((byte)(targetValue % 100));
                    break;
                case SyntaxCategory.KeyLabel:
                case SyntaxCategory.IndirectMemory:
                    targetMnemonic = $"{targetValue:D2}";
                    opCodes.Add((byte)targetValue);
                    break;
                default:
                    Console.WriteLine($"Invalid terminal category: {cat}");
                    Debugger.Break();
                    break;
            }
        }
        Debug.Assert(ixInstruction == ltn.Count);

        var aa = new AstInstructionBranch(at, opCodes, inverted, targetIndirect, targetMnemonic, targetValue);
        Program.Statements.Add(aa);

        return null!;
    }

    public override object VisitInstruction_test_flag([NotNull] ti58Parser.Instruction_test_flagContext context)
        => InstructionArgBranch(context);

    public override object VisitInstruction_decrement_and_skip_on_zero([NotNull] ti58Parser.Instruction_decrement_and_skip_on_zeroContext context)
        => InstructionArgBranch(context);

    object InstructionArgBranch(ParserRuleContext context)
    {
        var ltn = GetTerminalNodes(context);

        if (ltn.Count == 11)
        {
            Console.WriteLine();
            foreach (var s in ltn)
            {
                Console.WriteLine($"{s.GetText()}\t{s.Symbol.Type} {ColorVisitor.GetTerminalSyntaxCategory(s)}");
            }
        }

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        YesNoImplicit argIndirect = YesNoImplicit.No;
        List<byte> opCodes = [];

        // INV prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            ixInstruction++;
        }

        string symbolicName = parser.Vocabulary.GetSymbolicName(ltn[ixInstruction].Symbol.Type);
        Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
        opCodes.Add(byte.Parse(symbolicName[1..3]));
        if (symbolicName.Contains("indirect"))
            argIndirect = YesNoImplicit.Implicit;
        ixInstruction++;

        // Ind prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            // We don't add Ind prefix twice in case we have something like SM* Ind 40, just consider it is SUM Ind 40 or SM* 40
            if (argIndirect == YesNoImplicit.No)
            {
                argIndirect = YesNoImplicit.Yes;
                opCodes.Add(40);
            }
            ixInstruction++;
        }

        byte argValue = 0;
        var cat = ColorVisitor.GetTerminalSyntaxCategory(ltn[ixInstruction]);
        while (ColorVisitor.GetTerminalSyntaxCategory(ltn[ixInstruction]) == cat)
        {
            var txt = ltn[ixInstruction].GetText();
            if (!byte.TryParse(txt, out byte b))
                break;
            argValue = (byte)(10 * argValue + b);
            ixInstruction++;
        }
        Debug.Assert(argValue != 40);       // Temp for dev; Dsz 40 should be rejected as invalid, since it means Dsz Ind
        opCodes.Add(argValue);
        // Don't move to next terminal, at the end of previous decoding loop we already point to next terminal

        // Target ---------------------------------
        YesNoImplicit targetIndirect = YesNoImplicit.No;

        // Ind prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            targetIndirect = YesNoImplicit.Yes;
            opCodes.Add(40);
            ixInstruction++;
        }

        // Determine if target is a label, a numeric label or an address
        symbolicName = parser.Vocabulary.GetSymbolicName(ltn[ixInstruction].Symbol.Type);
        string targetMnemonic = "?";
        int targetValue = 0;           // byte is not enough since addresses are 0..999
        if (symbolicName != null)
        {   // Instruction label
            targetMnemonic = ltn[ixInstruction].GetText();
            Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
            targetValue = byte.Parse(symbolicName[1..3]);
            opCodes.Add((byte)targetValue);
            ixInstruction++;
        }
        else
        {
            cat = ColorVisitor.GetTerminalSyntaxCategory(ltn[ixInstruction]);
            for (; ; )
            {
                targetValue = (byte)(10 * targetValue + byte.Parse(ltn[ixInstruction].GetText()));
                ixInstruction++;
                if (ixInstruction == ltn.Count)
                    break;
            }

            switch (cat)
            {
                case SyntaxCategory.DirectAddress:
                    targetMnemonic = $"{targetValue:D3}";
                    opCodes.Add((byte)(targetValue / 100));
                    opCodes.Add((byte)(targetValue % 100));
                    break;
                case SyntaxCategory.IndirectMemory:
                case SyntaxCategory.KeyLabel:
                    targetMnemonic = $"{targetValue:D2}";
                    opCodes.Add((byte)targetValue);
                    break;
                default:
                    Console.WriteLine($"Invalid terminal category: {cat}");
                    Debugger.Break();
                    break;
            }
        }
        Debug.Assert(ixInstruction == ltn.Count);

        var at = new List<AstToken>();
        foreach (var tn in ltn)
            at.Add(new AstToken(tn.GetText(), ColorVisitor.GetTerminalSyntaxCategory(tn)));

        var aa = new AstInstructionArgBranch(at, opCodes, inverted, argIndirect, argValue, targetIndirect, targetMnemonic, targetValue);
        Program.Statements.Add(aa);

        return null!;
    }

    private static List<ITerminalNode> GetTerminalNodes(ParserRuleContext context)
    {
        var tn = new List<ITerminalNode>();

        foreach (var child in context.children)
        {
            if (child is ParserRuleContext ps)
                tn.AddRange(GetTerminalNodes(ps));
            else if (child is ITerminalNode term)
            {
                if (term.Symbol.Type != ti58Lexer.WS)
                    tn.Add(term);
            }
            else
                Debugger.Break();
        }

        return tn;
    }

}