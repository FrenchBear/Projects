// class GameDataBag
// Data set of a game session = View Model
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


        // Helper
        public IEnumerable<GameStack> AllStacks()
        {
            foreach (var gs in Bases)
                yield return gs;
            foreach (var gs in Columns)
                yield return gs;
            yield return TalonFU;
            yield return TalonFD;
        }



        public GameDataBag()
        {
            UndoStack = new Stack<MovingGroup>();
            GameStatus = "Not started";
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
            UpdateGameStatus();
        }

        internal void UpdateGameStatus()
        {
            MoveCount = UndoStack.Count;
            if (IsGameFinished())
                GameStatus = "Game finished!";
            else if (IsGameSolvable())
                GameStatus = "Game solvable!";
            else
                GameStatus = "In progress";

        }

        internal MovingGroup PopUndo()
        {
            if (UndoStack.Count == 0)
                return null;
            return UndoStack.Pop();
            // Cannot update game status here, before executing the undo, contrary to
            // PushUndo, called after the move has been performed
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
