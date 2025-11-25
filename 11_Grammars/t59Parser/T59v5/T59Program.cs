// Representation of a T9 program
//
// 2025-11-25   PV

using System;
using System.Collections.Generic;

namespace T59v5;

internal sealed class T59Program
{
    public List<L1Token> L1Tokens = [];
    public List<L2StatementBase> L2Statements = [];
    public List<L2StatementBase> L3Statements = [];
    public int OpCodesCount;

    public void PrintL1()
    {
        foreach (L1Token t1 in L1Tokens)
            Console.WriteLine(t1.AsString());
    }

    public void PrintL2()
    {
        foreach (L2StatementBase l2s in L2Statements)
            Console.WriteLine(l2s.AsString());
    }

    public void PrintL3Debug()
    {
        foreach (L2StatementBase l3s in L3Statements)
            Console.WriteLine(l3s.AsStringWithOpcodes());
    }

    public void PrintL3Reformated()
    {
        // ToDo
    }
}
