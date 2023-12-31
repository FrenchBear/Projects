﻿// QwirkleUI
// ViewModel of Main Window
//
// 2017-07-22   PV      First version
// 2023-11-20   PV      Net8 C#12

using LibQwirkle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static QwirkleUI.App;
using static QwirkleUI.Helpers;

namespace QwirkleUI;

enum MoveStatus
{
    Empty,
    Invalid,
    Valid,
}

internal class MainViewModel: INotifyPropertyChanged
{
    // Model and View
    private readonly Model Model;
    private readonly MainWindow View;

    // Game state
    private readonly HandViewModel[] HandViewModels = [];
    internal readonly HashSet<UITileRowCol> CurrentMoves = [];
    internal MoveStatus CurrentMovesStatus = MoveStatus.Empty;
    private PlaySuggestion? PlaySuggestion;     // Suggestion with maximum possible scire with current hand

    // Helpers
    public Player CurrentPlayer => Model.CurrentPlayer;
    public HandViewModel CurrentHandViewModel => HandViewModels[Model.PlayerIndex];

    // Helper to initialize HandViewModel, since model is common to all ViewModels
    public Model GetModel => Model;

    // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Commands

    // Menus
    public ICommand NewGameCommand { get; }
    public ICommand AutoPlayCommand { get; }
    public ICommand QuitCommand { get; }

    // Edit
    public ICommand ValidateCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand DeleteAllCommand { get; }
    public ICommand SuggestPlayCommand { get; }
    public ICommand ExchangeTilesCommand { get; }

    // View
    public ICommand RescaleAndCenterCommand { get; }

    // About
    public ICommand AboutCommand { get; }

    // Constructor
    public MainViewModel(MainWindow view)
    {
        // Initialize ViewModel
        View = view;
        Model = new Model(this);

        // Just 1 player for now
        HandViewModels = new HandViewModel[1];
        HandViewModels[0] = new HandViewModel(view, view.Player1HandUserControl, Model, 0);

        // Binding commands with behavior

        // File
        NewGameCommand = new RelayCommand<object>(NewGameExecute);
        AutoPlayCommand = new RelayCommand<object>(AutoPlayExecute);
        QuitCommand = new RelayCommand<object>(QuitExecute);

        // Edit
        ValidateCommand = new RelayCommand<object>(ValidateExecute, ValidateCanExecute);
        UndoCommand = new RelayCommand<object>(UndoExecute, UndoCanExecute);
        DeleteCommand = new RelayCommand<object>(DeleteExecute, DeleteCanExecute);
        DeleteAllCommand = new RelayCommand<object>(DeleteAllExecute, DeleteAllCanExecute);
        SuggestPlayCommand = new RelayCommand<object>(SuggestPlayExecute, SuggestPlayCanExecute);
        ExchangeTilesCommand = new RelayCommand<object>(ExchangeTilesExecute, ExchangeTilesCanExecute);

        // View
        RescaleAndCenterCommand = new RelayCommand<object>(RescaleAndCenterExecute);

        // Help
        AboutCommand = new RelayCommand<object>(AboutExecute);
    }

    // -------------------------------------------------
    // Relays to model

    internal void NewBoard(bool withTestInit)
        => Model.NewBoard(withTestInit);

    public BoundingRectangle Bounds()
    {
        int rowMin = Model.Board.RowMin;
        int colMin = Model.Board.ColMin;
        int rowMax = Model.Board.RowMax;
        int colMax = Model.Board.ColMax;

        Debug.WriteLine($"Board bounds: ({rowMin}, {colMin})-({rowMax}, {colMax})");

        if (CurrentMoves.Count > 0)
        {
            Debug.WriteLine($"MainWindowCurrentMoves bounds: ({CurrentMoves.Min(m => m.Row)}, {CurrentMoves.Min(m => m.Col)})-({CurrentMoves.Max(m => m.Row)}, {CurrentMoves.Max(m => m.Col)})");
            rowMin = Math.Min(rowMin, CurrentMoves.Min(m => m.Row));
            colMin = Math.Min(colMin, CurrentMoves.Min(m => m.Col));
            rowMax = Math.Max(rowMax, CurrentMoves.Max(m => m.Row));
            colMax = Math.Max(colMax, CurrentMoves.Max(m => m.Col));
        }

        Debug.WriteLine($"Global bounds: ({rowMin}, {colMin})-({rowMax}, {colMax})");

        return new(new RowCol(rowMin, colMin), new RowCol(rowMax, colMax));

    }

    internal void EvaluateCurrentMoves()
    {
        TraceCall();

        // If there is no move, provide hint about possible best play
        if (CurrentMoves.Count == 0)
        {
            CurrentMovesStatus = MoveStatus.Empty;

            // ToDo: Implement endgame detection
            // Only for dev, until endgame is handled
            if (CurrentPlayer.Hand.Count == 0)
            {
                StatusMessage = "Info: La main est vide, pas de jeu possible.";
                return;
            }

            if (PlaySuggestion == null)
            {
                Debug.WriteLine("PlaySuggestion calculated");
                PlaySuggestion = Model.Board.Play(CurrentPlayer.Hand);
            }
            else
                Debug.WriteLine("PlaySuggestion already defined");
            if (PlaySuggestion.PB.Points == 0)
                StatusMessage = "Info: Aucune tuile jouable, échangez les tuiles.";
            else
                StatusMessage = $"Info: Il existe un placement à {PlaySuggestion.PB.Points} points.";
            return;
        }

        bool status;
        string msg;
        var moves = new HashSet<TileRowCol>(CurrentMoves.Select(uitrc => new TileRowCol(uitrc.Tile, uitrc.RC)));
        (status, msg) = Model.EvaluateMoves(new HashSet<TileRowCol>(moves));
        if (status)
        {
            PointsBonus pb = Model.CountPoints(moves);
            StatusMessage = $"OK: {pb.Points} points";
            CurrentMovesStatus = MoveStatus.Valid;
        }
        else
        {
            StatusMessage = $"Problème: {msg}.";
            CurrentMovesStatus = MoveStatus.Invalid;
        }
    }

    // -------------------------------------------------
    // Bindings

    private string m_StatusMessage = "";

    public string StatusMessage
    {
        get => m_StatusMessage;
        set
        {
            if (m_StatusMessage != value)
            {
                m_StatusMessage = value;
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    public string Caption => AppName;

    // -------------------------------------------------
    // Undo support

    // Should implement singleton pattern
    //public UndoStackClass UndoStack = new();

    internal void PerformUndo()
    {
        View.BoardIM.EndAnimationsInProgress();

        //UndoStackClass.UndoAction action = UndoStack.Pop();

        //switch (action.Action)
        //{
        //    case UndoStackClass.UndoActions.TileRowCol:
        //        UpdateWordRowColLocation(action.WordAndCanvasList, action.RowColOrientationList, false);   // Coordinates in wordRowColList are updated
        //        view.MoveWordAndCanvasList(action.WordAndCanvasList);
        //        break;

        //    case UndoStackClass.UndoActions.Delete:
        //        view.AddWordAndCanvasList(action.WordAndCanvasList, false);
        //        break;

        //    case UndoStackClass.UndoActions.Add:
        //        view.DeleteWordAndCanvasList(action.WordAndCanvasList, false);
        //        break;

        //    case UndoStackClass.UndoActions.SwapOrientation:
        //        UpdateWordRowColLocation(action.WordAndCanvasList, action.RowColOrientationList, false);   // Coordinates in wordRowColList are updated
        //        view.SwapOrientation(action.WordAndCanvasList, false);
        //        break;

        //    default:
        //        Debug.Assert(false, "Unknown/Unsupported Undo Action");
        //        break;
        //}

        //view.FinalRefreshAfterUpdate();
    }

    void RefillPlayerHand()
    {
        TraceCall();

        while (!Model.Bag.IsEmpty && CurrentHandViewModel.UIHand.Count < 6)
        {
            var t = Model.Bag.GetTile();
            CurrentHandViewModel.AddAndDrawTile(t);
            CurrentPlayer.Hand.Add(t);

            PlaySuggestion = null;
        }
    }

    // Validate command
    void PerformValidate()
    {
        TraceCall();

        if (CurrentMoves.Count == 0)
            return;

        View.BoardIM.EndAnimationsInProgress();

        var moves = new HashSet<TileRowCol>(CurrentMoves.Select(uitrc => new TileRowCol(uitrc.Tile, uitrc.RC)));
        var pb = Model.Board.CountPoints(moves);
        Debug.Assert(pb.Points > 0);
        CurrentPlayer.Score += pb.Points;
        CurrentHandViewModel.Score = CurrentPlayer.Score.ToString();
        Model.UpdateRanks();
        CurrentHandViewModel.Rank = CurrentPlayer.Rank;

        Model.Board.AddMoves(moves, true);
        foreach (var move in CurrentMoves)
        {
            move.UIT.SelectionBorder = false;
            move.UIT.GrayBackground = false;
            Debug.Assert(CurrentPlayer.Hand.Contains(move.Tile));
            CurrentPlayer.Hand.Remove(move.Tile);
        }
        CurrentMoves.Clear();
        CurrentMovesStatus = MoveStatus.Empty;
        StatusMessage = string.Empty;

        RefillPlayerHand();

        EvaluateCurrentMoves();

        // ToDo: Switch to next player
    }

    // Delete command moves selection back to player hand
    // CurrentPlayer.Hand is not updated untit current moves are validated
    // Note that EvaluateCurrentMoves is not called here since this is used as a subprogram for Suggestion
    internal void PerformDelete(bool allCurrentMoves)
    {
        TraceCall();

        if (allCurrentMoves)
        {
            if (CurrentMoves.Count == 0)
                return;
        }
        else
        {
            if (View.BoardIM.Selection.IsEmpty)
                return;
        }

        View.BoardIM.EndAnimationsInProgress();

        foreach (var uitrc in new List<UITileRowCol>(allCurrentMoves ? CurrentMoves : View.BoardIM.Selection))
        {
            // Add to Player Hand
            CurrentHandViewModel.AddAndDrawTile(uitrc.Tile);

            // Remove from CurrentMoves and DrawingCanvas
            var todel = CurrentMoves.FirstOrDefault(item => item.UIT == uitrc.UIT);
            Debug.Assert(todel != null);
            CurrentMoves.Remove(todel);
            View.BoardDrawingCanvasRemoveUITile(uitrc.UIT);
        }
    }

    internal void PerformExchangeTiles()
    {
        TraceCall();

        if (Model.Bag.IsEmpty || CurrentPlayer.Hand.Count == 0)
            return;

        Model.Bag.ReturnTiles(CurrentPlayer.Hand);
        CurrentPlayer.Hand.Clear();
        CurrentHandViewModel.RemoveAllUITiles();
        RefillPlayerHand();

        EvaluateCurrentMoves();
    }

    // Suggest a tiles placement
    // Returns true if a valid suggestion has been made, false otherwise
    internal int PerformSuggestPlay(bool interactive = true)
    {
        TraceCall();

        // First, move all tiles back to hand
        View.BoardIM.Selection.Clear();
        PerformDelete(true);

        // ToDo: Review this, actually as soon as one hand is empty after validation, last turn mode begins.
        // Once last player has played, it's endgame, so in theory we shouldn't meet this case
        if (CurrentPlayer.Hand.Count == 0)
        {
            if (interactive)
                MessageBox.Show("Désolé, la main est vide!", AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return 1;
        }

        Debug.Assert(PlaySuggestion != null);
        // $$$ Commenting next line causes a problem, while it should not...
        //PlaySuggestion = Model.Board.Play(CurrentPlayer.Hand);
        if (PlaySuggestion.PB.Points == 0)
        {
            if (interactive)
                MessageBox.Show("Désolé, aucune tuile ne peut être jouée. Échangez les tuiles!", AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return 2;
        }

        // Delete from hand, add to Board
        foreach (var trc in PlaySuggestion.Moves)
        {
            // Delete from Hand
            CurrentHandViewModel.RemoveUITileFromTile(trc.Tile);
            // Add to Board
            var uitile = View.BoardDrawingCanvasAddUITile(trc.Tile, trc.RC, true);
            CurrentMoves.Add(uitile);
        }

        EvaluateCurrentMoves();
        View.RescaleAndCenter(true);
        return 0;
    }

    private void PerformAutoPlay()
    {
        for (; ; )
        {
            for (; ; )
            {
                int res = PerformSuggestPlay(false);
                if (res == 2)
                {
                    PerformExchangeTiles();
                    continue;
                }
                if (res == 1)
                    break;
                PerformValidate();
                View.Refresh();
            }

            PerformNewGame();
            View.RescaleAndCenter(false, true);
        }
    }

    // Remove from Model and HandViewModel
    internal void RemoveUITileFromHand(UITile uit)
        => CurrentHandViewModel.RemoveUITile(uit);

    // -------------------------------------------------
    // Model helpers

    internal void PerformNewGame()
    {
        TraceCall();

        Model.NewBoard(false);
        View.BoardDrawingCanvasRemoveAllUITiles();
        CurrentMoves.Clear();
        for (int i = 0; i < Model.Players.Length; i++)
            HandViewModels[i].RemoveAllUITiles();
        DrawHands();
        //UndoStack.Clear();

        EvaluateCurrentMoves();
    }

    // -------------------------------------------------
    // View helpers

    // ToDo: who's calling?
    //internal void InitializeBoard() => Model.InitializeBoard();

    // Draw board placed tiles with a dark background
    // ToDo: Only for dev I think
    internal void DrawBoard()
    {
        foreach (TileRowCol m in Model.Board)
            View.BoardDrawingCanvasAddUITile(m.Tile, new RowCol(m.Row, m.Col), false);
    }

    // For dev, draw test tiles with gray background
    internal void DrawCurrentMoves()
    {
        foreach (UITileRowCol m in CurrentMoves)
            CurrentMoves.Add(View.BoardDrawingCanvasAddUITile(m.Tile, m.RC, true));
    }

    internal void DrawHands()
    {
        foreach (var hvm in HandViewModels)
            hvm.DrawHand();
    }

    internal void AddCurrentMove(UITileRowCol uitrc)
        => CurrentMoves.Add(uitrc);

    internal void UpdateCurrentMoves(UITilesSelection selection)
    {
        foreach (UITileRowCol uitp in selection)
        {
            bool found = false;
            foreach (UITileRowCol m in CurrentMoves)
            {
                if (m.Tile == uitp.Tile)
                {
                    RemoveCurrentMove(m);
                    found = true;
                    break;
                }
            }
            Debug.Assert(found);
            CurrentMoves.Add(new UITileRowCol(uitp.UIT, uitp.RC));
        }
    }

    internal void RemoveCurrentMove(UITileRowCol m)
    {
        Debug.Assert(CurrentMoves.Contains(m));
        CurrentMoves.Remove(m);
    }

    internal CellState GetCellState(int row, int col)
        => Model.Board.GetCellState(row, col);

    // -------------------------------------------------
    // Commands

    // Since the command is in the view, MainWindow.xaml should directly reference command in the view, no need for this ViewModel intermediate...
    private void RescaleAndCenterExecute(object obj)
        => View.RescaleAndCenter(true, true);        // Use animations

    // -----------------------------------

    private bool ValidateCanExecute(object obj) => CurrentMovesStatus == MoveStatus.Valid;

    private void ValidateExecute(object obj) => PerformValidate();

    // -----------------------------------

    private bool UndoCanExecute(object obj) => false;    // UndoStack.CanUndo;

    private void UndoExecute(object obj) => PerformUndo();

    // -----------------------------------

    private bool DeleteCanExecute(object obj) => !View.BoardIM.Selection.IsEmpty;

    private void DeleteExecute(object obj)
    {
        PerformDelete(false);
        EvaluateCurrentMoves();
    }

    // -----------------------------------

    private bool DeleteAllCanExecute(object obj) => CurrentMoves.Count > 0;

    private void DeleteAllExecute(object obj)
    {
        PerformDelete(true);
        EvaluateCurrentMoves();
    }

    // -----------------------------------

    private bool SuggestPlayCanExecute(object obj) => Model.Players.Length != 0 && CurrentPlayer.Hand.Count > 0;

    private void SuggestPlayExecute(object obj) => PerformSuggestPlay();

    // -----------------------------------

    private bool ExchangeTilesCanExecute(object obj) => !Model.IsBagEmpty;

    private void ExchangeTilesExecute(object obj) => PerformExchangeTiles();

    // -----------------------------------

    private void NewGameExecute(object obj) => PerformNewGame();

    private void AutoPlayExecute(object obj) => PerformAutoPlay();

    private void QuitExecute(object obj) => Environment.Exit(0);

    private void AboutExecute(object obj)
    {
        //var aw = new View.AboutWindow();
        //aw.ShowDialog();
    }

}
