// Solitaire Library 
// Quick and dirty simple Solitaire Solver
// 2019-04-07   PV
//
// ToDo: When all cards are visible, game is solved

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Console;


namespace SolLib
{
    public class SolitaireDeck
    {
        readonly GameStack[] Bases;        // 0..3     Index 0 contains top card on base.  Index=color (bases are fixed in this version)
        readonly GameStack[] Columns;      // 0..6
        readonly int[] Visible;             // 0..6
        readonly GameStack Talon;

        internal bool IsSolved()          // IsGameFinished
        {
            foreach (var b in Bases)
                if (b.PlayingCards.Count != 13)
                    return false;
            return true;
        }

        internal bool IsSolvable()      // IsGameSolvable
        {
            foreach (var c in Columns)
                if (c.PlayingCards.Count > 0 && !c.PlayingCards[c.PlayingCards.Count - 1].IsFaceUp)
                    return false;
            return true;
        }



        public SolitaireDeck(int seed)
        {
            Talon = new GameStack();
            Bases = (new GameStack[4]).InitializeArray();
            Columns = (new GameStack[7]).InitializeArray();
            Visible = new int[7];

            foreach (var c in PlayingCard.Set52().Shuffle(seed))
            {
                c.IsFaceUp = false;
                Talon.PlayingCards.Add(c);
            }

            for (int c = 0; c < 7; c++)
            {
                Visible[c] = 1;
                for (int i = 0; i <= c; i++)
                {
                    var card = Talon.PlayingCards[0];
                    Talon.PlayingCards.RemoveAt(0);
                    card.IsFaceUp = i == 0;
                    Columns[c].PlayingCards.Add(card);
                }
            }
        }

        // ToDo: Validate args; do not crash if column is empty
        public PlayingCard ColumnTopCard(int col) => Columns[col].PlayingCards[0];

        public void Print()
        {
            WriteLine("----------------------------------------------------------");
            WriteLine("Deck:");
            for (int b = 0; b < 4; b++)
                PrintCards($"Base {b}  ", Bases[b]);
            for (int c = 0; c < 7; c++)
                PrintCards($"Column {c}", Columns[c], Visible[c]);
            PrintCards("Talon       ", Talon);
        }

        private void PrintCards(string header, GameStack st, int visible = -1)
        {
            Write(header + " ");
            if (visible >= 0) Write($"V={visible} ");
            foreach (PlayingCard c in st.PlayingCards)
                Write(c.Signature() + " ");
            WriteLine();
        }



        // col==7 is Talon, otherwise col in [0..6]
        private void MoveToBase(int c_from)
        {
            Debug.Assert(c_from >= 0 && c_from <= 7);
            if (c_from == 7)
            {
                // Move from talon
                Debug.Assert(Talon.PlayingCards.Count > 0);
                PlayingCard ca = Talon.PlayingCards[0];
                Talon.PlayingCards.RemoveAt(0);
                Debug.Assert(Bases[ca.Color].PlayingCards.Count == 0 && ca.Value == 1 || Bases[ca.Color].PlayingCards.Count > 0 && Bases[ca.Color].PlayingCards.First().Value + 1 == ca.Value);
                Bases[ca.Color].PlayingCards.Insert(0, ca);
            }
            else
            {
                // Move from column col
                Debug.Assert(Columns[c_from].PlayingCards.Count > 0);
                PlayingCard ca = Columns[c_from].PlayingCards[0];
                Columns[c_from].PlayingCards.RemoveAt(0);
                Debug.Assert(Bases[ca.Color].PlayingCards.Count == 0 && ca.Value == 1 || Bases[ca.Color].PlayingCards.Count > 0 && Bases[ca.Color].PlayingCards.First().Value + 1 == ca.Value);
                Bases[ca.Color].PlayingCards.Insert(0, ca);
                Visible[c_from]--;
                if (Visible[c_from] == 0 && Columns[c_from].PlayingCards.Count > 0)
                    Visible[c_from] = 1;
                Debug.Assert((Columns[c_from].PlayingCards.Count == 0 && Visible[c_from] == 0) || (Columns[c_from].PlayingCards.Count > 0 && Visible[c_from] >= 1 && Visible[c_from] <= Columns[c_from].PlayingCards.Count));
            }
        }


        // col==7 is Talon, otherwise col in [0..6]
        // Only 1 can move to an empty base, otherwise card must be base top+1
        private bool CanMoveToBase(int col)
        {
            Debug.Assert(col >= 0 && col <= 7);
            PlayingCard ca;
            if (col == 7)
            {
                // Can move from talon?
                if (Talon.PlayingCards.Count == 0) return false;
                ca = Talon.PlayingCards[0];
            }
            else
            {
                // Can move from column col?
                if (Columns[col].PlayingCards.Count == 0) return false;
                ca = Columns[col].PlayingCards[0];
            }
            return Bases[ca.Color].PlayingCards.Count == 0 && ca.Value == 1 || Bases[ca.Color].PlayingCards.Count > 0 && Bases[ca.Color].PlayingCards.First().Value + 1 == ca.Value;
        }

        private void MoveColumnToColumn(int c_from, int c_to, int n)
        {
            Debug.Assert(c_from >= 0 && c_from <= 7);
            Debug.Assert(c_to >= 0 && c_to < 7);
            Debug.Assert(c_from != c_to);
            Debug.Assert(c_from == 7 || n >= 1);
            Debug.Assert(c_from < 7 || n == 1);
            Debug.Assert(c_from == 7 || Columns[c_from].PlayingCards.Count >= n);
            Debug.Assert(c_from == 7 || Visible[c_from] >= n);

            for (int i = n - 1; i >= 0; i--)
            {
                PlayingCard ca;
                if (c_from == 7)
                    ca = Talon.PlayingCards[0];
                else
                    ca = Columns[c_from].PlayingCards[i];

                Debug.Assert(Columns[c_to].PlayingCards.Count > 0 || ca.Value == 13);  // Can only move a King to an empty column
                Debug.Assert(Columns[c_to].PlayingCards.Count == 0 || ca.Value == Columns[c_to].PlayingCards[0].Value - 1 && ca.Color % 2 != Columns[c_to].PlayingCards[0].Color % 2);
                Columns[c_to].PlayingCards.Insert(0, ca);
            }

            if (c_from == 7)
            {
                Talon.PlayingCards.RemoveAt(0);
                Visible[c_to]++;
            }
            else
            {
                for (int i = 0; i < n; i++)
                    Columns[c_from].PlayingCards.RemoveAt(0);
                Visible[c_from] -= n;
                if (Visible[c_from] == 0 && Columns[c_from].PlayingCards.Count > 0)
                {
                    Visible[c_from] = 1;
                }
                Visible[c_to] += n;
                Debug.Assert((Columns[c_from].PlayingCards.Count == 0 && Visible[c_from] == 0) || (Columns[c_from].PlayingCards.Count > 0 && Visible[c_from] >= 1 && Visible[c_from] <= Columns[c_from].PlayingCards.Count));
            }
        }

        private bool CanMoveColumnToColumn(int c_from, int c_to, int n)
        {
            Debug.Assert(c_from >= 0 && c_from <= 7);
            Debug.Assert(c_to >= 0 && c_to < 7);
            Debug.Assert(c_from != c_to);
            Debug.Assert(c_from == 7 || n >= 1);
            Debug.Assert(c_from < 7 || n == 1);
            Debug.Assert(c_from == 7 || Columns[c_from].PlayingCards.Count >= n);
            Debug.Assert(c_from == 7 || Visible[c_from] >= n);

            PlayingCard ca;
            if (c_from == 7)
                ca = Talon.PlayingCards[0];
            else
                ca = Columns[c_from].PlayingCards[n - 1];

            if (Columns[c_to].PlayingCards.Count == 0 && ca.Value == 13) return true;  // Can move a King to an empty column
            if (Columns[c_to].PlayingCards.Count > 0 && ca.Value == Columns[c_to].PlayingCards[0].Value - 1 && ca.Color % 2 != Columns[c_to].PlayingCards[0].Color % 2) return true; // Can move if value-1 and alternating colors 

            return false;
        }

        public bool OneMovementToBase(bool showTraces = false)
        {
            HashSet<string> Signatures;
            Signatures = new HashSet<string>();

        restart_reset_talon:
            int talonRotateCount = Talon.PlayingCards.Count;

            // Move one card from columns to Base?
            for (int c = 0; c < 7; c++)
            {
                if (CanMoveToBase(c))
                {
                    MoveToBase(c);
                    if (showTraces)
                        WriteLine($"Move from Column {c} to base: " + Signature());
                    return true;
                }
            }

        restart:
            // Move one card from Talon to Base?
            if (CanMoveToBase(7))
            {
                MoveToBase(7);
                if (showTraces)
                    WriteLine("Move from Talon to base: " + Signature());
                return true;
            }

            // Move from Column to Column, max cards possible
            for (int c_from = 0; c_from < 7; c_from++)
                if (Columns[c_from].PlayingCards.Count > 0)
                    for (int c_to = 0; c_to < 7; c_to++)
                        if (c_to != c_from)
                            for (int n = Visible[c_from]; n >= 1; n--)
                                if (CanMoveColumnToColumn(c_from, c_to, n))
                                {
                                    var s = Signature(c_from, c_to, n);
                                    if (!Signatures.Contains(s))
                                    {
                                        MoveColumnToColumn(c_from, c_to, n);
                                        if (showTraces)
                                            WriteLine($"Move {n} card(s) from Column {c_from} to Column {c_to}: " + Signature());
                                        var s2 = Signature();
                                        Debug.Assert(s == s2);
                                        Signatures.Add(s);
                                        goto restart_reset_talon;
                                    }
                                }

            // Move from Talon to Columns
            if (Talon.PlayingCards.Count > 0)
                for (int c_to = 0; c_to < 7; c_to++)
                    if (CanMoveColumnToColumn(7, c_to, 1))
                    {
                        var s = Signature(7, c_to);
                        if (!Signatures.Contains(s))
                        {
                            MoveColumnToColumn(7, c_to, 1);
                            if (showTraces)
                                WriteLine($"Move {1} card from Talon to Column {c_to}: " + Signature());
                            goto restart;
                        }
                    }

            // Rotate talon and restart
            if (Talon.PlayingCards.Count == 0 || talonRotateCount == 0)
                return false;
            talonRotateCount--;
            Talon.PlayingCards.Add(Talon.PlayingCards[0]);
            Talon.PlayingCards.RemoveAt(0);
            goto restart;
        }

        public bool Solve(bool printSteps = false)
        {
            if (printSteps)
                Print();

            while (OneMovementToBase(false))
            {
                if (printSteps)
                    Print();
                if (IsSolvable()) break;
            }

            if (printSteps)
                Print();

            // Solved if all bases contain a King
            return IsSolvable();
        }

        // A unique representation of current game configuration
        private string Signature(int c_from = -1, int c_to = -1, int n = 1)
        {
            var sb = new StringBuilder();

            // Only include top base card in signature
            for (int b = 0; b < 4; b++)
                if (Bases[b].PlayingCards.Count == 0)
                    sb.Append("@@");
                else
                    sb.Append(Bases[b].PlayingCards.First().Signature());

            // Then add columns signature.  Note than all cards of a column are included, not only the visible ones (not sure it's useful)
            for (int c = 0; c < 7; c++)
            {
                sb.Append('|');

                if (c == c_to)
                    if (c_from == 7)
                    {
                        Debug.Assert(n == 1);
                        sb.Append(Talon.PlayingCards[0].Signature());
                    }
                    else
                    {
                        // Add n cards from c_from
                        for (int i = 0; i < n; i++)
                            sb.Append(Columns[c_from].PlayingCards[i].Signature());
                    }

                for (int i = (c == c_from) ? n : 0; i < Columns[c].PlayingCards.Count; i++)
                    sb.Append(Columns[c].PlayingCards[i].Signature());
            }
            return sb.ToString();
        }

    }   // class SolitaireDeck
}
