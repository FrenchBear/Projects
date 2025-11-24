// Class L3Encoder
// Generate opcodes for L2Statement and L2Number
// This layer doesn't implement new classes, it updates existing L2 objects
//
// 2025-11-24   PV      First version

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
