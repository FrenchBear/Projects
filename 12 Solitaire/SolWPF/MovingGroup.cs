// MovingGroup
// A group of cards moving together
// 2019-04-12   PV

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
        public GameStack ToStack;
        public List<PlayingCard> MovingCards;
        private List<Vector> offVec;

        public MovingGroup(List<PlayingCard> hitList)
        {
            MovingCards = new List<PlayingCard>();
            offVec = new List<Vector>();
            MovingCards.AddRange(hitList);
            Point P0 = new Point((double)MovingCards[0].GetValue(Canvas.LeftProperty), (double)MovingCards[0].GetValue(Canvas.TopProperty));
            foreach (PlayingCard pc in hitList)
            {
                Point P = new Point((double)pc.GetValue(Canvas.LeftProperty), (double)pc.GetValue(Canvas.TopProperty));
                offVec.Add(P - P0);
            }

        }

        internal Point GetTopLeft()
        {
            Point P = new Point((double)MovingCards[0].GetValue(Canvas.LeftProperty), (double)MovingCards[0].GetValue(Canvas.TopProperty));
            return P;
        }

        internal void SetTopLeft(Point P)
        {
            for (int i = 0; i < MovingCards.Count; i++)
            {
                MovingCards[i].SetValue(Canvas.LeftProperty, P.X+offVec[i].X);
                MovingCards[i].SetValue(Canvas.TopProperty, P.Y + offVec[i].Y);
            }
        }

        internal void DoMove()
        {
            FromStack.MoveOutCards(MovingCards);
            ToStack.MoveInCards(MovingCards);
        }
    }
}
