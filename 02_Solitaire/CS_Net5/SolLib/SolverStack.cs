// Solitaire Solver Library
// class SolverStack
// Implements a stack of cards, used by SolverDeck
// Contrary to WPF equivalent class, it's minimalistic since it doesn't care about visual rendering of stack and movments.
//
// 2019-04-07   PV
// 2021-11-13   PV      Net6 C#10

using System.Collections.Generic;

namespace SolLib
{

    public class SolverStack
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