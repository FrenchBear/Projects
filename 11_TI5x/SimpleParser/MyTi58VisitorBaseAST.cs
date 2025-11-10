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

public record ASTProgram(List<ASTStatement> statements);

public abstract record ASTStatement(List<ITerminalNode> nodes);
public record ASTWhiteSpace(List<ITerminalNode> nodes): ASTStatement(nodes);
public record ASTComment(List<ITerminalNode> nodes): ASTStatement(nodes);
public abstract record ASTInstruction(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte>OpCodes): ASTStatement(nodes);
public record ASTNumber(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> OpCodes, string mnemonic): ASTInstruction(nodes, ruleContext, OpCodes);
public record ASTAtomicInstruction(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> OpCodes, YesNoImplicit inverted, string mnemonic): ASTInstruction(nodes, ruleContext, OpCodes);
public record ASTInstructionArg(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> OpCodes, YesNoImplicit inverted, string mnemonic, YesNoImplicit argIndirect, int Arg): ASTAtomicInstruction(nodes, ruleContext, OpCodes, inverted, mnemonic);
public record ASTInstructionLabel(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> OpCodes, YesNoImplicit inverted, string mnemonic, string labelMnemonic, int labelOpCode): ASTAtomicInstruction(nodes, ruleContext, OpCodes, inverted, mnemonic);
// Branch includes GTO, GT*, SBR, x=t, x≥t 
public record ASTInstructionBranch(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> OpCodes, YesNoImplicit inverted, string mnemonic, YesNoImplicit targetIndirect, string targetMnemonic, int targetValue): ASTAtomicInstruction(nodes, ruleContext, OpCodes, inverted, mnemonic);
public record ASTInstructionConditionalArg(List<ITerminalNode> nodes, ParserRuleContext ruleContext, List<byte> OpCodes, YesNoImplicit inverted, string mnemonic, YesNoImplicit targetIndirect, string targetMnemonic, int targetValue, YesNoImplicit argIndirect, int argValue): ASTInstructionBranch(nodes, ruleContext, OpCodes, inverted, mnemonic, targetIndirect, targetMnemonic, targetValue);


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
        foreach (ASTStatement sta in program.statements)
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

                case ASTAtomicInstruction(_, _, var opCodes, var inverted, var mnemonic):
                    Console.Write($"Instruction: {string.Join(" ", opCodes.Select(b => b.ToString("D2")))}: ");
                    //if (inverted==YesNoImplicit.Yes)
                    //    Console.Write("INV ");
                    Console.WriteLine(mnemonic);
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

    public override object VisitAtomic_instruction([NotNull] ti58Parser.Atomic_instructionContext context)
        => AtomicInstruction(context);
    //{
    //    var tn = GetTerminalNodes(context);
    //    var text = context.GetText();

    //    int tokenType = tn.First().Symbol.Type;
    //    string symbolicName = _parser.Vocabulary.GetSymbolicName(tokenType);
    //    Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3]=='_');
    //    var opCode = byte.Parse(symbolicName[1..3]);
    //    var ai = new ASTAtomicInstruction(tn, context, new List<byte> { opCode }, YesNoImplicit.No, text);
    //    program.statements.Add(ai);

    //    return null;
    //}

    public override object VisitAtomic_instruction_inverted([NotNull] ti58Parser.Atomic_instruction_invertedContext context)
                => AtomicInstruction(context);

    //{
    //    var tn = GetTerminalNodes(context);
    //    var text = context.GetText();

    //    int tokenType = tn.First().Symbol.Type;

    //    var opcodes = tokenType switch {
    //        ti58Lexer.I123_e_power_x => new List<byte> { 22, 23 },
    //        ti58Lexer.I128_10_power_x => new List<byte> { 22, 28 },
    //        _ => new List<byte>()
    //    };

    //    string symbolicName = _parser.Vocabulary.GetSymbolicName(tokenType);
    //    Debug.Assert(symbolicName.StartsWith("I1"));
    //    var ai = new ASTAtomicInstruction(tn, context, opcodes, YesNoImplicit.Implicit, text);
    //    program.statements.Add(ai);

    //    return null;
    //}

    public override object VisitAtomic_instruction_invertible([NotNull] ti58Parser.Atomic_instruction_invertibleContext context)
        => AtomicInstruction(context);

    object AtomicInstruction(ParserRuleContext context)
    { 
        var tn = GetTerminalNodes(context);
        var text = context.GetText();

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        List<byte> opCodes = new();
        if (tn.First().Symbol.Type==ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            ixInstruction++;
            // Skip optional WS if present
            if (tn[ixInstruction].Symbol.Type == ti58Lexer.WS)
                ixInstruction++;
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

        var ai = new ASTAtomicInstruction(tn, context, opCodes, inverted, text);
        program.statements.Add(ai);

        return null;
    }

    private List<ITerminalNode> GetTerminalNodes(ParserRuleContext context)
    {
        var tn = new List<ITerminalNode>();
        //IParseTree pt;
        //Antlr4.Runtime.IToken it;
        //Antlr4.Runtime.RuleContext rc;

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