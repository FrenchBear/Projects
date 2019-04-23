using System;
using System.Collections.Generic;
using System.Text;

namespace SolLib
{
    public class MovingGroup
    {
        public GameStack FromStack;
        public GameStack ToStack;
        public List<PlayingCard> MovingCards;

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
