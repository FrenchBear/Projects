// MovingGroup
// A group of cards moving together
// 2019-04-12   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
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


        public MovingGroup(GameStack from, List<PlayingCard> hitList, bool isMovable, bool reverseCardsDuringMove = false)
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

        internal void DoMove(bool withAnimation = false)
        {
            var sb = new StringBuilder($"DoMove r={reverseCardsDuringMove} From=({FromStack}) To=({ToStack}) MovingCards=");
            foreach (var c in MovingCards)
            {
                sb.Append(c.Face);
                sb.Append(c.IsFaceUp ? "^ " : "v ");
            }

            if (reverseCardsDuringMove) MovingCards.Reverse();
            cardMadeVisibleDuringMove = FromStack.MoveOutCards(MovingCards);
            ToStack.MoveInCards(MovingCards, withAnimation);
            ToStack.ClearTargetHighlight();

            Debug.WriteLine($"{sb} Cmv={cardMadeVisibleDuringMove?.Face}");

            // Keep this move for undo
            FromStack.b.PushUndo(this);

            // Finally refresh status bar
            FromStack.b.UpdateGameStatus();
        }

        internal void UndoMove()
        {
            var sb = new StringBuilder($"UndoMove r={reverseCardsDuringMove} From=({FromStack}) To=({ToStack}) MovingCards=");
            foreach (var c in MovingCards)
            {
                sb.Append(c.Face);
                sb.Append(c.IsFaceUp ? "^ " : "v ");
            }
            sb.Append($" Cmv={cardMadeVisibleDuringMove?.Face}");
            Debug.WriteLine(sb.ToString());

            if (reverseCardsDuringMove) MovingCards.Reverse();
            if (cardMadeVisibleDuringMove != null) cardMadeVisibleDuringMove.IsFaceUp = false;
            ToStack.MoveOutCards(MovingCards);
            FromStack.MoveInCards(MovingCards);
        }
    }
}
