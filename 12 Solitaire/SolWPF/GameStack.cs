// GameStack class
// A base class to represent a location and a collection of PlayingCards
// 2019-04-11   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace SolWPF
{
    class GameStack
    {
        public readonly GameDataBag b;
        public readonly string Name;
        protected readonly Canvas PlayingCanvas;
        protected readonly Rectangle BaseRect;
        public readonly List<PlayingCard> PlayingCards;

        public GameStack(GameDataBag b, String n, Canvas c, Rectangle r)
        {
            this.b = b;
            Name = n;
            PlayingCanvas = c;
            BaseRect = r;
            r.Width = MainWindow.cardWidth;
            r.Height = MainWindow.cardHeight;
            PlayingCards = new List<PlayingCard>();
        }

        // Must be called BEFORE adding the card to PlayingCards list
        protected virtual Point getNewCardPosition()
        {
            var P = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));
            return P;
        }

        public void AddCard(string face, bool isFaceUp)
        {
            var MyCard = new PlayingCard(face, isFaceUp);
            MyCard.Width = MainWindow.cardWidth;
            MyCard.Height = MainWindow.cardHeight;
            Point P = getNewCardPosition();
            MyCard.SetValue(Canvas.LeftProperty, P.X);
            MyCard.SetValue(Canvas.TopProperty, P.Y);
            PlayingCanvas.Children.Add(MyCard);
            PlayingCards.Insert(0, MyCard);
        }

        // MouveOut is reponsible for putting moved cards on top of display stack
        // Returns a PlayingCard if it's been made visible in the process, null otherwise
        internal protected virtual PlayingCard MoveOutCards(List<PlayingCard> movedCards)
        {
            Debug.Assert(PlayingCards.Count >= movedCards.Count);
            for (int i = 0; i < movedCards.Count; i++)
            {
                Debug.Assert(PlayingCards[0].IsFaceUp);
                PlayingCards.RemoveAt(0);
            }
            return null;
        }

        internal protected virtual void MoveInCards(List<PlayingCard> movedCards)
        {
            for (int i = movedCards.Count - 1; i >= 0; i--)
            {
                Point P = getNewCardPosition();
                movedCards[i].SetValue(Canvas.LeftProperty, P.X);
                movedCards[i].SetValue(Canvas.TopProperty, P.Y);
                PlayingCards.Insert(0, movedCards[i]);
            }
        }


        // Internal hit test
        // Base version should only check rectangle, derived classes are responsible to implement
        // specialized versions with possible offsets
        protected virtual bool isStackHit(Point P, bool onlyTopCard, bool includeCardFaceDown, bool includeEmptyStack, out List<PlayingCard> hitList, out bool isMovable)
        {
            Point Q;
            hitList = null;
            isMovable = true;

            // Handle first an empty stack
            if (PlayingCards.Count == 0)
            {
                if (!includeEmptyStack)
                    return false;

                isMovable = false;  // An empty stack is never movable
                // Note that hitList is returned null in this case
                Q = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));
                return (P.X >= Q.X && P.X <= Q.X + MainWindow.cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + MainWindow.cardHeight);
            }

            int iMax = onlyTopCard ? 1 : PlayingCards.Count;
            for (int i = 0; i < iMax; i++)
            {
                if (!includeCardFaceDown && !PlayingCards[i].IsFaceUp)
                    break;

                Q = new Point((double)PlayingCards[i].GetValue(Canvas.LeftProperty), (double)PlayingCards[i].GetValue(Canvas.TopProperty));
                if (P.X >= Q.X && P.X <= Q.X + MainWindow.cardWidth && P.Y >= Q.Y && P.Y <= Q.Y + MainWindow.cardHeight)
                {
                    hitList = new List<PlayingCard>();
                    for (int j = 0; j <= i; j++)
                        hitList.Add(PlayingCards[j]);
                    isMovable = PlayingCards[i].IsFaceUp;
                    return true;
                }
            }
            return false;
        }

        protected virtual bool isStackFromHit(Point P, out List<PlayingCard> hitList, out bool isMovable)
        {
            return isStackHit(P, true, false, false, out hitList, out isMovable);
        }

        protected virtual bool isStackToHit(Point P)
        {
            return isStackHit(P, true, false, true, out _, out _);
        }


        // Main function to detect a valid click on a stack, that can either start a move or not.
        // Returns NULL if point P does not match any valid area of the stack or the stack is empty.
        // Otherwise returns a MovingGroup containing a list of potentialy moved cards.
        // IsMovable is True for group of cards that can move, in this case, the group of cards is already on top ov display stack.
        // IsMovable is always False for TalonFaceDown, if there is at least one card in TalonFaceDown, then hitList contains this card, 
        // otherise hitList is empty, indicating that TalonFaceDown must be reset
        public virtual MovingGroup FromHitTest(Point P)
        {
            if (!isStackFromHit(P, out List<PlayingCard> hitList, out bool isMovable)) return null;

            var mg = new MovingGroup(this, hitList, isMovable);
            Debug.Write($"FromHitTest {Name}: {mg.ToString()}");
            if (isMovable)
                for (int i = hitList.Count - 1; i >= 0; i--)
                {
                    PlayingCanvas.Children.Remove(hitList[i]);
                    PlayingCanvas.Children.Add(hitList[i]);
                    hitList[i].IsFaceUp = true;
                }
            return mg;
        }


        protected virtual bool RulesAllowMoveInCards(MovingGroup mg)
        {
            return true;
        }

        public virtual bool ToHitTest(Point P, MovingGroup mg)
        {
            if (isStackToHit(P) && RulesAllowMoveInCards(mg))
            {
                BaseRect.Stroke = Brushes.Red;
                BaseRect.StrokeThickness = 5.0;
                return true;
            }
            else
            {
                ClearTargetHighlight();
                return false;
            }
        }

        internal void ClearTargetHighlight()
        {
            BaseRect.Stroke = Brushes.Black;
            BaseRect.StrokeThickness = 3.0;
        }
    }


    abstract class TalonBaseStack : GameStack
    {
        public TalonBaseStack(GameDataBag b, String n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        // Talon is never a target
        public override bool ToHitTest(Point P, MovingGroup mg)
        {
            Debug.WriteLine($"TalonBaseStack.ToHitTest  Name={Name}");
            return false;
        }
    }   // class TalonBaseStack


    class TalonFaceDownStack : TalonBaseStack
    {
        public TalonFaceDownStack(GameDataBag b, String n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        // For Talon face down, empty stack is valid to generate a Click to reset the talon
        protected override bool isStackFromHit(Point P, out List<PlayingCard> hitList, out bool isMovable)
        {
            var r = isStackHit(P, true, true, true, out hitList, out isMovable);
            Debug.Assert(r == false || isMovable == false);
            return r;
        }

        internal void ResetTalon(TalonFaceUpStack TalonFU)
        {
            Debug.Assert(PlayingCards.Count == 0);
            // Talon empty;
            if (TalonFU.PlayingCards.Count == 0)
                return;

            var mg = new MovingGroup(TalonFU, TalonFU.PlayingCards, true, reverseCardsDuringMove: true);
            mg.ToStack = this;
            mg.DoMove();
        }


        internal protected override void MoveInCards(List<PlayingCard> movedCards)
        {
            base.MoveInCards(movedCards);
            // In TalonFD, in cards are always face down (only called during a ResetTalon
            foreach (var c in movedCards)
                c.IsFaceUp = false;
        }

        internal protected override PlayingCard MoveOutCards(List<PlayingCard> movedCards)
        {
            Debug.Assert(PlayingCards.Count >= 1);
            var c = PlayingCards[0];
            PlayingCards.RemoveAt(0);

            PlayingCanvas.Children.Remove(c);
            PlayingCanvas.Children.Add(c);
            return null;
        }
    }   // class TalonFaceDownStack


    class TalonFaceUpStack : TalonBaseStack
    {
        public TalonFaceUpStack(GameDataBag b, String n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        protected override bool isStackFromHit(Point P, out List<PlayingCard> hitList, out bool isMovable)
        {
            return isStackHit(P, true, false, false, out hitList, out isMovable);
        }

        internal protected override void MoveInCards(List<PlayingCard> movedCards)
        {
            base.MoveInCards(movedCards);
            // In TalonFU, in cards are always face up (card moved from TalonFD)
            foreach (var c in movedCards)
                c.IsFaceUp = true;
        }
    }   // class TalonFaceUpStack


    // New cards are shown in a visible stack
    class ColumnStack : GameStack
    {
        const double visibleYOffset = 45.0;
        const double notVvisibleYOffset = 10.0;

        public ColumnStack(GameDataBag b, String n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        protected override Point getNewCardPosition()
        {
            Point P = base.getNewCardPosition();
            if (PlayingCards.Count == 0) return P;
            double off = 0;
            for (int i = PlayingCards.Count - 1; i >= 0; i--)
                off += PlayingCards[i].IsFaceUp ? visibleYOffset : notVvisibleYOffset;

            return new Point(P.X, P.Y + off);
        }

        protected override bool isStackFromHit(Point P, out List<PlayingCard> hitList, out bool isMovable)
        {
            return isStackHit(P, false, false, false, out hitList, out isMovable);
        }

        internal protected override PlayingCard MoveOutCards(List<PlayingCard> movedCards)
        {
            base.MoveOutCards(movedCards);

            // For columns, Make card just below the moved group face up if needed
            if (PlayingCards.Count > 0 && !PlayingCards[0].IsFaceUp)
            {
                PlayingCards[0].IsFaceUp = true;
                return PlayingCards[0];
            }
            return null;
        }

    }   // class ColumnStack


    class BaseStack : GameStack
    {
        public BaseStack(GameDataBag b, String n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        protected override bool RulesAllowMoveInCards(MovingGroup mg)
        {
            // Can only add 1 card to a base
            if (mg.MovingCards.Count != 1)
                return false;

            // Need to access other bases
            // Simplified model for now
            if (PlayingCards.Count == 0)
                return mg.MovingCards[0].Value == 1;

            return PlayingCards[0].Color == mg.MovingCards[0].Color && PlayingCards[0].Value + 1 == mg.MovingCards[0].Value;
        }

    }   // class BaseStack

}
