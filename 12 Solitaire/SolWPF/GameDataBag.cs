// class GameDataBag
// Data set of a game session = View Model
// 2019-04-18   PV


using SolLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
        private readonly Dictionary<string, GameStack> StacksDictionary;

        public GameDataBag()
        {
            UndoStack = new Stack<MovingGroup>();
            StacksDictionary = new Dictionary<string, GameStack>();
            SeedRnd = new Random();
        }

        internal void InitializeStacksDictionary()
        {
            foreach (var b in Bases)
                StacksDictionary.Add(b.Name, b);
            foreach (var c in Columns)
                StacksDictionary.Add(c.Name, c);
            StacksDictionary.Add(TalonFD.Name, TalonFD);
            StacksDictionary.Add(TalonFU.Name, TalonFU);
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

            // Initial status
            UpdateGameStatus();
        }


        private void Clear()
        {
            UndoStack.Clear();
            foreach (var b in AllStacks())
                b.Clear();
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

        private SolverDeck GetSolverDeck()
        {
            List<(SolverCard, bool)>[] SolverBases = new List<(SolverCard, bool)>[4];
            for (int b = 0; b < 4; b++)
                CopyStack(Bases[b], out SolverBases[b]);
            List<(SolverCard, bool)>[] SolverColumns = new List<(SolverCard, bool)>[7];
            for (int c = 0; c < 7; c++)
                CopyStack(Columns[c], out SolverColumns[c]);
            CopyStack(TalonFU, out List<(SolverCard, bool)> SolverTalonFU);
            CopyStack(TalonFD, out List<(SolverCard, bool)> SolverTalonFD);

            // ToDo: If too long, do it in a separate thread
            var sd = new SolverDeck(SolverBases, SolverColumns, SolverTalonFU, SolverTalonFD);
            return sd;
        }

        private void CopyStack(GameStack st, out List<(SolverCard, bool)> solverSt)
        {
            solverSt = new List<(SolverCard, bool)>();
            foreach (var c in st.PlayingCards)
                solverSt.Add((new SolverCard(c.Value, c.Color, c.IsFaceUp), c.IsFaceUp));
        }

        public List<MovingGroup> GetNextMoves()
        {
            SolverDeck sd = GetSolverDeck();
            List<SolverGroup> lsg = sd.GetMovingGroupsForOneMovmentToBase();
            if (lsg == null)
                return null;

            List<MovingGroup> lmg = new List<MovingGroup>();
            foreach (var sg in lsg)
            {
                var from = StacksDictionary[sg.FromStack.Name];
                var to = StacksDictionary[sg.ToStack.Name];
                var hitList = new List<PlayingCard>();
                for (int i = 0; i < sg.MovingCards.Count; i++)
                    hitList.Add(from.PlayingCards[i]);
                var mg = new MovingGroup(from, hitList, true, false);
                mg.ToStack = to;
                lmg.Add(mg);
            }
            return lmg;
        }

        private void CheckDeck()
        {
            int nc = 0;
            for (int i = 0; i < 4; i++)
            {
                var b = Bases[i];
                b.CheckStack();
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
                c.CheckStack();
                nc += c.PlayingCards.Count;
            }

            TalonFU.CheckStack();
            TalonFD.CheckStack();

            nc += TalonFU.PlayingCards.Count + TalonFD.PlayingCards.Count;
            Debug.Assert(nc == 52);
        }


        internal bool CanUndo() => UndoStack.Count > 0;

        internal void PushUndo(MovingGroup mg)
        {
            UndoStack.Push(mg);
            CheckDeck();
        }

        internal void UpdateGameStatus()
        {
            CheckDeck();        // To be safe in debug mode

            MoveCount = UndoStack.Count;

            if (MoveCount == 0)
                GameStatus = "Not started";
            else if (IsGameFinished())
                GameStatus = "Game finished!";
            else if (IsGameSolvable())
                GameStatus = "Game solvable!";
            else
                GameStatus = "In progress";

            ComputeAndUpdateGameSolvability();
        }

        CancellationTokenSource cts;

        // Do the job in a background worker, that can be interrupted if needed
        private void ComputeAndUpdateGameSolvability()
        {
            // If a previous evaluation is still in progress, cancel it
            cts?.Cancel();

            cts = new CancellationTokenSource();
            // We need to use a Progress<T> to update interface from a different thread
            IProgress<string> progress = new Progress<string>(UpdateHashProgress);
            // GetSolverDeck access WPF objects, so it must run in UI thread
            SolverDeck sd = GetSolverDeck();
            Task t = new Task(() =>
            {
                if (cts.Token.IsCancellationRequested) goto EndTask;
                bool? res = sd.Solve(cancellationToken: cts.Token);
                if (res.HasValue)
                    progress.Report(res.Value ? "Solvable" : "No solution");
                EndTask:
                ;
                //cts = null;
            });
            t.Start();
        }


        private void UpdateHashProgress(string s)
        {
            SolverStatus = s;
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

        private string _SolverStatus;
        public string SolverStatus
        {
            get { return _SolverStatus; }
            set
            {
                if (_SolverStatus != value)
                {
                    _SolverStatus = value;
                    NotifyPropertyChanged(nameof(SolverStatus));
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

    }
}
