// GameStack class
// A base class to represent a location and a collection of PlayingCards
// 2019-04-11   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SolWPF
{
    class GameStack
    {
        private Canvas PlayingCanvas;
        private Rectangle BaseRect;
        protected List<PlayingCard> PlayingCards;

        public GameStack(Canvas c, Rectangle r)
        {
            PlayingCanvas = c;
            BaseRect = r;
            r.Width = MainWindow.cardWidth;
            r.Height = MainWindow.cardHeight;
            PlayingCards = new List<PlayingCard>();
        }


        // Must be called BEFORE adding the card to PlayingCards list
        protected virtual Point getNewCardPosition()
        {
            var P = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));
            return P;
        }

        public void AddCard(string face, bool isFaceUp)
        {
            var MyCard = new PlayingCard(face, isFaceUp);
            MyCard.Width = MainWindow.cardWidth;
            MyCard.Height = MainWindow.cardHeight;
            Point P = getNewCardPosition();
            MyCard.SetValue(Canvas.LeftProperty, P.X);
            MyCard.SetValue(Canvas.TopProperty, P.Y );
            PlayingCanvas.Children.Add(MyCard);
            PlayingCards.Insert(0, MyCard);
        }

        internal void MoveOutCards(List<PlayingCard> cards)
        {
            // For now, only move 1 card
            Debug.Assert(cards.Count == 1);
            Debug.Assert(PlayingCards[0] == cards[0]);
            PlayingCards.RemoveAt(0);
            if (PlayingCards.Count > 0 && !PlayingCards[0].IsFaceUp)
                PlayingCards[0].IsFaceUp = true;
        }

        internal void MoveInCards(List<PlayingCard> cards)
        {
            // For now, only move 1 card
            Debug.Assert(cards.Count == 1);
            Point P = getNewCardPosition();
            cards[0].SetValue(Canvas.LeftProperty, P.X);
            cards[0].SetValue(Canvas.TopProperty, P.Y);
            PlayingCards.Insert(0, cards[0]);
        }


        // Internal hit test
        // Base version should only check rectangle, derived classes are responsible to implement
        // specialized versions with possible offsets
        private bool isStackTopHit(Point P, out List<PlayingCard> hitList)
        {
            Point Q;
            hitList = null;
            if (PlayingCards.Count == 0)
            {
                Q = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));
                return (P.X >= Q.X && P.X <= Q.X + MainWindow.cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + MainWindow.cardHeight);
            }


            Q = new Point((double)PlayingCards[0].GetValue(Canvas.LeftProperty), (double)PlayingCards[0].GetValue(Canvas.TopProperty));
            if (P.X >= Q.X && P.X <= Q.X + MainWindow.cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + MainWindow.cardHeight)
            {
                hitList = new List<PlayingCard>();
                hitList.Add(PlayingCards[0]);
                return true;
            }
            return false;
        }

        public MovingGroup FromHitTest(Point P)
        {
            if (PlayingCards.Count == 0) return null;
            if (!isStackTopHit(P, out List<PlayingCard> hitList)) return null;
            Debug.Assert(hitList != null && hitList.Count > 0);

            var mg = new MovingGroup(hitList);
            foreach (PlayingCard pc in mg.Cards)
                PlayingCanvas.Children.Remove(pc);
            foreach (PlayingCard pc in mg.Cards)
                PlayingCanvas.Children.Add(pc);
            return mg;
        }

        public virtual bool ToHitTest(Point P)
        {
            if (isStackTopHit(P, out _))
            {
                BaseRect.Stroke = Brushes.Red;
                BaseRect.StrokeThickness = 5.0;
                return true;
            }
            else
            {
                ClearTargetHighlight();
                return false;
            }
        }

        internal void ClearTargetHighlight()
        {
            BaseRect.Stroke = Brushes.Black;
            BaseRect.StrokeThickness = 3.0;
        }
    }

    class TalonStack : GameStack
    {
        public TalonStack(Canvas c, Rectangle r) : base(c, r)
        {
        }

        // Talon is never a target
        public override bool ToHitTest(Point P)
        {
            return false;
        }
    }

    // New cards are shown in a visible stack
    class ColumnStack: GameStack
    {
        const double visibleYOffset = 45.0;
        const double notVvisibleYOffset = 10.0;

        public ColumnStack(Canvas c, Rectangle r) : base(c, r)
        {
        }

        protected override Point getNewCardPosition()
        {
            Point P = base.getNewCardPosition();
            if (PlayingCards.Count == 0) return P;
            double off = 0;
            for (int i = PlayingCards.Count - 1; i >= 0; i--)
                off += PlayingCards[i].IsFaceUp ? visibleYOffset : notVvisibleYOffset;

            return new Point(P.X, P.Y + off);
        }

    }

}
