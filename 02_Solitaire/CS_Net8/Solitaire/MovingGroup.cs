// Solitaire WPF
// class MovingGroup
// A group of cards moving together between two GameStacks
//
// 2019-04-12   PV
// 2020-12-19   PV      Net5 C#9, nullable enable
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SolWPF;

internal class MovingGroup
{
    public readonly GameStack FromStack;
    public GameStack? ToStack;
    public readonly List<PlayingCard>? MovingCards;
    public readonly bool IsMovable;

    private readonly List<Vector>? offVec;
    private PlayingCard? cardMadeVisibleDuringMove;

    public MovingGroup(GameStack from, List<PlayingCard>? hitList, bool isMovable)
    {
        FromStack = from;
        IsMovable = isMovable;

        if (hitList != null)
        {
            MovingCards = [.. hitList];
            offVec = [];
            var P0 = new Point((double)MovingCards[0].GetValue(Canvas.LeftProperty), (double)MovingCards[0].GetValue(Canvas.TopProperty));
            foreach (PlayingCard pc in hitList)
            {
                var P = new Point((double)pc.GetValue(Canvas.LeftProperty), (double)pc.GetValue(Canvas.TopProperty));
                offVec.Add(P - P0);
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"MovingGroup FromStack={FromStack.Name}, ToStack={ToStack?.Name}, IsMovable={IsMovable}, MovingCards=");
        if (MovingCards == null)
            sb.Append('∅');
        else
            foreach (var c in MovingCards)
                sb.Append(c.Signature()).Append(' ');
        return sb.ToString();
    }

    internal Point GetTopLeft()
    {
        Point P;
        if (MovingCards != null)
            P = new Point((double)MovingCards[0].GetValue(Canvas.LeftProperty), (double)MovingCards[0].GetValue(Canvas.TopProperty));
        else
            P = new Point(0, 0);
        return P;
    }

    internal void SetTopLeft(Point P)
    {
        Debug.Assert(MovingCards is not null);
        Debug.Assert(offVec is not null);

        for (int i = 0; i < MovingCards.Count; i++)
        {
            MovingCards[i].SetValue(Canvas.LeftProperty, P.X + offVec[i].X);
            MovingCards[i].SetValue(Canvas.TopProperty, P.Y + offVec[i].Y);
        }
    }

    internal void DoMove(bool withAnimation = false)
    {
        Debug.Assert(MovingCards is not null);
        Debug.Assert(ToStack is not null);
#if DEBUG

        var sb = new StringBuilder($"\nDoMove From=({FromStack}) To=({ToStack}) MovingCards=");
        foreach (var c in MovingCards)
            sb.Append(c.Signature()).Append(' ');

        // Check that the first cards of FromStack match MovingCards
        // Since TalonFU reverses card order during a MoveIn, comparison is reversed
        if (FromStack.Name == "TalonFU")
            for (int i = 0; i < MovingCards.Count; i++)
                Debug.Assert(FromStack.PlayingCards[MovingCards.Count - 1 - i] == MovingCards[i]);
        else
            for (int i = 0; i < MovingCards.Count; i++)
                Debug.Assert(FromStack.PlayingCards[i] == MovingCards[i]);
#endif
        cardMadeVisibleDuringMove = FromStack.MoveOutCards(MovingCards);
        ToStack.MoveInCards(MovingCards, withAnimation);
        ToStack.ClearTargetHighlight();

#if DEBUG
        sb.Append($" CMV={cardMadeVisibleDuringMove?.Signature()}");
        Debug.WriteLine(sb.ToString());
#endif

        // Keep this move for undo
        FromStack.b.PushUndo(this);

        // Finally refresh status bar.
        // Not really a good idea to do this here, nor using the reference "FromStack.b", but since DoMove is called from
        // various places (7 references), it's just convenient to do it here in a unique location.
        FromStack.b.UpdateGameStatus();
    }

    internal void UndoMove()
    {
        Debug.Assert(ToStack is not null);
        Debug.Assert(MovingCards is not null);
#if DEBUG
        var sb = new StringBuilder($"\nUndoMove From=({FromStack}) To=({ToStack}) MovingCards=");
        foreach (var c in MovingCards)
            sb.Append(c.Signature()).Append(' ');
        sb.Append($" CMV={cardMadeVisibleDuringMove?.Signature()}");
        Debug.WriteLine(sb.ToString());
#endif

        if (cardMadeVisibleDuringMove != null)
            cardMadeVisibleDuringMove.IsFaceUp = false;
        ToStack.MoveOutCards(MovingCards);
        FromStack.MoveInCards(MovingCards);
    }
}
