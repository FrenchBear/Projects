// Visitor used to build an AST tree from antlr4 rules/tokens tree
// An AST is a more high level representation with a hierarchy of data objects holding properties
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
using System.Reflection.Emit;
using System.Xml.Linq;

namespace SimpleParser;

public enum YesNoImplicit
{
    No,
    Yes,
    Implicit
}

public record ASTProgram(List<ASTStatementBase> statements);

public abstract record ASTStatementBase(List<ITerminalNode> nodes);
public record ASTWhiteSpace(List<ITerminalNode> nodes): ASTStatementBase(nodes);
public record ASTComment(List<ITerminalNode> nodes): ASTStatementBase(nodes);
public abstract record ASTInstruction(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> opCodes, YesNoImplicit inverted, string text): ASTStatementBase(nodes);
public record ASTNumber(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> opCodes, string text): ASTInstruction(nodes, ruleContext, opCodes, YesNoImplicit.No, text);
public record ASTInstructionAtomic(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> opCodes, YesNoImplicit inverted, string text): ASTInstruction(nodes, ruleContext, opCodes, inverted, text);
public record ASTInstructionArg(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> opCodes, YesNoImplicit inverted, string text, YesNoImplicit argIndirect, byte argValue): ASTInstructionAtomic(nodes, ruleContext, opCodes, inverted, text);
public record ASTInstructionLabel(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> opCodes, YesNoImplicit inverted, string text, string labelMnemonic, byte labelOpCode): ASTInstruction(nodes, ruleContext, opCodes, inverted, text);
// Branch includes GTO, GT*, SBR, [INV] x=t, [INV] x≥t
public record ASTInstructionBranch(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> opCodes, YesNoImplicit inverted, string text, YesNoImplicit targetIndirect, string targetMnemonic, int targetValue): ASTInstructionAtomic(nodes, ruleContext, opCodes, inverted, text);
// ArgBranch includes If Flag, Dsz
public record ASTInstructionArgBranch(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> opCodes, YesNoImplicit inverted, string text, YesNoImplicit argIndirect, byte argValue, YesNoImplicit targetIndirect, string targetMnemonic, int targetValue): ASTInstructionArg(nodes, ruleContext, opCodes, inverted, text, argIndirect, argValue);


// Inherit from the generated base visitor.
// The 'object' type means your visit methods can return any type.
public class MyTi58VisitorBaseAST: ti58BaseVisitor<object>
{
    private readonly ti58Parser _parser;
    public readonly ASTProgram program;

    public MyTi58VisitorBaseAST(ti58Parser parser)
    {
        _parser = parser;
        program = new(new());
    }

    // Helper
    internal void PrintAST()
    {
        Console.WriteLine("\nAST Tree");
        foreach (ASTStatementBase sta in program.statements)
        {
            switch (sta)
            {
                case ASTComment(var c1):
                    Console.WriteLine($"Comment: {c1.First().GetText()}");
                    break;

                case ASTWhiteSpace(_):
                    //Console.WriteLine("Inter-statement WhiteSpace");
                    break;

                case ASTNumber(_, _, var opCodes, var mnemonic):
                    Console.WriteLine($"Number: {string.Join(" ", opCodes.Select(b => b.ToString("D2")))}: {mnemonic}");
                    break;

                // Need to be placed before case ASTAtomicInstruction since ASTInstructionArg inherits from ASTAtomicInstruction
                case ASTInstructionArg(_, _, var opCodes, var inverted, var mnemonic, YesNoImplicit argIndirect, byte _argValue):
                    Console.Write($"InstructionArg: {string.Join(" ", opCodes.Select(b => b.ToString("D2")))}: ");
                    Console.WriteLine(mnemonic);
                    break;

                case ASTInstructionAtomic(_, _, var opCodes, var inverted, var mnemonic):
                    Console.Write($"AtomicInstruction: {string.Join(" ", opCodes.Select(b => b.ToString("D2")))}: ");
                    Console.WriteLine(mnemonic);
                    break;

                case ASTInstructionLabel(_, _, var opCodes, _, var mnemonic, var labelMnemonic, byte labelOpCode):
                    Console.Write($"LabelInstruction: {string.Join(" ", opCodes.Select(b => b.ToString("D2")))}: ");
                    Console.Write(mnemonic);
                    Console.WriteLine($"\t\tLabelMnemonic: «{labelMnemonic}» labelOpCode: {labelOpCode}");
                    break;

                default:
                    Console.WriteLine("Other");
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
            var com = new ASTComment(new List<ITerminalNode> { node });
            program.statements.Add(com);
            goto exit;
        }

        // White space is only processed if top level
        if (node.Parent is ParserRuleContext ruleContext && ruleContext.RuleIndex == ti58Parser.RULE_program)
        {
            var ws = new ASTWhiteSpace(new List<ITerminalNode> { node });
            program.statements.Add(ws);
            goto exit;
        }


    exit:
        return base.VisitTerminal(node);
    }

    public override object VisitNumber([NotNull] ti58Parser.NumberContext context)
    {
        var tn = GetTerminalNodes(context);
        var text = context.GetText();
        // Will build list of opcodes later
        var num = new ASTNumber(tn, context, new(), text);
        program.statements.Add(num);

        // Don't think we need to analyze deeper
        return null;
        //return base.VisitNumber(context);
    }

    public override object VisitInstruction_atomic_simple([NotNull] ti58Parser.Instruction_atomic_simpleContext context)
        => AtomicInstruction(context);

    public override object VisitInstruction_atomic_invertible([NotNull] ti58Parser.Instruction_atomic_invertibleContext context)
        => AtomicInstruction(context);

    public override object VisitInstruction_atomic_inverted([NotNull] ti58Parser.Instruction_atomic_invertedContext context)
        => AtomicInstruction(context);

    // Skip optional WS if present
    void MoveToNextSymbol(List<ITerminalNode> tn, ref int ix)
    {
        ix++;
        if (tn[ix].Symbol.Type == ti58Lexer.WS)
            ix++;
    }

    object AtomicInstruction(ParserRuleContext context)
    {
        var tn = GetTerminalNodes(context);
        var text = context.GetText();

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        List<byte> opCodes = new();

        // INV prefix?
        if (tn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            MoveToNextSymbol(tn, ref ixInstruction);
        }

        string symbolicName = _parser.Vocabulary.GetSymbolicName(tn[ixInstruction].Symbol.Type);
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
        else
        {
            Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
            opCodes.Add(byte.Parse(symbolicName[1..3]));
        }

        var ai = new ASTInstructionAtomic(tn, context, opCodes, inverted, text);
        program.statements.Add(ai);

        return null;
    }

    public override object VisitInstruction_fix([NotNull] ti58Parser.Instruction_fixContext context) => InstructionArg(context);
    public override object VisitInstruction_setflag([NotNull] ti58Parser.Instruction_setflagContext context) => InstructionArg(context);
    public override object VisitInstruction_op([NotNull] ti58Parser.Instruction_opContext context) => InstructionArg(context);
    public override object VisitInstruction_pgm([NotNull] ti58Parser.Instruction_pgmContext context) => InstructionArg(context);
    public override object VisitInstruction_memory([NotNull] ti58Parser.Instruction_memoryContext context) => InstructionArg(context);


    object InstructionArg(ParserRuleContext context)
    {
        var tn = GetTerminalNodes(context);
        var text = context.GetText();

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        YesNoImplicit argIndirect = YesNoImplicit.No;
        List<byte> opCodes = new();

        // INV prefix?
        if (tn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            MoveToNextSymbol(tn, ref ixInstruction);
        }

        string symbolicName = _parser.Vocabulary.GetSymbolicName(tn[ixInstruction].Symbol.Type);
        Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
        opCodes.Add(byte.Parse(symbolicName[1..3]));
        if (symbolicName.Contains("indirect"))
            argIndirect = YesNoImplicit.Implicit;

        MoveToNextSymbol(tn, ref ixInstruction);

        // Ind prefix?
        if (tn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            // We don't add Ind prefix twice in case we have something like SM* Ind 40, just consider it's SUM Ind 40 or SM* 40
            if (argIndirect == YesNoImplicit.No)
            {
                argIndirect = YesNoImplicit.Yes;
                opCodes.Add(40);
            }
            MoveToNextSymbol(tn, ref ixInstruction);
        }

        byte argValue = byte.Parse(tn[ixInstruction].GetText());
        opCodes.Add(argValue);

        var aa = new ASTInstructionArg(tn, context, opCodes, inverted, text, argIndirect, argValue);
        program.statements.Add(aa);

        return null;
    }


    public override object VisitInstruction_label([NotNull] ti58Parser.Instruction_labelContext context)
    {
        var tn = GetTerminalNodes(context);
        var text = context.GetText();

        int ixInstruction = 0;
        List<byte> opCodes = new();

        Debug.Assert(tn[ixInstruction].Symbol.Type == ti58Lexer.I76_label);
        opCodes.Add(76);
        MoveToNextSymbol(tn, ref ixInstruction);

        string symbolicName = _parser.Vocabulary.GetSymbolicName(tn[ixInstruction].Symbol.Type);
        string labelMnemonic;
        byte labelOpCode = 0;
        if (symbolicName != null)
        {   // Instruction label
            labelMnemonic = tn[ixInstruction].GetText();
            Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
            labelOpCode = byte.Parse(symbolicName[1..3]);
        }
        else
        {
            foreach (var d in tn[ixInstruction..])
                labelOpCode = (byte)(10 * labelOpCode + byte.Parse(d.GetText()));
            labelMnemonic = $"{labelOpCode:D2}";
        }
        opCodes.Add(labelOpCode);

        var ls = new ASTInstructionLabel(tn, context, opCodes, YesNoImplicit.No, text, labelMnemonic, labelOpCode);
        program.statements.Add(ls);

        return null;
    }


    public override object VisitInstruction_branch([NotNull] ti58Parser.Instruction_branchContext context)
    {
        var tn = GetTerminalNodes(context);
        var text = context.GetText();

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        YesNoImplicit targetIndirect = YesNoImplicit.No;
        List<byte> opCodes = new();

        // INV prefix?
        if (tn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            MoveToNextSymbol(tn, ref ixInstruction);
        }

        string symbolicName = _parser.Vocabulary.GetSymbolicName(tn[ixInstruction].Symbol.Type);
        Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
        opCodes.Add(byte.Parse(symbolicName[1..3]));
        if (symbolicName.Contains("indirect"))
            targetIndirect = YesNoImplicit.Implicit;
        MoveToNextSymbol(tn, ref ixInstruction);

        // Ind prefix?
        if (tn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            // We don't add Ind prefix twice in case we have something like SM* Ind 40, just consider it's SUM Ind 40 or SM* 40
            if (targetIndirect == YesNoImplicit.No)
            {
                targetIndirect = YesNoImplicit.Yes;
                opCodes.Add(40);
            }
            MoveToNextSymbol(tn, ref ixInstruction);
        }

        // Determine if target is a label, a numeric label or an address
        symbolicName = _parser.Vocabulary.GetSymbolicName(tn[ixInstruction].Symbol.Type);
        string targetMnemonic = "?";
        int targetValue = 0;           // byte is not enough since addresses are 0..999
        if (symbolicName != null)
        {   // Instruction label
            targetMnemonic = tn[ixInstruction].GetText();
            Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
            targetValue = byte.Parse(symbolicName[1..3]);
            opCodes.Add((byte)targetValue);
        }
        else
        {
                int digitsCount = 0;
                foreach (var d in tn[ixInstruction..])
                    if (d.Symbol.Type != ti58Lexer.WS)
                    {
                        digitsCount++;
                        targetValue = 10 * targetValue + byte.Parse(d.GetText());
                    }
                switch (digitsCount)
                {
                    case 2:
                        targetMnemonic = $"{targetValue:D2}";
                        opCodes.Add((byte)targetValue);
                        break;
                    case 3:
                    case 4:
                        targetMnemonic = $"{targetValue/100:D2} {targetValue % 100:D2}";
                        opCodes.Add((byte)(targetValue/100));
                        opCodes.Add((byte)(targetValue%100));
                        break;
                }
        }

        var aa = new ASTInstructionBranch(tn, context, opCodes, inverted, text, targetIndirect, targetMnemonic, targetValue);
        program.statements.Add(aa);

        return null;
    }


    private List<ITerminalNode> GetTerminalNodes(ParserRuleContext context)
    {
        var tn = new List<ITerminalNode>();

        foreach (var child in context.children)
        {
            if (child is ParserRuleContext ps)
                tn.AddRange(GetTerminalNodes(ps));
            else if (child is ITerminalNode term)
                tn.Add(term);
            else
                Debugger.Break();
        }

        return tn;
    }

}