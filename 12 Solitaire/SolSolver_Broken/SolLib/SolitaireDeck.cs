// Solitaire Library 
// Quick and dirty simple Solitaire Solver
//
// 2019-04-07   PV
// 2019-04-23   PV      Refactoring remplacing Visible[] by PlayingCard.IsFaceUp

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
        readonly GameStack[] Bases;         // 0..3     Index 0 contains top card on base.  Index=color (bases are fixed in this version)
        readonly GameStack[] Columns;       // 0..6
        readonly GameStack TalonFD;         // Talon Face Down
        readonly GameStack TalonFU;         // Talon Face Up

        internal bool IsGameFinished()
        {
            foreach (var b in Bases)
                if (b.PlayingCards.Count != 13)
                    return false;
            return true;
        }

        internal bool IsGameSolvable()
        {
            foreach (var c in Columns)
                if (c.PlayingCards.Count > 0 && !c.PlayingCards[c.PlayingCards.Count - 1].IsFaceUp)
                    return false;
            return true;
        }


        public SolitaireDeck(int seed)
        {
            TalonFD = new GameStack("TalonFD");
            TalonFU = new GameStack("TalonFU");
            Bases = new GameStack[4];
            for (int b = 0; b < 4; b++)
                Bases[b] = new GameStack($"Base[{b}]");
            Columns = new GameStack[7];
            for (int c = 0; c < 7; c++)
                Columns[c] = new GameStack($"Column[{c}]");

            foreach (var c in PlayingCard.Set52().Shuffle(seed))
                TalonFD.AddCard(c, false);

            for (int c = 0; c < 7; c++)
                for (int i = 0; i <= c; i++)
                {
                    var card = TalonFD.PlayingCards[0];
                    TalonFD.PlayingCards.RemoveAt(0);
                    Columns[c].AddCard(card, i == 0);
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
                PrintCards($"Column {c}", Columns[c]);
            PrintCards("Talon FU    ", TalonFU);
            PrintCards("Talon FD    ", TalonFD);
        }

        private void PrintCards(string header, GameStack st)
        {
            Write(header + " ");
            foreach (PlayingCard c in st.PlayingCards)
                Write(c.Signature() + " ");
            WriteLine();

            // As a safety, check that Bases and Columns are valid
            if (header.StartsWith("Base"))
            {
                if (st.PlayingCards.Count > 0)
                {
                    int color = st.PlayingCards[0].Color;
                    for (int i = st.PlayingCards.Count - 1; i >= 0; i--)
                    {
                        Debug.Assert(st.PlayingCards[i].Color == color);
                        Debug.Assert(st.PlayingCards[i].Value == st.PlayingCards.Count - i);
                    }
                }
            }
            else if (header.StartsWith("Column"))
            {
                bool visiblePart = true;
                for (int i = 0; i < st.PlayingCards.Count; i++)
                {
                    if (!st.PlayingCards[i].IsFaceUp)
                        visiblePart = false;
                    if (visiblePart)
                    {
                        if (i > 0)
                        {
                            Debug.Assert(st.PlayingCards[i].Value == st.PlayingCards[i - 1].Value + 1);
                            Debug.Assert(st.PlayingCards[i].Color % 2 != st.PlayingCards[i - 1].Color % 2);
                        }
                    }
                    else
                        Debug.Assert(!st.PlayingCards[i].IsFaceUp);
                }
            }
        }



        // col==7 is TalonFU, otherwise col in [0..6]
        private MovingGroup MoveToBase(int c_from)
        {
            var mg = new MovingGroup();
            Debug.Assert(c_from >= 0 && c_from <= 7);
            if (c_from == 7)
            {
                // Move from talon
                Debug.Assert(TalonFU.PlayingCards.Count > 0);
                PlayingCard ca = TalonFU.PlayingCards[0];
                TalonFU.PlayingCards.RemoveAt(0);
                Debug.Assert(Bases[ca.Color].PlayingCards.Count == 0 && ca.Value == 1 || Bases[ca.Color].PlayingCards.Count > 0 && Bases[ca.Color].PlayingCards.First().Value + 1 == ca.Value);
                Bases[ca.Color].PlayingCards.Insert(0, ca);
                ca.IsFaceUp = true;
                mg.FromStack = TalonFU;
                mg.ToStack = Bases[ca.Color];
                mg.MovingCards = new List<PlayingCard> { ca };
            }
            else
            {
                // Move from column col
                Debug.Assert(Columns[c_from].PlayingCards.Count > 0);
                PlayingCard ca = Columns[c_from].PlayingCards[0];
                Columns[c_from].PlayingCards.RemoveAt(0);
                Debug.Assert(Bases[ca.Color].PlayingCards.Count == 0 && ca.Value == 1 || Bases[ca.Color].PlayingCards.Count > 0 && Bases[ca.Color].PlayingCards.First().Value + 1 == ca.Value);
                Bases[ca.Color].PlayingCards.Insert(0, ca);
                Debug.Assert(ca.IsFaceUp);
                // Turn top of From column face up
                if (Columns[c_from].PlayingCards.Count > 0) Columns[c_from].PlayingCards[0].IsFaceUp = true;
                mg.FromStack = Columns[c_from];
                mg.ToStack = Bases[ca.Color];
                mg.MovingCards = new List<PlayingCard> { ca };
            }
            return mg;
        }


        // col==7 is TalonFU, otherwise col in [0..6]
        // Only A can move to an empty base, otherwise card must be base top+1
        private bool CanMoveToBase(int col)
        {
            Debug.Assert(col >= 0 && col <= 7);
            PlayingCard ca;
            if (col == 7)
            {
                // Can move from talon?
                if (TalonFU.PlayingCards.Count == 0) return false;
                ca = TalonFU.PlayingCards[0];
            }
            else
            {
                // Can move from column col?
                if (Columns[col].PlayingCards.Count == 0) return false;
                ca = Columns[col].PlayingCards[0];
            }
            return Bases[ca.Color].PlayingCards.Count == 0 && ca.Value == 1 || Bases[ca.Color].PlayingCards.Count > 0 && Bases[ca.Color].PlayingCards.First().Value + 1 == ca.Value;
        }

        private MovingGroup MoveColumnToColumn(int c_from, int c_to, int n)
        {
            Debug.Assert(c_from >= 0 && c_from <= 7);
            Debug.Assert(c_to >= 0 && c_to < 7);
            Debug.Assert(c_from != c_to);
            Debug.Assert(c_from == 7 || n >= 1);
            Debug.Assert(c_from < 7 || n == 1);
            Debug.Assert(c_from == 7 || Columns[c_from].PlayingCards.Count >= n);
            Debug.Assert(c_from == 7 || Columns[c_from].PlayingCards[n - 1].IsFaceUp);

            var mg = new MovingGroup();
            mg.FromStack = c_from == 7 ? TalonFU : Columns[c_from];
            mg.ToStack = Columns[c_to];
            mg.MovingCards = new List<PlayingCard>();

            for (int i = n - 1; i >= 0; i--)
            {
                PlayingCard ca;
                if (c_from == 7)
                {
                    ca = TalonFU.PlayingCards[0];
                    ca.IsFaceUp = true;
                }
                else
                {
                    ca = Columns[c_from].PlayingCards[i];
                    Debug.Assert(ca.IsFaceUp);
                }

                Debug.Assert(Columns[c_to].PlayingCards.Count > 0 || ca.Value == 13);  // Can only move a King to an empty column
                Debug.Assert(Columns[c_to].PlayingCards.Count == 0 || ca.Value == Columns[c_to].PlayingCards[0].Value - 1 && ca.Color % 2 != Columns[c_to].PlayingCards[0].Color % 2);
                Columns[c_to].PlayingCards.Insert(0, ca);
                mg.MovingCards.Add(ca);
            }

            if (c_from == 7)
            {
                TalonFU.PlayingCards.RemoveAt(0);
            }
            else
            {
                for (int i = 0; i < n; i++)
                    Columns[c_from].PlayingCards.RemoveAt(0);
                // Turn top of From column face up
                if (Columns[c_from].PlayingCards.Count > 0) Columns[c_from].PlayingCards[0].IsFaceUp = true;
            }

            return mg;
        }

        private bool CanMoveColumnToColumn(int c_from, int c_to, int n)
        {
            Debug.Assert(c_from >= 0 && c_from <= 7);
            Debug.Assert(c_to >= 0 && c_to < 7);
            Debug.Assert(c_from != c_to);
            Debug.Assert(c_from == 7 || n >= 1);
            Debug.Assert(c_from < 7 || n == 1);
            Debug.Assert(c_from == 7 || Columns[c_from].PlayingCards.Count >= n);
            Debug.Assert(c_from == 7 || Columns[c_from].PlayingCards[n - 1].IsFaceUp);

            PlayingCard ca;
            if (c_from == 7)
                ca = TalonFU.PlayingCards[0];
            else
                ca = Columns[c_from].PlayingCards[n - 1];

            if (Columns[c_to].PlayingCards.Count == 0 && ca.Value == 13) return true;  // Can move a King to an empty column
            if (Columns[c_to].PlayingCards.Count > 0 && ca.Value == Columns[c_to].PlayingCards[0].Value - 1 && ca.Color % 2 != Columns[c_to].PlayingCards[0].Color % 2) return true; // Can move if value-1 and alternating colors 

            return false;
        }


        public bool OneMovementToBase(bool showTraces = false)
        {
            var Signatures = new HashSet<string>();
            var Movments = new List<MovingGroup>();

        restart_reset_talon:
            int talonRotateCount = TalonFU.PlayingCards.Count + TalonFD.PlayingCards.Count;
            if (showTraces)
                WriteLine($"Set talonRotateCount = {talonRotateCount}");

            // Move one card from columns to Base?
            for (int c = 0; c < 7; c++)
            {
                if (CanMoveToBase(c))
                {
                    var mg = MoveToBase(c);
                    Movments.Add(mg);
                    if (showTraces)
                        WriteLine(mg.ToString());
                    return true;
                }
            }

        restart:
            // Move one card from Talon to Base?
            if (CanMoveToBase(7))
            {
                var mg = MoveToBase(7);
                Movments.Add(mg);
                if (showTraces)
                    WriteLine(mg.ToString());
                return true;
            }

            // Move from column to column?
            for (int c_from = 0; c_from < 7; c_from++)
                if (Columns[c_from].PlayingCards.Count > 0)
                    for (int c_to = 0; c_to < 7; c_to++)
                        if (c_to != c_from)
                            for (int n = Columns[c_from].PlayingCards.Count - 1; n >= 0; n--)
                                if (Columns[c_from].PlayingCards[n].IsFaceUp)
                                    if (CanMoveColumnToColumn(c_from, c_to, n + 1))
                                    {
                                        var s = Signature(c_from, c_to, n + 1);
                                        if (!Signatures.Contains(s))
                                        {
                                            var mg = MoveColumnToColumn(c_from, c_to, n + 1);
                                            Movments.Add(mg);
                                            if (showTraces)
                                                WriteLine(mg.ToString());
                                            var s2 = Signature();
                                            Debug.Assert(s == s2);
                                            Signatures.Add(s);
                                            goto restart_reset_talon;
                                        }
                                    }


            // Move from TalonFU to Columns
            if (TalonFU.PlayingCards.Count > 0)
                for (int c_to = 0; c_to < 7; c_to++)
                    if (CanMoveColumnToColumn(7, c_to, 1))
                    {
                        var s = Signature(7, c_to);
                        if (!Signatures.Contains(s))
                        {
                            var mg = MoveColumnToColumn(7, c_to, 1);
                            Movments.Add(mg);
                            if (showTraces)
                                WriteLine(mg.ToString());
                            goto restart;
                        }
                    }


            // If we have used all logical rotations, then no need to continue
            if (TalonFU.PlayingCards.Count + TalonFD.PlayingCards.Count == 0 || talonRotateCount == 0)
                return false;

            // Rotate talon and restart
            talonRotateCount--;
            if (showTraces)
                WriteLine($"talonRotateCount = {talonRotateCount}");

            // In case we must rotate 'physically' the Talon
            if (TalonFD.PlayingCards.Count == 0)
            {
                if (showTraces)
                {
                    WriteLine("Before Talon Rotation");
                    PrintCards("TalonFU", TalonFU);
                    PrintCards("TalonFD", TalonFD);
                }
                foreach (var ca in TalonFU.PlayingCards)
                {
                    ca.IsFaceUp = false;
                    TalonFD.PlayingCards.Insert(0, ca);
                }
                TalonFU.PlayingCards.Clear();
                if (showTraces)
                {
                    WriteLine("After Talon Rotation");
                    PrintCards("TalonFU", TalonFU);
                    PrintCards("TalonFD", TalonFD);
                }
            }

            TalonFU.PlayingCards.Add(TalonFD.PlayingCards[0]);
            TalonFD.PlayingCards.RemoveAt(0);
            TalonFU.PlayingCards[0].IsFaceUp = true;            // Solver don't require a click to see card in TalonFU
            if (showTraces)
            {
                WriteLine($"Moving card {TalonFU.PlayingCards[0].Signature()} from TalonFD to TalonFU");
                PrintCards("TalonFU", TalonFU);
                PrintCards("TalonFD", TalonFD);
            }
            goto restart;
        }

        public bool Solve(bool printSteps = false)
        {
            if (printSteps)
                Print();

            while (OneMovementToBase(true))
            {
                if (printSteps)
                    Print();
                if (IsGameSolvable()) break;
            }

            if (printSteps)
                Print();

            // Solved if all bases contain a King
            return IsGameSolvable();
        }

        // A unique representation of current game configuration
        // If parameters are provided, they represent a simulated move, and signature should represent the game after this simulated move
        // Where do we include talon?  Maybe not needed.
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
                        sb.Append(TalonFU.PlayingCards[0].Signature());
                    }
                    else
                    {
                        // Add n cards from c_from
                        for (int i = 0; i < n; i++)
                            sb.Append(Columns[c_from].PlayingCards[i].Signature());
                    }

                bool firstCard = true;
                for (int i = (c == c_from) ? n : 0; i < Columns[c].PlayingCards.Count; i++)
                {
                    sb.Append(Columns[c].PlayingCards[i].Signature(firstCard));
                    firstCard = false;
                }
            }
            return sb.ToString();
        }

    }   // class SolitaireDeck
}
