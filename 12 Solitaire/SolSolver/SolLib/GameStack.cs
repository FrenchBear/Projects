using System.Collections.Generic;

namespace SolLib
{
    public class GameStack
    {
        public string Name;
        public List<PlayingCard> PlayingCards;

        public GameStack(string name)
        {
            Name = name;
            PlayingCards = new List<PlayingCard>();
        }

        internal void AddCard(PlayingCard c, bool isFaceUp)
        {
            c.IsFaceUp = isFaceUp;
            PlayingCards.Add(c);
        }
    }
}