// Ast post-processing
// After Ast has been built, some transformations of the Ast itself:
// - Instruction normalisation: Sto -> STO, STO IND -> ST*, ABS -> |x|...
// - Digits grouping in memory, addresses (because of grammar, STO 02 contains two tokens DirectMemoryOrNumber 0 and 2)
// - An extension, well-formed sequences of numbers are printed on one line (6 . 0 2 EE 2 3 +/-  -->  6.02E-23)
//
// 2025-11-13   PV      Deep refactoring

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static SimpleParser.MyTi58VisitorBaseColorize;
using static SimpleParser.StandardInstructions;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0062 // Make local function 'static'
#pragma warning disable IDE0046 // Convert to conditional expression

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

    // Standardization of instructions: replaces STA or SIG+ by Σ+, Sto by STO...
    // Use the first symbol in lists of synonyms in StandardInstructions.Sill as standard representation
    private void StandardizeInstructions(AstProgram Program)
    {
        foreach (var sta in Program.Statements)
            if (sta is AstInstruction(List<AstToken> astTokens, _, _) inst)
                for (var i = 0; i < astTokens.Count; i++)
                    if (astTokens[i].Cat is SyntaxCategory.Instruction or SyntaxCategory.KeyLabel)
                        foreach (var lsi in Sill)
                            if ((string.Equals(astTokens[i].Text, lsi[0], StringComparison.CurrentCultureIgnoreCase) && astTokens[i].Text != lsi[0]) || lsi[1..].Any(af => string.Equals(astTokens[i].Text, af, StringComparison.CurrentCultureIgnoreCase)))
                                astTokens[i] = astTokens[i] with { Text = lsi[0] };
    }

    // Group digits in registers (direct and indirect), Pp, Pgm, ... (flags and Dsz use 1 digit by default)
    // since parser breaks down each digit in its own separate token
    private void GroupDigits(AstProgram Program)
    {
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

    // ----------------

    // Stores info about current number to detect if following tokens can be merged with it
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

    // GroupNumbers helper, returns AstNumber or AstInstructionAtomic[dot, EE, +/-] at index startIndex+1 ignoring
    // AstInterStatementWhiteSpace and AstComment
    // Returns null if it's not a match
    private AstStatementBase? GetNextCompatibleStatement(AstProgram program, int startIndex)
    {
        for (int j = startIndex + 1; j < program.Statements.Count; j++)
        {
            if (program.Statements[j] is AstInterStatementWhiteSpace(_) or AstComment(_))
                continue;
            if (program.Statements[j] is AstNumber(_, _))
                return program.Statements[j];
            if (program.Statements[j] is AstInstructionAtomic(_, _, _) next_inst)
            {
                var nop = next_inst.OpCodes[0];
                if (nop is 93 or 94 or 52)      // . +/- or EE   Note that we can't test mnemonic because "INV EE" has a mnemonic EE but is not valid
                    return next_inst;
                return null;
            }
            return null;
        }
        return null;
    }

    // GroupNumbers helper, analyzes the number represented by the list of opCodes, updating nc
    // Return true if opCodes are compatible with provided NumberContext, false otherwise
    bool AnalyzeNumber(List<byte> opCodes, ref NumberContext nc)
    {
        // Check that the whole stuff can be added
        for (int j = 0; j < opCodes.Count; j++)
        {
            var opCode = opCodes[j];

            if (opCode is >= 0 and <= 9)
            {
                if (nc.eeFound)
                {
                    nc.exponentDigits++;
                    if (nc.exponentDigits > 2)
                        return false;
                }
                else
                {
                    nc.mantissaDigits++;
                    if (nc.exponentDigits > 2)
                        return false;
                }
                continue;
            }
            if (opCode == 93)   // .
            {
                if (nc.dotFound)
                    return false;
                nc.dotFound = true;
                continue;
            }
            if (opCode == 52)   // EE
            {
                if (nc.eeFound)
                    return false;
                nc.eeFound = true;
                nc.ixEE = nc.ixExisting + j;
                continue;
            }
            if (opCode == 94)   // +/-
            {
                if (nc.eeFound)
                {
                    if (nc.exponentSignFound)
                        return false;
                    nc.exponentSignFound = true;
                }
                else
                {
                    if (nc.mantissaSignFound)
                        return false;
                    nc.mantissaSignFound = true;
                }
                continue;
            }
            // Unreachable
            Debugger.Break();
        }

        return true;
    }

    // // GroupNumbers helper; delete next AstNumber (is checkForNumber is true) or AstInstruction
    // Also remove inter-statement white space (tough all these may be removed at some point)
    // Ignoring comments.
    // By design, the code is guaranteed to find the statement we want to delete, or we have a logic problem

    void EraseNextAstNumberOrInstruction(AstProgram program, int startIndex, bool checkForNumber)
    {
        bool preserveISWS = false;
        int j = startIndex + 1;
        for (; ; )
        {
            if (program.Statements[j] is AstInterStatementWhiteSpace(_))
            {
                if (!preserveISWS)
                    program.Statements.RemoveAt(j);
                else
                    preserveISWS = false;
                continue;
            }

            // Don't remember if AstComment are surrounded by AstInterStatementWhiteSpace...
            // Right now, I keep a ISWS after a comment, should be tested...
            if (program.Statements[j] is AstComment(_))
            {
                preserveISWS = true;
                j++;
                continue;
            }

            if (checkForNumber)
            {
                if (program.Statements[j] is AstNumber(_, _))
                {
                    program.Statements.RemoveAt(j);
                    return;
                }
            }
            else
            {
                if (program.Statements[j] is AstInstruction(_, _, _))
                {
                    program.Statements.RemoveAt(j);
                    return;
                }
            }
        }
        // Should not happen
        Debugger.Break();
    }

    // Group a well-formed sequence of numbers  and instructions into a single number, preserving
    // original tokens order
    // for instance converts sequence "1 . 6 +/- EE +/- 1 9" into single number "-1.6e-19"
    private void GroupNumbers(AstProgram program)
    {
        // Group top level numbers
        // ToDo: . also starts grouping
        for (int i = 0; i < program.Statements.Count; i++)
        {

            // First, determine if this can start a sequence
            // Probably need to be extracted to a separate function, it's too long

            // ToDo: We just consider numbers, but a dot can start a sequence (just EE not, too special: in entry mode (for instance, after
            // CLR) a sequence EE 1 2 . 45 actually means 0.45E12) but we don't  know if we are in entry mode or not from static analysis.
            // Also note that if it starts with a dot, we need to change statement from AstInstruction to AstNumber... But a valid next statement
            // is guaranteed to be a number: . . is stupid, . +/- ok but unelikely, and . EE barely Ok
            // There are several esamples in ML-25.t59
            NumberContext nc = new();
            if (program.Statements[i] is AstNumber(List<AstToken> tokens, List<byte> opCodes))
            {
                var isOk = AnalyzeNumber(opCodes, ref nc);
                Debug.Assert(isOk);

            AnalyzeNextStatement:
                var next_st = GetNextCompatibleStatement(program, i);
                if (next_st == null)
                    continue;

                if (next_st is AstNumber(List<AstToken> next_tokens, List<byte> next_opCodes))
                {
                    // Check that the whole stuff can be added
                    isOk = AnalyzeNumber(next_opCodes, ref nc);
                    if (!isOk)
                        continue;

                    // Good, if we're there, next number can be merged
                    for (int j = 0; j < next_opCodes.Count; j++)
                    {
                        opCodes.Add(next_opCodes[j]);
                        tokens.Add(next_tokens[j]);
                        nc.ixExisting++;
                    }

                    EraseNextAstNumberOrInstruction(program, i, true);

                    // Restart in case there's also something mergeable just after
                    goto AnalyzeNextStatement;
                }

                if (next_st is AstInstruction(List<AstToken> ni_tokens, List<byte> ni_opCodes, _))
                {
                    var nop = ni_opCodes[0];

                    Debug.Assert(ni_tokens.Count == 1);
                    Debug.Assert(ni_opCodes.Count == 1);

                    // Check if the statement can be merged
                    isOk = AnalyzeNumber(ni_opCodes, ref nc);
                    if (!isOk)
                        continue;

                    // Ok, we can merge
                    opCodes.Add(nop);
                    tokens.Add(ni_tokens[0]);

                    // Don't need next program Number (and also remove inter-statement white space
                    EraseNextAstNumberOrInstruction(program, i, false);

                    // Restart in case there's also something mergeable just after
                    goto AnalyzeNextStatement;
                }

                // unreachable:
                Debugger.Break();
            }
        }
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
