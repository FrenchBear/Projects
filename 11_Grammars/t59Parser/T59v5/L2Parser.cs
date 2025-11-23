// Class L2Parser, my own version of T59 language grammar analyzer
// Transforms a stream of L2Token into a stream of L2Statement
//
// 2025-11-21   PV      First version

using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T59v5;

abstract record L2Statement
{
    public List<L1Token> L1Tokens { get; set; }
}


record L2Eof: L2Statement { }
record L2ProgramSeparator: L2Statement { }
record L2LineComment: L2Statement { }
record L2InvalidStatement: L2Statement { }
record L2Instruction: L2Statement { }


internal class L2Parser
{
    private List<L1Token> Context { get; set; } = [];

    public void ClearContext()
    {
        Context.Clear();
    }


}
