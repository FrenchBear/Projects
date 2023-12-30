// QwirkleUI
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
    public int PlayerIndex = 0;

    // Helpers
    public Player CurrentPlayer => Model.Players[PlayerIndex];
    public HandViewModel CurrentHandViewModel => HandViewModels[PlayerIndex];

    // Helper to initialize HandViewModel, since model is common to all ViewModels
    public Model GetModel => Model;

    // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Commands

    // Menus
    public ICommand NewGameCommand { get; }
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
        // If there is no move, profind hint about possible best play
        if (CurrentMoves.Count == 0)
        {
            CurrentMovesStatus = MoveStatus.Empty;

            var ps = Model.Board.Play(CurrentPlayer.Hand);
            StatusMessage = $"Info: Il existe un pacement à {ps.PB.Points} points";
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

    public string Caption => App.AppName;

    
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

    // Validate command
    void PerformValidate()
    {
        if (CurrentMoves.Count==0)
            return;

        View.BoardIM.EndAnimationsInProgress();

        Model.Board.AddMoves(new HashSet<TileRowCol>(CurrentMoves.Select(uitrc => new TileRowCol(uitrc.Tile, uitrc.RC))));
        foreach (var move in CurrentMoves)
        {
            move.UIT.SelectionBorder = false;
            move.UIT.GrayBackground= false;
        }
        CurrentMoves.Clear();
        CurrentMovesStatus = MoveStatus.Empty;
        StatusMessage = string.Empty;

        // Refill player hand
        while (!Model.Bag.IsEmpty && CurrentHandViewModel.UIHand.Count<6)
        {
            var t = Model.Bag.GetTile();
            CurrentHandViewModel.AddAndDrawTile(t);
        }

        // ToDo: switch to next player
    }

    // Delete command moves selection back to player hand
    // CurrentPlayer.Hand is not updated untit current moves are validated
    internal void PerformDelete(bool allCurrentMoves)
    {
        if (View.BoardIM.Selection.IsEmpty)
            return;

        View.BoardIM.EndAnimationsInProgress();

        foreach (var uitrc in new List<UITileRowCol>(allCurrentMoves ? CurrentMoves : View.BoardIM.Selection))
        {
            // Add to Player Hand
            HandViewModels[PlayerIndex].AddAndDrawTile(uitrc.Tile);

            // Remove from CurrentMoves and DrawingCanvas
            var todel = CurrentMoves.FirstOrDefault(item => item.UIT == uitrc.UIT);
            Debug.Assert(todel != null);
            CurrentMoves.Remove(todel);
            View.BoardDrawingCanvasRemoveUITile(uitrc.UIT);
        }

        EvaluateCurrentMoves();
    }

    // Suggest a tiles placement
    internal void PerformSuggestPlay()
    {
        View.BoardIM.EndAnimationsInProgress();

        MessageBox.Show("PerformSuggestPlay: ToDo");
    }

    // Remove from Model and HandViewModel
    internal void RemoveUITileFromHand(UITile uit) =>
        //Debug.Assert(CurrentPlayer.Hand.Contains(Tile));
        //CurrentPlayer.Hand.Remove(Tile);
        CurrentHandViewModel.RemoveUITile(uit);

    // -------------------------------------------------
    // Model helpers

    internal void ClearLayout()
    {
        //UndoStack.Clear();
    }

    // -------------------------------------------------
    // View helpers

    internal void InitializeBoard() => Model.InitializeBoard();

    // Draw board placed tiles with a dark background
    internal void DrawBoard()
    {
        View.AddCircle(new RowCol(50, 50));
        foreach (TileRowCol m in Model.Board)
            View.BoardDrawingCanvasAddUITile(new RowCol(m.Row, m.Col), m.Tile, false);
    }

    // For dev, draw test tiles with gray background
    internal void DrawCurrentMoves()
    {
        foreach (UITileRowCol m in CurrentMoves)
            CurrentMoves.Add(View.BoardDrawingCanvasAddUITile(m.RC, m.Tile, true));
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
            AddCurrentMove(new UITileRowCol(uitp.UIT, uitp.RC));
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

    private void DeleteExecute(object obj) => PerformDelete(false);

    // -----------------------------------

    private bool DeleteAllCanExecute(object obj) => CurrentMoves.Count>0;

    private void DeleteAllExecute(object obj) => PerformDelete(true);

    // -----------------------------------

    private bool SuggestPlayCanExecute(object obj) => true;

    private void SuggestPlayExecute(object obj) => PerformSuggestPlay();

    // -----------------------------------

    private bool ExchangeTilesCanExecute(object obj) => !Model.IsBagEmpty;

    private void ExchangeTilesExecute(object obj)
    {
        View.BoardIM.EndAnimationsInProgress();
        // Delegate work to view since we have no access to Sel here
        MessageBox.Show("SuggestPlayExecute: ToDo");
    }

    // -----------------------------------

    private void NewGameExecute(object obj)
    {
        Model.NewBoard();
        ClearLayout();
    }

    private void QuitExecute(object obj) => Environment.Exit(0);

    private void AboutExecute(object obj)
    {
        //var aw = new View.AboutWindow();
        //aw.ShowDialog();
    }

}
