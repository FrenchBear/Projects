// class GameDataBag
// Data set of a game session = View Model
// 2019-04-18   PV


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SolWPF
{
    internal class GameDataBag : INotifyPropertyChanged
    {
        public BaseStack[] Bases;
        public ColumnStack[] Columns;
        public TalonFaceDownStack TalonFD;
        public TalonFaceUpStack TalonFU;
        private readonly Stack<MovingGroup> UndoStack;
        private readonly Random SeedRnd;

        public GameDataBag()
        {
            UndoStack = new Stack<MovingGroup>();
            SeedRnd = new Random();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


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

        internal void InitRandomDeck(int seed = 0)
        {
            Clear();
            if (seed == 0)
                seed = SeedRnd.Next(1, 1_000_000);
            GameSerial = seed;
            var rnd = new Random(seed);
            var lc = new List<string>();
            foreach (char c in "HDSC")
                foreach (char v in "A23456789XJQK")
                    lc.Add($"{c}{v}");
            for (int i = 0; i < lc.Count; i++)
            {
                var i1 = rnd.Next(lc.Count);
                var i2 = rnd.Next(lc.Count);
                var t = lc[i1];
                lc[i1] = lc[i2];
                lc[i2] = t;
            }

            // Distribute cards to columns
            for (int c = 0; c < 7; c++)
                for (int i = 0; i <= c; i++)
                {
                    string s = lc[0];
                    lc.RemoveAt(0);
                    Columns[c].AddCard(s, i == c);    // Only top one is face up
                }
            // The rest goes to the TalonFaceDown
            for (int mt = 0; mt < lc.Count; mt++)
                TalonFD.AddCard(lc[mt], false);

            UpdateGameStatus();
        }

        private void Clear()
        {
            UndoStack.Clear();
            foreach (var b in AllStacks())
                b.Clear();
        }

        internal void InitTestDeck1()
        {
            Clear();

            Columns[0].AddCard("HA", true);
            Columns[1].AddCard("D2", false);
            Columns[1].AddCard("C8", true);
            Columns[2].AddCard("H4", false);
            Columns[2].AddCard("DA", false);
            Columns[2].AddCard("DJ", true);
            Columns[3].AddCard("D3", false);
            Columns[3].AddCard("S3", false);
            Columns[3].AddCard("S2", false);
            Columns[3].AddCard("H6", true);
            Columns[4].AddCard("CQ", false);
            Columns[4].AddCard("C6", false);
            Columns[4].AddCard("SX", false);
            Columns[4].AddCard("SJ", false);
            Columns[4].AddCard("C5", true);
            Columns[5].AddCard("H7", false);
            Columns[5].AddCard("D8", false);
            Columns[5].AddCard("D5", false);
            Columns[5].AddCard("DK", false);
            Columns[5].AddCard("H8", false);
            Columns[5].AddCard("CA", true);
            Columns[6].AddCard("D9", false);
            Columns[6].AddCard("DQ", false);
            Columns[6].AddCard("CX", false);
            Columns[6].AddCard("HQ", false);
            Columns[6].AddCard("H2", false);
            Columns[6].AddCard("C2", false);
            Columns[6].AddCard("S7", true);
            TalonFD.AddCard("CJ", false);
            TalonFD.AddCard("DX", false);
            TalonFD.AddCard("S9", false);
            TalonFD.AddCard("S5", false);
            TalonFD.AddCard("HJ", false);
            TalonFD.AddCard("C3", false);
            TalonFD.AddCard("S6", false);
            TalonFD.AddCard("H5", false);
            TalonFD.AddCard("C9", false);
            TalonFD.AddCard("SK", false);
            TalonFD.AddCard("H9", false);
            TalonFD.AddCard("SQ", false);
            TalonFD.AddCard("D7", false);
            TalonFD.AddCard("HX", false);
            TalonFD.AddCard("SA", false);
            TalonFD.AddCard("S8", false);
            TalonFD.AddCard("H3", false);
            TalonFD.AddCard("C7", false);
            TalonFD.AddCard("D4", false);
            TalonFD.AddCard("S4", false);
            TalonFD.AddCard("D6", false);
            TalonFD.AddCard("HK", false);
            TalonFD.AddCard("C4", false);
            TalonFD.AddCard("CK", false);
        }

        internal void InitTestDeck2()
        {
            Clear();

            Bases[0].AddCard("HA", true);
            Bases[0].AddCard("H2", true);
            Bases[1].AddCard("CA", true);
            Bases[1].AddCard("C2", true);
            Columns[0].AddCard("CK", true);
            Columns[0].AddCard("HQ", true);
            Columns[0].AddCard("SJ", true);
            Columns[1].AddCard("D2", false);
            Columns[1].AddCard("C8", true);
            Columns[2].AddCard("H4", false);
            Columns[2].AddCard("DA", false);
            Columns[2].AddCard("DJ", true);
            Columns[2].AddCard("SX", true);
            Columns[3].AddCard("D3", false);
            Columns[3].AddCard("S3", false);
            Columns[3].AddCard("S2", true);
            Columns[4].AddCard("CQ", false);
            Columns[4].AddCard("C6", true);
            Columns[5].AddCard("H7", false);
            Columns[5].AddCard("D8", false);
            Columns[5].AddCard("D5", false);
            Columns[5].AddCard("DK", false);
            Columns[5].AddCard("H8", true);
            Columns[5].AddCard("S7", true);
            Columns[5].AddCard("H6", true);
            Columns[5].AddCard("C5", true);
            Columns[6].AddCard("D9", false);
            Columns[6].AddCard("DQ", false);
            Columns[6].AddCard("CX", true);
            TalonFU.AddCard("C4", true);
            TalonFU.AddCard("HK", true);
            TalonFD.AddCard("CJ", false);
            TalonFD.AddCard("DX", false);
            TalonFD.AddCard("S9", false);
            TalonFD.AddCard("S5", false);
            TalonFD.AddCard("HJ", false);
            TalonFD.AddCard("C3", false);
            TalonFD.AddCard("S6", false);
            TalonFD.AddCard("H5", false);
            TalonFD.AddCard("C9", false);
            TalonFD.AddCard("SK", false);
            TalonFD.AddCard("H9", false);
            TalonFD.AddCard("SQ", false);
            TalonFD.AddCard("D7", false);
            TalonFD.AddCard("HX", false);
            TalonFD.AddCard("SA", false);
            TalonFD.AddCard("S8", false);
            TalonFD.AddCard("H3", false);
            TalonFD.AddCard("C7", false);
            TalonFD.AddCard("D4", false);
            TalonFD.AddCard("S4", false);
            TalonFD.AddCard("D6", false);
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

        internal bool CanUndo() => UndoStack.Count > 0;

        internal void PushUndo(MovingGroup mg)
        {
            UndoStack.Push(mg);
        }

        internal void UpdateGameStatus()
        {
            MoveCount = UndoStack.Count;
            if (MoveCount == 0)
                GameStatus = "Not started";
            else if (IsGameFinished())
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

        private int _GameSerial;
        public int GameSerial
        {
            get { return _GameSerial; }
            set
            {
                if (_GameSerial != value)
                {
                    _GameSerial = value;
                    NotifyPropertyChanged(nameof(GameSerial));
                }
            }
        }

        internal MovingGroup GetNextMove()
        {
            // ToDp: Just for tests, to replace by real solver code
            var hitList = new List<PlayingCard>();
            for (int i = 0; i < 7; i++)
                if (Columns[i].PlayingCards.Count > 0)
                {
                    hitList.Add(Columns[i].PlayingCards[0]);
                    var mg = new MovingGroup(Columns[i], hitList, true, false);
                    mg.ToStack = Bases[Columns[i].PlayingCards[0].Color];
                    return mg;
                }
            return null;
        }
    }
}
