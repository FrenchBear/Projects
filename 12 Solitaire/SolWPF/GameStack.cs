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
        private List<PlayingCard> PlayingCards;

        public GameStack(Canvas c,Rectangle r)
        {
            PlayingCanvas = c;
            BaseRect = r;
            r.Width = MainWindow.cardWidth;
            r.Height = MainWindow.cardHeight;
            PlayingCards = new List<PlayingCard>();
        }

        public void AddCard(string face)
        {
            var MyCard = new PlayingCard(face);
            MyCard.Width = MainWindow.cardWidth;
            MyCard.Height = MainWindow.cardHeight;
            MyCard.SetValue(Canvas.LeftProperty, (double)BaseRect.GetValue(Canvas.LeftProperty));
            MyCard.SetValue(Canvas.TopProperty, (double)BaseRect.GetValue(Canvas.TopProperty));

            PlayingCanvas.Children.Add(MyCard);
            PlayingCards.Insert(0, MyCard);
        }

        internal void MoveOutCards(List<PlayingCard> cards)
        {
            // For now, only move 1 card
            Debug.Assert(cards.Count == 1);
            Debug.Assert(PlayingCards[0] == cards[0]);
            PlayingCards.RemoveAt(0);
        }

        internal void MoveInCards(List<PlayingCard> cards)
        {
            // For now, only move 1 card
            Debug.Assert(cards.Count == 1);
            PlayingCards.Insert(0, cards[0]);
            cards[0].SetValue(Canvas.LeftProperty, (double)BaseRect.GetValue(Canvas.LeftProperty));
            cards[0].SetValue(Canvas.TopProperty, (double)BaseRect.GetValue(Canvas.TopProperty));
        }


        private bool isStackHit(Point P)
        {
            Point Q = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));
            Debug.WriteLine($"isStactHit mouse=({P.X}; {P.Y})  Q=({Q.X}; {Q.Y})");

            return (P.X >= Q.X && P.X <= Q.X + MainWindow.cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + MainWindow.cardHeight);
            /*
            {
                BaseRect.Stroke = Brushes.Red;
                BaseRect.StrokeThickness = 5.0;
                return true;
            }
            else
            {
                BaseRect.Stroke = Brushes.Black;
                BaseRect.StrokeThickness = 3.0;
                return false;
            }
            */
        }

        public MovingGroup startingHit(Point P)
        {
            if (PlayingCards.Count == 0) return null;
            if (!isStackHit(P)) return null;

            var mg = new MovingGroup(PlayingCards[0]);
            PlayingCanvas.Children.Remove(PlayingCards[0]);
            PlayingCanvas.Children.Add(PlayingCards[0]);

            return mg;
        }

        public virtual bool isTargetHit(Point P)
        {
            //Point Q = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));
            //Debug.WriteLine($"isTargetHit mouse=({P.X}; {P.Y})  Q=({Q.X}; {Q.Y})");

            //if (P.X >= Q.X && P.X <= Q.X + MainWindow.cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + MainWindow.cardHeight)
            if (isStackHit(P))
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

    class TalonStack: GameStack
    {
        public TalonStack(Canvas c, Rectangle r): base(c, r)
        {
        }

        // Talon is never a target
        public override bool isTargetHit(Point P)
        {
            return false;
        }
    }

}
