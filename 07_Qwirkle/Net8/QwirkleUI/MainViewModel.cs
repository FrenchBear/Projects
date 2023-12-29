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

    public BoundingRectangle Bounds => Model.Bounds();

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
        //    case UndoStackClass.UndoActions.Move:
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

        //var cmp = EqualityComparer<UITileRowCol>.Default;

        Debug.WriteLine($"PerformDelete Start: Hand.Count={CurrentPlayer.Hand.Count} CurrentHandViewModel.UIHand.Count={CurrentHandViewModel.UIHand.Count}");
        Debug.WriteLine($"PerformDelete Start: MainWindowCurrentMoves.Count={View.MainWindowCurrentMoves.Count} Model.CurrentMoves.Count={Model.CurrentMoves.Count}");
        UITileRowCol item1 = null;
        foreach (var item in View.MainWindowCurrentMoves)
        {
            Debug.WriteLine($"Contains: {View.MainWindowCurrentMoves.Contains(item)}");
            item1 = item;
            //Debug.WriteLine($"Hash1: {item.GetHashCode():X08}  {cmp.GetHashCode(item)}");
        }

        foreach (var uitrc in new List<UITileRowCol>(View.BoardIM.Selection))
        {
            // Add to Player Hand
            Debug.Assert(!CurrentPlayer.Hand.Contains(uitrc.UIT.Tile));
            CurrentPlayer.Hand.Add(uitrc.UIT.Tile);
            HandViewModels[PlayerIndex].AddAndDrawTile(uitrc.UIT.Tile);

            // Remove from Board
            var todel = View.MainWindowCurrentMoves.FirstOrDefault(item => item.UIT == uitrc.UIT);
            Debug.Assert(todel!=null);
            bool ok = View.MainWindowCurrentMoves.Contains(todel);
            Debug.WriteLine($"View.MainWindowCurrentMoves.Contains(todel): {ok}");
            //Debug.WriteLine($"Hash2: {todel.GetHashCode():X08}  {cmp.GetHashCode(todel):X08}");
            //Debug.WriteLine($"Equals: {cmp.Equals(item1, todel)}");
            
            if (!ok) Debugger.Break();
            View.MainWindowCurrentMoves.Remove(todel);              // $$$ Does not work
// todel==x1
// true
// View.MainWindowCurrentMoves.Contains(todel)
// false
             
            View.BoardRemoveUITile(uitrc.UIT);

            // Remove from model current moves
            var todel2 = Model.CurrentMoves.FirstOrDefault(item => item.Tile == uitrc.UIT.Tile);
            Debug.Assert(todel2 != null);
            Model.CurrentMoves.Remove(todel2);
        }

        Debug.WriteLine($"PerformDelete End: Hand.Count={CurrentPlayer.Hand.Count} CurrentHandViewModel.UIHand.Count={CurrentHandViewModel.UIHand.Count}");
        Debug.WriteLine($"PerformDelete End: MainWindowCurrentMoves.Count={View.MainWindowCurrentMoves.Count} Model.CurrentMoves.Count={Model.CurrentMoves.Count}");
        Debug.Assert(CurrentPlayer.Hand.Count == CurrentHandViewModel.UIHand.Count);
        Debug.Assert(View.MainWindowCurrentMoves.Count == Model.CurrentMoves.Count);
        Debug.Assert(CurrentPlayer.Hand.Count + View.MainWindowCurrentMoves.Count == 6);
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
        foreach (Move m in Model.Board)
            View.BoardAddUITile(new RowCol(m.Row, m.Col), m.Tile, false);
    }

    // For dev, draw test tiles with gray background
    internal void DrawCurrentMoves()
    {
        foreach (Move m in Model.CurrentMoves)
            View.MainWindowCurrentMoves.Add(View.BoardAddUITile(new RowCol(m.Row, m.Col), m.Tile, true));
    }

    internal void DrawHands()
    {
        foreach (var hvm in HandViewModels)
            hvm.DrawHand();
    }

    internal void AddCurrentMove(Move m)
        => Model.CurrentMoves.Add(m);

    internal void UpdateCurrentMoves(UITilesSelection selection)
    {
        foreach (UITileRowCol uitp in selection)
        {
            // ToDo: Move this to ViewModel once it works
            bool found = false;
            foreach (Move m in Model.CurrentMoves)
            {
                if (m.T == uitp.UIT.Tile)
                {
                    RemoveCurrentMove(m);
                    found = true;
                    break;
                }
            }
            Debug.Assert(found);
            AddCurrentMove(new Move(uitp.RC.Row, uitp.RC.Col, uitp.UIT.Tile));
        }
    }

    internal void RemoveCurrentMove(Move m)
    {
        Debug.Assert(Model.CurrentMoves.Contains(m));
        Model.CurrentMoves.Remove(m);
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
