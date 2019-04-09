﻿// Solitaire Library 
// Card class
// 2019-04-07   PV

using System;
using System.Collections.Generic;

namespace SolLib
{
    public struct Card : IEquatable<Card>
    {
        public int value;  // 1..13 (As..King)
        public int couleur; // 0..3

        public Card(int v, int c) : this()
        {
            value = v;
            couleur = c;
        }

        public override string ToString()
        {
            // "♠♣♥♦".Substring(couleur, 1)
            return "HDSC".Substring(couleur, 1) + ("A23456789XJQK".Substring(value - 1, 1));
        }

        public static bool operator ==(Card a, Card b) => a.value == b.value && a.couleur == b.couleur;
        public static bool operator !=(Card a, Card b) => a.value != b.value || a.couleur != b.couleur;

        internal string Signature() //=> (char)(32 + 16 * couleur + valeur);
        {
            return ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Card card && Equals(card);
        }

        public bool Equals(Card other) => value == other.value && couleur == other.couleur;


        public override int GetHashCode()
        {
            return HashCode.Combine(value, couleur);
        }



        public static List<Card> Set52()
        {
            var Cards = new List<Card>();
            for (int v = 1; v <= 13; v++)
                for (int c = 0; c <= 3; c++)
                    Cards.Add(new Card(v, c));
            return Cards;
        }

    }   // Struct Card
}
