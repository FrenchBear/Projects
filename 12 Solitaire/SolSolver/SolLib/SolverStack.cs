using System.Collections.Generic;

namespace SolLib
{
    internal class SolverStack
    {
        public string Name;
        public List<SolverCard> PlayingCards;

        public SolverStack(string name)
        {
            Name = name;
            PlayingCards = new List<SolverCard>();
        }

        internal void AddCard(SolverCard c, bool isFaceUp)
        {
            c.IsFaceUp = isFaceUp;
            PlayingCards.Add(c);
        }
    }
}