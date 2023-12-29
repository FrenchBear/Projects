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

internal class MainViewModel: INotifyPropertyChanged
{
    // Model and View
    private readonly Model Model;
    private readonly MainWindow View;
    private readonly HandViewModel[] HandViewModels = [];
    internal readonly HashSet<UITileRowCol> MainWindowCurrentMoves = [];

    public int PlayerIndex = 0;

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
    public ICommand UndoCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SuggestPlayCommand { get; }

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
        UndoCommand = new RelayCommand<object>(UndoExecute, UndoCanExecute);
        DeleteCommand = new RelayCommand<object>(DeleteExecute, DeleteCanExecute);
        SuggestPlayCommand = new RelayCommand<object>(SuggestPlayExecute, SuggestPlayCanExecute);

        // View
        RescaleAndCenterCommand = new RelayCommand<object>(RescaleAndCenterExecute);

        // Help
        AboutCommand = new RelayCommand<object>(AboutExecute);
    }

    // -------------------------------------------------
    // Simple relays to model

    public BoundingRectangle Bounds()
    {
        int rowMin = Model.Board.RowMin;
        int colMin = Model.Board.ColMin;
        int rowMax = Model.Board.RowMax;
        int colMax = Model.Board.ColMax;

        Debug.WriteLine($"Board bounds: ({rowMin}, {colMin})-({rowMax}, {colMax})");

        if (MainWindowCurrentMoves.Count > 0)
        {
            Debug.WriteLine($"MainWindowCurrentMoves bounds: ({MainWindowCurrentMoves.Min(m => m.Row)}, {MainWindowCurrentMoves.Min(m => m.Col)})-({MainWindowCurrentMoves.Max(m => m.Row)}, {MainWindowCurrentMoves.Max(m => m.Col)})");
            rowMin = Math.Min(rowMin, MainWindowCurrentMoves.Min(m => m.Row));
            colMin = Math.Min(colMin, MainWindowCurrentMoves.Min(m => m.Col));
            rowMax = Math.Max(rowMax, MainWindowCurrentMoves.Max(m => m.Row));
            colMax = Math.Max(colMax, MainWindowCurrentMoves.Max(m => m.Col));
        }

        Debug.WriteLine($"Global bounds: ({rowMin}, {colMin})-({rowMax}, {colMax})");

        return new(new RowCol(rowMin, colMin), new RowCol(rowMax, colMax));

    }

    // -------------------------------------------------
    // Selection helpers

    // -------------------------------------------------
    // Bindings

    private string m_StatusText = "";

    public string StatusText
    {
        get => m_StatusText;
        set
        {
            if (m_StatusText != value)
            {
                m_StatusText = value;
                NotifyPropertyChanged(nameof(StatusText));
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

    // Delete command moves selection back to player hand
    internal void PerformDelete()
    {
        if (View.BoardIM.Selection.IsEmpty)
            return;

        Debug.WriteLine($"PerformDelete Start: Hand.Count={CurrentPlayer.Hand.Count} CurrentHandViewModel.UIHand.Count={CurrentHandViewModel.UIHand.Count}");
        Debug.WriteLine($"PerformDelete Start: MainWindowCurrentMoves.Count={MainWindowCurrentMoves.Count}");

        foreach (var uitrc in new List<UITileRowCol>(View.BoardIM.Selection))
        {
            // Add to Player Hand
            Debug.Assert(!CurrentPlayer.Hand.Contains(uitrc.UIT.Tile));
            CurrentPlayer.Hand.Add(uitrc.UIT.Tile);
            HandViewModels[PlayerIndex].AddAndDrawTile(uitrc.UIT.Tile);

            // Remove from Board
            var todel = MainWindowCurrentMoves.FirstOrDefault(item => item.UIT == uitrc.UIT);
            Debug.Assert(todel != null);
            MainWindowCurrentMoves.Remove(todel);              
            View.BoardRemoveUITile(uitrc.UIT);
        }

        Debug.WriteLine($"PerformDelete End: Hand.Count={CurrentPlayer.Hand.Count} CurrentHandViewModel.UIHand.Count={CurrentHandViewModel.UIHand.Count}");
        Debug.WriteLine($"PerformDelete End: MainWindowCurrentMoves.Count={MainWindowCurrentMoves.Count}");
        Debug.Assert(CurrentPlayer.Hand.Count == CurrentHandViewModel.UIHand.Count);
        Debug.Assert(CurrentPlayer.Hand.Count + MainWindowCurrentMoves.Count == 6);
    }

    // Remove from Model and HandViewModel
    internal void RemoveTileFromHand(UITile uit)
    {
        Debug.Assert(CurrentPlayer.Hand.Contains(uit.Tile));
        CurrentPlayer.Hand.Remove(uit.Tile);
        CurrentHandViewModel.RemoveUITile(uit);
    }

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
            View.BoardAddUITile(new RowCol(m.Row, m.Col), m.Tile, false);
    }

    // For dev, draw test tiles with gray background
    internal void DrawCurrentMoves()
    {
        foreach (UITileRowCol m in MainWindowCurrentMoves)
            MainWindowCurrentMoves.Add(View.BoardAddUITile(m.RC, m.UIT.Tile, true));
    }

    internal void DrawHands()
    {
        foreach (var hvm in HandViewModels)
            hvm.DrawHand();
    }

    internal void AddCurrentMove(UITileRowCol uitrc)
        => MainWindowCurrentMoves.Add(uitrc);

    internal void UpdateCurrentMoves(UITilesSelection selection)
    {
        foreach (UITileRowCol uitp in selection)
        {
            bool found = false;
            foreach (UITileRowCol m in MainWindowCurrentMoves)
            {
                if (m.UIT.Tile == uitp.UIT.Tile)
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
        Debug.Assert(MainWindowCurrentMoves.Contains(m));
        MainWindowCurrentMoves.Remove(m);
    }

    internal CellState GetCellState(int row, int col)
        => Model.Board.GetCellState(row, col);

    // -------------------------------------------------
    // Commands

    // Since the command is in the view, MainWindow.xaml should directly reference command in the view, no need for this ViewModel intermediate...
    private void RescaleAndCenterExecute(object obj)
        => View.RescaleAndCenter(true);        // Use animations

    private bool UndoCanExecute(object obj) => true;    // UndoStack.CanUndo;

    private void UndoExecute(object obj)
    {
        View.BoardIM.EndAnimationsInProgress();
        PerformUndo();
    }

    private bool DeleteCanExecute(object obj) => true;    // DeleteStack.CanDelete;

    private void DeleteExecute(object obj)
    {
        View.BoardIM.EndAnimationsInProgress();
        PerformDelete();
    }

    private bool SuggestPlayCanExecute(object obj) => true;

    private void SuggestPlayExecute(object obj)
    {
        View.BoardIM.EndAnimationsInProgress();
        // Delegate work to view since we have no access to Sel here
        MessageBox.Show("SuggestPlayExecute: ToDo");
    }

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
