// ASTBuilder (MyTi58VisitorBaseAst)
// Visitor used to build an Ast tree from antlr4 parser tree (rules and tokens/terminals tree)
// An Ast is a more high level, simpled representation with a hierarchy of data objects holding
// properties ideal for matching, while a parser tree is very detailed
//
// 2025-11-10   PV
// 2025-11-15   PV      Do not include AstInterStatementWhiteSpace in AST, seems to be Ok

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static SimpleParser.MyT59ColorizeVisitor;

namespace SimpleParser;

// ---------------------------------------------------------------------
// AST Data classes definitions

public enum YesNoImplicit { No, Yes, Implicit }

public class AstProgram
{
    public readonly List<AstStatementBase> Statements = [];
    public int ProgramSize;     // Number of OpCodes
}

public record AstToken(string Text, SyntaxCategory Cat);

public abstract record AstStatementBase(List<AstToken> AstTokens);
//public record AstInterStatementWhiteSpace(List<AstToken> AstTokens): AstStatementBase(AstTokens);
public record AstComment(List<AstToken> AstTokens): AstStatementBase(AstTokens);
public record AstNumber(List<AstToken> AstTokens, List<byte> OpCodes): AstStatementBase(AstTokens)
{
    public int Address { get; set; }

    public string GetMnemonic()
    {
        string mnemonic = "";
        var ixSign = 0;
        foreach (var opCode in OpCodes)
        {
            if (opCode <= 9)    // ≥0 implicit!
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
    public int Address { get; set; }

    /* Unused
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
    */

    // Return instruction opcode, skipping potential 22 (INV) prefix
    // Also unused for now
    // public byte GetOpCode() => Inverted == YesNoImplicit.Yes ? OpCodes[1] : OpCodes[0];
}

public record AstInstructionAtomic(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted): AstInstruction(AstTokens, OpCodes, Inverted);
public record AstInstructionArg(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, YesNoImplicit ArgIndirect, byte ArgValue): AstInstructionAtomic(AstTokens, OpCodes, Inverted);
public record AstInstructionLabel(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, string LabelMnemonic, byte LabelOpCode): AstInstruction(AstTokens, OpCodes, Inverted);
// Branch includes GTO, GT*, SBR, [INV] x=t, [INV] x≥t
public record AstInstructionBranch(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, YesNoImplicit TargetIndirect, string TargetMnemonic, int TargetValue): AstInstructionAtomic(AstTokens, OpCodes, Inverted);
// ArgBranch includes If Flag, Dsz
public record AstInstructionArgBranch(List<AstToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, YesNoImplicit ArgIndirect, byte ArgValue, YesNoImplicit TargetIndirect, string TargetMnemonic, int TargetValue): AstInstructionArg(AstTokens, OpCodes, Inverted, ArgIndirect, ArgValue);

// ---------------------------------------------------------------------

// Inherit from the generated base visitor.
public class MyTi58VisitorAstBuilder(t59Parser parser): t59BaseVisitor<object>
{
    private readonly MyT59ColorizeVisitor ColorVisitor = new(parser);

    private readonly List<AstProgram> Programs = [new()];     // Build a list containing a single program containing en empty list of statements

    // Program just reference the latest program (the obe being built)
    private AstProgram Program => Programs[^1];

    // Main method, visiting tree and returning a list of programs
    internal List<AstProgram> BuildPrograms(IParseTree tree)
    {
        Visit(tree);
        return Programs;
    }

    // This method is called for every single token in the tree
    // We only process comments and top-level white space which are not handled as instructions
    public override object VisitTerminal(ITerminalNode node)
    {
        // Get the token's integer type
        int tokenType = node.Symbol.Type;

        if (tokenType == t59Lexer.Program_separator)
        {
            Programs.Add(new());
        }
        else if (tokenType == t59Lexer.LineComment)
        {
            var at = new List<AstToken> {
                new(node.GetText(), SyntaxCategory.Comment)
            };
            var com = new AstComment(at);
            Program.Statements.Add(com);
        }
        else if (tokenType == t59Lexer.INVALID_TOKEN)
        {
            Console.WriteLine($"Invalid token, skipped: {node.GetText()}");
        }

        // DO NOT add InterStatementWhileSpace in the AST

        return base.VisitTerminal(node);
    }

    // Generate opcodes for a scientific number
    public override object VisitNumber([NotNull] t59Parser.NumberContext context)
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

    public override object VisitInstruction_atomic_simple([NotNull] t59Parser.Instruction_atomic_simpleContext context)
        => AtomicInstruction(context);

    public override object VisitInstruction_atomic_invertible([NotNull] t59Parser.Instruction_atomic_invertibleContext context)
        => AtomicInstruction(context);

    public override object VisitInstruction_atomic_inverted([NotNull] t59Parser.Instruction_atomic_invertedContext context)
        => AtomicInstruction(context);

    public override object VisitInstruction_invert_isolated([NotNull] t59Parser.Instruction_invert_isolatedContext context)
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
        if (ltn[ixInstruction].Symbol.Type == t59Lexer.I22_invert)
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

    public override object VisitInstruction_fix([NotNull] t59Parser.Instruction_fixContext context) => InstructionArg(context);
    public override object VisitInstruction_setflag([NotNull] t59Parser.Instruction_setflagContext context) => InstructionArg(context);
    public override object VisitInstruction_op([NotNull] t59Parser.Instruction_opContext context) => InstructionArg(context);
    public override object VisitInstruction_pgm([NotNull] t59Parser.Instruction_pgmContext context) => InstructionArg(context);
    public override object VisitInstruction_memory([NotNull] t59Parser.Instruction_memoryContext context) => InstructionArg(context);

    object InstructionArg(ParserRuleContext context)
    {
        try
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
            if (ltn[ixInstruction].Symbol.Type == t59Lexer.I22_invert)
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
            if (ltn[ixInstruction].Symbol.Type == t59Lexer.I40_indirect)
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

        }
        catch (Exception e)
        {
            Console.WriteLine("*** Error processing InstructionArg, ignoring it");
        }

        return null!;
    }

    public override object VisitInstruction_label([NotNull] t59Parser.Instruction_labelContext context)
    {
        var ltn = GetTerminalNodes(context);

        var at = new List<AstToken>();
        foreach (var tn in ltn)
            at.Add(new AstToken(tn.GetText(), ColorVisitor.GetTerminalSyntaxCategory(tn)));

        int ixInstruction = 0;
        List<byte> opCodes = [];

        Debug.Assert(ltn[ixInstruction].Symbol.Type == t59Lexer.I76_label);
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

    public override object VisitInstruction_branch([NotNull] t59Parser.Instruction_branchContext context)
        => InstructionBranch(context);
    public override object VisitInstruction_x_equals_t([NotNull] t59Parser.Instruction_x_equals_tContext context)
        => InstructionBranch(context);
    public override object VisitInstruction_x_greater_or_equal_than_t([NotNull] t59Parser.Instruction_x_greater_or_equal_than_tContext context)
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
        if (ltn[ixInstruction].Symbol.Type == t59Lexer.I22_invert)
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
        if (ltn[ixInstruction].Symbol.Type == t59Lexer.I40_indirect)
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

    public override object VisitInstruction_test_flag([NotNull] t59Parser.Instruction_test_flagContext context)
        => InstructionArgBranch(context);

    public override object VisitInstruction_decrement_and_skip_on_zero([NotNull] t59Parser.Instruction_decrement_and_skip_on_zeroContext context)
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
        if (ltn[ixInstruction].Symbol.Type == t59Lexer.I22_invert)
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
        if (ltn[ixInstruction].Symbol.Type == t59Lexer.I40_indirect)
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
        if (ltn[ixInstruction].Symbol.Type == t59Lexer.I40_indirect)
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
                if (term.Symbol.Type != t59Lexer.WS)
                    tn.Add(term);
            }
            else
                Debugger.Break();
        }

        return tn;
    }
}
