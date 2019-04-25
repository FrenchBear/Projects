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
            sb.Append($"{FromStack.Name,-9} => {ToStack.Name,-9} ");
            foreach (var ca in MovingCards)
                sb.Append(ca.Signature() + " ");
            return sb.ToString();
        }
    }
}