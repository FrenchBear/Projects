using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SolWPF
{
    class MovingGroup
    {
        public GameStack FromStack;
        public List<PlayingCard> Cards;

        public MovingGroup(PlayingCard pc)
        {
            Cards = new List<PlayingCard>();
            Cards.Add(pc);
        }

        internal Point GetTopLeft()
        {
            Point P = new Point((double)Cards[0].GetValue(Canvas.LeftProperty), (double)Cards[0].GetValue(Canvas.TopProperty));
            return P;
        }

        internal void SetTopLeft(Point P)
        {
            Cards[0].SetValue(Canvas.LeftProperty, P.X);
            Cards[0].SetValue(Canvas.TopProperty, P.Y);
        }
    }
}
