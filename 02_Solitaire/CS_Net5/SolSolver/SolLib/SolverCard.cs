// Solitaire Solver Library
// class SolverCard
// Implements a game card, Face combines Color and Value such as D3 for a Three of Diamonds, and a boolean indicating if face is up (visible)
// 2019-04-07   PV

using System;
using System.Collections.Generic;


namespace SolLib
{
    public class SolverCard
    {
        public const string Colors = "HSDC";
        public const string Values = "A23456789XJQK";

        private const string SignatureColors = "♥♠♦♣";
        private const string SignatureValues = "A23456789XJQK";
        private const string SignatureFaceUp = "˄";
        private const string SignatureFaceDown = "˅";


        public string Face { get; set; }
        public bool IsFaceUp { get; set; }

        public int Color => Colors.IndexOf(Face[0], StringComparison.Ordinal);            // 0..3; %2==0 => Red, %2==1 => Black
        public int Value => Values.IndexOf(Face[1], StringComparison.Ordinal) + 1;        // 1..13

        public SolverCard(int v, int c, bool isFaceUp)
        {
            Face = Colors.Substring(c, 1) + Values.Substring(v - 1, 1);
            IsFaceUp = isFaceUp;
        }

        public override string ToString() => $"PlayingCard {Face}, IsFaceUp={IsFaceUp}";

        internal string Signature(bool overrideFaceUp = false) => SignatureValues.Substring(Value - 1, 1) + SignatureColors.Substring(Color, 1) + ((IsFaceUp || overrideFaceUp) ? SignatureFaceUp : SignatureFaceDown);

        public static List<SolverCard> Set52()
        {
            var Cards = new List<SolverCard>();
            for (int v = 1; v <= 13; v++)
                for (int ci = 0; ci <= 3; ci++)
                    Cards.Add(new SolverCard(v, ci, true));
            return Cards;
        }
    }   // class SolverCard
}