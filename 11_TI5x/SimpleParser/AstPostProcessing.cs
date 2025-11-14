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
    private void StandardizeInstructions(AstProgram program)
    {
        foreach (var sta in program.Statements)
            if (sta is AstInstruction(List<AstToken> astTokens, _, _))
                for (var i = 0; i < astTokens.Count; i++)
                    if (astTokens[i].Cat is SyntaxCategory.Instruction or SyntaxCategory.KeyLabel)
                        foreach (var lsi in Sill)
                            if ((string.Equals(astTokens[i].Text, lsi[0], StringComparison.CurrentCultureIgnoreCase) && astTokens[i].Text != lsi[0]) || lsi[1..].Any(af => string.Equals(astTokens[i].Text, af, StringComparison.CurrentCultureIgnoreCase)))
                                astTokens[i] = astTokens[i] with { Text = lsi[0] };
    }

    // Group digits in registers (direct and indirect), Pp, Pgm, ... (flags and Dsz use 1 digit by default)
    // since parser breaks down each digit in its own separate token
    private void GroupDigits(AstProgram program)
    {
        foreach (var sta in program.Statements)
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
                var isFlgDsz = false;
                for (var i = 0; i < astTokens.Count; i++)
                {
                    if (astTokens[i].Cat == SyntaxCategory.Instruction && (astTokens[i].Text == "STF" || astTokens[i].Text == "IFF" || astTokens[i].Text == "Dsz"))
                        isFlgDsz = true;
                    if ((astTokens[i].Cat == SyntaxCategory.DirectMemoryOrNumber && !isFlgDsz) || astTokens[i].Cat == SyntaxCategory.IndirectMemory)
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
        public bool DotFound;
        public bool EeFound;
        public int MantissaDigits;
        public int ExponentDigits;
        public bool MantissaSignFound;
        public bool ExponentSignFound;
        public int IxEe;
        public int IxExisting;
        public bool isNumber;
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
            if (program.Statements[j] is AstInstructionAtomic(_, _, _) nextInst)
            {
                var nop = nextInst.OpCodes[0];
                if (nop is 93 or 94 or 52)      // . +/- or EE   Note that we can't test mnemonic because "INV EE" has a mnemonic EE but is not valid
                    return nextInst;
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

            if (opCode <= 9)
            {
                if (nc.EeFound)
                {
                    nc.ExponentDigits++;
                    if (nc.ExponentDigits > 2)
                        return false;
                }
                else
                {
                    nc.MantissaDigits++;
                    if (nc.MantissaDigits > 10)
                        return false;
                }
                continue;
            }
            if (opCode == 93)   // .
            {
                if (nc.DotFound)
                    return false;
                nc.DotFound = true;
                continue;
            }
            if (opCode == 52)   // EE
            {
                if (nc.EeFound)
                    return false;
                nc.EeFound = true;
                nc.IxEe = nc.IxExisting + j;
                continue;
            }
            if (opCode == 94)   // +/-
            {
                if (nc.EeFound)
                {
                    if (nc.ExponentSignFound)
                        return false;
                    nc.ExponentSignFound = true;
                }
                else
                {
                    if (nc.MantissaSignFound)
                        return false;
                    nc.MantissaSignFound = true;
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
            // is guaranteed to be a number: . . is stupid, . +/- ok but unlikely, and . EE barely Ok
            // There are several examples in ML-25.t59
            NumberContext nc = new();
            List<byte> opCodes = [];
            List<AstToken> tokens = [];
            bool isOk = false;
            if (program.Statements[i] is AstNumber(List<AstToken> numTokens, List<byte> numOpCodes))
            {
                isOk = AnalyzeNumber(numOpCodes, ref nc);
                nc.isNumber = true;
                Debug.Assert(isOk);
                opCodes = numOpCodes;
                tokens = numTokens;
                isOk = true;
            }
            else if (program.Statements[i] is AstInstructionAtomic(List<AstToken> instTokens, List<byte> instOpCodes, _) && instOpCodes[0] == 93)    // .
            {
                nc.DotFound = true;
                nc.isNumber = false;        // starting instruction is not a number, if next instructions is compatible, we should convert this instruction to a number
                opCodes = instOpCodes;
                tokens = instTokens;
                isOk = true;             // Not ready yet!
            }

            if (!isOk)
                continue;

            AnalyzeNextStatement:
            var nextStat = GetNextCompatibleStatement(program, i);
            if (nextStat == null)
                continue;

            if (nextStat is AstNumber(List<AstToken> nextNumberTokens, List<byte> nextNumberOpCodes))
            {
                // Check that the whole stuff can be added
                isOk = AnalyzeNumber(nextNumberOpCodes, ref nc);
                if (!isOk)
                    continue;

                // Good, if we're there, next number can be merged

                // Should we convert previous statement to a number?
                if (!nc.isNumber)
                {
                    program.Statements.Insert(i, new AstNumber(tokens, opCodes));
                    program.Statements.RemoveAt(i+1);
                }

                // Append tokens and OpCodes
                for (int j = 0; j < nextNumberOpCodes.Count; j++)
                {
                    tokens.Add(nextNumberTokens[j]);
                    opCodes.Add(nextNumberOpCodes[j]);
                    nc.IxExisting++;
                }

                EraseNextAstNumberOrInstruction(program, i, true);

                // Restart in case there's also something mergeable just after
                goto AnalyzeNextStatement;
            }

            if (nextStat is AstInstruction(List<AstToken> nextInstTokens, List<byte> nextInstOpCodes, _))
            {
                var nop = nextInstOpCodes[0];

                Debug.Assert(nextInstTokens.Count == 1);
                Debug.Assert(nextInstOpCodes.Count == 1);

                // Check if the statement can be merged
                isOk = AnalyzeNumber(nextInstOpCodes, ref nc);
                if (!isOk)
                    continue;

                // Ok, we can merge

                // Should we convert previous statement to a number?
                if (!nc.isNumber)
                {
                    program.Statements.Insert(i, new AstNumber(tokens, opCodes));
                    program.Statements.RemoveAt(i + 1);
                }

                // Append tokens and OpCodes
                opCodes.Add(nop);
                tokens.Add(nextInstTokens[0]);

                // Don't need next program Number (and also remove inter-statement white space
                EraseNextAstNumberOrInstruction(program, i, false);

                // Restart in case there's also something mergeable just after
                goto AnalyzeNextStatement;
            }

            // unreachable:
            Debugger.Break();
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
