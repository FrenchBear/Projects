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
using static SimpleParser.MyTi58VisitorBaseColorize;
using static SimpleParser.StandardInstructions;

namespace SimpleParser;

public enum YesNoImplicit
{
    No,
    Yes,
    Implicit
}

public record ASTToken(string Text, SyntaxCategory Cat);
//{
//    public string Text { get; set; } = text;
//    public SyntaxCategory Cat { get; set; } = cat;
//}

public record ASTProgram(List<ASTStatementBase> Statements);

public abstract record ASTStatementBase(List<ASTToken> AstTokens);
public record ASTInterStatementWhiteSpace(List<ASTToken> AstTokens): ASTStatementBase(AstTokens);
public record ASTComment(List<ASTToken> AstTokens): ASTStatementBase(AstTokens);
public abstract record ASTInstruction(List<ASTToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted): ASTStatementBase(AstTokens);
public record ASTNumber(List<ASTToken> AstTokens, List<byte> OpCodes): ASTInstruction(AstTokens, OpCodes, YesNoImplicit.No);
public record ASTInstructionAtomic(List<ASTToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted): ASTInstruction(AstTokens, OpCodes, Inverted);
public record ASTInstructionArg(List<ASTToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, YesNoImplicit ArgIndirect, byte ArgValue): ASTInstructionAtomic(AstTokens, OpCodes, Inverted);
public record ASTInstructionLabel(List<ASTToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, string LabelMnemonic, byte LabelOpCode): ASTInstruction(AstTokens, OpCodes, Inverted);
// Branch includes GTO, GT*, SBR, [INV] x=t, [INV] x≥t
public record ASTInstructionBranch(List<ASTToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, YesNoImplicit TargetIndirect, string TargetMnemonic, int TargetValue): ASTInstructionAtomic(AstTokens, OpCodes, Inverted);
// ArgBranch includes If Flag, Dsz
public record ASTInstructionArgBranch(List<ASTToken> AstTokens, List<byte> OpCodes, YesNoImplicit Inverted, YesNoImplicit ArgIndirect, byte ArgValue, YesNoImplicit TargetIndirect, string TargetMnemonic, int TargetValue): ASTInstructionArg(AstTokens, OpCodes, Inverted, ArgIndirect, ArgValue);

// Inherit from the generated base visitor.
// The 'object' type means your visit methods can return any type.
public class MyTi58VisitorBaseAST(ti58Parser parser): ti58BaseVisitor<object>
{
    public readonly ASTProgram program = new([]);
    public readonly MyTi58VisitorBaseColorize colorVisitor = new(parser);

    internal void PostProcessAST()
    {
        // Standardization of instructions: replaces STA or SIG+ by Σ+
        foreach (var sta in program.Statements)
        {
            // Normalize instruction names and keys labels
            if (sta is ASTInstruction(List<ASTToken> astTokens, _, _) inst)
                for (var i = 0; i < astTokens.Count; i++)
                    if (inst.AstTokens[i].Cat is SyntaxCategory.Instruction or SyntaxCategory.KeyLabel)
                    {
                        string txt = inst.AstTokens[i].Text;
                        foreach (var lsi in Sill)
                            if ((string.Equals(txt, lsi[0], StringComparison.CurrentCultureIgnoreCase) && txt != lsi[0]) || lsi[1..].Any(af => string.Equals(txt, af, StringComparison.CurrentCultureIgnoreCase)))
                                inst.AstTokens[i] = inst.AstTokens[i] with { Text = lsi[0] };
                    }
        }

        // ToDo: Group top level numbers

        // ToDo: Group digits in registers (direct and indirect), op, pgm, ... (flags and Dsz use 1 digit by default)

        // ToDo: Replace addresses "0n nn" by "nnn"

        // ToDo: Line comment processing so they can be printed after an instruction in case they're behind an instruction in the code
        // (and maybe align comments Rust or Go style)

        // ToDo: build a list of labels and a list of statements start instruction to validate direct address and labels
    }

    internal void PrintFormattedAST()
    {
        Console.WriteLine();
        int cp = 0;
        foreach (ASTStatementBase sta in program.Statements)
        {
            switch (sta)
            {
                // Should do a better formatting, if comment is at the end of a source line, it should
                // be printed at the correct location, not on following line...
                case ASTComment(var astTokens):
                    Colorize(astTokens[0]);
                    Console.WriteLine();
                    break;

                case ASTInterStatementWhiteSpace(_):
                    //Console.WriteLine("Inter-statement WhiteSpace");
                    break;

                case ASTInstruction(var astTokens, var opCodes, _):
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{cp:D3}: ");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    Console.Write($"{string.Join(" ", opCodes.Take(5).Select(b => b.ToString("D2"))),-15} ");
                    if (astTokens[0].Text.StartsWith("LBL", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("■ ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                        Console.Write("  ");

                    foreach (ASTToken token in astTokens)
                        if (token.Cat != SyntaxCategory.WhiteSpace)
                            //    Console.Write(" ");     // Normalize existing white spaces to a single space
                            //                            // Should do a better reformatting in the future, add missing spaces
                            //else
                            Colorize("‹" + token.Text + "› ", token.Cat);
                    Console.WriteLine();

                    while (opCodes.Count > 5)
                    {
                        cp += 5;
                        opCodes.RemoveRange(0, 5);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{cp:D3}: ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"{string.Join(" ", opCodes.Take(5).Select(b => b.ToString("D2"))),-15} ");
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
            var at = new List<ASTToken> {
                new(node.GetText(), SyntaxCategory.Comment)
            };
            var com = new ASTComment(at);
            program.Statements.Add(com);
        }
        //else if (node.Parent is ParserRuleContext ruleContext && ruleContext.RuleIndex == ti58Parser.RULE_program)
        else if (node.Parent is ParserRuleContext { RuleIndex: ti58Parser.RULE_program })
        {
            // White space is only processed if top level

            var at = new List<ASTToken> {
                new(node.GetText(), SyntaxCategory.WhiteSpace)
            };
            var ws = new ASTInterStatementWhiteSpace(at);
            program.Statements.Add(ws);
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

        var at = new List<ASTToken>();
        foreach (var tn in ltn)
            at.Add(new ASTToken(tn.GetText(), colorVisitor.GetTerminalSyntaxCategory(tn)));

        var num = new ASTNumber(at, opCodes);
        program.Statements.Add(num);

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

    // Skip optional WS if present
    static void MoveToNextSymbol(List<ITerminalNode> tn, ref int ix)
    {
        ix++;
        if (tn[ix].Symbol.Type == ti58Lexer.WS)
            ix++;
    }

    object AtomicInstruction(ParserRuleContext context)
    {
        var ltn = GetTerminalNodes(context);
        //var text = context.GetText();

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        List<byte> opCodes = [];

        // INV prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            MoveToNextSymbol(ltn, ref ixInstruction);
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
            opCodes[0] = 92;
        }
        else if (symbolicName == "Bang")
        {
            // Nop
        }
        else
        {
            Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
            opCodes.Add(byte.Parse(symbolicName[1..3]));
        }

        var at = new List<ASTToken>();
        foreach (var tn in ltn)
            at.Add(new ASTToken(tn.GetText(), colorVisitor.GetTerminalSyntaxCategory(tn)));

        var ai = new ASTInstructionAtomic(at, opCodes, inverted);
        program.Statements.Add(ai);

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

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        YesNoImplicit argIndirect = YesNoImplicit.No;
        List<byte> opCodes = [];

        // INV prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            MoveToNextSymbol(ltn, ref ixInstruction);
        }

        int mainType = ltn[ixInstruction].Symbol.Type;
        string symbolicName = parser.Vocabulary.GetSymbolicName(mainType);
        Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
        opCodes.Add(byte.Parse(symbolicName[1..3]));
        if (symbolicName.Contains("indirect"))
            argIndirect = YesNoImplicit.Implicit;

        MoveToNextSymbol(ltn, ref ixInstruction);

        // Ind prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            // Merged indirect operations (IND GTO is handled in InstructionBranch)
            var l = opCodes.Count - 1;
            switch (opCodes[l])
            {
                case 42:        // STO
                    opCodes[l] = 72;
                    argIndirect = YesNoImplicit.Implicit;
                    break;
                case 43:        // RCL
                    opCodes[l] = 73;
                    argIndirect = YesNoImplicit.Implicit;
                    break;
                case 44:        // SUM
                    opCodes[l] = 74;
                    argIndirect = YesNoImplicit.Implicit;
                    break;
                case 48:        // EXC
                    opCodes[l] = 63;
                    argIndirect = YesNoImplicit.Implicit;
                    break;
                case 49:        // PRD
                    opCodes[l] = 64;
                    argIndirect = YesNoImplicit.Implicit;
                    break;
                case 36:        // PGM
                    opCodes[l] = 62;
                    argIndirect = YesNoImplicit.Implicit;
                    break;
                case 69:        // Op
                    opCodes[l] = 84;
                    argIndirect = YesNoImplicit.Implicit;
                    break;
            }

            // For non-merged instruction, add explicitly 40
            if (argIndirect == YesNoImplicit.No)
            {
                argIndirect = YesNoImplicit.Yes;
                opCodes.Add(40);
            }
            MoveToNextSymbol(ltn, ref ixInstruction);
        }

        byte argValue = 0;
        foreach (var d in ltn[ixInstruction..])
            argValue = (byte)(10 * argValue + byte.Parse(d.GetText()));
        opCodes.Add(argValue);

        var at = new List<ASTToken>();
        foreach (var tn in ltn)
            at.Add(new ASTToken(tn.GetText(), colorVisitor.GetTerminalSyntaxCategory(tn)));

        var aa = new ASTInstructionArg(at, opCodes, inverted, argIndirect, argValue);
        program.Statements.Add(aa);

        return null!;
    }

    public override object VisitInstruction_label([NotNull] ti58Parser.Instruction_labelContext context)
    {
        var ltn = GetTerminalNodes(context);

        int ixInstruction = 0;
        List<byte> opCodes = [];

        Debug.Assert(ltn[ixInstruction].Symbol.Type == ti58Lexer.I76_label);
        opCodes.Add(76);
        MoveToNextSymbol(ltn, ref ixInstruction);

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

        var at = new List<ASTToken>();
        foreach (var tn in ltn)
            at.Add(new ASTToken(tn.GetText(), colorVisitor.GetTerminalSyntaxCategory(tn)));

        var ls = new ASTInstructionLabel(at, opCodes, YesNoImplicit.No, labelMnemonic, labelOpCode);
        program.Statements.Add(ls);

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

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        YesNoImplicit targetIndirect = YesNoImplicit.No;
        List<byte> opCodes = [];

        // INV prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            MoveToNextSymbol(ltn, ref ixInstruction);
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

        MoveToNextSymbol(ltn, ref ixInstruction);

        // Ind prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            // Merged IND GTO
            var l = opCodes.Count - 1;
            if (opCodes[l] == 61)
            {
                opCodes[l] = 83;
                targetIndirect = YesNoImplicit.Implicit;
            }

            if (targetIndirect == YesNoImplicit.No)
            {
                targetIndirect = YesNoImplicit.Yes;
                opCodes.Add(40);
            }
            MoveToNextSymbol(ltn, ref ixInstruction);
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
        }
        else
        {
            int digitsCount = 0;
            foreach (var d in ltn[ixInstruction..])
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
                    targetMnemonic = $"{targetValue:D3}";
                    opCodes.Add((byte)(targetValue / 100));
                    opCodes.Add((byte)(targetValue % 100));
                    break;
            }
        }

        var at = new List<ASTToken>();
        foreach (var tn in ltn)
            at.Add(new ASTToken(tn.GetText(), colorVisitor.GetTerminalSyntaxCategory(tn)));

        var aa = new ASTInstructionBranch(at, opCodes, inverted, targetIndirect, targetMnemonic, targetValue);
        program.Statements.Add(aa);

        return null!;
    }

    public override object VisitInstruction_test_flag([NotNull] ti58Parser.Instruction_test_flagContext context)
        => InstructionArgBranch(context);

    public override object VisitInstruction_decrement_and_skip_on_zero([NotNull] ti58Parser.Instruction_decrement_and_skip_on_zeroContext context)
        => InstructionArgBranch(context);

    object InstructionArgBranch(ParserRuleContext context)
    {
        var ltn = GetTerminalNodes(context);

        int ixInstruction = 0;
        YesNoImplicit inverted = YesNoImplicit.No;
        YesNoImplicit argIndirect = YesNoImplicit.No;
        List<byte> opCodes = [];

        // INV prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I22_invert)
        {
            inverted = YesNoImplicit.Yes;
            opCodes.Add(22);
            MoveToNextSymbol(ltn, ref ixInstruction);
        }

        string symbolicName = parser.Vocabulary.GetSymbolicName(ltn[ixInstruction].Symbol.Type);
        Debug.Assert(symbolicName.StartsWith('I') && symbolicName[3] == '_');
        opCodes.Add(byte.Parse(symbolicName[1..3]));
        if (symbolicName.Contains("indirect"))
            argIndirect = YesNoImplicit.Implicit;

        MoveToNextSymbol(ltn, ref ixInstruction);

        // Ind prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            // We don't add Ind prefix twice in case we have something like SM* Ind 40, just consider it's SUM Ind 40 or SM* 40
            if (argIndirect == YesNoImplicit.No)
            {
                argIndirect = YesNoImplicit.Yes;
                opCodes.Add(40);
            }
            MoveToNextSymbol(ltn, ref ixInstruction);
        }

        byte argValue = 0;
        for (; ; )
        {
            var txt = ltn[ixInstruction].GetText();
            if (!byte.TryParse(txt, out byte b))
                break;
            argValue = (byte)(10 * argValue + b);
            ixInstruction++;
        }
        Debug.Assert(argValue != 40);       // Temp for dev; Dsz 40 should be rejected as invalid, since it means Dsz Ind
        opCodes.Add(argValue);
        MoveToNextSymbol(ltn, ref ixInstruction);

        // Target ---------------------------------
        YesNoImplicit targetIndirect = YesNoImplicit.No;

        // Ind prefix?
        if (ltn[ixInstruction].Symbol.Type == ti58Lexer.I40_indirect)
        {
            targetIndirect = YesNoImplicit.Yes;
            opCodes.Add(40);
            MoveToNextSymbol(ltn, ref ixInstruction);
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
        }
        else
        {
            int digitsCount = 0;
            foreach (var d in ltn[ixInstruction..])
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
                    targetMnemonic = $"{targetValue:D3}";
                    opCodes.Add((byte)(targetValue / 100));
                    opCodes.Add((byte)(targetValue % 100));
                    break;
            }
        }

        var at = new List<ASTToken>();
        foreach (var tn in ltn)
            at.Add(new ASTToken(tn.GetText(), colorVisitor.GetTerminalSyntaxCategory(tn)));

        var aa = new ASTInstructionArgBranch(at, opCodes, inverted, argIndirect, argValue, targetIndirect, targetMnemonic, targetValue);
        program.Statements.Add(aa);

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
                tn.Add(term);
            else
                Debugger.Break();
        }

        return tn;
    }

}