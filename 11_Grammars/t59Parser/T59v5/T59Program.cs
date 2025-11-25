// Representation of a T9 program
//
// 2025-11-25   PV

using System.Collections.Generic;

namespace T59v5;

internal sealed class T59Program
{
    public List<L1Token> L1Tokens = [];
    public List<L2StatementBase> L2Statements = [];
    public List<L2StatementBase> L3Statements = [];
}
