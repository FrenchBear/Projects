// Solitaire WPF
// GameStack class
// A hierarchy of classes to represent a location and a collection of PlayingCards
// 2019-04-11   PV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace SolWPF
{
    class GameStack
    {
        public readonly GameDeck b;
        public readonly string Name;
        public readonly List<PlayingCard> PlayingCards;     // By convention, element [0] of this list will become element [0] of stack it will be moved to

        protected readonly Canvas PlayingCanvas;
        protected readonly Rectangle BaseRect;

        public GameStack(GameDeck b, String n, Canvas c, Rectangle r)
        {
            this.b = b;
            Name = n;
            PlayingCanvas = c;
            BaseRect = r;
            r.Width = MainWindow.cardWidth;
            r.Height = MainWindow.cardHeight;
            PlayingCards = new List<PlayingCard>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder($"{Name} Cards=");
            foreach (var c in PlayingCards)
                sb.Append(c.Signature()).Append(' ');
            return sb.ToString();
        }

        // Must be called BEFORE adding the card to PlayingCards list
        protected virtual Point GetNewCardPosition()
        {
            var P = new Point((double)BaseRect.GetValue(Canvas.LeftProperty), (double)BaseRect.GetValue(Canvas.TopProperty));
            return P;
        }

        public void AddCard(string face, bool isFaceUp)
        {
            var MyCard = new PlayingCard(face, isFaceUp)
            {
                Width = MainWindow.cardWidth,
                Height = MainWindow.cardHeight
            };
            Point P = GetNewCardPosition();
            MyCard.SetValue(Canvas.LeftProperty, P.X);
            MyCard.SetValue(Canvas.TopProperty, P.Y);
            PlayingCanvas.Children.Add(MyCard);
            PlayingCards.Insert(0, MyCard);
        }

        internal void Clear()
        {
            foreach (var c in PlayingCards)
                PlayingCanvas.Children.Remove(c);
            PlayingCards.Clear();
        }


        // MouveOut is responsible for putting moved cards on top of display stack
        // Returns a PlayingCard if it's been made visible in the process, null otherwise
        internal protected virtual PlayingCard MoveOutCards(List<PlayingCard> movedCards)
        {
            Debug.Assert(PlayingCards.Count >= movedCards.Count);
            for (int i = movedCards.Count - 1; i >= 0; i--)
            {
                Debug.Assert(PlayingCards[i].IsFaceUp);
                PlayingCanvas.Children.Remove(PlayingCards[i]);
                PlayingCanvas.Children.Add(PlayingCards[i]);
                PlayingCards.RemoveAt(i);
            }
            return null;
        }

        // Storyboard and animation is static, there's only one animation in progress for all stacks
        private static Storyboard sb;
        private static Action AnimationCompleted;

        internal protected virtual void MoveInCards(List<PlayingCard> movedCards, bool withAnimation = false)
        {
            if (withAnimation)
            {
                // Terminate previous animation if needed
                AnimationCompleted?.Invoke();

                sb = new Storyboard();
                var duration = new Duration(TimeSpan.FromSeconds(0.2));
                Point[] to = new Point[movedCards.Count];

                for (int i = movedCards.Count - 1; i >= 0; i--)
                {
                    int j = i;
                    Point from = new Point((double)movedCards[j].GetValue(Canvas.LeftProperty), (double)movedCards[j].GetValue(Canvas.TopProperty));
                    to[j] = GetNewCardPosition();

                    var ax = new DoubleAnimation(from.X, to[j].X, duration, FillBehavior.HoldEnd);
                    Storyboard.SetTarget(ax, movedCards[j]);
                    Storyboard.SetTargetProperty(ax, new PropertyPath(Canvas.LeftProperty));
                    sb.Children.Add(ax);
                    var ay = new DoubleAnimation(from.Y, to[i].Y, duration, FillBehavior.HoldEnd);
                    Storyboard.SetTarget(ay, movedCards[j]);
                    Storyboard.SetTargetProperty(ay, new PropertyPath(Canvas.TopProperty));
                    sb.Children.Add(ay);

                    PlayingCards.Insert(0, movedCards[j]);
                }

                AnimationCompleted = () =>
                {
                    for (int i = movedCards.Count - 1; i >= 0; i--)
                    {
                        movedCards[i].BeginAnimation(Canvas.LeftProperty, null);
                        movedCards[i].BeginAnimation(Canvas.TopProperty, null);
                        movedCards[i].SetValue(Canvas.LeftProperty, to[i].X);
                        movedCards[i].SetValue(Canvas.TopProperty, to[i].Y);
                        sb = null;
                        AnimationCompleted = null;
                    };
                };

                sb.Completed += (object sender, EventArgs e) => AnimationCompleted?.Invoke();
                sb.Begin();
            }
            else
                for (int i = movedCards.Count - 1; i >= 0; i--)
                {
                    Point P = GetNewCardPosition();
                    movedCards[i].SetValue(Canvas.LeftProperty, P.X);
                    movedCards[i].SetValue(Canvas.TopProperty, P.Y);
                    PlayingCards.Insert(0, movedCards[i]);
                }
        }


        // Internal hit test
        // Base version should only check rectangle, derived classes are responsible to implement
        // specialized versions with possible offsets
        protected virtual bool IsStackHit(Point P, bool onlyTopCard, bool includeCardFaceDown, bool includeEmptyStack, out List<PlayingCard> hitList, out bool isMovable)
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

        protected virtual bool IsStackFromHit(Point P, out List<PlayingCard> hitList, out bool isMovable)
        {
            return IsStackHit(P, true, false, false, out hitList, out isMovable);
        }

        protected virtual bool IsStackToHit(Point P)
        {
            return IsStackHit(P, true, false, true, out _, out _);
        }


        // Main function to detect a valid click on a stack, that can either start a move or not.
        // Returns NULL if point P does not match any valid area of the stack or the stack is empty.
        // Otherwise returns a MovingGroup containing a list of potentially moved cards.
        // IsMovable is True for group of cards that can move, in this case, the group of cards is already on top of display stack.
        // IsMovable is always False for TalonFaceDown, if there is at least one card in TalonFaceDown, then hitList contains this card, 
        // otherwise hitList is empty, indicating that TalonFaceDown must be reset
        public virtual MovingGroup FromHitTest(Point P)
        {
            if (!IsStackFromHit(P, out List<PlayingCard> hitList, out bool isMovable)) return null;

            var mg = new MovingGroup(this, hitList, isMovable);
            if (isMovable)
                for (int i = hitList.Count - 1; i >= 0; i--)
                {
                    PlayingCanvas.Children.Remove(hitList[i]);
                    PlayingCanvas.Children.Add(hitList[i]);
                    hitList[i].IsFaceUp = true;
                }
            return mg;
        }


        public virtual bool ToHitTest(Point P, MovingGroup mg)
        {
            if (IsStackToHit(P) && RulesAllowMoveInCards(mg))
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

        protected virtual bool RulesAllowMoveInCards(MovingGroup mg)
        {
            return true;
        }

        internal void ClearTargetHighlight()
        {
            BaseRect.Stroke = Brushes.Black;
            BaseRect.StrokeThickness = 3.0;
        }

        public virtual void CheckStack()
        {
        }

    }


    abstract class TalonBaseStack : GameStack
    {
        public TalonBaseStack(GameDeck b, String n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        // Talon is never a target
        public override bool ToHitTest(Point P, MovingGroup mg)
        {
            return false;
        }
    }   // class TalonBaseStack


    class TalonFaceDownStack : TalonBaseStack
    {
        public TalonFaceDownStack(GameDeck b, String n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        // For Talon face down, empty stack is valid to generate a Click to reset the talon
        protected override bool IsStackFromHit(Point P, out List<PlayingCard> hitList, out bool isMovable)
        {
            var r = IsStackHit(P, true, true, true, out hitList, out isMovable);
            Debug.Assert(r == false || isMovable == false);
            return r;
        }

        internal void ResetTalon(TalonFaceUpStack TalonFU)
        {
            Debug.Assert(PlayingCards.Count == 0);
            // Talon empty;
            if (TalonFU.PlayingCards.Count == 0)
                return;

            var mg = new MovingGroup(TalonFU, TalonFU.PlayingCards.Reverse<PlayingCard>().ToList(), true);
            mg.ToStack = this;
            mg.DoMove(true);        // Can do animation, since ResetTalon is not called during Undo
        }


        internal protected override void MoveInCards(List<PlayingCard> movedCards, bool withAnimation = false)
        {
            base.MoveInCards(movedCards, withAnimation);        // No standard animation for now
            // In TalonFD, in cards are always face down (only called during a ResetTalon)
            foreach (var c in movedCards)
                c.IsFaceUp = false;
        }

        internal protected override PlayingCard MoveOutCards(List<PlayingCard> movedCards)
        {
            Debug.Assert(PlayingCards.Count >= movedCards.Count);
            for (int i = movedCards.Count - 1; i >= 0; i--)
            {
                var c = PlayingCards[0];
                PlayingCards.RemoveAt(0);

                PlayingCanvas.Children.Remove(c);
                PlayingCanvas.Children.Add(c);
            }
            return null;
        }
        public override void CheckStack()
        {
            Debug.Assert(PlayingCards.All(c => !c.IsFaceUp));
        }

    }   // class TalonFaceDownStack


    class TalonFaceUpStack : TalonBaseStack
    {
        public TalonFaceUpStack(GameDeck b, string n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        protected override bool IsStackFromHit(Point P, out List<PlayingCard> hitList, out bool isMovable)
        {
            return IsStackHit(P, true, false, false, out hitList, out isMovable);
        }

        internal protected override void MoveInCards(List<PlayingCard> movedCards, bool withAnimation = false)
        {
            // TalonFU inverts cards to support Undo correctly
            base.MoveInCards(movedCards.Reverse<PlayingCard>().ToList(), withAnimation);

            // In TalonFU, in cards are always face up (card moved from TalonFD)
            foreach (var c in movedCards)
                c.IsFaceUp = true;
        }

        public override void CheckStack()
        {
            Debug.Assert(PlayingCards.All(c => c.IsFaceUp));
        }

    }   // class TalonFaceUpStack


    // New cards are shown in a visible stack
    class ColumnStack : GameStack
    {
        const double visibleYOffset = 45.0;
        const double notVvisibleYOffset = 10.0;

        public ColumnStack(GameDeck b, string n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        protected override Point GetNewCardPosition()
        {
            Point P = base.GetNewCardPosition();
            if (PlayingCards.Count == 0) return P;
            double off = 0;
            for (int i = PlayingCards.Count - 1; i >= 0; i--)
                off += PlayingCards[i].IsFaceUp ? visibleYOffset : notVvisibleYOffset;

            return new Point(P.X, P.Y + off);
        }

        protected override bool IsStackFromHit(Point P, out List<PlayingCard> hitList, out bool isMovable)
        {
            return IsStackHit(P, false, false, false, out hitList, out isMovable);
        }

        protected override bool RulesAllowMoveInCards(MovingGroup mg)
        {
            Debug.Assert(mg.MovingCards.Count > 0);

            // If column is empty, can only add a group starting with a King
            if (PlayingCards.Count == 0)
                return mg.MovingCards[mg.MovingCards.Count - 1].Value == 13;

            Debug.WriteLine($"ST.C={PlayingCards[0].Color % 2}  MG.C={mg.MovingCards[mg.MovingCards.Count - 1].Color % 2}  ST.V={PlayingCards[0].Value}  MG.V={mg.MovingCards[mg.MovingCards.Count - 1].Value}");

            // Otherwise alternate color, decreasing value
            return PlayingCards[0].Color % 2 != mg.MovingCards[mg.MovingCards.Count - 1].Color % 2 && PlayingCards[0].Value - 1 == mg.MovingCards[mg.MovingCards.Count - 1].Value;
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

        public override void CheckStack()
        {
            bool visiblePart = true;
            for (int i = 0; i < PlayingCards.Count; i++)
            {
                if (!PlayingCards[i].IsFaceUp)
                    visiblePart = false;
                if (visiblePart)
                {
                    if (i > 0)
                    {
                        Debug.Assert(PlayingCards[i].Value == PlayingCards[i - 1].Value + 1);
                        Debug.Assert(PlayingCards[i].Color % 2 != PlayingCards[i - 1].Color % 2);
                    }
                }
                else
                    Debug.Assert(!PlayingCards[i].IsFaceUp);
            }
        }
    }   // class ColumnStack


    class BaseStack : GameStack
    {
        public BaseStack(GameDeck b, String n, Canvas c, Rectangle r) : base(b, n, c, r) { }

        protected override bool RulesAllowMoveInCards(MovingGroup mg)
        {
            // Can only add 1 card to a base
            if (mg.MovingCards.Count != 1)
                return false;

            // Can only drop an Ace on an empty base
            if (PlayingCards.Count == 0)
                return mg.MovingCards[0].Value == 1;

            // Otherwise same color, increasing values
            return PlayingCards[0].Color == mg.MovingCards[0].Color && PlayingCards[0].Value + 1 == mg.MovingCards[0].Value;
        }

        public override void CheckStack()
        {
            if (PlayingCards.Count > 0)
            {
                int color = PlayingCards[0].Color;
                for (int i = PlayingCards.Count - 1; i >= 0; i--)
                {
                    Debug.Assert(PlayingCards[i].Color == color);
                    Debug.Assert(PlayingCards[i].Value == PlayingCards.Count - i);
                }
            }
        }


    }   // class BaseStack

}
