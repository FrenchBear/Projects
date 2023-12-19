// QwirkleUI
// ViewModel of Main Window
//
// 2017-07-22   PV      First version
// 2023-11-20   PV      Net8 C#12

using LibQwirkle;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace QwirkleUI;

internal class MainViewModel: INotifyPropertyChanged
{
    // Model and View
    private readonly Model Model;
    private readonly MainWindow View;

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
    public ICommand SuggestPlayCommand { get; }

    // View
    public ICommand RecenterLayoutViewCommand { get; }

    // About
    public ICommand AboutCommand { get; }

    // Constructor
    public MainViewModel(MainWindow view)
    {
        // Initialize ViewModel
        this.View = view;
        Model = new Model(this);

        // Binding commands with behavior

        // File
        NewGameCommand = new RelayCommand<object>(NewGameExecute);
        QuitCommand = new RelayCommand<object>(QuitExecute);

        // Edit
        UndoCommand = new RelayCommand<object>(UndoExecute, UndoCanExecute);
        SuggestPlayCommand = new RelayCommand<object>(SuggestPlayExecute, SuggestPlayCanExecute);

        // View
        RecenterLayoutViewCommand = new RelayCommand<object>(RecenterLayoutViewExecute);

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

    // -------------------------------------------------
    // Model helpers

    internal void ClearLayout()
    {
        //UndoStack.Clear();
    }

    // -------------------------------------------------
    // View helpers

    internal void InitializeBoard() => Model.InitializeBoard();

    internal void DrawAllTiles()
    {
        // ToDo: Should I ensure that DrawinfCanvas is empty here, or is it a view responsibility?
        View.AddCircle(new RowCol(50, 50));
        foreach (Move m in Model.Board)
            View.AddUITile(new RowCol(m.Row, m.Col), m.Tile.Shape.ToString() + m.Tile.Color.ToString(), m.Tile.Instance);

        // ToDo: Probably fill a structure maintainig connection between Move/Tile and UITile
    }

    internal CellState GetCellState(int row, int col) 
        => Model.Board.GetCellState(row, col);

    // -------------------------------------------------
    // Commands

    private void RecenterLayoutViewExecute(object obj) => View.RescaleAndCenter(true);        // Use animations

    private bool UndoCanExecute(object obj) => true;    // UndoStack.CanUndo;

    private void UndoExecute(object obj)
    {
        View.BoardIM.EndAnimationsInProgress();
        PerformUndo();
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
