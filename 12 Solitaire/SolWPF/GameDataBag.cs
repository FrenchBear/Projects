// class GameDataBag
// Data set of a game session
// 2019-04-18   PV


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SolWPF
{
    internal class GameDataBag: INotifyPropertyChanged
    {
        public BaseStack[] Bases;
        public ColumnStack[] Columns;
        public TalonFaceDownStack TalonFD;
        public TalonFaceUpStack TalonFU;
        private Stack<MovingGroup> UndoStack;

        public GameDataBag()
        {
            UndoStack = new Stack<MovingGroup>();
            GameStatus = "In progress";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


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

        internal bool CanUndo() => UndoStack.Count > 0;

        internal void PushUndo(MovingGroup mg)
        {
            UndoStack.Push(mg);
            MoveCount = UndoStack.Count;
        }

        internal MovingGroup PopUndo()
        {
            if (UndoStack.Count == 0)
                return null;
            MoveCount = UndoStack.Count-1;
            return UndoStack.Pop();
        }


        private int _MoveCount;
        public int MoveCount
        {
            get { return _MoveCount; }
            set
            {
                if (_MoveCount != value)
                {
                    _MoveCount = value;
                    NotifyPropertyChanged(nameof(MoveCount));
                }
            }
        }

        private string _GameStatus;
        public string GameStatus
        {
            get { return _GameStatus; }
            set
            {
                if (_GameStatus != value)
                {
                    _GameStatus = value;
                    NotifyPropertyChanged(nameof(GameStatus));
                }
            }
        }

    }
}
