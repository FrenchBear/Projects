// Ast post-processing
// After Ast has been built, some transformations of the Ast itself:
// - Instruction normalisation: Sto -> STO, STO IND -> ST*, ABS -> |x|...
// - Digits grouping in memory, addresses (because of grammar, STO 02 contains two tokens DirectMemoryOrNumber 0 and 2)
// - An extension, well-formed sequences of numbers are printed on one line (6 . 0 2 EE 2 3 +/-  -->  6.02E-23)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static SimpleParser.MyTi58VisitorBaseColorize;
using static SimpleParser.StandardInstructions;

#pragma warning disable CA1822 // Mark members as static

namespace SimpleParser;

public class AstPostProcessor
{
    internal void PostProcessAst(AstProgram program)
    {
        StandardizeInstructions(program);
        GroupDigits(program);
        GroupNumbers(program);
        BuildInstructionAddresses(program);
    }

    private void StandardizeInstructions(AstProgram Program)
    {
        // Standardization of instructions: replaces STA or SIG+ by Σ+, Sto by STO...
        // Use the first symbol in lists of synonyms in StandardInstructions.Sill as standard representation
        foreach (var sta in Program.Statements)
            if (sta is AstInstruction(List<AstToken> astTokens, _, _) inst)
                for (var i = 0; i < astTokens.Count; i++)
                    if (astTokens[i].Cat is SyntaxCategory.Instruction or SyntaxCategory.KeyLabel)
                        foreach (var lsi in Sill)
                            if ((string.Equals(astTokens[i].Text, lsi[0], StringComparison.CurrentCultureIgnoreCase) && astTokens[i].Text != lsi[0]) || lsi[1..].Any(af => string.Equals(astTokens[i].Text, af, StringComparison.CurrentCultureIgnoreCase)))
                                astTokens[i] = astTokens[i] with { Text = lsi[0] };
    }

    private void GroupDigits(AstProgram Program)
    {
        // Group digits in registers (direct and indirect), op, pgm, ... (flags and Dsz use 1 digit by default)
        foreach (var sta in Program.Statements)
        {
            if (sta is AstInstruction(List<AstToken> astTokens, _, _))
            {
                // Merge DirectMemoryOrNumber, IndirectMemory, numeric KeyLabel and DirectAddresses
                for (var i = 0; i < astTokens.Count; i++)
                    while (i < astTokens.Count - 1 && (astTokens[i].Cat == SyntaxCategory.DirectMemoryOrNumber || astTokens[i].Cat == SyntaxCategory.IndirectMemory || astTokens[i].Cat == SyntaxCategory.KeyLabel || astTokens[i].Cat == SyntaxCategory.DirectAddress) && astTokens[i].Cat == astTokens[i + 1].Cat)
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
    }

    private struct NumberContext
    {
        public bool dotFound;
        public bool eeFound;
        public int mantissaDigits;
        public int exponentDigits;
        public bool mantissaSignFound;
        public bool exponentSignFound;
        public int ixEE;
        public int ixExisting;
    }

    private void GroupNumbers(AstProgram Program)
    {
        // Group top level numbers
        // ToDo: . also starts grouping
        for (int i = 0; i < Program.Statements.Count; i++)
        {
            AstStatementBase? GetNextStatement()
            {
                for (int j = i + 1; j < Program.Statements.Count; j++)
                {
                    if (Program.Statements[j] is AstInterStatementWhiteSpace(_))
                        continue;
                    if (Program.Statements[j] is AstNumber(_, _))
                        return Program.Statements[j];
                    if (Program.Statements[j] is AstInstructionAtomic(_, _, _) next_inst)
                    {
                        var nop = next_inst.OpCodes[0];
#pragma warning disable IDE0046 // Convert to conditional expression
                        if (nop is 93 or 94 or 52)      // . +/- or EE   Note that we can't test mnemonic because "INV EE" has a mnemonic EE but is not valid
                            return next_inst;
                        return null;
                    }
                    return null;
                }
                return null;
            }

            // First, determine if this can start a sequence
            // Probably need to be extracted to a separate function, it's too long

            // ToDo: We just consider numbers, but a dot can start a sequence (just EE not, too special: in entry mode (for instance, after
            // CLR) a sequence EE 1 2 . 45 actually means 0.45E12) but we don't  know if we are in entry mode or not from static analysis.
            // Also note that if it starts with a dot, we need to change statement from AstInstruction to AstNumber... But a valid next statement
            // is guaranteed to be a number: . . is stupid, . +/- ok but unelikely, and . EE barely Ok
            // There are several esamples in ML-25.t59
            var nc = new NumberContext();
            if (Program.Statements[i] is AstNumber(List<AstToken> tokens, List<byte> opCodes))
            {
                // Analyse current number state
                for (; nc.ixExisting < opCodes.Count; nc.ixExisting++)
                {
                    var opCode = opCodes[nc.ixExisting];

                    if (opCode is >= 0 and <= 9)
                    {
                        if (nc.eeFound)
                            nc.exponentDigits++;
                        else
                            nc.mantissaDigits++;
                        continue;
                    }
                    if (opCode == 93)
                    {
                        nc.dotFound = true;
                        continue;
                    }
                    if (opCode == 52)
                    {
                        nc.eeFound = true;
                        nc.ixEE = nc.ixExisting;
                        continue;
                    }
                    if (opCode == 94)
                    {
                        if (nc.eeFound)
                            nc.exponentSignFound = true;
                        else
                            nc.mantissaSignFound = true;
                        continue;
                    }
                    // Unreachable
                    Debugger.Break();
                }

            AnalyzeNextStatement:
                var next_st = GetNextStatement();

                if (next_st == null)
                    continue;

                if (next_st is AstNumber(List<AstToken> next_tokens, List<byte> next_opCodes))
                {
                    // Check that the whole stuff can be added
                    for (int j = 0; j < next_opCodes.Count; j++)
                    {
                        var opCode = next_opCodes[j];

                        if (opCode is >= 0 and <= 9)
                        {
                            if (nc.eeFound)
                            {
                                nc.exponentDigits++;
                                if (nc.exponentDigits > 2)
                                    goto ContinueNextMainStatement;
                            }
                            else
                            {
                                nc.mantissaDigits++;
                                if (nc.exponentDigits > 2)
                                    goto ContinueNextMainStatement;
                            }
                            continue;
                        }
                        if (opCode == 93)   // .
                        {
                            if (nc.dotFound)
                                goto ContinueNextMainStatement;
                            nc.dotFound = true;
                            continue;
                        }
                        if (opCode == 52)   // EE
                        {
                            if (nc.eeFound)
                                goto ContinueNextMainStatement;
                            nc.eeFound = true;
                            nc.ixEE = nc.ixExisting + j;
                            continue;
                        }
                        if (opCode == 94)   // +/-
                        {
                            if (nc.eeFound)
                            {
                                if (nc.exponentSignFound)
                                    goto ContinueNextMainStatement;
                                nc.exponentSignFound = true;
                            }
                            else
                            {
                                if (nc.mantissaSignFound)
                                    goto ContinueNextMainStatement;
                                nc.mantissaSignFound = true;
                            }
                            continue;
                        }
                        // Unreachable
                        Debugger.Break();
                    }

                    // Good, if we're there, next number can be merged
                    for (int j = 0; j < next_opCodes.Count; j++)
                    {
                        opCodes.Add(next_opCodes[j]);
                        tokens.Add(next_tokens[j]);
                        nc.ixExisting++;
                    }

                    // Don't need next program Number (and also remove inter-statement white space
                    // Should be more compact...
                    if (i + 1 == Program.Statements.Count)
                        goto ContinueNextMainStatement; // Actually, end of the loop

                    if (Program.Statements[i + 1] is AstInterStatementWhiteSpace(_))
                        Program.Statements.RemoveAt(i + 1);
                    else
                        Debugger.Break();

                    if (i + 1 == Program.Statements.Count)
                        goto ContinueNextMainStatement;

                    if (Program.Statements[i + 1] is AstNumber(_, _))
                        Program.Statements.RemoveAt(i + 1);
                    else
                        Debugger.Break();

                    // Restart in case there's also something mergeable just after
                    goto AnalyzeNextStatement;
                }

                if (next_st is AstInstruction(List<AstToken> ni_tokens, List<byte> ni_opCodes, _))
                {
                    var nop = ni_opCodes[0];

                    Debug.Assert(ni_tokens.Count == 1);
                    Debug.Assert(ni_opCodes.Count == 1);

                    // Check if the statement can be merged:
                    switch (nop)
                    {
                        case 93:    // .
                            if (nc.dotFound)
                                goto ContinueNextMainStatement;
                            nc.dotFound = true;
                            break;

                        case 94:    // +/-
                            if (nc.eeFound)
                            {
                                if (nc.mantissaSignFound)
                                    goto ContinueNextMainStatement;
                                nc.mantissaSignFound = true;
                            }
                            else
                            {
                                if (nc.exponentSignFound)
                                    goto ContinueNextMainStatement;
                                nc.exponentSignFound = true;
                            }
                            break;

                        case 52:    // EE
                            if (nc.eeFound)
                                goto ContinueNextMainStatement;
                            nc.eeFound = true;
                            break;

                        default:
                            Debugger.Break();       // Unreachable
                            break;
                    }

                    // Ok, we can merge
                    opCodes.Add(nop);
                    tokens.Add(ni_tokens[0]);

                    // Don't need next program Number (and also remove inter-statement white space
                    // Should be more compact...
                    if (i + 1 == Program.Statements.Count)
                        goto ContinueNextMainStatement; // Actually, end of the loop

                    if (Program.Statements[i + 1] is AstInterStatementWhiteSpace(_))
                        Program.Statements.RemoveAt(i + 1);
                    else
                        Debugger.Break();

                    if (i + 1 == Program.Statements.Count)
                        goto ContinueNextMainStatement;

                    if (Program.Statements[i + 1] is AstInstruction(_, _, _))
                        Program.Statements.RemoveAt(i + 1);
                    else
                        Debugger.Break();

                    // Restart in case there's also something mergeable just after
                    goto AnalyzeNextStatement;
                }

                // unreachable:
                Debugger.Break();

            ContinueNextMainStatement:
                ;
            }
        }

        // ToDo: Line comment processing so they can be printed after an instruction in case they're behind an instruction in the code
        // (and maybe align comments Rust or Go style)
    }

    private void BuildInstructionAddresses(AstProgram program)
    {
        int address = 0;
        foreach (var sta in program.Statements)
            if (sta is AstInstruction(_, _, _) inst)
            {
                inst.Address = address;
                address += inst.OpCodes.Count;
            }
            else if (sta is AstNumber(_, _) num)
            {
                num.Address = address;
                address += num.OpCodes.Count;
            }

        // Keep the whole number of OpCodes
        program.ProgramSize = address;
    }
}
