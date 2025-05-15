// Solitaire Solver Library
// class SolverGroup
// Implements a group of cards to be moved from FromStack to ToStack
//
// 2019-04-07   PV
// 2021-11-13   PV      Net6 C#10

using System.Collections.Generic;
using System.Text;

namespace SolLib
{

    public class SolverGroup
    {
        public SolverStack FromStack;
        public SolverStack ToStack;
        public List<SolverCard> MovingCards;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"SolverGroup: {FromStack.Name,-9} => {ToStack.Name,-9} ");
            foreach (var ca in MovingCards)
                sb.Append(ca.Signature() + " ");
            return sb.ToString();
        }
    }
}