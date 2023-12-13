// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// ViewModel of main Editor window
//
// 2017-07-22   PV      First version
// 2023-11-20   PV      Net8 C#12

using LibQwirkle;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Xps;

namespace QwirkleUI;

internal class ViewModel: INotifyPropertyChanged
{
    // Model and View
    private readonly Model model;

    private readonly MainWindow view;

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
    public ViewModel(MainWindow view)
    {
        // Initialize ViewModel
        this.view = view;
        model = new Model(this);

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

    public BoundingRectangle Bounds => model.Bounds();

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
        //        UpdateWordPositionLocation(action.WordAndCanvasList, action.PositionOrientationList, false);   // Coordinates in wordPositionList are updated
        //        view.MoveWordAndCanvasList(action.WordAndCanvasList);
        //        break;

        //    case UndoStackClass.UndoActions.Delete:
        //        view.AddWordAndCanvasList(action.WordAndCanvasList, false);
        //        break;

        //    case UndoStackClass.UndoActions.Add:
        //        view.DeleteWordAndCanvasList(action.WordAndCanvasList, false);
        //        break;

        //    case UndoStackClass.UndoActions.SwapOrientation:
        //        UpdateWordPositionLocation(action.WordAndCanvasList, action.PositionOrientationList, false);   // Coordinates in wordPositionList are updated
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

    internal void InitializeBoard() => model.InitializeBoard();

    internal void DrawAllTiles()
    {
        // ToDo: Should I ensure that DrawinfCanvas is empty here, or is it a view responsibility?
        view.AddCircle(new Position(50, 50));
        foreach (Move m in model.Board)
            view.AddUITile(new Position(m.Row, m.Col), m.Tile.Shape.ToString() + m.Tile.Color.ToString());
    }

    internal CellState GetCellState(int row, int col) 
        => model.Board.GetCellState(row, col);

    // -------------------------------------------------
    // Commands

    private void RecenterLayoutViewExecute(object obj) => view.RescaleAndCenter(true);        // Use animations

    private bool UndoCanExecute(object obj) => true;    // UndoStack.CanUndo;

    private void UndoExecute(object obj)
    {
        view.EndAnimationsInProgress();
        PerformUndo();
    }

    private bool SuggestPlayCanExecute(object obj) => true;

    private void SuggestPlayExecute(object obj)
    {
        view.EndAnimationsInProgress();
        // Delegate work to view since we have no access to Sel here
        MessageBox.Show("SuggestPlayExecute: ToDo");
    }

    private void NewGameExecute(object obj)
    {
        model.NewBoard();
        ClearLayout();
    }

    private void QuitExecute(object obj) => Environment.Exit(0);

    private void AboutExecute(object obj)
    {
        //var aw = new View.AboutWindow();
        //aw.ShowDialog();
    }

}
