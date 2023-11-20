// Solitaire WPF
// class GameDeck
// Data set of a solitaire session = View Model
//
// 2019-04-18   PV
// 2021-11-13   PV      Net6 C#10
// 2023-11-20   PV      Net8 C#12

using SolLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SolWPF;

internal class GameDeck: INotifyPropertyChanged
{
    public BaseStack[] Bases;
    public ColumnStack[] Columns;
    public TalonFaceDownStack TalonFD;
    public TalonFaceUpStack TalonFU;
    private readonly Stack<MovingGroup> UndoStack;
    private readonly Random SeedRnd;
    private readonly Dictionary<string, GameStack> StacksDictionary;        // Map name -> Stack, name in [Base0..Base3, Column0..Column6, TalonFU, TalonFD]
    private readonly Dictionary<string, PlayingCard> CardsDictionary;       // Map face -> PlayingCard, name in [Colors]×[Values] such as H4 or DQ for 4 of ♥ or Queen of ♦

    public GameDeck()
    {
        UndoStack = new Stack<MovingGroup>();
        StacksDictionary = [];
        CardsDictionary = [];
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
        ClearDeck();
        if (seed == 0)
            seed = SeedRnd.Next(1, 1_000_000);
        GameSerial = seed;
        var rnd = new Random(seed);
        var lc = new List<string>();
        // Build set
        foreach (char c in PlayingCard.Colors)
            foreach (char v in PlayingCard.Values)
                lc.Add($"{c}{v}");
        // shuffle it
        for (int i = 0; i < lc.Count; i++)
        {
            var i1 = rnd.Next(lc.Count);
            var i2 = rnd.Next(lc.Count);
            (lc[i2], lc[i1]) = (lc[i1], lc[i2]);
        }

        // Distribute cards to columns
        for (int c = 0; c < 7; c++)
            for (int i = 0; i <= c; i++)
            {
                string s = lc[0];
                lc.RemoveAt(0);
                Columns[c].AddCard(s, i == c);    // Only top card is face up
            }
        // The rest goes to the TalonFaceDown
        for (int mt = 0; mt < lc.Count; mt++)
            TalonFD.AddCard(lc[mt], false);

        // Fill Cards Dictionary
        foreach (GameStack st in AllStacks())
            foreach (PlayingCard c in st.PlayingCards)
                CardsDictionary.Add(c.Face, c);

        // Initial status
        UpdateGameStatus();
#if DEBUG
        PrintGame();
#endif
    }

    private void ClearDeck()
    {
        UndoStack.Clear();
        CardsDictionary.Clear();
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
            if (c.PlayingCards.Count > 0 && !c.PlayingCards[^1].IsFaceUp)
                return false;
        return true;
    }

    private SolverDeck GetSolverDeck()
    {
        static void CopyStack(GameStack st, out List<(SolverCard, bool)> solverSt)
        {
            solverSt = [];
            foreach (var c in st.PlayingCards)
                solverSt.Add((new SolverCard(c.Value, c.Color, c.IsFaceUp), c.IsFaceUp));
        }

        var SolverBases = new List<(SolverCard, bool)>[4];
        for (int b = 0; b < 4; b++)
            CopyStack(Bases[b], out SolverBases[b]);
        var SolverColumns = new List<(SolverCard, bool)>[7];
        for (int c = 0; c < 7; c++)
            CopyStack(Columns[c], out SolverColumns[c]);
        CopyStack(TalonFU, out List<(SolverCard, bool)> SolverTalonFU);
        CopyStack(TalonFD, out List<(SolverCard, bool)> SolverTalonFD);

        return new SolverDeck(SolverBases, SolverColumns, SolverTalonFU, SolverTalonFD);
    }

    public List<MovingGroup> GetNextMoves()
    {
        SolverDeck sd = GetSolverDeck();
        List<SolverGroup> lsg = sd.GetMovingGroupsForOneMovmentToBase();
        if (lsg == null)
            return null;

        Debug.WriteLine("");
        var lmg = new List<MovingGroup>();
        foreach (var sg in lsg)
        {
            Debug.WriteLine(sg.ToString());
            var from = StacksDictionary[sg.FromStack.Name];
            var to = StacksDictionary[sg.ToStack.Name];
            var hitList = new List<PlayingCard>();
            for (int i = 0; i < sg.MovingCards.Count; i++)
                hitList.Add(CardsDictionary[sg.MovingCards[i].Face]);
            var mg = new MovingGroup(from, hitList, true)
            {
                ToStack = to
            };
            lmg.Add(mg);
        }

        Debug.WriteLine("");
        foreach (var mg in lmg)
            Debug.WriteLine(mg.ToString());

        return lmg;
    }

#if DEBUG

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

#endif

    internal bool CanUndo() => UndoStack.Count > 0;

    internal void PushUndo(MovingGroup mg)
    {
        UndoStack.Push(mg);
#if DEBUG
        CheckDeck();            // For debugging
        PrintGame();            // For debugging
#endif
    }

    internal MovingGroup PopUndo()
    {
        if (UndoStack.Count == 0)
            return null;
        return UndoStack.Pop();
        // Cannot update game status here, before executing the undo, contrary to
        // PushUndo, called after the move has been performed
    }

    internal void UpdateGameStatus()
    {
#if DEBUG
        CheckDeck();            // To be safe in debug mode
#endif
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

    // Used to abort a running previous computation if it's not finished
    private CancellationTokenSource cts;

    // Do the job in a background task, that can be interrupted if needed
    private void ComputeAndUpdateGameSolvability()
    {
        // If a previous evaluation is still in progress, cancel it
        cts?.Cancel();

        cts = new CancellationTokenSource();
        // Use a Progress<T>, simple mechanism to update interface from a different thread
        IProgress<string> progress = new Progress<string>((s) => SolverStatus = s);
        // GetSolverDeck access WPF objects (PlayingCard), so it must run in UI thread
        SolverDeck sd = GetSolverDeck();
        var t = new Task(() =>
        {
            if (cts.Token.IsCancellationRequested)
                goto EndTask;
            bool? res = sd.Solve(cancellationToken: cts.Token);
            if (res.HasValue)
                progress.Report(res.Value ? "Solvable" : "No solution");
            EndTask:
            ;
        });
        t.Start();      // Don't care about task ending
    }

    private int _MoveCount;

    public int MoveCount
    {
        get => _MoveCount;
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
        get => _GameStatus;
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
        get => _SolverStatus;
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
        get => _GameSerial;
        set
        {
            if (_GameSerial != value)
            {
                _GameSerial = value;
                NotifyPropertyChanged(nameof(GameSerial));
            }
        }
    }

#if DEBUG

    internal void PrintGame()
    {
        Debug.WriteLine("----------------------------------------------------------");
        Debug.WriteLine("Deck:");
        for (int bi = 0; bi < 4; bi++)
            PrintStack($"Base {bi}  ", Bases[bi]);
        for (int ci = 0; ci < 7; ci++)
            PrintStack($"Column {ci}", Columns[ci]);
        PrintStack("Talon FU    ", TalonFU);
        PrintStack("Talon FD    ", TalonFD);
    }

    private static void PrintStack(string header, GameStack st)
    {
        Debug.Write(header + " ");
        foreach (PlayingCard c in st.PlayingCards)
            Debug.Write(c.Signature() + " ");
        Debug.WriteLine("");
    }

#endif
}

public static class ExtensionMethods
{
    private static readonly Action EmptyDelegate = delegate () { };

    // Extension method to force the refresh of a UIElement
    public static void Refresh(this UIElement uiElement) =>
        // By calling Dispatcher.Invoke, the code essentially asks the system to execute all operations that are Render or higher priority,
        // thus the control will then render itself (drawing the new content).  Afterwards, it will then execute the provided delegate,
        // which is our empty method.
        uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
}
