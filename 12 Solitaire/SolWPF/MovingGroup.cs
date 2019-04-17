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
        public readonly GameStack FromStack;
        public GameStack ToStack;
        public readonly List<PlayingCard> MovingCards;
        public readonly bool IsMovable;

        private readonly List<Vector> offVec;
        private readonly bool reverseCardsDuringMove;
        private PlayingCard cardMadeVisibleDuringMove;


        public MovingGroup(GameStack from, List<PlayingCard> hitList, bool isMovable, bool reverseCardsDuringMove=false)
        {
            FromStack = from;
            this.reverseCardsDuringMove = reverseCardsDuringMove;
            IsMovable = isMovable;

            if (hitList != null)
            {
                MovingCards = new List<PlayingCard>();
                MovingCards.AddRange(hitList);
                offVec = new List<Vector>();
                Point P0 = new Point((double)MovingCards[0].GetValue(Canvas.LeftProperty), (double)MovingCards[0].GetValue(Canvas.TopProperty));
                foreach (PlayingCard pc in hitList)
                {
                    Point P = new Point((double)pc.GetValue(Canvas.LeftProperty), (double)pc.GetValue(Canvas.TopProperty));
                    offVec.Add(P - P0);
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"MovingGroup FromStack={FromStack.Name}, ToStack={ToStack?.Name}, IsMovable={IsMovable}, MovingCards=");
            if (MovingCards == null)
                sb.Append("∅");
            else
                foreach (var c in MovingCards)
                {
                    sb.Append(c.Face);
                    sb.Append(' ');
                }
            return sb.ToString();
        }

        internal Point GetTopLeft()
        {
            Point P;
            if (MovingCards != null)
                P = new Point((double)MovingCards[0].GetValue(Canvas.LeftProperty), (double)MovingCards[0].GetValue(Canvas.TopProperty));
            else
                P = new Point(0, 0);
            return P;
        }

        internal void SetTopLeft(Point P)
        {
            for (int i = 0; i < MovingCards.Count; i++)
            {
                MovingCards[i].SetValue(Canvas.LeftProperty, P.X + offVec[i].X);
                MovingCards[i].SetValue(Canvas.TopProperty, P.Y + offVec[i].Y);
            }
        }

        internal void DoMove()
        {
            if (reverseCardsDuringMove) MovingCards.Reverse();
            cardMadeVisibleDuringMove= FromStack.MoveOutCards(MovingCards);
            ToStack.MoveInCards(MovingCards);
            ToStack.ClearTargetHighlight();
        }
    }
}
