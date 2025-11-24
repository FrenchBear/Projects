// Class L3Encoder
// Generate opcodes for L2Statement and L2Number
// This layer doesn't implement new classes, it updates existing L2 objects
//
// 2025-11-24   PV      First version

using Antlr4.Runtime;
using System.Collections.Generic;
using System.Diagnostics;

namespace T59v5;

internal sealed class L3Encoder(L2Parser L2P)
{

    internal IEnumerable<L2StatementBase> EnumerateStatements()
    {
        var e = L2P.EnumerateL2Statements().GetEnumerator();

        for (; ; )
        {
            // Don't need peeking support
            //L1Token token;
            //if (nextToken != null)
            //{
            //    token = nextToken;
            //    nextToken = null;
            //}
            //else
            //{
            //    if (!e.MoveNext())
            //        break;
            //    token = e.Current;
            //}

            if (!e.MoveNext())
                break;
            var l2s = e.Current;

            switch (l2s)
            {
                case L2Instruction l2si:
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
                        l0tokens.AddRange(l2si.L1Tokens[0].Tokens);
                        l0tokens.AddRange(l2si.L1Tokens[1].Tokens);

                        var l1inst = new L1Instruction() { Tokens = l0tokens, Cat = SyntaxCategory.Instruction, Inst = L1Tokenizer.TIKeys[newTIKey] };
                        l2si.L1Tokens.Clear();
                        l2si.L1Tokens.Add(l1inst);
                        l2si.OpCodes.RemoveAt(0);
                        l2si.OpCodes[0] = 92;
                    }

                    // Special case, merge INV SBR -> RTN
                    if (l2si.OpCodes is [22, 71])
                        MergeInstructions(0, Vocab.I92_return);
                    //{
                    //    // Merge Vocab tokens (can't keep L1 tokens since they will be deleted)
                    //    List<IToken> l0tokens = [];
                    //    l0tokens.AddRange(l2si.L1Tokens[0].Tokens);
                    //    l0tokens.AddRange(l2si.L1Tokens[1].Tokens);

                    //    var l1inst = new L1Instruction() { Tokens = l0tokens, Cat = SyntaxCategory.Instruction, Inst = L1Tokenizer.TIKeys[Vocab.I92_return] };
                    //    l2si.L1Tokens.Clear();
                    //    l2si.L1Tokens.Add(l1inst);
                    //    l2si.OpCodes.RemoveAt(0);
                    //    l2si.OpCodes[0] = 92;
                    //}
                    /*
                    // Check if we have a mergeable instruction, skipping initial INV
                    int start = (l2si.L1Tokens[0] is L1Instruction { Inst.Op: [22] }) ? 1 : 0;
                    if (start + 1 < l2si.L1Tokens.Count && l2si.L1Tokens[start] is L1Instruction inst && l2si.L1Tokens[start + 1] is L1Instruction next)
                    {
                        if (inst.Inst.MOp > 0 && next.Inst.Op is [40])      // Meargeable opcode followed by IND?
                        {
                            var merged = new L1Instruction();
                        }
                    }
                    */

                    yield return l2si;
                    break;

                case L2Number l2n:
                    // ToDo
                    yield return l2n;
                    break;

                default:
                    yield return l2s;
                    break;
            }
        }

        yield break;
    }
}
