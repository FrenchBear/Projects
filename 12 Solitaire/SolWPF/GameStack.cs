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

        private bool isStackHit(Point P)
        {
            Point Q = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));

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
    }
}
