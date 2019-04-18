// class GameDataBag
// Data set of a game session
// 2019-04-18   PV


using System;
using System.Collections.Generic;
using System.Text;

namespace SolWPF
{
    internal class GameDataBag
    {
        public BaseStack[] Bases;
        public ColumnStack[] Columns;
        public TalonFaceDownStack TalonFD;
        public TalonFaceUpStack TalonFU;
        public Stack<MovingGroup> UndoStack;

        public GameDataBag()
        {
            UndoStack = new Stack<MovingGroup>();
        }

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
    }
}
