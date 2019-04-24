// Solitaire Library
// Card class
// 2019-04-07   PV

using System;
using System.Collections.Generic;

namespace SolLib
{
    public class SolverCard //: IEquatable<PlayingCard>
    {
        public string Face { get; set; }
        public bool IsFaceUp { get; set; }

        public int Color => "HSDC".IndexOf(Face[0]);                // 0..3; %2==0 => Red, %2==1 => Black
        public int Value => "A23456789XJQK".IndexOf(Face[1]) + 1;   // 1..13

        public SolverCard(int v, int c, bool isFaceUp)
        {
            char vc = "A23456789XJQK"[v - 1];
            char cc = "HSDC"[c];
            Face = $"{cc}{vc}";
            IsFaceUp = isFaceUp;
        }

        public override string ToString() => $"PlayingCard {Face}, IsFaceUp={IsFaceUp}";

        internal string Signature(bool isFaceUpForced = false) => Face + ((IsFaceUp || isFaceUpForced) ? "^" : "v");

        public static bool operator ==(SolverCard a, SolverCard b) => a.Face == b.Face;

        public static bool operator !=(SolverCard a, SolverCard b) => a.Face != b.Face;

        public override bool Equals(object obj)
        {
            return obj is SolverCard card && Equals(card);
        }

        public bool Equals(SolverCard other) => Value == other.Value && Color == other.Color;

        public override int GetHashCode()
        {
            return HashCode.Combine(Face);
        }

        public static List<SolverCard> Set52()
        {
            var Cards = new List<SolverCard>();
            for (int v = 1; v <= 13; v++)
                for (int c = 0; c <= 3; c++)
                    Cards.Add(new SolverCard(v, c, true));
            return Cards;
        }
    }   // Struct Card
}