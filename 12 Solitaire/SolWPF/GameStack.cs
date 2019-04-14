﻿// GameStack class
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
        protected Rectangle BaseRect;
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
            MyCard.SetValue(Canvas.TopProperty, P.Y);
            PlayingCanvas.Children.Add(MyCard);
            PlayingCards.Insert(0, MyCard);
        }

        internal void MoveOutCards(List<PlayingCard> movedCards)
        {
            Debug.Assert(PlayingCards.Count >= movedCards.Count);
            for (int i = 0; i < movedCards.Count; i++)
            {
                Debug.Assert(PlayingCards[0].IsFaceUp);
                PlayingCards.RemoveAt(0);
            }
            if (PlayingCards.Count > 0 && !PlayingCards[0].IsFaceUp)
                PlayingCards[0].IsFaceUp = true;
        }

        internal void MoveInCards(List<PlayingCard> movedCards)
        {
            for (int i = movedCards.Count - 1; i >= 0; i--)
            {
                Point P = getNewCardPosition();
                movedCards[i].SetValue(Canvas.LeftProperty, P.X);
                movedCards[i].SetValue(Canvas.TopProperty, P.Y);
                PlayingCards.Insert(0, movedCards[i]);
            }
        }


        // Internal hit test
        // Base version should only check rectangle, derived classes are responsible to implement
        // specialized versions with possible offsets
        protected virtual bool isStackHit(Point P, bool onlyTopCard, bool includeEmptyStack, out List<PlayingCard> hitList)
        {
            Point Q;
            hitList = null;
            if (PlayingCards.Count == 0)
            {
                if (!includeEmptyStack)
                    return false;

                Q = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));
                return (P.X >= Q.X && P.X <= Q.X + MainWindow.cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + MainWindow.cardHeight);
            }

            int iMax = onlyTopCard ? 1 : PlayingCards.Count;
            for (int i = 0; i < iMax; i++)
            {
                if (!PlayingCards[i].IsFaceUp)
                    break;

                Q = new Point((double)PlayingCards[i].GetValue(Canvas.LeftProperty), (double)PlayingCards[i].GetValue(Canvas.TopProperty));
                if (P.X >= Q.X && P.X <= Q.X + MainWindow.cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + MainWindow.cardHeight)
                {
                    hitList = new List<PlayingCard>();
                    for (int j = 0; j <= i; j++)
                        hitList.Add(PlayingCards[j]);
                    return true;
                }
            }
            return false;
        }

        protected virtual bool isStackFromHit(Point P, out List<PlayingCard> hitList)
        {
            return isStackHit(P, true, false, out hitList);
        }

        protected virtual bool isStackToHit(Point P)
        {
            return isStackHit(P, true, true, out _);
        }

        public MovingGroup FromHitTest(Point P)
        {
            if (!isStackFromHit(P, out List<PlayingCard> hitList)) return null;
            Debug.Assert(hitList != null && hitList.Count > 0);

            var mg = new MovingGroup(hitList);
            for (int i = hitList.Count - 1; i >= 0; i--)
            {
                PlayingCanvas.Children.Remove(hitList[i]);
                PlayingCanvas.Children.Add(hitList[i]);
            }
            return mg;
        }


        protected virtual bool RulesAllowMoveInCards(MovingGroup mg)
        {
            return true;
        }

        public virtual bool ToHitTest(Point P, MovingGroup mg)
        {
            if (isStackToHit(P) && RulesAllowMoveInCards(mg))
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
        public override bool ToHitTest(Point P, MovingGroup mg)
        {
            return false;
        }

        protected override bool isStackHit(Point P, bool onlyTopCard, bool includeEmptyStack, out List<PlayingCard> hitList)
        {
            hitList = null;

            // For talon, when it's empty, it can't be hit
            if (PlayingCards.Count == 0)
                return false;

            // For now, just check base rect
            // May evolve and be sophisticated if Talon shows last three cards
            Point Q;
            Q = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));
            if (P.X >= Q.X && P.X <= Q.X + MainWindow.cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + MainWindow.cardHeight)
            {
                if (!PlayingCards[0].IsFaceUp)
                {
                    // ToDo: DO this after detecting a click especially if we show multiple cards at once
                    // Moreover, we must do talon rotation which is not done here...
                    PlayingCards[0].IsFaceUp = true;
                    return false;
                }

                hitList = new List<PlayingCard>();
                hitList.Add(PlayingCards[0]);
                return true;
            }
            return false;
        }


    }

    // New cards are shown in a visible stack
    class ColumnStack : GameStack
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

        protected override bool isStackFromHit(Point P, out List<PlayingCard> hitList)
        {
            return isStackHit(P, false, false, out hitList);
        }
    }


    class BaseStack : GameStack
    {
        public BaseStack(Canvas c, Rectangle r) : base(c, r)
        {
        }

        protected override bool RulesAllowMoveInCards(MovingGroup mg)
        {
            // Can only add 1 card to a base
            if (mg.MovingCards.Count != 1)
                return false;

            // Need to access other bases
            // Simplified model for now
            if (PlayingCards.Count == 0)
                return mg.MovingCards[0].Value == 1;

            return PlayingCards[0].Color == mg.MovingCards[0].Color && PlayingCards[0].Value + 1 == mg.MovingCards[0].Value;
        }

    }

}
