// Class L3Encoder
// Generate opcodes for L2Statement and L2Number
// This layer doesn't implement new classes, it updates existing L2 objects
//
// 2025-11-24   PV      First version

using Antlr4.Runtime;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace T59v5;

internal sealed class L3Encoder(T59Program Prog)
{
    internal void L3Process()
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
                            l2si.OpCodes.Add(byte.Parse(l1t.Tokens[0].Text));
                        else if (l1t is L1A3)
                        {
                            int addr = int.Parse(l1t.Tokens[0].Text);
                            l2si.OpCodes.Add((byte)(addr / 100));
                            l2si.OpCodes.Add((byte)(addr % 100));
                        }
                        else if (l1t is L1Tag)
                            l2si.OpCodes.Add(100);  // Special, placeholder for now
                        else
                            Debugger.Break();
                    }

                    // Merge instructions at (ix, ix+1) in current l2si, preserving Vocab tokens
                    void MergeInstructions(int ix, int newTIKey)
                    {
                        // Merge Vocab tokens (can't keep L1 tokens since they will be deleted)
                        List<IToken> l0tokens = [];
                        l0tokens.AddRange(l2si.L1Tokens[ix].Tokens);
                        l0tokens.AddRange(l2si.L1Tokens[ix + 1].Tokens);

                        var l1inst = new L1Instruction() { Tokens = l0tokens, Cat = SyntaxCategory.Instruction, Inst = L1Tokenizer.TIKeys[newTIKey] };
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
                        foreach (char c in string.Join("", l1t.Tokens.Select(t => t.Text)))
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

                case L2Eof:
                    Prog.OpCodesCount = pc;
                    break;
            }
        }
    }

}