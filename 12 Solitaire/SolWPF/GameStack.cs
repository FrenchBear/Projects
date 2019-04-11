using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        public bool isMouseHit(double x, double y)
        {
            return false;
        }
    }
}
