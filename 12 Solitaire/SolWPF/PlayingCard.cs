// Playing card
// A simple class to visually represent a card
// 2019-04-12   PV

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows;


namespace SolWPF
{
    public class PlayingCard : ButtonBase
    {
        public static DependencyProperty FaceProperty;
        public static DependencyProperty IsFaceUpProperty;

        static PlayingCard()
        {
            // Override style
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayingCard), new FrameworkPropertyMetadata(typeof(PlayingCard)));

            // Register dependency properties
            FaceProperty = DependencyProperty.Register("Face", typeof(string), typeof(PlayingCard));
            IsFaceUpProperty = DependencyProperty.Register("IsFaceUp", typeof(bool), typeof(PlayingCard));
        }

        public PlayingCard(string face, bool isFaceUp)
        {
            Face = face;
            IsFaceUp = isFaceUp;
        }

        public string Face
        {
            get { return (string)GetValue(FaceProperty); }
            set { SetValue(FaceProperty, value); }
        }

        public bool IsFaceUp
        {
            get { return (bool)GetValue(IsFaceUpProperty); }
            set { SetValue(IsFaceUpProperty, value); }
        }

        public override string ToString()
        {
            return $"PlayingCard {Face}, IsFaceUp={IsFaceUp}";
        }

        public int Color => "HDSC".IndexOf(Face[0]);                // 0..3
        public int Value => "A23456789XJQK".IndexOf(Face[1]) + 1;   // 1..13
    }

}
