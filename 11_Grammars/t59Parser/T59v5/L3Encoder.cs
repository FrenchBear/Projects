// Class L3Encoder
// Generate opcodes for L2Statement and L2Number
// This layer doesn't implement new classes, it updates existing L2 objects
//
// 2025-11-24   PV      First version

// ToDo: During encoding pass, replace tag temp addresses 100 100 by actual address

using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace T59v5;

internal sealed class L3Encoder(T59Program Prog)
{
    internal void L3Process()
    {
        MergeInstructionsAndEncode();
        GroupNumbers();
        CheckErrors();
    }

    private void CheckErrors()
    {
        // First pass, build labels, tags and valid addresses tables, report invalid statements
        var Labels = new Dictionary<byte, L2Instruction>();
        var Tags = new Dictionary<string, L2Tag>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var l2s in Prog.L2Statements)
        {
            switch (l2s)
            {
                case L2Instruction { OpCodes: [76, var lab] } l2i:
                    if (Labels.TryGetValue(lab, out L2Instruction? oldi))
                    {
                        l2i.Problem = true;
                        Prog.Errors.Add($"Duplicate label: {oldi.Address}: {oldi.AsFormattedString()} and {l2i.Address}: {l2i.AsFormattedString()}");
                    }
                    else
                    {
                        Labels.Add(lab, l2i);
                        Prog.ValidAddresses.Add(l2i.Address, false);
                    }
                    break;

                case L2Instruction l2i:
                    Prog.ValidAddresses.Add(l2i.Address, false);
                    // Special case, if an instruction (other than standalone INV) starts with INV, then following address is a valid address
                    if (l2i.OpCodes.Count > 1 && l2i.OpCodes[0] == 22)
                        Prog.ValidAddresses.Add(l2i.Address + 1, false);
                    break;

                case L2Tag { Tag: var t } l2t:
                    if (Tags.TryGetValue(t, out L2Tag? oldt))
                        Prog.Errors.Add($"Duplicate tag: {oldt.Address}: {oldt.AsFormattedString()} and {l2t.Address}: {l2t.AsFormattedString()}");
                    else
                        // No need to add tag address to valid instructions, since tag will inherit 
                        Tags.Add(t, l2t);
                    break;

                case L2Number l2n:
                    // Any address inside a L2Number can be considered as valid
                    for (var i = 0; i < l2n.OpCodes.Count; i++)
                        Prog.ValidAddresses.TryAdd(l2n.Address + 1, false);      // ToDo: Investigate how I can get an error with existing key
                    break;

                case L2InvalidStatement l2is:
                    Prog.Errors.Add("Invalid statement: " + l2is.AsFormattedString());
                    break;
            }
        }

        // Second pass, check branch addresses
        foreach (var l2s in Prog.L2Statements)
            if (l2s is L2Instruction l2i)
            {
                foreach (var l1t in l2i.L1Tokens)
                {
                    switch (l1t)
                    {
                        case L1A3 l1a3:
                            var a3 = int.Parse(l1a3.L0Tokens[0].Text);
                            if (!Prog.ValidAddresses.ContainsKey(a3))
                            {
                                Prog.Errors.Add($"Target address invalid: {l2i.Address}: {l2i.AsFormattedString()}");
                                l2i.Problem = true;
                            }
                            else
                                Prog.ValidAddresses[a3] = true;
                            break;

                        case L1A4 l1a4:
                            var a4 = 100 * int.Parse(l1a4.L0Tokens[0].Text) + int.Parse(l1a4.L0Tokens[1].Text);
                            if (!Prog.ValidAddresses.ContainsKey(a4))
                            {
                                Prog.Errors.Add($"Target address invalid: {l2i.Address}: {l2i.AsFormattedString()}");
                                l2i.Problem = true;
                            }
                            else
                                Prog.ValidAddresses[a4] = true;
                            break;

                        case L1Tag l1tag:
                            var tag = l1tag.L0Tokens[0].Text;
                            if (!Tags.ContainsKey(tag))
                            {
                                Prog.Errors.Add($"Target tag invalid: {l2i.Address}: {l2i.AsFormattedString()}");
                                l2i.Problem = true;
                            }
                            else
                                Prog.ValidAddresses[l2i.Address] = true;
                            break;

                        case L1D2 { Cat: SyntaxCategory.Label } l1d2:
                            var label = byte.Parse(l1d2.L0Tokens[0].Text);
                            if (!Labels.TryGetValue(label, out L2Instruction? value))
                            {
                                Prog.Errors.Add($"Target label invalid: {l2i.Address}: {l2i.AsFormattedString()}");
                                l2i.Problem = true;
                            }
                            else
                                Prog.ValidAddresses[value.Address] = true;
                            break;

                        case L1Instruction { Cat: SyntaxCategory.Label } l1i:
                            var ilabel = l1i.Inst.Op[0];
                            if (!Labels.TryGetValue(ilabel, out L2Instruction? ivalue))
                            {
                                Prog.Errors.Add($"Target label invalid: {l2i.Address}: {l2i.AsFormattedString()}");
                                l2i.Problem = true;
                            }
                            else
                                Prog.ValidAddresses[ivalue.Address] = true;
                            break;
                    }
                }
            }
    }

    internal void MergeInstructionsAndEncode()
    {
        int pc = 0;     // Program counter
        foreach (var l2s in Prog.L2Statements)
        {
            switch (l2s)
            {
                case L2Instruction l2si:
                    l2si.Address = pc;
                    foreach (var l1t in l2s.L1Tokens)
                    {
                        if (l1t is L1Instruction l1i)
                            l2si.OpCodes.AddRange(l1i.Inst.Op);
                        else if (l1t is L1D1 or L1D2)
                            l2si.OpCodes.Add(byte.Parse(l1t.L0Tokens[0].Text));
                        else if (l1t is L1A3)
                        {
                            int addr = int.Parse(l1t.L0Tokens[0].Text);
                            l2si.OpCodes.Add((byte)(addr / 100));
                            l2si.OpCodes.Add((byte)(addr % 100));
                        }
                        else if (l1t is L1A4)
                        {
                            l2si.OpCodes.Add(byte.Parse(l1t.L0Tokens[0].Text));
                            l2si.OpCodes.Add(byte.Parse(l1t.L0Tokens[1].Text));
                        }
                        else if (l1t is L1Tag)
                        {
                            l2si.OpCodes.Add(100);  // Special, placeholder for now
                            l2si.OpCodes.Add(100);  // Will be replaced by actual tag address during encoding pass
                        }
                        else
                            Debugger.Break();
                    }

                    // Merge instructions at (ix, ix+1) in current l2si, preserving Vocab tokens
                    void MergeInstructions(int ix, int newTIKey)
                    {
                        // Merge Vocab tokens (can't keep L1 tokens since they will be deleted)
                        List<IToken> l0tokens = [];
                        l0tokens.AddRange(l2si.L1Tokens[ix].L0Tokens);
                        l0tokens.AddRange(l2si.L1Tokens[ix + 1].L0Tokens);

                        var l1inst = new L1Instruction() { L0Tokens = l0tokens, Cat = SyntaxCategory.Instruction, Inst = L1Tokenizer.TIKeys[newTIKey] };
                        l2si.L1Tokens.RemoveAt(ix);
                        l2si.L1Tokens[ix] = l1inst;
                        l2si.OpCodes.RemoveAt(ix);
                        l2si.OpCodes[ix] = l1inst.Inst.Op[0];
                    }

                    // Special case, merge INV SBR -> RTN
                    if (l2si.OpCodes is [22, 71])
                        MergeInstructions(0, Vocab.I92_return);

                    // Check if we have a mergeable instruction, skipping optional initial INV
                    int start = (l2si.L1Tokens[0] is L1Instruction { Inst.Op: [22] }) ? 1 : 0;
                    if (start + 1 < l2si.L1Tokens.Count && l2si.L1Tokens[start] is L1Instruction inst && l2si.L1Tokens[start + 1] is L1Instruction next)
                    {
                        if (inst.Inst.MOp > 0 && next.Inst.Op is [40])      // Meargeable opcode followed by IND?
                            MergeInstructions(start, inst.Inst.MOp);
                    }
                    // Finally update pc
                    pc += l2si.OpCodes.Count;
                    break;

                case L2Number l2n:
                    l2n.Address = pc;
                    foreach (var l1t in l2n.L1Tokens)
                        foreach (char c in string.Join("", l1t.L0Tokens.Select(t => t.Text)))
                        {
                            if (c is >= '0' and <= '9')
                                l2n.OpCodes.Add((byte)(c - '0'));
                            else if (c == '-')
                                l2n.OpCodes.Add(94);
                            else if (c == '.')
                                l2n.OpCodes.Add(93);
                            else if (c is 'e' or 'E')
                                l2n.OpCodes.Add(52);
                            else if (c != '+')
                                Debugger.Break();
                        }
                    pc += l2n.OpCodes.Count;
                    break;

                case L2Tag l2t:
                    l2t.Address = pc;
                    break;

                case L2Eof:
                    Prog.OpCodesCount = pc;
                    break;
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
    private L2StatementBase? GetNextCompatibleStatement(int startIndex)
    {
        for (int j = startIndex + 1; j < Prog.L2Statements.Count; j++)
        {
            if (Prog.L2Statements[j] is L2LineComment)
                continue;
            if (Prog.L2Statements[j] is L2Number)
                return Prog.L2Statements[j];
            if (Prog.L2Statements[j] is L2Instruction nextInst && nextInst.OpCodes.Count == 1)
            {
                var nop = nextInst.OpCodes[0];
                if (nop is 93 or 94 or 52 or < 10)      // . +/- EE or single digit
                    return nextInst;
            }
            return null;
        }
        return null;
    }

    // GroupNumbers helper, analyzes the number represented by the list of opCodes, updating nc
    // Return true if opCodes are compatible with provided NumberContext, false otherwise
    static bool AnalyzeNumber(List<byte> opCodes, ref NumberContext nc)
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

    void EraseNextAstNumberOrInstruction(int startIndex, bool checkForNumber)
    {
        //bool preserveISWS = false;
        int j = startIndex + 1;
        for (; ; )
        {
            if (Prog.L2Statements[j] is L2LineComment)
            {
                j++;
                continue;
            }

            if (checkForNumber)
            {
                if (Prog.L2Statements[j] is L2Number)
                {
                    Prog.L2Statements.RemoveAt(j);
                    return;
                }
            }
            else
            {
                if (Prog.L2Statements[j] is L2Instruction)
                {
                    Prog.L2Statements.RemoveAt(j);
                    return;
                }
            }
        }
    }

    // Group a well-formed sequence of numbers  and instructions into a single number, preserving
    // original tokens order
    // for instance converts sequence "1 . 6 +/- EE +/- 1 9" into single number "-1.6e-19"
    private void GroupNumbers()
    {
        // Group top level numbers
        for (int i = 0; i < Prog.L2Statements.Count; i++)
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
            List<L1Token> tokens = [];
            int address = 0;
            bool isOk = false;
            if (Prog.L2Statements[i] is L2Number l2n)
            {
                isOk = AnalyzeNumber(l2n.OpCodes, ref nc);
                nc.isNumber = true;
                Debug.Assert(isOk);
                opCodes = l2n.OpCodes;
                tokens = l2n.L1Tokens;
                isOk = true;
            }
            else if (Prog.L2Statements[i] is L2Instruction { OpCodes: [var op] } l2d && op < 10)    // single digit
            {
                nc.isNumber = false;        // starting instruction is not a number, if next instructions is compatible, we should convert this instruction to a number
                opCodes = l2d.OpCodes;
                tokens = l2d.L1Tokens;
                address = l2d.Address;
                isOk = true;
            }
            else if (Prog.L2Statements[i] is L2Instruction { OpCodes: [93] } l2i)    // .
            {
                nc.DotFound = true;
                nc.isNumber = false;        // starting instruction is not a number, if next instructions is compatible, we should convert this instruction to a number
                opCodes = l2i.OpCodes;
                tokens = l2i.L1Tokens;
                address = l2i.Address;
                isOk = true;
            }

            if (!isOk)
                continue;

            AnalyzeNextStatement:
            var nextStat = GetNextCompatibleStatement(i);
            if (nextStat == null)
                continue;

            if (nextStat is L2Number nextl2n)
            {
                // Check that the whole stuff can be added
                isOk = AnalyzeNumber(nextl2n.OpCodes, ref nc);
                if (!isOk)
                    continue;

                // Good, if we're there, next number can be merged

                // Should we convert previous statement to a number?
                if (!nc.isNumber)
                {
                    Prog.L2Statements.Insert(i, new L2Number { OpCodes = opCodes, L1Tokens = tokens, Address = address });
                    Prog.L2Statements.RemoveAt(i + 1);
                }

                // Append tokens and OpCodes
                // ToDo: can I use AddRange?
                for (int j = 0; j < nextl2n.OpCodes.Count; j++)
                {
                    tokens.Add(nextl2n.L1Tokens[j]);
                    opCodes.Add(nextl2n.OpCodes[j]);
                    nc.IxExisting++;
                }

                EraseNextAstNumberOrInstruction(i, true);

                // Restart in case there's also something mergeable just after
                goto AnalyzeNextStatement;
            }

            if (nextStat is L2Instruction nl2i)
            {
                var nop = nl2i.OpCodes[0];

                Debug.Assert(nl2i.L1Tokens.Count == 1);
                Debug.Assert(nl2i.OpCodes.Count == 1);

                // Check if the statement can be merged
                isOk = AnalyzeNumber(nl2i.OpCodes, ref nc);
                if (!isOk)
                    continue;

                // Ok, we can merge

                // Should we convert previous statement to a number?
                if (!nc.isNumber)
                {
                    Prog.L2Statements.Insert(i, new L2Number { OpCodes = opCodes, L1Tokens = tokens, Address = address });
                    Prog.L2Statements.RemoveAt(i + 1);
                }

                // Append tokens and OpCodes
                opCodes.Add(nop);
                tokens.Add(nl2i.L1Tokens[0]);

                // Don't need next program Number (and also remove inter-statement white space
                EraseNextAstNumberOrInstruction(i, false);

                // Restart in case there's also something mergeable just after
                goto AnalyzeNextStatement;
            }

            // unreachable:
            Debugger.Break();
        }
    }
}