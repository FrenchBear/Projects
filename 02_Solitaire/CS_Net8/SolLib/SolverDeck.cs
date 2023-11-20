// Solitaire Solver Library 
// class SolverDeck
// Implements a solitaire game model with all the stacks
//
// 2019-04-07   PV
// 2019-04-23   PV      Refactoring replacing Visible[] by PlayingCard.IsFaceUp
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using static System.Console;

namespace SolLib;

public class SolverDeck
{
    readonly SolverStack[] Bases;         // 0..3     Index 0 contains top card on base.  Index=color (bases are fixed in this version)
    readonly SolverStack[] Columns;       // 0..6
    readonly SolverStack TalonFD;         // Talon Face Down
    readonly SolverStack TalonFU;         // Talon Face Up

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
            if (c.PlayingCards.Count > 0 && !c.PlayingCards[^1].IsFaceUp)
                return false;
        return true;
    }

    private SolverDeck()
    {
        TalonFD = new SolverStack("TalonFD");
        TalonFU = new SolverStack("TalonFU");
        Bases = new SolverStack[4];
        for (int bi = 0; bi < 4; bi++)
            Bases[bi] = new SolverStack($"Base{bi}");
        Columns = new SolverStack[7];
        for (int ci = 0; ci < 7; ci++)
            Columns[ci] = new SolverStack($"Column{ci}");
    }

    public SolverDeck(List<(SolverCard, bool)>[] bases, List<(SolverCard, bool)>[] columns, List<(SolverCard, bool)> talonFU, List<(SolverCard, bool)> talonFD) : this()
    {
        for (int bi = 0; bi < 4; bi++)
            foreach ((SolverCard card, bool isFaceUp) in bases[bi])
                Bases[bi].AddCard(card, isFaceUp);
        for (int ci = 0; ci < 7; ci++)
            foreach ((SolverCard card, bool isFaceUp) in columns[ci])
                Columns[ci].AddCard(card, isFaceUp);
        foreach ((SolverCard card, bool isFaceUp) in talonFU)
            TalonFU.AddCard(card, isFaceUp);
        foreach ((SolverCard card, bool isFaceUp) in talonFD)
            TalonFD.AddCard(card, isFaceUp);
    }

    //public PlayingCard ColumnTopCard(int col) => Columns[col].PlayingCards[0];

    public void PrintSolverDeck()
    {
        Debug.WriteLine("----------------------------------------------------------");
        Debug.WriteLine("Deck:");
        for (int bi = 0; bi < 4; bi++)
            PrintSolverStack($"Base {bi}  ", Bases[bi]);
        for (int ci = 0; ci < 7; ci++)
            PrintSolverStack($"Column {ci}", Columns[ci]);
        PrintSolverStack("Talon FU    ", TalonFU);
        PrintSolverStack("Talon FD    ", TalonFD);
    }

    private static void PrintSolverStack(string header, SolverStack st)
    {
        Debug.Write(header + " ");
        foreach (SolverCard c in st.PlayingCards)
            Debug.Write(c.Signature() + " ");
        Debug.WriteLine("");
    }

#if DEBUG
    public void CheckSolverDeck()
    {
        int nc = 0;
        for (int i = 0; i < 4; i++)
        {
            var b = Bases[i];
            CheckSolverStack(b);
            nc += b.PlayingCards.Count;
            if (b.PlayingCards.Count > 0)
            {
                for (int j = 0; j < 4; j++)
                    if (j != i && Bases[j].PlayingCards.Count > 0)
                        Debug.Assert(b.PlayingCards[0].Color != Bases[j].PlayingCards[0].Color);
            }
        }
        foreach (var c in Columns)
        {
            CheckSolverStack(c);
            nc += c.PlayingCards.Count;
        }
        nc += TalonFU.PlayingCards.Count + TalonFD.PlayingCards.Count;
        Debug.Assert(nc == 52);
        Debug.Assert(TalonFU.PlayingCards.All(c => c.IsFaceUp));
        Debug.Assert(TalonFD.PlayingCards.All(c => !c.IsFaceUp));
    }

    private static void CheckSolverStack(SolverStack st)
    {
        // As a safety, check that Bases and Columns are valid
        if (st.Name.StartsWith("Base", StringComparison.Ordinal))
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
        else if (st.Name.StartsWith("Column", StringComparison.Ordinal))
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
#endif

    // col==7 is TalonFU, otherwise col in [0..6]
    private SolverGroup MoveToBase(int c_from)
    {
        var mg = new SolverGroup();
        Debug.Assert(c_from >= 0 && c_from <= 7);
        if (c_from == 7)
        {
            // Move from talon
            Debug.Assert(TalonFU.PlayingCards.Count > 0);
            SolverCard ca = TalonFU.PlayingCards[0];
            TalonFU.PlayingCards.RemoveAt(0);
            int targetBase = GetMatchingBase(ca);
            Debug.Assert((Bases[targetBase].PlayingCards.Count == 0 && ca.Value == 1) || (Bases[targetBase].PlayingCards.Count > 0 && Bases[targetBase].PlayingCards.First().Value + 1 == ca.Value));
            Bases[targetBase].PlayingCards.Insert(0, ca);
            ca.IsFaceUp = true;
            mg.FromStack = TalonFU;
            mg.ToStack = Bases[targetBase];
            mg.MovingCards = [ca];
        }
        else
        {
            // Move from column col
            Debug.Assert(Columns[c_from].PlayingCards.Count > 0);
            SolverCard ca = Columns[c_from].PlayingCards[0];
            Columns[c_from].PlayingCards.RemoveAt(0);
            int targetBase = GetMatchingBase(ca);
            Debug.Assert((Bases[targetBase].PlayingCards.Count == 0 && ca.Value == 1) || (Bases[targetBase].PlayingCards.Count > 0 && Bases[targetBase].PlayingCards.First().Value + 1 == ca.Value));
            Debug.Assert(ca.IsFaceUp);
            Bases[targetBase].PlayingCards.Insert(0, ca);
            // Turn top of From column face up
            if (Columns[c_from].PlayingCards.Count > 0)
                Columns[c_from].PlayingCards[0].IsFaceUp = true;
            mg.FromStack = Columns[c_from];
            mg.ToStack = Bases[targetBase];
            mg.MovingCards = [ca];
        }
        return mg;
    }

    private int GetMatchingBase(SolverCard ca)
    {
        int emptyBase = -1;
        int targetBase = -1;
        for (int bi = 0; bi < 4; bi++)
        {
            if (Bases[bi].PlayingCards.Count == 0)
            {
                if (emptyBase < 0)
                    emptyBase = bi;
            }
            else
            {
                if (Bases[bi].PlayingCards[0].Color == ca.Color)
                {
                    Debug.Assert(targetBase < 0);
                    targetBase = bi;
                    break;
                }
            }
        }
        if (targetBase == -1)
        {
            targetBase = emptyBase;
            Debug.Assert(targetBase >= 0);
        }
        return targetBase;
    }

    // col==7 is TalonFU, otherwise col in [0..6]
    // Only A can move to an empty base, otherwise card must be base top+1
    private bool CanMoveToBase(int col)
    {
        Debug.Assert(col >= 0 && col <= 7);
        SolverCard ca;
        if (col == 7)
        {
            // Can move from talon?
            if (TalonFU.PlayingCards.Count == 0)
                return false;
            ca = TalonFU.PlayingCards[0];
        }
        else
        {
            // Can move from column col?
            if (Columns[col].PlayingCards.Count == 0)
                return false;
            ca = Columns[col].PlayingCards[0];
        }

        int targetBase = GetMatchingBase(ca);
        return (Bases[targetBase].PlayingCards.Count == 0 && ca.Value == 1) || (Bases[targetBase].PlayingCards.Count > 0 && Bases[targetBase].PlayingCards.First().Value + 1 == ca.Value);
    }

    private SolverGroup MoveColumnToColumn(int c_from, int c_to, int n)
    {
        Debug.Assert(c_from >= 0 && c_from <= 7);
        Debug.Assert(c_to >= 0 && c_to < 7);
        Debug.Assert(c_from != c_to);
        Debug.Assert(c_from == 7 || n >= 1);
        Debug.Assert(c_from < 7 || n == 1);
        Debug.Assert(c_from == 7 || Columns[c_from].PlayingCards.Count >= n);
        Debug.Assert(c_from == 7 || Columns[c_from].PlayingCards[n - 1].IsFaceUp);

        var mg = new SolverGroup
        {
            FromStack = c_from == 7 ? TalonFU : Columns[c_from],
            ToStack = Columns[c_to],
            MovingCards = []
        };

        for (int i = n - 1; i >= 0; i--)
        {
            SolverCard ca;
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
            Debug.Assert(Columns[c_to].PlayingCards.Count == 0 || (ca.Value == Columns[c_to].PlayingCards[0].Value - 1 && ca.Color % 2 != Columns[c_to].PlayingCards[0].Color % 2));
            Columns[c_to].PlayingCards.Insert(0, ca);
            mg.MovingCards.Insert(0, ca);
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
            if (Columns[c_from].PlayingCards.Count > 0)
                Columns[c_from].PlayingCards[0].IsFaceUp = true;
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

        SolverCard ca;
        if (c_from == 7)
            ca = TalonFU.PlayingCards[0];
        else
            ca = Columns[c_from].PlayingCards[n - 1];

        if (Columns[c_to].PlayingCards.Count == 0 && ca.Value == 13 && (c_from == 7 || Columns[c_from].PlayingCards.Count != n))
            return true;  // Can move a King to an empty column, but only if it's not a whole column starting with a King (useless move)
        if (Columns[c_to].PlayingCards.Count > 0 && ca.Value == Columns[c_to].PlayingCards[0].Value - 1 && ca.Color % 2 != Columns[c_to].PlayingCards[0].Color % 2)
            return true; // Can move if value-1 and alternating colors 

        return false;
    }

    public List<SolverGroup> GetMovingGroupsForOneMovmentToBase() => OneMovementToBase(false);

    internal List<SolverGroup> OneMovementToBase(bool showTraces = false)
    {
        var Signatures = new HashSet<string>();
        var Movments = new List<SolverGroup>();

    restart_reset_talon:
        int talonRotateCount = TalonFU.PlayingCards.Count + TalonFD.PlayingCards.Count;
        if (showTraces)
            WriteLine($"Set talonRotateCount = {talonRotateCount}");

        // Move one card from columns to Base?
        for (int ci = 0; ci < 7; ci++)
        {
            if (CanMoveToBase(ci))
            {
                var mg = MoveToBase(ci);
                Movments.Add(mg);
                if (showTraces)
                    WriteLine(mg.ToString());
                return Movments;
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
            return Movments;
        }

        // Move from column to column?
        // No need to go through it if game is solvable, only moves to base and from talon are enough
        if (!IsGameSolvable())
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
#if DEBUG
                                            var s2 = Signature();
                                            Debug.Assert(s == s2);
#endif
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

        // Rotate talon and restart
        if (TalonFU.PlayingCards.Count + TalonFD.PlayingCards.Count == 0 || talonRotateCount == 0)
            return null;

        talonRotateCount--;
        if (showTraces)
            WriteLine($"talonRotateCount = {talonRotateCount}");

        // Need to reset TalonFD ?
        if (TalonFD.PlayingCards.Count == 0)
        {
            var mg = MoveAllTalonFUToTalonFD(showTraces);
            Movments.Add(mg);
        }

        // Move 1 Talon FD to Talon FU
        TalonFU.PlayingCards.Insert(0, TalonFD.PlayingCards[0]);
        TalonFU.PlayingCards[0].IsFaceUp = true;
        TalonFD.PlayingCards.RemoveAt(0);

        var mgt1 = new SolverGroup
        {
            FromStack = TalonFD,
            ToStack = TalonFU,
            MovingCards = []
        };
        mgt1.MovingCards.Add(TalonFU.PlayingCards[0]);
        Movments.Add(mgt1);

        if (showTraces)
        {
            WriteLine("Move 1 TalonFD -> TalonFU");
            PrintSolverStack("Talon FU    ", TalonFU);
            PrintSolverStack("Talon FD    ", TalonFD);
        }

        goto restart;
    }

    private SolverGroup MoveAllTalonFUToTalonFD(bool showTraces)
    {
        var mg = new SolverGroup
        {
            FromStack = TalonFU,
            ToStack = TalonFD,
            MovingCards = []
        };

        if (showTraces)
        {
            WriteLine("Before All TalonFU -> TalonFD");
            PrintSolverStack("Talon FU    ", TalonFU);
            PrintSolverStack("Talon FD    ", TalonFD);
        }
        // Move all TalonFU --> TalonFD
        foreach (var c in TalonFU.PlayingCards)
        {
            c.IsFaceUp = false;
            TalonFD.PlayingCards.Insert(0, c);
            mg.MovingCards.Insert(0, c);
        }
        TalonFU.PlayingCards.Clear();
        if (showTraces)
        {
            WriteLine("After All TalonFU -> TalonFD");
            PrintSolverStack("Talon FU    ", TalonFU);
            PrintSolverStack("Talon FD    ", TalonFD);
        }
        return mg;
    }

    public bool? Solve(bool printSteps = false, CancellationToken? cancellationToken = null)
    {
        if (printSteps)
            PrintSolverDeck();
#if DEBUG
        CheckSolverDeck();
#endif
        while (OneMovementToBase(showTraces: printSteps) != null)
        {
            if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                return null;

            if (printSteps)
                PrintSolverDeck();
#if DEBUG
            CheckSolverDeck();
#endif
            if (IsGameSolvable())
                break;
        }

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
        for (int bi = 0; bi < 4; bi++)
            if (Bases[bi].PlayingCards.Count == 0)
                sb.Append("@@");
            else
                sb.Append(Bases[bi].PlayingCards.First().Signature());

        // Then add columns signature.  Note than all cards of a column are included, not only the visible ones (not sure it's useful)
        for (int ci = 0; ci < 7; ci++)
        {
            sb.Append('|');

            if (ci == c_to)
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
            for (int i = (ci == c_from) ? n : 0; i < Columns[ci].PlayingCards.Count; i++)
            {
                sb.Append(Columns[ci].PlayingCards[i].Signature(firstCard));
                firstCard = false;
            }
        }
        return sb.ToString();
    }

}   // class SolitaireDeck
