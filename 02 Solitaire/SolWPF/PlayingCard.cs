// Solitaire WPF
// class PlayingCard
// A visual class (inherits from ButtonBase) to represent a Solitaire card and its representation.
// Use Face dependency property to select correct representation in PlayingCards.xaml when visually rendered.
// 2019-04-12   PV

using System;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Diagnostics;

namespace SolWPF
{

    [DebuggerDisplay("PlayingCard {Signature()}")]
    public class PlayingCard : ButtonBase
    {
        // Declare and register dependency properties
        public static DependencyProperty FaceProperty = DependencyProperty.Register("Face", typeof(string), typeof(PlayingCard));
        public static DependencyProperty IsFaceUpProperty = DependencyProperty.Register("IsFaceUp", typeof(bool), typeof(PlayingCard));

        public const string Colors = "HSDC";
        public const string Values = "A23456789XJQK";

        private const string SignatureColors = "♥♠♦♣";
        private const string SignatureValues = "A23456789XJQK";
        private const string SignatureFaceUp = "˄";
        private const string SignatureFaceDown = "˅";


        static PlayingCard()
        {
            // Override style
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayingCard), new FrameworkPropertyMetadata(typeof(PlayingCard)));
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

        // More user-friendly representation than ToString
        internal string Signature() => SignatureValues.Substring(Value - 1, 1) + SignatureColors.Substring(Color, 1) + (IsFaceUp ? SignatureFaceUp : SignatureFaceDown);

        public override string ToString()
        {
            return $"PlayingCard {Face}, IsFaceUp={IsFaceUp}";
        }

        // 
        public int Color => Colors.IndexOf(Face[0], StringComparison.Ordinal);        // 0..3; %2==0 => Red, %2==1 => Black
        public int Value => Values.IndexOf(Face[1], StringComparison.Ordinal) + 1;    // 1..13
    }

}
